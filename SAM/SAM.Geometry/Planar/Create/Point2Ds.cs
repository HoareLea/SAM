using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Point2D> Point2Ds(this JArray jArray)
        {
            if (jArray == null)
                return null;

            List<Point2D> result = new List<Point2D>();

            foreach (JObject jObject in jArray)
                result.Add(new Point2D(jObject));

            return result;
        }
    }
}