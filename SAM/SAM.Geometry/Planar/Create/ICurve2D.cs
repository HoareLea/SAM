using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static ICurve2D ICurve2D(this JObject jObject)
        {
            return Geometry.Create.ISAMGeometry(jObject) as ICurve2D;
        }
    }
}