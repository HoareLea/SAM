using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string Name(this JObject jObject)
        {
            if (jObject == null)
                return null;
            
            return jObject.Value<string>("Name");
        }
    }
}
