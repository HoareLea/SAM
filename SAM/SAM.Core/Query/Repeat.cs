using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<T> Repeat<T>(this IEnumerable<T> values, Period period_Destination, Period period_Source = Core.Period.Undefined)
        {
            if(values == null || period_Destination == Core.Period.Undefined)
            {
                return null;
            }

            if(period_Source == Core.Period.Undefined)
            {
                period_Source = Query.Period(values.Count());
            }

            if (period_Source == Core.Period.Undefined)
            {
                return null;
            }

            if (period_Destination == period_Source)
            {
                return new List<T>(values);
            }

            int count = Count(period_Source, period_Destination);
            if (count == -1)
            {
                return null;
            }

            List<T> result = null;
            foreach (T value in values)
            {
                for (int i = 0; i < count; i++)
                {
                    result.Add(value);
                }
            }

            return result;
        }
    }
}