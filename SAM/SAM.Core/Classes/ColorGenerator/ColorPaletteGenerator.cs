using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SAM.Core
{
    public static class ColorPaletteGenerator
    {
        /// <summary>
        /// Returns N colors from a categorical base palette. If N > base size, generates additional colours.
        /// </summary>
        public static List<Color> GetCategorical(int n, Color[] basePalette, bool extendIfNeeded = true)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
            if (basePalette == null || basePalette.Length == 0) throw new ArgumentException("Base palette empty.");

            var result = new List<Color>(n);

            if (!extendIfNeeded)
            {
                for (int i = 0; i < n; i++)
                    result.Add(basePalette[i % basePalette.Length]);
                return result;
            }

            // Start with base palette unique colours
            for (int i = 0; i < Math.Min(n, basePalette.Length); i++)
                result.Add(basePalette[i]);

            // Extend beyond base using evenly spaced HSV hues while keeping decent saturation/value.
            if (n > basePalette.Length)
            {
                int extra = n - basePalette.Length;
                var generated = GenerateDistinctHsv(extra, saturation: 0.62, value: 0.92, hueOffset: 0.11);
                result.AddRange(generated);
            }

            return result.Take(n).ToList();
        }

        /// <summary>
        /// Returns N colors for a continuous variable (e.g. Temperature) using gradient stops.
        /// </summary>
        public static List<Color> GetSequentialFromStops(int n, IReadOnlyList<(double pos, Color color)> stops)
            => ColorGradientInterpolationGenerator.GenerateDiscreteFromStops(stops, n, includeEndpoints: true);

        /// <summary>
        /// Assigns colors sequentially to items in the given order.
        /// </summary>
        public static Dictionary<string, Color> AssignSequential(
            IReadOnlyList<string> itemNames,
            IReadOnlyList<Color> colors)
        {
            if (itemNames == null) throw new ArgumentNullException(nameof(itemNames));
            if (colors == null || colors.Count == 0) throw new ArgumentException("Colors must not be empty.");

            var map = new Dictionary<string, Color>();
            for (int i = 0; i < itemNames.Count; i++)
                map[itemNames[i]] = colors[i % colors.Count];

            return map;
        }

        /// <summary>
        /// Stable deterministic mapping: each name hashes to a color index.
        /// This is best when item counts change per project and you want the same name to keep its colour.
        /// </summary>
        public static Dictionary<string, Color> AssignStableByName(
            IReadOnlyList<string> itemNames,
            IReadOnlyList<Color> palette,
            string salt = "SAM_UI")
        {
            if (itemNames == null) throw new ArgumentNullException(nameof(itemNames));
            if (palette == null || palette.Count == 0) throw new ArgumentException("Palette must not be empty.");

            var map = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in itemNames)
            {
                int idx = StableIndex(name ?? "", palette.Count, salt);
                map[name] = palette[idx];
            }

            return map;
        }

        /// <summary>
        /// Stable mapping but tries to avoid collisions by expanding palette on demand.
        /// Useful when you have many items and want better separation.
        /// </summary>
        public static Dictionary<string, Color> AssignStableDistinct(
            IReadOnlyList<string> itemNames,
            int minPaletteSize = 12,
            double saturation = 0.62,
            double value = 0.92,
            string salt = "SAM_UI")
        {
            if (itemNames == null) throw new ArgumentNullException(nameof(itemNames));
            int n = Math.Max(minPaletteSize, itemNames.Count);

            // generate palette equal to item count so collisions are minimal
            var palette = GenerateDistinctHsv(n, saturation, value, hueOffset: 0.11);

            return AssignStableByName(itemNames, palette, salt);
        }

        // ---- Generators ----

        /// <summary>
        /// Generates N visually distinct colors using HSV hue spacing.
        /// hueOffset allows changing starting hue to avoid too many similar colours when N is small.
        /// </summary>
        public static List<Color> GenerateDistinctHsv(int n, double saturation, double value, double hueOffset = 0.0)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));

            saturation = Query.Clamp(saturation, 0, 1);
            value = Query.Clamp(value, 0, 1);

            var colors = new List<Color>(n);
            // golden ratio conjugate gives good distribution for arbitrary n
            const double phi = 0.618033988749895;

            double h = hueOffset % 1.0;
            for (int i = 0; i < n; i++)
            {
                h = (h + phi) % 1.0;
                colors.Add(FromHsv(h * 360.0, saturation, value));
            }

            return colors;
        }

        /// <summary>
        /// HSV to RGB conversion.
        /// H in degrees [0..360), S,V in [0..1]
        /// </summary>
        public static Color FromHsv(double hDeg, double s, double v)
        {
            s = Query.Clamp(s, 0, 1);
            v = Query.Clamp(v, 0, 1);
            hDeg = (hDeg % 360 + 360) % 360;

            double c = v * s;
            double x = c * (1 - Math.Abs((hDeg / 60.0 % 2) - 1));
            double m = v - c;

            double r1, g1, b1;
            if (hDeg < 60) (r1, g1, b1) = (c, x, 0);
            else if (hDeg < 120) (r1, g1, b1) = (x, c, 0);
            else if (hDeg < 180) (r1, g1, b1) = (0, c, x);
            else if (hDeg < 240) (r1, g1, b1) = (0, x, c);
            else if (hDeg < 300) (r1, g1, b1) = (x, 0, c);
            else (r1, g1, b1) = (c, 0, x);

            int r = (int)Math.Round((r1 + m) * 255);
            int g = (int)Math.Round((g1 + m) * 255);
            int b = (int)Math.Round((b1 + m) * 255);

            return Color.FromArgb(255, Query.Clamp(r, 0, 255), Query.Clamp(g, 0, 255), Query.Clamp(b, 0, 255));
        }

        public static List<Color> GetColors<T>(PaletteDefinition paletteDefinition, IEnumerable<T> values)
        {
            if(paletteDefinition == null || values == null)
            {
                return null;
            }

            List<Color> result = [];

            int count = values.Count();

            if(count == 0)
            {
                return result;
            }

            List<double> values_Number = values.Where(x => Query.IsNumeric(x)).ToList().ConvertAll(x => Query.ParseDouble(x.ToString()));
            bool hasNumeric = values_Number.Count != 0;


            double min = hasNumeric ? values_Number.Min() : double.NaN;
            double max = hasNumeric ? values_Number.Max() : double.NaN;

            switch (paletteDefinition.PaletteType)
            {
                case PaletteType.Categorical:

                    var names = values.Select(v => v?.ToString() ?? "").ToList();

                    if (paletteDefinition.StableByName)
                    {
                        var map = AssignStableUnique(
                            names,
                            paletteDefinition.Colors.ToList(),
                            paletteDefinition.Name);

                        foreach (var name in names)
                            result.Add(map[name]);
                    }
                    else
                    {
                        var palette = EnsureCategoricalPalette(
                            paletteDefinition.Colors.ToList(),
                            names.Count);

                        for (int i = 0; i < names.Count; i++)
                            result.Add(palette[i]);
                    }

                    return result;

                case PaletteType.Sequential:

                    if (hasNumeric)
                    {
                        double span = Math.Max(Tolerance.MicroDistance, max - min);

                        foreach (T value in values)
                        {
                            double t = Query.IsNumeric(value) ? (Query.ParseDouble(value.ToString()) - min) / span : 0.5;
                            result.Add(SampleStops(paletteDefinition.Colors, t));
                        }
                    }
                    else
                    {
                        result = GenerateEvenFromStops(paletteDefinition.Colors, count);
                    }
                    break;

                case PaletteType.Diverging:

                    // Expect palette.Colors to be 3 stops: low, mid, high (or more)
                    // If items have Value: map around midpoint (by value or by palette.Midpoint)
                    // If no Value: just distribute evenly over stops

                    if (!hasNumeric)
                    {
                        result = GenerateEvenFromStops(paletteDefinition.Colors, count);

                        return result;
                    }

                    double midValue = min + (max - min) * paletteDefinition.Midpoint;
                    double spanLow = Math.Max(Tolerance.MicroDistance, midValue - min);
                    double spanHigh = Math.Max(Tolerance.MicroDistance, max - midValue);

                    foreach (T value in values)
                    {
                        if (value == null || !Query.IsNumeric(value))
                        {
                            result.Add(SampleStops(paletteDefinition.Colors, 0.5));
                            continue;
                        }

                        double v = Query.ParseDouble(value.ToString());
                        double t;
                        if (v <= midValue)
                        {
                            // map min..mid => 0..0.5
                            t = 0.5 * ((v - min) / spanLow);
                        }
                        else
                        {
                            // map mid..max => 0.5..1
                            t = 0.5 + 0.5 * ((v - midValue) / spanHigh);
                        }

                        result.Add(SampleStops(paletteDefinition.Colors, t));
                    }
                    break;
            }


            return result;
        }

        public static Dictionary<string, Color> AssignStableUnique(
    IReadOnlyList<string> itemNames,
    IReadOnlyList<Color> basePalette,
    string salt = "SAM_UI")
        {
            if (itemNames == null) throw new ArgumentNullException(nameof(itemNames));
            if (basePalette == null || basePalette.Count == 0) throw new ArgumentException("Palette must not be empty.");

            var uniqueNames = itemNames
                .Select(v => v?.ToString() ?? "")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var palette = EnsureCategoricalPalette(basePalette.ToList(), uniqueNames.Count);

            // Stable order by hash, then assign unique colours sequentially
            var ordered = uniqueNames
                .Select(name => new
                {
                    Name = name,
                    Hash = StableUInt(name, salt)
                })
                .OrderBy(x => x.Hash)
                .ToList();

            var map = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < ordered.Count; i++)
                map[ordered[i].Name] = palette[i];

            return map;
        }

        private static List<Color> EnsureCategoricalPalette(IReadOnlyList<Color> basePalette, int requiredCount)
        {
            var result = basePalette.ToList();

            if (requiredCount <= result.Count)
                return result;

            int extra = requiredCount - result.Count;

            // Generate extras with stronger saturation and slightly lower value
            var generated = GenerateDistinctHsv(extra * 3, saturation: 0.72, value: 0.88, hueOffset: 0.15);

            foreach (var c in generated)
            {
                if (result.Count >= requiredCount)
                    break;

                if (result.All(existing => ColorDistance(existing, c) > 80))
                    result.Add(c);
            }

            // fallback if filtering was too strict
            int k = 0;
            while (result.Count < requiredCount)
            {
                double h = (k * 137.50776405) % 360.0;
                var c = FromHsv(h, 0.75, 0.85);
                result.Add(c);
                k++;
            }

            return result;
        }

        private static uint StableUInt(string name, string salt)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{salt}:{name}"));
            return BitConverter.ToUInt32(bytes, 0);
        }

        private static double ColorDistance(Color a, Color b)
        {
            int dr = a.R - b.R;
            int dg = a.G - b.G;
            int db = a.B - b.B;
            return Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        private static List<Color> GenerateEvenFromStops(IReadOnlyList<Color> stops, int n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
            if (n == 1) return new List<Color> { SampleStops(stops, 0.5) };

            var list = new List<Color>(n);
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / (n - 1);
                list.Add(SampleStops(stops, t));
            }
            return list;
        }

        private static Color SampleStops(IReadOnlyList<Color> stops, double t)
        {
            t = Query.Clamp(t, 0, 1);
            if (stops.Count == 1) return stops[0];

            // Treat list as evenly spaced stops
            double scaled = t * (stops.Count - 1);
            int i0 = (int)Math.Floor(scaled);
            int i1 = Math.Min(i0 + 1, stops.Count - 1);
            double u = scaled - i0;

            return Lerp(stops[i0], stops[i1], u);
        }

        private static Color Lerp(Color a, Color b, double t)
        {
            t = Query.Clamp(t, 0, 1);
            byte A = (byte)Math.Round(a.A + (b.A - a.A) * t);
            byte R = (byte)Math.Round(a.R + (b.R - a.R) * t);
            byte G = (byte)Math.Round(a.G + (b.G - a.G) * t);
            byte B = (byte)Math.Round(a.B + (b.B - a.B) * t);
            return Color.FromArgb(A, R, G, B);
        }

        // ---- Utilities ----

        private static int StableIndex(string name, int modulo, string salt)
        {
            // SHA256(name + salt) -> int -> modulo
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{salt}:{name}"));

            // use first 4 bytes as uint
            uint value = BitConverter.ToUInt32(bytes, 0);
            return (int)(value % (uint)modulo);
        }

        private static Color Hex(string hex)
        {
            hex = hex.Trim().TrimStart('#');
            int r = System.Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = System.Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = System.Convert.ToInt32(hex.Substring(4, 2), 16);
            return Color.FromArgb(255, r, g, b);
        }
    }
}
