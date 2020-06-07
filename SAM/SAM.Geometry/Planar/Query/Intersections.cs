using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> Intersections(this ISegmentable2D segmentable2D_1, ISegmentable2D segmentable2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D_1 == null || segmentable2D_2 == null)
                return null;

            List<Segment2D> segment2Ds_1 = segmentable2D_1.GetSegments();
            if (segment2Ds_1 == null)
                return null;

            List<Segment2D> segment2Ds_2 = segmentable2D_2.GetSegments();
            if (segment2Ds_2 == null)
                return null;

            HashSet<Point2D> point2Ds = new HashSet<Point2D>();
            foreach (Segment2D segment2D_1 in segment2Ds_1)
            {
                if (segment2D_1 == null)
                    continue;

                foreach (Segment2D segment2D_2 in segment2Ds_2)
                {
                    if (segment2D_2 == null)
                        continue;

                    Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, true, tolerance);
                    if (point2D_Intersection == null)
                        continue;

                    point2Ds.Add(point2D_Intersection);
                }
            }

            return point2Ds.ToList();
        }

        public static List<Point2D> Intersections(this ISegmentable2D segmentable2D, IEnumerable<Segment2D> segment2Ds)
        {
            if (segmentable2D == null || segment2Ds == null)
                return null;

            List<Segment2D> segment2Ds_Segmentable2D = segmentable2D.GetSegments();
            if (segment2Ds_Segmentable2D == null)
                return null;

            HashSet<Point2D> point2Ds = new HashSet<Point2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                List<Point2D> point2Ds_Temp = Intersections(segmentable2D, segment2D);
                if (point2Ds_Temp == null || point2Ds_Temp.Count == 0)
                    continue;

                point2Ds_Temp.ForEach(x => point2Ds.Add(x));
            }

            return point2Ds.ToList();
        }

        public static List<Point2D> Intersections(this Point2D point2D, Vector2D vector2D, ISegmentable2D segmentable2D, bool keepDirection, bool removeCollinear = true, bool sort = true, bool selfIntersection = true, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null || vector2D == null)
                return null;

            return IntersectionDictionary(point2D, vector2D, segmentable2D, keepDirection, removeCollinear, sort, selfIntersection, tolerance)?.Keys?.ToList();
        }

        public static List<Point2D> Intersections(this Point2D point2D, Vector2D vector2D, IEnumerable<ISegmentable2D> segmentable2Ds, bool keepDirection, bool removeCollinear = true, bool sort = true, bool selfIntersection = true, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null || vector2D == null)
                return null;

            return IntersectionDictionary(point2D, vector2D, Segment2Ds(segmentable2Ds), keepDirection, removeCollinear, sort, selfIntersection, tolerance)?.Keys?.ToList();
        }
    }
}