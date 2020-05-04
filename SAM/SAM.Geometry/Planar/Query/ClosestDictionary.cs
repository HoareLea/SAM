using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Dictionary<Segment2D, double> ClosestDictionary(this ISegmentable2D segmentable2D_1, ISegmentable2D segmentable2D_2, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment2D> segment2Ds = segmentable2D_2?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            List<Tuple<Segment2D, double>> tupleList = new List<Tuple<Segment2D, double>>();
            double distance_Min = double.MaxValue;
            foreach (Segment2D segment2D in segment2Ds)
            {
                double distance = segmentable2D_1.Distance(segment2D);
                tupleList.Add(new Tuple<Segment2D, double>(segment2D, distance));
                if (distance < distance_Min)
                    distance_Min = distance;
            }

            Dictionary<Segment2D, double> result = new Dictionary<Segment2D, double>();
            foreach (Tuple<Segment2D, double> tuple in tupleList)
            {
                if (System.Math.Abs(tuple.Item2 - distance_Min) >= tolerance)
                    continue;

                double distance;
                if (!result.TryGetValue(tuple.Item1, out distance))
                    distance = double.MaxValue;

                result[tuple.Item1] = System.Math.Min(distance, tuple.Item2);
            }

            return result;
        }

        public static Dictionary<Segment2D, double> ClosestDictionary(this Point2D point2D, ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null)
                return null;

            List<Segment2D> segment2Ds = segmentable2D?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            List<Tuple<Segment2D, double>> tupleList = new List<Tuple<Segment2D, double>>();
            double distance_Min = double.MaxValue;
            foreach (Segment2D segment2D in segment2Ds)
            {
                double distance = segment2D.Distance(point2D);
                tupleList.Add(new Tuple<Segment2D, double>(segment2D, distance));
                if (distance < distance_Min)
                    distance_Min = distance;
            }

            Dictionary<Segment2D, double> result = new Dictionary<Segment2D, double>();
            foreach (Tuple<Segment2D, double> tuple in tupleList)
            {
                if (System.Math.Abs(tuple.Item2 - distance_Min) >= tolerance)
                    continue;

                double distance;
                if (!result.TryGetValue(tuple.Item1, out distance))
                    distance = double.MaxValue;

                result[tuple.Item1] = System.Math.Min(distance, tuple.Item2);
            }

            return result;
        }
    }
}