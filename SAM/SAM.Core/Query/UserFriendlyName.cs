using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string UserFriendlyName(this string name)
        {
            if (name == null)
                return null;

            string result = name;

            if (result.StartsWith("get_") && result.Length > 4)
                result = result.Substring(4);

            if (result.StartsWith("Get") && result.Length > 3)
                result = result.Substring(3);

            if (result.StartsWith("get") && result.Length > 3)
                result = result.Substring(3);

            return result;
        }
    }
}