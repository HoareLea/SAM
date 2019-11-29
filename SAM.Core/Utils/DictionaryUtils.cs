using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public static class DictionaryUtils
    {
        public static bool Add(this Dictionary<string, object> dictionary, string key, string value)
        {
            if (dictionary == null || key == null )
                return false;

            dictionary[key] = value;
            return true;
        }

        public static T GetValue<T>(this Dictionary<string, object> dictionary, string key)
        {
            if (dictionary == null || key == null)
                return default(T);

            object value;
            if (!dictionary.TryGetValue(key, out value))
                return default(T);

            if (value == null)
                return default(T);

            if (value.GetType() != typeof(T))
                return default(T);

            return (T)(object)(value);
        }
    }
}
