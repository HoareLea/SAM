using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static IClosed2D IClosed2D(this JObject jObject)
        {
            return Geometry.Create.ISAMGeometry(jObject) as IClosed2D;
        }
    }
}