using System.Collections.Generic;
using System.Linq;

namespace SAM.Math
{
    public static partial class Query
    {
        public static T Modal<T>(this IEnumerable<T> values)
        {
            if (values == null)
                return default;

            return values.GroupBy(i => i)  //Grouping same items
                        .OrderByDescending(g => g.Count()) //now getting frequency of a value
                        .Select(g => g.Key) //selecting key of the group
                        .FirstOrDefault();
        }
    }
}