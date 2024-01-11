using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Truncate(this Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if(point2D == null)
            {
                return null;
            }

            return new Point2D(System.Math.Truncate(point2D.X / tolerance) * tolerance, System.Math.Truncate(point2D.Y / tolerance) * tolerance);
        }

        public static List<Point2D> Truncate(this IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if(point2Ds == null)
            {
                return null;
            }

            List<Point2D> result = new List<Point2D>();
            foreach(Point2D point2D in point2Ds)
            {
                result.Add(Truncate(point2D, tolerance));
            }

            return result;
        }

        public static Polygon2D Truncate(this Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            List<Point2D> point2Ds = polygon2D?.Points;
            if(point2Ds == null)
            {
                return null;
            }

            return new Polygon2D(point2Ds.Truncate(tolerance));
        }

        public static Segment2D Truncate(this Segment2D segment2D, double tolerance = Core.Tolerance.Distance)
        {
            if(segment2D == null)
            {
                return null;
            }

            return new Segment2D(Truncate(segment2D[0], tolerance), Truncate(segment2D[1], tolerance));
        }
    }
}