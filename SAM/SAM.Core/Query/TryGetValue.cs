using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryGetValue<T>(this Dictionary<string, object> dictionary, string key, out T result)
        {
            result = default(T);

            if (dictionary == null || key == null)
                return false;

            object value;
            if (!dictionary.TryGetValue(key, out value))
                return false;

            if (value == null)
                return false;

            if (value.GetType() != typeof(T))
                return false;

            result = (T)(object)(value);
            return true;
        }
    }
}
