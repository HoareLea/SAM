using ClipperLib;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon2D> SelfIntersectionPolygon2Ds(this Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D == null)
                return null;

            List<List<IntPoint>> intPointsList = Clipper.SimplifyPolygon(Convert.ToClipper((ISegmentable2D)polygon2D, tolerance));
            if (intPointsList == null)
                return null;

            List<Polygon2D> polygon2Ds = new List<Polygon2D>();
            foreach (List<IntPoint> intPoints in intPointsList)
            {
                List<Point2D> point2Ds = intPoints.ToSAM(tolerance);
                polygon2Ds.Add(new Polygon2D(point2Ds));
            }

            return polygon2Ds;
        }

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