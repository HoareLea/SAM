using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static int DayOfYear(int hourOfYear)
        {
            return System.Convert.ToInt32(Math.Truncate(System.Convert.ToDouble(hourOfYear) / 24.0));
        }
    }
}