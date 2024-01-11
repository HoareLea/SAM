using Newtonsoft.Json.Linq;

namespace SAM.Geometry
{
    public static partial class Create
    {
        public static ISAMGeometry ISAMGeometry(this JObject jObject)
        {
            return Core.Create.IJSAMObject(jObject) as ISAMGeometry;
        }

        public static T ISAMGeometry<T>(this JObject jObject) where T : ISAMGeometry
        {
            return Core.Create.IJSAMObject<T>(jObject);
        }
    }
}