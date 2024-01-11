using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static HashSet<Point2D> UniquePoint2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds)
        {
            if (segmentable2Ds == null)
                return null;

            HashSet<Point2D> result = new HashSet<Point2D>();
            foreach(ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Point2D> point2Ds = segmentable2D?.GetPoints();
                if (point2Ds == null || point2Ds.Count == 0)
                {
                    continue;
                }

                foreach(Point2D point2D in point2Ds)
                {
                    result.Add(point2D);
                }
            }

            return result;
        }

        public static List<Point2D> UniquePoint2Ds<T>(this IEnumerable<T> segmentable2Ds, double tolerance = Core.Tolerance.Distance) where T : ISegmentable2D
        {
            if (segmentable2Ds == null)
                return null;

            List<Point2D> result = new List<Point2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Point2D> point2Ds = segmentable2D?.GetPoints();
                if (point2Ds == null || point2Ds.Count == 0)
                    continue;

                foreach (Point2D point2D in point2Ds)
                {
                    Point2D point2D_Temp = result.Find(x => x.AlmostEquals(point2D, tolerance));
                    if (point2D_Temp == null)
                        result.Add(point2D);
                }
            }

            return result;
        }
    }
}