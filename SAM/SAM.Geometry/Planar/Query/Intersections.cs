using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> Intersections(this ISegmentable2D segmentable2D_1, ISegmentable2D segmentable2D_2)
        {
            if (segmentable2D_1 == null || segmentable2D_2 == null)
                return null;

            List<Segment2D> segment2Ds_1 = segmentable2D_1.GetSegments();
            if (segment2Ds_1 == null)
                return null;

            List<Segment2D> segment2Ds_2= segmentable2D_2.GetSegments();
            if (segment2Ds_2 == null)
                return null;

            HashSet<Point2D> point2Ds = new HashSet<Point2D>();
            foreach(Segment2D segment2D_1 in segment2Ds_1)
            {
                if (segment2D_1 == null)
                    continue;

                foreach(Segment2D segment2D_2 in segment2Ds_2)
                {
                    if (segment2D_2 == null)
                        continue;

                    Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, true);
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
            foreach(Segment2D segment2D in segment2Ds)
            {
                List<Point2D> point2Ds_Temp = Intersections(segmentable2D, segment2D);
                if (point2Ds_Temp == null || point2Ds_Temp.Count == 0)
                    continue;

                point2Ds_Temp.ForEach(x => point2Ds.Add(x));
            }

            return point2Ds.ToList();
        }

        public static List<Point2D> Intersections(this Point2D point2D, Vector2D vector2D, ISegmentable2D segmentable2D, double tolerance = Tolerance.MicroDistance)
        {
            if (point2D == null || vector2D == null)
                return null;

            List<Segment2D> segment2Ds = segmentable2D?.GetSegments();
            if (segment2Ds == null)
                return null;

            HashSet<Point2D> point2Ds = new HashSet<Point2D>();

            Segment2D segment2D = new Segment2D(point2D, vector2D);
            foreach(Segment2D segment2D_Temp in segment2Ds)
            {
                Point2D point2D_closest_1;
                Point2D point2D_closest_2;

                Point2D point2D_Intersection = segment2D.Intersection(segment2D_Temp, out point2D_closest_1, out point2D_closest_2);
                if (point2D_Intersection == null)
                    continue;

                if (point2D_closest_1 == null || point2D_closest_2 == null)
                {
                    point2Ds.Add(point2D_Intersection);
                    continue;
                }

                if(point2D_closest_2 != null && segment2D_Temp.Distance(point2D_closest_2) < tolerance)
                {
                    point2Ds.Add(point2D_Intersection);
                    continue;
                }
                    
            }

            return point2Ds.ToList();
        }

    }
}
