using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public interface IJSAMObject
    {
        bool FromJObject(JObject jObject);

        JObject ToJObject();
    }
}