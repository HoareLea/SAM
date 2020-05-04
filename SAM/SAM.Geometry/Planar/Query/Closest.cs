using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Closest(this ISegmentable2D segmentable2D, Point2D point2D)
        {
            if (segmentable2D == null || point2D == null)
                return null;

            double min = double.MaxValue;
            Point2D result = null;
            foreach (Segment2D segment2D in segmentable2D.GetSegments())
            {
                Point2D point2D_Closest = segment2D.Closest(point2D);
                double distance = point2D.Distance(point2D_Closest);
                if (distance < min)
                {
                    result = point2D_Closest;
                    min = distance;
                }
            }

            return result;
        }

        public static Point2D Closest(this IEnumerable<Point2D> point2Ds, Point2D point2D)
        {
            if (point2Ds == null || point2D == null)
                return null;

            double distance_Min = double.MaxValue;
            Point2D result = null;
            foreach (Point2D point2D_Temp in point2Ds)
            {
                double distance = point2D.Distance(point2D_Temp);
                if (distance < distance_Min)
                {
                    result = point2D_Temp;
                    if (distance == 0)
                        return result;

                    distance_Min = distance;
                }
            }
            return result;
        }

        public static Point2D Closest(this ISegmentable2D segmentable2D, IEnumerable<Point2D> point2Ds)
        {
            if (segmentable2D == null || point2Ds == null)
                return null;

            List<Segment2D> segment2Ds = segmentable2D.GetSegments();
            if (segment2Ds == null)
                return null;

            double min = double.MaxValue;
            Point2D result = null;
            foreach (Segment2D segment2D in segment2Ds)
            {
                foreach (Point2D point2D in point2Ds)
                {
                    Point2D point2D_Closest = segment2D.Closest(point2D);
                    double distance = point2D.Distance(point2D_Closest);
                    if (distance < min)
                    {
                        result = point2D_Closest;
                        min = distance;
                    }
                }
            }

            return result;
        }

        public static T Closest<T>(this Point2D point2D, IEnumerable<T> segmentable2Ds, double tolerance = Core.Tolerance.Distance) where T : ISegmentable2D
        {
            if (point2D == null || segmentable2Ds == null)
                return default;

            double distance_Min = double.MaxValue;
            T result = default;
            foreach (T segmentable2D_Temp in segmentable2Ds)
            {
                Dictionary<Segment2D, double> dictionary = ClosestDictionary(point2D, segmentable2D_Temp, tolerance);
                if (dictionary == null || dictionary.Count == 0)
                    continue;

                double distance = dictionary.Values.Min();
                if (distance < distance_Min)
                {
                    distance_Min = distance;

                    result = segmentable2D_Temp;
                    if (distance == 0)
                        return result;
                }
            }

            return default;
        }

        public static List<Segment2D> Closest(this ISegmentable2D segmentable2D_1, ISegmentable2D segmentable2D_2, double tolerance = Core.Tolerance.Distance)
        {
            return ClosestDictionary(segmentable2D_1, segmentable2D_2, tolerance)?.Keys?.ToList();
        }

        public static T Closest<T>(this ISegmentable2D segmentable2D, IEnumerable<T> segmentable2Ds, double tolerance = Core.Tolerance.Distance) where T : ISegmentable2D
        {
            if (segmentable2D == null || segmentable2Ds == null)
                return default;

            double distance_Min = double.MaxValue;
            T result = default;
            foreach (T segmentable2D_Temp in segmentable2Ds)
            {
                Dictionary<Segment2D, double> dictionary = ClosestDictionary(segmentable2D, segmentable2D_Temp, tolerance);
                if (dictionary == null || dictionary.Count == 0)
                    continue;

                double distance = dictionary.Values.Min();
                if (distance < distance_Min)
                {
                    distance_Min = distance;

                    result = segmentable2D_Temp;
                    if (distance == 0)
                        return result;
                }
            }

            return default;
        }
    }
}