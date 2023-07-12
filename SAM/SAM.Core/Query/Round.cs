using System;

namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Rounds given value to provided tolerance
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Rounded value</returns>
        public static double Round(this double value, double tolerance = Core.Tolerance.Distance)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return value;
            }

            if (tolerance.Equals(0.0))
            {
                return value;
            }

            return (double)(Math.Round((decimal)value / (decimal)tolerance) * (decimal)tolerance);
        }

        /// <summary>
        /// Rounds given value to provided tolerance
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Rounded value</returns>
        public static float Round(this float value, float tolerance = (float)Core.Tolerance.Distance)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return float.NaN;
            }

            if (tolerance.Equals(0.0))
            {
                return value;
            }

            return (float)(Math.Round((decimal)value / (decimal)tolerance) * (decimal)tolerance);
        }
    }
}