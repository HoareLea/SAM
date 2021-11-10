namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Limits given value to top and bottom
        /// </summary>
        /// <param name="value">Value to be clamped</param>
        /// <param name="bottom">Bottom Value</param>
        /// <param name="top">Top Value</param>
        /// <returns></returns>
        public static double Clamp(this double value, double bottom, double top)
        {
            if (value < bottom)
            {
                return bottom;
            }

            if (value > top)
            {
                return top;
            }

            return value;
        }
    }
}