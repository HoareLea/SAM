using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Segment2D> ClosestSegment2Ds(this ISegmentable2D segmentable2D, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D == null || point2D == null)
                return null;

            double min = double.MaxValue;
            Dictionary<Segment2D, double> dictionary = new Dictionary<Segment2D, double>();
            foreach (Segment2D segment2D in segmentable2D.GetSegments())
            {
                Point2D point2D_Closest = segment2D.Closest(point2D);
                double distance = point2D.Distance(point2D_Closest);
                dictionary[segment2D] = distance;
                if (distance < min)
                    min = distance;
            }

            List<double> distances = dictionary.Values.Distinct().ToList();
            distances.RemoveAll(x => x > min + tolerance);

            List<Segment2D> result = new List<Segment2D>();
            foreach (KeyValuePair<Segment2D, double> keyValuePair in dictionary)
                if (distances.Contains(keyValuePair.Value))
                    result.Add(keyValuePair.Key);

            return result;
        }
    }
}