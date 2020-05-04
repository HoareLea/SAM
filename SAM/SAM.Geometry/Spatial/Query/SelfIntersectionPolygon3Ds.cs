using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Polygon3D> SelfIntersectionPolygon3Ds(this Polygon3D polygon3D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon3D == null)
                return null;

            Plane plane = polygon3D.GetPlane();
            if (plane == null)
                return null;

            Planar.Polygon2D polygon2D = plane.Convert(polygon3D);

            List<Planar.Polygon2D> polygon2Ds = Planar.Query.SelfIntersectionPolygon2Ds(polygon2D, tolerance);
            if (polygon2Ds == null)
                return null;

            return polygon2Ds.ConvertAll(x => plane.Convert(x));
        }
    }
}