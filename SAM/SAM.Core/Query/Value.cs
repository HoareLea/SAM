using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static T Value<T>(this Dictionary<string, object> dictionary, string key)
        {
            if (dictionary == null || key == null)
                return default;

            object value;
            if (!dictionary.TryGetValue(key, out value))
                return default;

            if (value == null)
                return default;

            if (value.GetType() != typeof(T))
                return default;

            return (T)(object)(value);
        }
    }
}