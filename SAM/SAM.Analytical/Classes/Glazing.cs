using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// EN 410:2011 normal-incidence glazing calculator (luminous + solar bands) with g-value.
    /// - Optics: multi-reflection per EN410 §5.2–§5.3 (eqs. (2), (5); generalized to N panes).
    /// - Solar factor: g = tau_e + sum(q_i * A_i) per §5.4.1/eq. (7).
    /// - A_i per-pane absorptance from the same multi-reflection optics (consistent with §5.4.5, eqs. (14–18), (20–24)).
    /// - q_i from EN410 §5.4.6 thermal network (Figs. 4–5) using films he=23, hi=8 (§5.4.6.1).
    ///   The inter-pane conductance Λ is built from the gap’s convective part (your “Conv. Coeff.”)
    ///   plus a radiative part hr(ε1, ε2, Tm) (consistent with EN410’s reference to EN673 in §5.4.6.3).
    /// Assumptions:
    ///   • Normal incidence only (EN410 scope).
    ///   • Vertical glazing, unventilated cavities; he=23, hi=8 W/m²K by default (§5.4.6.1).
    ///   • Band-integrated inputs (solar/visible) are supplied per pane (as in your data).
    /// </summary>
    public class Glazing
    {
        // ---------- User-facing inputs ----------
        public class Layer
        {
            // Solar band (EN410)
            public double TauSolar, RfSolar, RbSolar;
            // Visible band (for Tv/Rv)
            public double TauVis, RfVis, RbVis;
            // For qi split (EN673-style thermal path)
            public double Thickness_m;      // e.g. 0.006
            public double Conductivity_WmK; // e.g. 1.0 for float glass

            public Layer(double tauSolar, double rfSolar, double rbSolar,
                         double tauVis, double rfVis, double rbVis,
                         double thickness_m, double conductivity_WmK)
            {
                TauSolar = tauSolar; RfSolar = rfSolar; RbSolar = rbSolar;
                TauVis = tauVis; RfVis = rfVis; RbVis = rbVis;
                Thickness_m = thickness_m; Conductivity_WmK = conductivity_WmK;
            }
        }

        public class Gap
        {
            /// <summary>Total cavity conductance h_gap = h_conv + h_rad  [W/m²K]</summary>
            public double Conductance_Wm2K;
            public Gap(double conductance_Wm2K) { Conductance_Wm2K = conductance_Wm2K; }
            public double R => 1.0 / Conductance_Wm2K; // m²K/W
        }

        // ---------- Outputs ----------
        public class Result
        {
            // Visible (outside incidence)
            public double Tv, RvExt, RvInt;

            // Solar (outside incidence)
            public double TauSolar, RSolarExt, RSolarInt;
            public double[] ASolarByLayer;   // per-pane absorptance
            public double ASolarTotal;

            // Thermal split and g
            public double[] QiByLayer;       // fraction of each pane’s absorption flowing to room
            public double GValue;            // total solar energy transmittance (solar factor)
            public double ShadingCoefficient;

            // Numerical energy checks (should be ~0)
            public double SolarBalanceResidualExt, SolarBalanceResidualInt;
        }

        // ---------- Public API ----------
        /// <summary>
        /// Compute EN410 outputs for an arbitrary stack (single/double/triple/...).
        /// Required inputs: per-pane solar/visible (tau, Rf, Rb), pane thickness & conductivity,
        /// and per-gap total conductance (often built from "Conv. Coeff." + hr(ε1,ε2, Tm), see helpers).
        /// Defaults (EN410 §5.4.6.1): he=23, hi=8 W/m²K (vertical glazing, unventilated).
        /// </summary>
        public static Result Compute(
            IList<Layer> layers,
            IList<Gap> gaps,
            double he_Wm2K = 23.0, double hi_Wm2K = 8.0,
            double cutoff = 1e-12, int maxBounces = 40000)
        {
            if (layers == null || layers.Count == 0) throw new ArgumentException("At least one layer is required.");
            if (gaps == null) gaps = Array.Empty<Gap>();
            if (gaps.Count != System.Math.Max(0, layers.Count - 1))
                throw new ArgumentException("gaps.Count must equal layers.Count - 1.");

            // 1) Optics (solar + visible), outside & inside incidence
            var solExt = SolveOptics(layers, band: "solar", exteriorIncidence: true, cutoff, maxBounces);
            var solInt = SolveOptics(layers, band: "solar", exteriorIncidence: false, cutoff, maxBounces);
            var visExt = SolveOptics(layers, band: "vis", exteriorIncidence: true, cutoff, maxBounces);
            var visInt = SolveOptics(layers, band: "vis", exteriorIncidence: false, cutoff, maxBounces);

            // 2) Thermal network per EN410 §5.4.6 (Figs. 4–5): films + glass conduction + gaps
            var qi = ComputeQi(layers, gaps, he_Wm2K, hi_Wm2K);

            // 3) g-value (EN410 §5.4.1, eq. (7))
            double g = solExt.Tau;
            double Atot = 0.0;
            for (int k = 0; k < layers.Count; k++) 
            { 
                g += qi[k] * solExt.Abs[k]; 
                Atot += solExt.Abs[k]; }

            return new Result
            {
                // Visible (outside)
                Tv = visExt.Tau,
                RvExt = visExt.R,
                RvInt = visInt.R,

                // Solar (outside)
                TauSolar = solExt.Tau,
                RSolarExt = solExt.R,
                RSolarInt = solInt.R,
                ASolarByLayer = solExt.Abs,
                ASolarTotal = Atot,

                // Thermal + g
                QiByLayer = qi,
                GValue = g,
                ShadingCoefficient = g / 0.87, // EN410 §5.7/eq. (48)

                // Checks
                SolarBalanceResidualExt = System.Math.Abs(1.0 - (solExt.Tau + solExt.R + Sum(solExt.Abs))),
                SolarBalanceResidualInt = System.Math.Abs(1.0 - (solInt.Tau + solInt.R + Sum(solInt.Abs)))
            };
        }

        // ---------- Helpers: build total gap conductance from your data ----------
        /// <summary>
        /// Build a Gap from your software’s “Conv. Coeff.” (gas part) and the two facing emissivities.
        /// h_gap = h_conv + h_rad, with  h_rad = 4 σ Tm^3 / (1/ε1 + 1/ε2 - 1)
        /// (Tm is a mean cavity temperature; 293 K ~ 20 °C is a common choice).
        /// EN410 §5.4.6.3 requires the glazing conductance per EN673; this linearized hr is consistent with that practice.
        /// </summary>
        public static Gap MakeGapFromConvective(double hc_Wm2K, double epsFace1, double epsFace2, double TmeanC = 20.0)
        {
            const double sigma = 5.670374419e-8; // W/m²K⁴
            double T = TmeanC + 273.15;
            double hr = 4.0 * sigma * System.Math.Pow(T, 3) / (1.0 / epsFace1 + 1.0 / epsFace2 - 1.0);
            return new Gap(hc_Wm2K + hr);
        }

        /// <summary>
        /// Convenience: if you only know gas λ and gap d (vertical, Nu≈1), estimate h_conv ≈ λ/d, then add hr.
        /// </summary>
        public static Gap MakeGapFromGas(double lambda_WmK, double gap_m, double epsFace1, double epsFace2, double TmeanC = 20.0)
        {
            double hc = lambda_WmK / gap_m;
            return MakeGapFromConvective(hc, epsFace1, epsFace2, TmeanC);
        }

        // ---------- Internal implementation ----------
        private class Optics
        {
            public double Tau;  // total transmittance (outside -> room if exterior incidence)
            public double R;    // reflectance (back to incident side)
            public double[] Abs; // per-pane absorptance (for the actual direction treated)
        }

        private static Optics SolveOptics(IList<Layer> layers, string band, bool exteriorIncidence, double cutoff, int maxBounces)
        {
            int N = layers.Count;
            // Choose the band properties
            (double tau, double Rf, double Rb) Get(int i)
            {
                var L = layers[i];
                return band == "solar"
                    ? (L.TauSolar, L.RfSolar, L.RbSolar)
                    : (L.TauVis, L.RfVis, L.RbVis);
            }

            // Orient stack so beam always propagates left->right from the incident side
            var t = new double[N];
            var rf = new double[N];
            var rb = new double[N];
            if (exteriorIncidence)
            {
                for (int i = 0; i < N; i++) { var p = Get(i); t[i] = p.tau; rf[i] = p.Rf; rb[i] = p.Rb; }
            }
            else
            {
                for (int i = 0; i < N; i++)
                {
                    var p = Get(N - 1 - i); t[i] = p.tau; rf[i] = p.Rb; rb[i] = p.Rf; // swap reflections
                }
            }

            var Abs = new double[N];
            double T = 0.0, R = 0.0;
            var q = new Queue<(int idx, bool fwd, double I)>();
            q.Enqueue((0, true, 1.0)); // unit irradiance

            int iter = 0;
            while (q.Count > 0)
            {
                var (i, fwd, I) = q.Dequeue();
                if (I < cutoff) continue;
                if (++iter > maxBounces) throw new InvalidOperationException("Optics did not converge.");

                if (fwd)
                {
                    if (i >= N) { T += I; continue; }
                    double absF = System.Math.Max(0.0, 1.0 - t[i] - rf[i]);
                    Abs[i] += absF * I;

                    double back = rf[i] * I;             // reflect to previous
                    if (i == 0) R += back; else q.Enqueue((i - 1, false, back));

                    double fwdT = t[i] * I;              // transmit forward
                    q.Enqueue((i + 1, true, fwdT));
                }
                else // backward beam
                {
                    if (i < 0) { R += I; continue; }     // escaped to incident side
                    double absB = System.Math.Max(0.0, 1.0 - t[i] - rb[i]);
                    Abs[i] += absB * I;

                    double fwdR = rb[i] * I;             // reflect forward
                    q.Enqueue((i + 1, true, fwdR));

                    double backT = t[i] * I;             // transmit backward
                    if (i == 0) R += backT; else q.Enqueue((i - 1, false, backT));
                }
            }

            if (!exteriorIncidence) Array.Reverse(Abs); // map back to original pane order
            return new Optics { Tau = T, R = R, Abs = Abs };
        }

        // q_i from mid-plane resistance split to outside vs. inside (equivalent to EN410 §5.4.6 networks)
        private static double[] ComputeQi(IList<Layer> layers, IList<Gap> gaps, double he, double hi)
        {
            int N = layers.Count;
            var Rg = new double[N];
            for (int i = 0; i < N; i++)
            {
                double k = System.Math.Max(layers[i].Conductivity_WmK, 1e-9);
                Rg[i] = layers[i].Thickness_m / k;
            }

            // Bridge resistance between pane centers: B_j = Rg_j/2 + R_gap_j + Rg_{j+1}/2
            var B = new double[System.Math.Max(0, N - 1)];
            for (int j = 0; j < B.Length; j++)
                B[j] = Rg[j] * 0.5 + gaps[j].R + Rg[j + 1] * 0.5;

            // Precompute prefix sums of bridges
            var pref = new double[N]; // pref[k] = sum_{m=0..k-1} B_m  (pref[0]=0)
            for (int k = 1; k < N; k++) pref[k] = pref[k - 1] + B[k - 1];

            double Rse = 1.0 / he, Rsi = 1.0 / hi;

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

        private static double Sum(double[] a) { double s = 0; for (int i = 0; i < a.Length; i++) s += a[i]; return s; }
    }
}

