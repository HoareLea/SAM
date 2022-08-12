using System;
using System.Collections.Generic;
using System.Xml.Linq;

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

        public static T Value<T>(this XAttribute xAttribute)
        {
            if (!TryGetValue(xAttribute, out T result))
            {
                return default(T);
            }

            return result;
        }

        public static T Value<T>(this XAttribute xAttribute, T defaultValue)
        {
            if(!TryGetValue(xAttribute, out T result))
            {
                return defaultValue;
            }

            return result;
        }
    }
}