using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<List<T>> Permutations<T>(this IEnumerable<T> @in) where T : IComparable
        {
            if(@in == null)
            {
                return null;
            }

            List<List<T>> result = new List<List<T>>();
            Permutations<T>(@in.ToList(), 0, result);

            return result;
        }

        public static List<List<T>> Permutations<T>(this IEnumerable<T> @in, bool unique) where T : IComparable
        {
            List<List<T>> result = Permutations<T>(@in);
            if (!unique)
            {
                return result;
            }

            if(result == null || result.Count < 2)
            {
                return result;
            }

            List<List<T>> result_New = new List<List<T>>();
            while(result.Count > 0)
            {
                List<T> values = result[0];

                result_New.Add(values);
                for (int i = result.Count - 1; i >= 0; i--)
                {
                    if (Similar<T>(values, result[i]))
                    {
                        result.RemoveAt(i);
                    }
                }
            }

            return result_New;
        }

        private static void Permutations<T>(this List<T> @in, int start, List<List<T>> @out) where T : IComparable
        {
            if(start == @in.Count - 1)
            {
                @out.Add(new List<T>(@in));
                return;
            }

            for (int i = start; i < @in.Count; i++)
            {
                // Swap elements at positions i and start
                T temp = @in[i];
                @in[i] = @in[start];
                @in[start] = temp;

                // Recurse on the remaining elements
                Permutations<T>(@in, start + 1, @out);

                // Backtrack: swap elements back to their original positions
                @in[start] = @in[i];
                @in[i] = temp;
            }
        }
    }
}