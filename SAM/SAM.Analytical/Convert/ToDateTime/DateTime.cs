using System;

namespace SAM.Analytical
{
    public static partial class Convert
    {  
        /// <summary>
        /// Converts hour index to dateTime default Year 2015
        /// </summary>
        /// <param name="hourIndex">Value usually between 0 and 8759 representing hour in year</param>
        /// <returns>DateTime</returns>
        public static DateTime ToDateTime(int hourIndex)
        {
            return new DateTime(TimeSpan.FromHours(hourIndex).Ticks + new DateTime(2015, 1, 1).Ticks);
        }
    }
}