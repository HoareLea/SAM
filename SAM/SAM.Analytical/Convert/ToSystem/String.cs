using System;

namespace SAM.Analytical
{
    public static partial class Convert
    {  
        /// <summary>
        /// Converts hour index to string default Year 2015
        /// </summary>
        /// <param name="hourIndex">Value usualy between 0 and 8759 representing hour in year</param>
        /// <returns>String</returns>
        public static string ToString(int hourIndex)
        {
            return ToString(ToDateTime(hourIndex));
        }

        public static string ToString(DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy@HH:mm");
        }
    }
}