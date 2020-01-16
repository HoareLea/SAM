using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool Add(this Dictionary<string, object> dictionary, string key, string value)
        {
            if (dictionary == null || key == null)
                return false;

            dictionary[key] = value;
            return true;
        }
    }
}
