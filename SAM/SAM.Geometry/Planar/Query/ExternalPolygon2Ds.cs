using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon2D> ExternalPolygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segmentable2Ds, tolerance);
            if (polygon2Ds == null)
                return null;

            return Union(polygon2Ds, tolerance);
        }
    }
}