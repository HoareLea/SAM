using Newtonsoft.Json.Linq;
using System.IO;

namespace SAM.Core
{
    public static partial class Query
    {
        public static JArray JArray(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            JToken jToken = JToken.Parse(json);
            if (jToken == null)
                return null;

            if (jToken is JObject)
                return new JArray() { jToken };
            else if (jToken is JArray)
                return (JArray)jToken;

            return null;
        }
    }
}