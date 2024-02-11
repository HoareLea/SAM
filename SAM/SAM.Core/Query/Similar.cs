using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Similar<T>(this IEnumerable<T> items_1, IEnumerable<T> items_2) where T :IComparable
        {
            if (items_1 == items_2)
            {
                return true;
            }

            if (items_1 == null || items_2 == null)
            {
                return false;
            }

            int count = items_1.Count();

            if (count != items_2.Count())
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                T element_1 = items_1.ElementAt(i);
                T element_2 = items_2.ElementAt(i);

                if(element_1 == null && element_2 == null)
                {
                    continue;
                }

                if(element_1 == null || element_2 == null)
                {
                    return false;
                }

                if (!element_1.Equals(element_2))
                {
                    return false;
                }
            }

            return true;
        }
    }
}