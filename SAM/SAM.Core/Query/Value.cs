using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static T Value<T>(this Dictionary<string, object> dictionary, string key)
        {
            if (dictionary == null || key == null)
            {
                return default;
            }

            if (!dictionary.TryGetValue(key, out object value) || value == null)
            {
                return default;
            }

            Type type = value.GetType();
            if (type != typeof(T))
            {
                if(!typeof(T).IsAssignableFrom(type))
                {
                    return default;
                }
            }

            return (T)value;
        }
    }
}