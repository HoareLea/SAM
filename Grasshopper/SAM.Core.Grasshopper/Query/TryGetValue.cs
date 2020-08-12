using Rhino.Geometry;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static bool TryGetValue<T>(this Dictionary<Interval, T> dictionary, double value, out T t)
        {
            t = default;

            if (dictionary == null)
                return false;

            foreach(KeyValuePair<Interval, T> keyValuePair in dictionary)
                if(keyValuePair.Key.IncludesParameter(value))
                {
                    t = keyValuePair.Value;
                    return true;
                }

            return false;
        }
    }
}