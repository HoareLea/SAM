using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Tag Tag(this JObject jObject)
        {
            if(jObject == null)
            {
                return null;
            }

            if (!jObject.ContainsKey("Tag"))
            {
                return null;
            }

            JObject jObject_Tag = jObject.Value<JObject>("Tag");
            if(jObject_Tag == null)
            {
                return null;
            }

            return new Tag(jObject_Tag);
        }
    }
}