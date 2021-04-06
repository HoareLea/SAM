using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static IClosed2D IClosed2D(this JObject jObject)
        {
            if (jObject == null)
                return null;

            return Geometry.Create.ISAMGeometry(jObject) as IClosed2D;
        }

        public static IClosed2D IClosed2D(this Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            List<Point2D> point2Ds = polygon2D?.GetPoints();
            if (point2Ds == null || point2Ds.Count < 3)
                return null;

            if (point2Ds.Count == 3)
                return new Triangle2D(point2Ds[0], point2Ds[1], point2Ds[2]);

            if(polygon2D.Rectangular(tolerance))
                return Rectangle2D(point2Ds);

            return new Polygon2D(point2Ds);
        }
    }
}