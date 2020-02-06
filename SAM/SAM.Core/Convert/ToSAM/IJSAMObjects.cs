using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static IEnumerable<T> ToJson<T>(string path) where T : IJSAMObject
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            return new SAMJsonCollection<T>(path);
        }

        public static IEnumerable<IJSAMObject> ToJson(string path)
        {
            return ToJson<IJSAMObject>(path);
        }
    }
}
