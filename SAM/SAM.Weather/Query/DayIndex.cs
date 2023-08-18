using System;

namespace SAM.Weather
{
    /// <summary>
    /// This class provides methods for creating and executing queries.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Gets day index.
        /// </summary>
        /// <returns>Day Index</returns>
        public static int DayIndex(this int hourIndex, out int dayHourIndex)
        {
            int result = System.Convert.ToInt32(Math.Truncate(System.Convert.ToDouble(hourIndex) / 24));

            dayHourIndex = hourIndex - (result * 24);

            return result;
        }

        public static int DayIndex(this int hourIndex)
        {
            return DayIndex(hourIndex, out int dayHourIndex);
        }
    }
}