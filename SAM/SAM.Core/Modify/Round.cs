using System;


namespace SAM.Core
{
    public static partial class Modify
    {
        public static double Round(this double value, double tolerance = Tolerance.MicroDistance)
        {
            if (double.IsNaN(value))
                return double.NaN;

            return Math.Round(value / tolerance) * tolerance;
        }
    }
}
