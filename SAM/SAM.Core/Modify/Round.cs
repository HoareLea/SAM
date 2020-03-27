using System;


namespace SAM.Core
{
    public static partial class Modify
    {
        public static double Round(this double value, double tolerance = Tolerance.MicroDistance)
        {
            if (double.IsNaN(value))
                return double.NaN;

            if (tolerance.Equals(0.0))
                return value;

            return Math.Round(value / tolerance) * tolerance;
        }
    }
}
