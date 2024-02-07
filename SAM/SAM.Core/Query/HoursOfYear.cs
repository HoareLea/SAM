using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static HashSet<int> HoursOfYear(int dayOfYear)
        {
            HashSet<int> result = new HashSet<int>();

            for(int i=0; i < 24; i++)
            {
                result.Add(HourOfYear(dayOfYear, i));
            }

            return result;
        }

        public static HashSet<int> HoursOfYear(IEnumerable<int> daysOfYear)
        {
            if(daysOfYear == null)
            {
                return null;
            }

            HashSet<int> result = new HashSet<int>();
            foreach(int dayOfYear in daysOfYear)
            {
                HashSet<int> hoursOfYear = HoursOfYear(dayOfYear);
                if(hoursOfYear == null)
                {
                    continue;
                }

                foreach(int hourOfYear in hoursOfYear)
                {
                    result.Add(hourOfYear);
                }
            }

            return result;
        }
    }
}