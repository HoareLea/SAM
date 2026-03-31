// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace SAM.Core
{
    public static class ColorGradientInterpolationGenerator
    {
        /// <summary>
        /// Linearly interpolates between two colors in sRGB space.
        /// t in [0..1]
        /// </summary>
        public static Color Lerp(Color a, Color b, double t)
        {
            t = Query.Clamp(t, 0, 1);
            int r = (int)Math.Round(a.R + (b.R - a.R) * t);
            int g = (int)Math.Round(a.G + (b.G - a.G) * t);
            int bl = (int)Math.Round(a.B + (b.B - a.B) * t);
            int al = (int)Math.Round(a.A + (b.A - a.A) * t);
            return Color.FromArgb(al, Query.Clamp(r, 0, 255), Query.Clamp(g, 0, 255), Query.Clamp(bl, 0, 255));
        }

        /// <summary>
        /// Interpolates across multiple color stops.
        /// Provide stops as (position, color) where position in [0..1].
        /// </summary>
        public static Color SampleStops(IReadOnlyList<(double pos, Color color)> stops, double t)
        {
            if (stops == null || stops.Count == 0)
                throw new ArgumentException("Stops must contain at least one entry.");

            t = Query.Clamp(t, 0, 1);

            // Ensure sorted by position
            var sorted = stops.OrderBy(s => s.pos).ToList();

            if (t <= sorted[0].pos) return sorted[0].color;
            if (t >= sorted[sorted.Count - 1].pos) return sorted[sorted.Count - 1].color;

            for (int i = 0; i < sorted.Count - 1; i++)
            {
                var (p0, c0) = sorted[i];
                var (p1, c1) = sorted[i + 1];
                if (t >= p0 && t <= p1)
                {
                    double u = (t - p0) / (p1 - p0);
                    return Lerp(c0, c1, u);
                }
            }
            return sorted[sorted.Count - 1].color;
        }

        /// <summary>
        /// Generates N colors from stops (discrete palette sampled evenly).
        /// If N=1 returns the mid color (t=0.5).
        /// </summary>
        public static List<Color> GenerateDiscreteFromStops(IReadOnlyList<(double pos, Color color)> stops, int n,bool includeEndpoints = true)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));

            if (n == 1)
                return new List<Color> { SampleStops(stops, 0.5) };

            var colors = new List<Color>(n);

            // t positions
            for (int i = 0; i < n; i++)
            {
                double t = includeEndpoints
                    ? (double)i / (n - 1)
                    : (i + 1.0) / (n + 1.0);

                colors.Add(SampleStops(stops, t));
            }

            return colors;
        }

        /// <summary>
        /// Optional: perceptually nicer interpolation using gamma correction in sRGB.
        /// gamma ~ 2.2. For UI gradients this often looks smoother.
        /// </summary>
        public static Color LerpGamma(Color a, Color b, double t, double gamma = 2.2)
        {
            t = Query.Clamp(t, 0, 1);

            double ar = SrgbToLinear(a.R / 255.0, gamma);
            double ag = SrgbToLinear(a.G / 255.0, gamma);
            double ab = SrgbToLinear(a.B / 255.0, gamma);

            double br = SrgbToLinear(b.R / 255.0, gamma);
            double bg = SrgbToLinear(b.G / 255.0, gamma);
            double bb = SrgbToLinear(b.B / 255.0, gamma);

            double rr = LinearToSrgb(ar + (br - ar) * t, gamma);
            double rg = LinearToSrgb(ag + (bg - ag) * t, gamma);
            double rb = LinearToSrgb(ab + (bb - ab) * t, gamma);

            int r = Query.Clamp((int)Math.Round(rr * 255), 0, 255);
            int g = Query.Clamp((int)Math.Round(rg * 255), 0, 255);
            int bl = Query.Clamp((int)Math.Round(rb * 255), 0, 255);
            int al = (int)Math.Round(a.A + (b.A - a.A) * t);

            return Color.FromArgb(Query.Clamp(al, 0, 255), r, g, bl);
        }

        /// <summary>
        /// Diverging gradient helper: low -> mid -> high.
        /// Useful for temperature deviation etc.
        /// </summary>
        public static List<Color> GenerateDiverging(Color low, Color mid, Color high, int n, double midpoint = 0.5)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
            midpoint = Query.Clamp(midpoint, 0, 1);

            var stops = new List<(double, Color)>
        {
            (0.0, low),
            (midpoint, mid),
            (1.0, high)
        };

            return GenerateDiscreteFromStops(stops, n, includeEndpoints: true);
        }

        // --- helpers ---
        //private static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
        
        //private static int Clamp255(int x) => x < 0 ? 0 : (x > 255 ? 255 : x);

        private static double SrgbToLinear(double c, double gamma)
            => Math.Pow(c, gamma);

        private static double LinearToSrgb(double c, double gamma)
            => Math.Pow(Math.Max(0, c), 1.0 / gamma);

        public static Color FromHex(string hex)
        {
            // supports "#RRGGBB" or "RRGGBB"
            hex = hex.Trim().TrimStart('#');
            if (hex.Length != 6) throw new FormatException("Hex must be 6 digits.");
            int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return Color.FromArgb(255, r, g, b);
        }
    }

}
