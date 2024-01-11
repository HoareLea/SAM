using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Calculates all self intersection points for segmentable if exists
        /// </summary>
        /// <param name="segmentable2D">Segmentable</param>
        /// <param name="maxCount">Max number of intersections will be calculated</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Self Intersection Points</returns>
        public static List<Point2D> SelfIntersectionPoint2Ds(this ISegmentable2D segmentable2D, int maxCount = int.MaxValue, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment2D> segment2Ds = segmentable2D?.GetSegments()?.Split(tolerance);
            if (segment2Ds == null)
            {
                return null;
            }

            segment2Ds.RemoveAll(x => x == null);
            if (segment2Ds.Count == 0)
            {
                return null;
            }

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                point2Ds.Add(segment2D[0], tolerance);
                point2Ds.Add(segment2D[1], tolerance);
            }

            List<Point2D> result = new List<Point2D>();
            foreach (Point2D point2D in point2Ds)
            {
                if (result.Count >= maxCount)
                    break;

                List<Segment2D> segment2Ds_Temp = segment2Ds.FindAll(x => x[0].Distance(point2D) <= tolerance || x[1].Distance(point2D) <= tolerance);
                if (segment2Ds_Temp.Count > 2)
                {
                    result.Add(point2D);
                }
            }

            return result;
        }
    }
}