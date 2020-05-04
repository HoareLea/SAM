using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool Reorder<T>(this List<T> objects, int startIndex)
        {
            if (objects == null || objects == null)
                return false;

            if (startIndex < 0)
                return false;

            int count = objects.Count();

            if (startIndex >= count)
                return false;

            List<T> result = new List<T>();
            for (int i = startIndex; i < count; i++)
                result.Add(objects.ElementAt(i));

            for (int i = 0; i < startIndex; i++)
                result.Add(objects.ElementAt(i));

            objects.Clear();
            objects.AddRange(result);
            return true;
        }
    }
}