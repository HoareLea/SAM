using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<int> IndexesOf<T>(this List<T> values, T value)
        {
            if (values == null)
                return null;

            List<int> result = new List<int>();
            for(int i=0; i < values.Count; i++)
                if ((value == null && values[i] == null) || values[i].Equals(value))
                    result.Add(i);

            return result;
        }
    }
}