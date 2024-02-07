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

            List<T> result = new List<T>();
            foreach (T value in values)
            {
                for (int i = 0; i < count; i++)
                {
                    result.Add(value);
                }
            }

            return result;
        }

        public static IndexedDoubles Repeat(this IndexedDoubles indexedDoubles, Period period_Destination, Period period_Source = Core.Period.Undefined)
        {
            if (indexedDoubles == null || period_Destination == Core.Period.Undefined)
            {
                return null;
            }

            if (period_Source == Core.Period.Undefined)
            {
                period_Source = Period(indexedDoubles.GetMaxIndex().Value - indexedDoubles.GetMinIndex().Value + 1);
            }

            if (period_Source == Core.Period.Undefined)
            {
                return null;
            }

            if (period_Destination == period_Source)
            {
                return new IndexedDoubles(indexedDoubles);
            }

            int count = Count(period_Source, period_Destination);
            if (count == -1)
            {
                return null;
            }

            IEnumerable<int> keys = indexedDoubles.Keys;
            if(keys == null)
            {
                return null;
            }


            IndexedDoubles result = new IndexedDoubles();
            foreach(int key in keys)
            {
                double value = indexedDoubles[key];

                for (int i = 0; i < count; i++)
                {
                    result.Add((key * count) + i ,value);
                }
            }

            return result;
        }
    }
}