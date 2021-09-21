using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon2D> SelfIntersectionPolygon2Ds(this Polygon2D polygon2D, double maxLength, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Segment2D> segment2Ds = SelfIntersectionSegment2Ds(polygon2D, maxLength, tolerance);
            if (segment2Ds == null || segment2Ds.Count < 2)
            {
                return null;
            }

            segment2Ds = segment2Ds.Split(tolerance);

            return Create.Polygon2Ds(segment2Ds, tolerance);
        }
    }
}