using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Similar(this Segment2D segment2D_1, Segment2D segment2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D_1 == segment2D_2)
                return true;

            if (segment2D_1 == null || segment2D_2 == null)
                return false;

            return (segment2D_1[0].AlmostEquals(segment2D_2[0], tolerance) && segment2D_1[1].AlmostEquals(segment2D_2[1], tolerance)) || (segment2D_1[0].AlmostEquals(segment2D_2[1], tolerance) && segment2D_1[1].AlmostEquals(segment2D_2[0], tolerance));
        }

        public static bool Similar(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon2D_1 == polygon2D_2)
                return true;

            if (polygon2D_1 == null || polygon2D_2 == null)
                return false;

            List<Point2D> point2Ds = polygon2D_1.GetPoints();
            foreach(Point2D point2D in point2Ds)
                if (!polygon2D_2.On(point2D, tolerance))
                    return false;


            point2Ds = polygon2D_2.GetPoints();
            foreach (Point2D point2D in point2Ds)
                if (!polygon2D_1.On(point2D, tolerance))
                    return false;

            return System.Math.Abs(polygon2D_1.GetArea() - polygon2D_2.GetArea()) <= tolerance;
        }
    }
}