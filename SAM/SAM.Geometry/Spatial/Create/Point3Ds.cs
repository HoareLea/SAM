using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<Point3D> Point3Ds(this JArray jArray)
        {
            if (jArray == null)
                return null;

            List<Point3D> result = new List<Point3D>();

            foreach (JObject jObject in jArray)
                result.Add(new Point3D(jObject));

            return result;
        }
    }
}