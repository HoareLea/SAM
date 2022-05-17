namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Limits given value to min and max
        /// </summary>
        /// <param name="value">Value to be clamped</param>
        /// <param name="min">Minimal Value</param>
        /// <param name="max">Maximal Value</param>
        /// <returns>Clamp value</returns>
        public static T Clamp<T>(this T value, T min, T max)
        {
            if ((dynamic)value < (dynamic)min)
            {
                return min;
            }

            if ((dynamic)value > (dynamic)max)
            {
                return max;
            }

            return value;
        }

        public static Range<T> Clamp<T>(this Range<T> range, T min, T max)
        {
            if(range == null)
            {
                return null;
            }

            return new Range<T>(Clamp(range.Min, min, max), Clamp(range.Max, min, max));
        }
    }
}