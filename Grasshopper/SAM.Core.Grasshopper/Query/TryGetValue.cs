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
            {
                double value_Temp = System.Math.Round(value, System.MidpointRounding.ToEven);

                if (keyValuePair.Key.IncludesParameter(value_Temp))
                {
                    t = keyValuePair.Value;
                    return true;
                }
            }


            return false;
        }
    }
}