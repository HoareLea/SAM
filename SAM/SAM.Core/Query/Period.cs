using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Period Period(int count)
        {
            if(count < 2)
            {
                return Core.Period.Undefined;
            }

            if (count > 365)
            {
                return Core.Period.Hourly;
            }
            else if (count > 53)
            {
                return Core.Period.Daily;
            }
            else if(count > 12)
            {
                return Core.Period.Weekly;
            }
            else
            {
                return Core.Period.Monthly;
            }
        }

        public static Period Period(this IEnumerable<double> values)
        {
            if (values == null)
            {
                return Core.Period.Undefined;
            }

            return Period(values.Count());
        }

        public static Period Period(this IndexedDoubles indexedDoubles)
        {
            if(indexedDoubles == null)
            {
                return Core.Period.Undefined;
            }

            IEnumerable<int> keys = indexedDoubles.Keys;
            if(keys == null)
            {
                return Core.Period.Undefined;
            }

            return Period(keys.Count());

        }
    }
}