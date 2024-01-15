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
            else
            {
                return Core.Period.Weekly;
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
    }
}