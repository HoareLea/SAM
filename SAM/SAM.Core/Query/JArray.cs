using Newtonsoft.Json.Linq;
using System.IO;

namespace SAM.Core
{
    public static partial class Query
    {
        public static JArray JArray(this string pathOrJson)
        {
            if (pathOrJson == null)
                return null;

            if (string.IsNullOrWhiteSpace(pathOrJson))
                return new JArray();

            string json = pathOrJson;
            if (File.Exists(pathOrJson))
                json = File.ReadAllText(pathOrJson);

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