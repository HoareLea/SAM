using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Dictionary<T, List<Z>> Dictionary<T, Z>(this IEnumerable<Z> objects, string name)
        {
            if (objects == null || string.IsNullOrEmpty(name))
                return null;

            Dictionary<T, List<Z>> result = new Dictionary<T, List<Z>>();
            foreach (Z z in objects)
            {
                object value;
                if (!TryGetValue(z, name, out value))
                    continue;

                if (!(value is T))
                    continue;

                List<Z> sAMObjects_Temp;
                if (!result.TryGetValue((T)value, out sAMObjects_Temp))
                {
                    sAMObjects_Temp = new List<Z>();
                    result[(T)value] = sAMObjects_Temp;
                }

                sAMObjects_Temp.Add(z);
            }

            return result;
        }
    }
}