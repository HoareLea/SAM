using System.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static List<T> Reorder<T>(this IEnumerable<T> objects, int startIndex)
        {
            if (objects == null || objects == null)
                return null;

            if (startIndex < 0)
                return null;

            int count = objects.Count();

            if (startIndex >= count)
                return null;

            List <T> result = new List<T>();
            for (int i = startIndex; i < count; i++)
                result.Add(objects.ElementAt(i));

            for (int i = 0; i < startIndex; i++)
                result.Add(objects.ElementAt(i));

            return result;
        }
    }
}
