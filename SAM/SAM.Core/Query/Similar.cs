using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Similar<T>(this IEnumerable<T> items_1, IEnumerable<T> items_2) where T :IComparable
        {
            return Similar(items_1, items_2, null);
        }

        public static bool Similar<T>(this IEnumerable<T> items_1, IEnumerable<T> items_2, Func<T, T, bool> equatableFunc) where T : IComparable
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

            if (equatableFunc == null)
            {
                equatableFunc = new Func<T, T, bool>((element_1, element_2) =>
                {
                    if (element_1 == null && element_2 == null)
                    {
                        return true;
                    }

                    if (element_1 == null || element_2 == null)
                    {
                        return false;
                    }

                    if (!element_1.Equals(element_2))
                    {
                        return false;
                    }

                    return true;
                });
            }

            for (int i = 0; i < count; i++)
            {
                if(!equatableFunc.Invoke(items_1.ElementAt(i), items_2.ElementAt(i)))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Similar(System.Drawing.Color color_1, System.Drawing.Color color_2)
        {
            return color_1.A == color_2.A && color_1.R == color_2.R && color_1.G == color_2.G && color_1.B == color_2.B;
        }
    }
}