using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double Round(this double value, double tolerance = Core.Tolerance.Distance)
        {
            if (double.IsNaN(value))
                return double.NaN;

            if (tolerance.Equals(0.0))
                return value;

            return (double)(Math.Round((decimal)value / (decimal)tolerance) * (decimal)tolerance);
        }

        public static float Round(this float value, float tolerance = (float)Core.Tolerance.Distance)
        {
            if (double.IsNaN(value))
                return float.NaN;

            if (tolerance.Equals(0.0))
                return value;

            return (float)(Math.Round((decimal)value / (decimal)tolerance) * (decimal)tolerance);
        }
    }
}