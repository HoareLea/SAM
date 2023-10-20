using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<List<T>> Graft<T>(this IEnumerable<T> values, int count)
        {
            if(values == null || count <= 0)
            {
                return null;
            }

            List<List<T>> result = new List<List<T>>();
            foreach(T value in values)
            {
                List<T> list = new List<T>();
                for (int i = 0; i < count; i++)
                {
                    list.Add(value);
                }
                result.Add(list);
            }

            return result;
        }
    }
}