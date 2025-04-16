using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool MatchLength<T, X>(List<T> list_1, List<X> list_2)
        {
            if ((object)list_1 == (object)list_2)
            {
                return true;
            }

            if (list_1 == null || list_2 == null)
            {
                return false;
            }

            int count_1 = list_1.Count;
            int count_2 = list_2.Count;

            if (count_1 == count_2)
            {
                return true;
            }

            if (count_1 == 0 || count_2 == 0)
            {
                return false;
            }

            int max;
            int min;
            if (count_1 > count_2)
            {
                max = count_1;
                min = count_2;
            }
            else
            {
                max = count_2;
                min = count_1;
            }

            for (int i = min; i < max; i++)
            {
                if (list_1.Count <= i)
                {
                    list_1.Add(list_1[i % count_1]);
                }

                if (list_2.Count <= i)
                {
                    list_2.Add(list_2[i % count_2]);
                }
            }

            return true;
        }
    }
}