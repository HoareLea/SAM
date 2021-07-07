using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon2D> ExternalPolygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Segment2D> segment2Ds = segmentable2Ds?.Segment2Ds();
            if (segment2Ds == null || segment2Ds.Count == 0)
            {
                return null;
            }

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds == null)
            {
                return null;
            }

            polygon2Ds = Union(polygon2Ds, tolerance);
            if (polygon2Ds != null)
            {
                polygon2Ds.RemoveAll(x => x.GetArea() < tolerance);
            }

            return polygon2Ds;
        }

        public static List<Polygon2D> ExternalPolygon2Ds(this IEnumerable<Polygon2D> polygon2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Polygon2D> result = Union(polygon2Ds, tolerance);
            if (result != null)
            {
                result.RemoveAll(x => x.GetArea() < tolerance);
            }

            return result;
        }

        public static List<Polygon2D> ExternalPolygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double maxDistance, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
            {
                return null;
            }

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segmentable2Ds, maxDistance, tolerance);
            if(polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return null;
            }

            return ExternalPolygon2Ds(polygon2Ds, tolerance);
        }
    }
}