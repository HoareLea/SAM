using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static int HourOfYear(this DateTime dateTime)
        {
            return ((dateTime.DayOfYear - 1) * 24) + dateTime.Hour;
        }
    }
}