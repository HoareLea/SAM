using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public static partial class Query
    {
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
