using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// EN 410:2011 normal-incidence glazing calculator with spectral pipeline (preferred) and
    /// band-averaged fallback. Computes Tv, Rv (ext/int), τe, ρe (ext/int), per-pane A_i,
    /// qi split (EN 410 §5.4.6-style network), g-value, and SC = g/0.87.
    ///
    /// Key points:
    ///  - Spectral path: combine optics per wavelength (multi-reflection) then integrate with
    ///    EN 410 solar weights and D65·V(λ) visible weights (user-supplied arrays).
    ///  - Fallback path: uses band-averaged inputs (your existing data).
    ///  - Thermal split: centre-of-pane network with he=23, hi=8 (defaults), glass conduction,
    ///    and gap conductance (convective + hr). Default hr linearized at 10 °C.
    ///
    /// NOTES:
    ///  - For best TAS parity, use the spectral path with proper EN 410 weight sets and EN 12898 emissivities.
    ///  - All arrays are validated and lightly clamped to [0,1], enforcing t + r ≤ 1 per side.
    /// </summary>
    public static class Glazing
    {
        // ---------- User-facing inputs ----------

        /// <summary>
        /// A single glazing layer (pane).
        /// Provide EITHER band-averaged (TauSolar/RfSolar/RbSolar + TauVis/RfVis/RbVis)
        /// OR spectral arrays Tau/Rf/Rb as functions of wavelength Lambda_nm.
        /// Thickness + conductivity are for the qi thermal split.
        /// </summary>
        public sealed class Layer
        {
            // ---- Band-averaged (fallback) ----
            public double TauSolar, RfSolar, RbSolar; // solar band averages (outside incidence)
            public double TauVis, RfVis, RbVis;   // visible band averages

            // ---- Spectral data (preferred) ----
            // Lambda_nm must be strictly increasing; Tau/Rf/Rb same length.
            // Spectral ranges typically 300–2500 nm (solar), with D65·V(λ) active 380–780 nm.
            public double[] Lambda_nm;  // optional
            public double[] Tau;        // optional
            public double[] Rf;         // optional (front reflectance vs. outside)
            public double[] Rb;         // optional (back reflectance vs. inside)

            // ---- Thermal (for qi) ----
            public double Thickness_m;       // e.g., 0.006 for 6 mm glass
            public double Conductivity_WmK;  // ~1.0 for float glass

            public Layer(
                double tauSolar, double rfSolar, double rbSolar,
                double tauVis, double rfVis, double rbVis,
                double thickness_m, double conductivity_WmK)
            {
                TauSolar = tauSolar; RfSolar = rfSolar; RbSolar = rbSolar;
                TauVis = tauVis; RfVis = rfVis; RbVis = rbVis;
                Thickness_m = thickness_m; Conductivity_WmK = conductivity_WmK;
            }

            public static Layer FromSpectral(
                double[] lambda_nm, double[] tau, double[] rf, double[] rb,
                double thickness_m, double conductivity_WmK)
            {
                if (lambda_nm == null || tau == null || rf == null || rb == null)
                    throw new ArgumentException("Spectral arrays must be non-null.");
                if (!(lambda_nm.Length == tau.Length && tau.Length == rf.Length && rf.Length == rb.Length))
                    throw new ArgumentException("Spectral arrays must be the same length.");
                if (lambda_nm.Length < 2) throw new ArgumentException("Provide at least 2 spectral samples.");
                // Defensive copy (caller may mutate later)
                var L = new Layer(0, 0, 0, 0, 0, 0, thickness_m, conductivity_WmK)
                {
                    Lambda_nm = (double[])lambda_nm.Clone(),
                    Tau = (double[])tau.Clone(),
                    Rf = (double[])rf.Clone(),
                    Rb = (double[])rb.Clone()
                };
                return L;
            }

            public bool HasSpectral =>
                Lambda_nm != null && Tau != null && Rf != null && Rb != null &&
                Lambda_nm.Length > 1 &&
                Lambda_nm.Length == Tau.Length &&
                Tau.Length == Rf.Length &&
                Rf.Length == Rb.Length;
        }

        /// <summary>
        /// A gas gap (cavity) between panes, represented by its total conductance h_gap [W/m²K].
        /// </summary>
        public sealed class Gap
        {
            /// <summary>Total cavity conductance h_gap = h_conv + h_rad [W/m²K]</summary>
            public double Conductance_Wm2K;

            public Gap(double conductance_Wm2K)
            {
                if (conductance_Wm2K <= 0) throw new ArgumentOutOfRangeException(nameof(conductance_Wm2K));
                Conductance_Wm2K = conductance_Wm2K;
            }

            public double R => 1.0 / Conductance_Wm2K; // m²K/W
        }

        /// <summary>
        /// Spectral integration weights: EN 410 solar spectrum and D65·V(λ) (photopic) on a common λ-grid.
        /// Lengths must match and λ must be strictly increasing.
        /// </summary>
        public sealed class SpectralWeights
        {
            public double[] Lambda_nm; // common grid (e.g., 300–2500 nm, Δλ=10 nm)
            public double[] Solar;     // EN 410 solar spectral distribution (arbitrary scale)
            public double[] D65V;      // D65 * V(λ) luminous weighting (non-negative; usually zero outside 380–780)

            private SpectralWeights(double[] lambda_nm, double[] solar, double[] d65v)
            {
                Lambda_nm = lambda_nm; Solar = solar; D65V = d65v;
            }

            public static SpectralWeights FromArrays(double[] lambda_nm, double[] solar, double[] d65v)
            {
                if (lambda_nm == null || solar == null || d65v == null)
                    throw new ArgumentException("Weights arrays must be non-null.");
                if (!(lambda_nm.Length == solar.Length && solar.Length == d65v.Length))
                    throw new ArgumentException("Weights arrays must be the same length.");
                if (lambda_nm.Length < 2) throw new ArgumentException("Provide at least 2 weight samples.");
                // Validate strictly increasing λ
                for (int i = 1; i < lambda_nm.Length; i++)
                    if (!(lambda_nm[i] > lambda_nm[i - 1]))
                        throw new ArgumentException("Lambda_nm must be strictly increasing.");
                // Defensive copy
                return new SpectralWeights(
                    (double[])lambda_nm.Clone(),
                    (double[])solar.Clone(),
                    (double[])d65v.Clone());
            }
        }

        // ---------- Outputs ----------
        public sealed class Result
        {
            // Visible (outside incidence)
            public double Tv;    // visible transmittance (D65·V)
            public double RvExt; // visible reflectance, exterior incidence (D65·V)
            public double RvInt; // visible reflectance, interior incidence (D65·V)

            // Solar (outside / inside incidence)
            public double TauSolar;  // τe, exterior incidence (EN 410)
            public double RSolarExt; // ρe, exterior incidence
            public double RSolarInt; // ρe, interior incidence
            public double[] ASolarByLayer; // per-pane solar absorptance (exterior incidence)
            public double ASolarTotal;

            // Thermal split and g
            public double[] QiByLayer; // fraction of each pane's absorbed energy to room
            public double GValue;      // total solar energy transmittance (solar factor)
            public double ShadingCoefficient; // g / 0.87

            // Numerical energy checks (should be near 0)
            public double SolarBalanceResidualExt; // |1 - (τe + ρe + ΣA_i)| exterior
            public double SolarBalanceResidualInt; // |1 - (τe(int?) + ρe(int) + ΣA_i(int))| interior (uses interior optical balance)

            // Meta
            public bool UsedSpectralPath;
        }

        // ---------- Public API ----------

        /// <summary>
        /// Compute EN 410 outputs for an arbitrary glazing stack (single/double/triple/...).
        /// Provide:
        ///  - spectral data for each pane + EN 410 weight sets (preferred), OR
        ///  - band-averaged inputs (fallback).
        /// Gaps.Count must be Layers.Count - 1.
        /// Defaults (EN 410 convention for g): he=23, hi=8 W/m²K (vertical, unventilated).
        /// </summary>
        public static Result Compute(
            IList<Layer> layers,
            IList<Gap> gaps,
            SpectralWeights weights = null,
            double he_Wm2K = 23.0,
            double hi_Wm2K = 8.0,
            double cutoff = 1e-12,
            int maxBounces = 40000)
        {
            if (layers == null || layers.Count == 0)
            {
                throw new ArgumentException("At least one layer is required.", nameof(layers));
            }

            if (gaps == null)
            {
                gaps = Array.Empty<Gap>();
            }
            
            if (gaps.Count != System.Math.Max(0, layers.Count - 1))
            {
                throw new ArgumentException("gaps.Count must equal layers.Count - 1.");
            }

            // Choose spectral vs. band-averaged path
            bool canDoSpectral = weights != null && layers.All(L => L.HasSpectral);
            SpectralDirectionIntegrals ext, intl;
            bool usedSpectral = false;

            if (canDoSpectral)
            {
                // Spectral EN 410 path
                usedSpectral = true;

                // Exterior incidence
                ext = IntegrateSpectralDirection(layers, weights, exteriorIncidence: true, cutoff: cutoff, maxBounces: maxBounces);

                // Interior incidence (for R_int and an internal energy check)
                intl = IntegrateSpectralDirection(layers, weights, exteriorIncidence: false, cutoff: cutoff, maxBounces: maxBounces);
            }
            else
            {
                // Band-averaged fallback
                var solExt = SolveOptics_Band(layers, band: "solar", exteriorIncidence: true, cutoff: cutoff, maxBounces: maxBounces);
                var solInt = SolveOptics_Band(layers, band: "solar", exteriorIncidence: false, cutoff: cutoff, maxBounces: maxBounces);
                var visExt = SolveOptics_Band(layers, band: "vis", exteriorIncidence: true, cutoff: cutoff, maxBounces: maxBounces);
                var visInt = SolveOptics_Band(layers, band: "vis", exteriorIncidence: false, cutoff: cutoff, maxBounces: maxBounces);

                ext = new SpectralDirectionIntegrals
                {
                    TauSolar = solExt.Tau,
                    RSolar = solExt.R,
                    ASolarByLayer = solExt.Abs,
                    Tv = visExt.Tau,
                    Rv = visExt.R,
                    SolarDenom = 1.0,
                    VisibleDenom = 1.0
                };
                intl = new SpectralDirectionIntegrals
                {
                    TauSolar = solInt.Tau,  // note: τ(int) isn’t needed for g, but used in residual check
                    RSolar = solInt.R,
                    ASolarByLayer = solInt.Abs,
                    Tv = visInt.Tau,
                    Rv = visInt.R,
                    SolarDenom = 1.0,
                    VisibleDenom = 1.0
                };
            }

            // Thermal network per EN 410 §5.4.6 (films + glass conduction + gaps)
            var qi = ComputeQi(layers, gaps, he_Wm2K, hi_Wm2K);

            // g-value (EN 410 §5.4.1 / eq. (7)): g = τe + Σ(q_i * A_i)
            double g = ext.TauSolar;
            double Atot = 0.0;
            for (int k = 0; k < layers.Count; k++)
            {
                g += qi[k] * ext.ASolarByLayer[k];
                Atot += ext.ASolarByLayer[k];
            }

            // Residuals (use integrated balances)
            double residualExt = System.Math.Abs(1.0 - (ext.TauSolar + ext.RSolar + Sum(ext.ASolarByLayer)));
            double residualInt = System.Math.Abs(1.0 - (intl.TauSolar + intl.RSolar + Sum(intl.ASolarByLayer)));

            return new Result
            {
                // Visible (outside)
                Tv = ext.Tv,
                RvExt = ext.Rv,
                RvInt = intl.Rv,

                // Solar
                TauSolar = ext.TauSolar,
                RSolarExt = ext.RSolar,
                RSolarInt = intl.RSolar,
                ASolarByLayer = ext.ASolarByLayer,
                ASolarTotal = Atot,

                // Thermal + g
                QiByLayer = qi,
                GValue = g,
                ShadingCoefficient = g / 0.87,

                // Checks
                SolarBalanceResidualExt = residualExt,
                SolarBalanceResidualInt = residualInt,

                // Meta
                UsedSpectralPath = usedSpectral
            };
        }

        // ---------- Helpers to build total gap conductance ----------

        /// <summary>
        /// Build a Gap from a convective part and the two facing far-IR emissivities.
        /// h_gap = h_conv + h_rad, with  h_rad = 4 σ Tm^3 / (1/ε1 + 1/ε2 - 1).
        /// Default Tm = 10 °C for better alignment with EN 673 reference conditions.
        /// </summary>
        public static Gap MakeGapFromConvective(double hc_Wm2K, double epsFace1, double epsFace2, double TmeanC = 10.0)
        {
            const double sigma = 5.670374419e-8; // W/m^2 K^4
            double T = TmeanC + 273.15;
            epsFace1 = ClampTo(epsFace1, 0.01, 0.98);
            epsFace2 = ClampTo(epsFace2, 0.01, 0.98);
            double denom = (1.0 / epsFace1 + 1.0 / epsFace2 - 1.0);
            double hr = (denom > 0) ? (4.0 * sigma * System.Math.Pow(T, 3) / denom) : 0.0;
            return new Gap(System.Math.Max(1e-6, hc_Wm2K + hr));
        }

        /// <summary>
        /// Convenience: if you know gas conductivity λ and gap d (vertical, Nu≈1),
        /// estimate h_conv ≈ λ/d, then add hr using emissivities.
        /// </summary>
        public static Gap MakeGapFromGas(double lambda_WmK, double gap_m, double epsFace1, double epsFace2, double TmeanC = 10.0)
        {
            if (gap_m <= 0) throw new ArgumentOutOfRangeException(nameof(gap_m));
            double hc = lambda_WmK / gap_m;
            return MakeGapFromConvective(hc, epsFace1, epsFace2, TmeanC);
        }

        // ---------- Internal implementation ----------

        private sealed class Optics
        {
            public double Tau;   // total transmittance for the direction treated
            public double R;     // reflectance back to the incident side
            public double[] Abs; // per-pane absorptance (for that direction)
        }

        private sealed class SpectralDirectionIntegrals
        {
            public double TauSolar, RSolar, Tv, Rv;
            public double[] ASolarByLayer;
            public double SolarDenom, VisibleDenom;
        }

        /// <summary>
        /// Spectral integration for one direction (exteriorIncidence true/false).
        /// </summary>
        private static SpectralDirectionIntegrals IntegrateSpectralDirection(
            IList<Layer> layers,
            SpectralWeights weights,
            bool exteriorIncidence,
            double cutoff,
            int maxBounces)
        {
            int N = layers.Count;
            int nλ = weights.Lambda_nm.Length;

            var Aacc = new double[N];
            double numT_solar = 0, numR_solar = 0, denom_solar = 0;
            double numT_vis = 0, numR_vis = 0, denom_vis = 0;

            // Pre-allocate
            var t = new double[N];
            var rf = new double[N];
            var rb = new double[N];

            for (int k = 0; k < nλ; k++)
            {
                double lam = weights.Lambda_nm[k];
                // Interpolate each layer at lam
                for (int i = 0; i < N; i++)
                {
                    t[i] = InterpLinearClamped(layers[i].Lambda_nm, layers[i].Tau, lam);
                    rf[i] = InterpLinearClamped(layers[i].Lambda_nm, layers[i].Rf, lam);
                    rb[i] = InterpLinearClamped(layers[i].Lambda_nm, layers[i].Rb, lam);

                    // Clamp and enforce t + r ≤ 1 per side to keep non-negative absorptance
                    t[i] = Clamp01(t[i]);
                    rf[i] = Clamp01(rf[i]);
                    rb[i] = Clamp01(rb[i]);
                    if (t[i] + rf[i] > 1.0) rf[i] = System.Math.Max(0.0, 1.0 - t[i]);
                    if (t[i] + rb[i] > 1.0) rb[i] = System.Math.Max(0.0, 1.0 - t[i]);
                }

                var opt = SolveOptics_Monochromatic(t, rf, rb, exteriorIncidence, cutoff, maxBounces);

                double ws = System.Math.Max(0.0, weights.Solar[k]);
                if (ws > 0.0)
                {
                    numT_solar += ws * opt.Tau;
                    numR_solar += ws * opt.R;
                    denom_solar += ws;
                    for (int i = 0; i < N; i++) Aacc[i] += ws * opt.Abs[i];
                }

                double wv = System.Math.Max(0.0, weights.D65V[k]);
                if (wv > 0.0)
                {
                    numT_vis += wv * opt.Tau;
                    numR_vis += wv * opt.R;
                    denom_vis += wv;
                }
            }

            // Normalize
            var A = new double[N];
            if (denom_solar <= 0) throw new InvalidOperationException("Solar weights are zero or invalid.");
            for (int i = 0; i < N; i++) A[i] = Aacc[i] / denom_solar;

            double tauSolar = numT_solar / denom_solar;
            double rSolar = numR_solar / denom_solar;
            double Tv = (denom_vis > 0) ? (numT_vis / denom_vis) : double.NaN;
            double Rv = (denom_vis > 0) ? (numR_vis / denom_vis) : double.NaN;

            return new SpectralDirectionIntegrals
            {
                TauSolar = tauSolar,
                RSolar = rSolar,
                ASolarByLayer = A,
                Tv = Tv,
                Rv = Rv,
                SolarDenom = denom_solar,
                VisibleDenom = denom_vis
            };
        }

        /// <summary>
        /// Band-averaged optics (fallback). Uses the existing Layer band fields.
        /// </summary>
        private static Optics SolveOptics_Band(
            IList<Layer> layers,
            string band,
            bool exteriorIncidence,
            double cutoff,
            int maxBounces)
        {
            int N = layers.Count;

            // Extract band numbers and orient for the incident side
            var t = new double[N];
            var rf = new double[N];
            var rb = new double[N];

            if (exteriorIncidence)
            {
                for (int i = 0; i < N; i++)
                {
                    if (band == "solar")
                    {
                        t[i] = layers[i].TauSolar; rf[i] = layers[i].RfSolar; rb[i] = layers[i].RbSolar;
                    }
                    else
                    {
                        t[i] = layers[i].TauVis; rf[i] = layers[i].RfVis; rb[i] = layers[i].RbVis;
                    }
                    ClampAndEnforce(t, rf, rb, i);
                }
            }
            else
            {
                for (int i = 0; i < N; i++)
                {
                    int j = N - 1 - i;
                    if (band == "solar")
                    {
                        t[i] = layers[j].TauSolar; rf[i] = layers[j].RbSolar; rb[i] = layers[j].RfSolar; // swap reflections
                    }
                    else
                    {
                        t[i] = layers[j].TauVis; rf[i] = layers[j].RbVis; rb[i] = layers[j].RfVis;
                    }
                    ClampAndEnforce(t, rf, rb, i);
                }
            }

            var opt = SolveOptics_Monochromatic(t, rf, rb, exteriorIncidence: true, cutoff: cutoff, maxBounces: maxBounces);

            // Map per-pane absorption back to original order if interior incidence
            if (!exteriorIncidence) Array.Reverse(opt.Abs);
            return opt;
        }

        /// <summary>
        /// Core multi-reflection optics solver at one wavelength, for the oriented stack
        /// (left->right is the propagation, index 0 is the incident-side pane).
        /// </summary>
        private static Optics SolveOptics_Monochromatic(
            double[] t_in, double[] rf_in, double[] rb_in,
            bool exteriorIncidence,
            double cutoff,
            int maxBounces)
        {
            // Copy (we'll orient here)
            int N = t_in.Length;
            var t = new double[N];
            var rf = new double[N];
            var rb = new double[N];

            if (exteriorIncidence)
            {
                Array.Copy(t_in, t, N);
                Array.Copy(rf_in, rf, N);
                Array.Copy(rb_in, rb, N);
            }
            else
            {
                for (int i = 0; i < N; i++)
                {
                    int j = N - 1 - i;
                    t[i] = t_in[j];
                    rf[i] = rb_in[j]; // swap reflections
                    rb[i] = rf_in[j];
                }
            }

            var Abs = new double[N];
            double T = 0.0, R = 0.0;
            var q = new Queue<(int idx, bool fwd, double I)>();
            q.Enqueue((0, true, 1.0)); // unit irradiance in

            int iter = 0;
            while (q.Count > 0)
            {
                var (i, fwd, I) = q.Dequeue();
                if (I < cutoff) continue;
                if (++iter > maxBounces)
                    throw new InvalidOperationException("Optics did not converge (maxBounces reached).");

                if (fwd)
                {
                    if (i >= N) { T += I; continue; }
                    double absF = System.Math.Max(0.0, 1.0 - t[i] - rf[i]);
                    Abs[i] += absF * I;

                    double back = rf[i] * I;                  // reflect to previous
                    if (i == 0) R += back; else q.Enqueue((i - 1, false, back));

                    double fwdT = t[i] * I;                   // transmit forward
                    q.Enqueue((i + 1, true, fwdT));
                }
                else
                {
                    if (i < 0) { R += I; continue; }          // escaped to incident side
                    double absB = System.Math.Max(0.0, 1.0 - t[i] - rb[i]);
                    Abs[i] += absB * I;

                    double fwdR = rb[i] * I;                  // reflect forward
                    q.Enqueue((i + 1, true, fwdR));

                    double backT = t[i] * I;                  // transmit backward
                    if (i == 0) R += backT; else q.Enqueue((i - 1, false, backT));
                }
            }

            // Map back to original pane order if direction was interior incidence
            if (!exteriorIncidence) Array.Reverse(Abs);
            return new Optics { Tau = T, R = R, Abs = Abs };
        }

        // q_i from mid-plane resistance split to outside vs. inside (equivalent to EN 410 §5.4.6)
        private static double[] ComputeQi(IList<Layer> layers, IList<Gap> gaps, double he, double hi)
        {
            int N = layers.Count;
            var Rg = new double[N]; // pane resistances
            for (int i = 0; i < N; i++)
            {
                double k = System.Math.Max(layers[i].Conductivity_WmK, 1e-9);
                double th = System.Math.Max(layers[i].Thickness_m, 1e-9);
                Rg[i] = th / k;
            }

            // Bridge resistance between pane centers: B_j = Rg_j/2 + R_gap_j + Rg_{j+1}/2
            var B = new double[System.Math.Max(0, N - 1)];
            for (int j = 0; j < B.Length; j++)
                B[j] = Rg[j] * 0.5 + gaps[j].R + Rg[j + 1] * 0.5;

            // Prefix sums
            var pref = new double[N]; // pref[k] = sum_{m=0..k-1} B_m (pref[0]=0)
            for (int k = 1; k < N; k++) pref[k] = pref[k - 1] + B[k - 1];

            double Rse = 1.0 / System.Math.Max(1e-9, he);
            double Rsi = 1.0 / System.Math.Max(1e-9, hi);

            // From pane center k to OUTSIDE: R_out = Rse + Rg[0]/2 + sum_{m=0..k-1} B_m
            // From pane center k to INSIDE : R_in  = Rsi + Rg[N-1]/2 + sum_{m=k..N-2} B_m
            double tailAll = pref[N - 1]; // sum of all bridges
            var qi = new double[N];
            for (int k = 0; k < N; k++)
            {
                double Rout = Rse + Rg[0] * 0.5 + pref[k];
                double Rin = Rsi + Rg[N - 1] * 0.5 + (tailAll - pref[k]);
                double Gin = 1.0 / Rin, Gout = 1.0 / Rout;
                qi[k] = Gin / (Gin + Gout);
            }
            return qi;
        }

        // ---------- Utilities ----------

        private static void ClampAndEnforce(double[] t, double[] rf, double[] rb, int i)
        {
            t[i] = Clamp01(t[i]);
            rf[i] = Clamp01(rf[i]);
            rb[i] = Clamp01(rb[i]);

            if (t[i] + rf[i] > 1.0) rf[i] = System.Math.Max(0.0, 1.0 - t[i]);
            if (t[i] + rb[i] > 1.0) rb[i] = System.Math.Max(0.0, 1.0 - t[i]);
        }

        private static double Clamp01(double x) => (x < 0.0) ? 0.0 : (x > 1.0 ? 1.0 : x);
        private static double ClampTo(double x, double lo, double hi) => (x < lo) ? lo : (x > hi ? hi : x);
        private static double Sum(double[] a) { double s = 0; for (int i = 0; i < a.Length; i++) s += a[i]; return s; }

        /// <summary>
        /// Linear interpolation with clamping at ends.
        /// Assumes x is strictly increasing (validated at Layer construction).
        /// </summary>
        private static double InterpLinearClamped(double[] x, double[] y, double xi)
        {
            if (xi <= x[0]) return y[0];
            int n = x.Length;
            if (xi >= x[n - 1]) return y[n - 1];

            // Binary search for interval
            int lo = 0, hi = n - 1;
            while (hi - lo > 1)
            {
                int mid = (lo + hi) >> 1;
                if (x[mid] <= xi) lo = mid; else hi = mid;
            }
            double t = (xi - x[lo]) / (x[hi] - x[lo]);
            return y[lo] + t * (y[hi] - y[lo]);
        }
    }
}
