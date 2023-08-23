using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static int Next<T>(this IEnumerable<T> values, T value, int itemCount, int start = 0)
        {
            if(values == null)
            {
                return -1;
            }

            int length = values.Count();
            if(length == 0 || length <= itemCount || length <= start)
            {
                return -1;
            }

            int count = 0;
            for(int i = start; i < length; i++)
            {
                T value_Current = values.ElementAt(i);
                if(value_Current as dynamic == value as dynamic)
                {
                    count++;
                }
                else
                {
                    count = 0;
                }

                if(count == itemCount)
                {
                    return i;
                }

            }

            return -1;
        }
    }
}