using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static double Parameter(this ISegmentable2D segmentable2D, Point2D point2D, bool inverted = false, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null || segmentable2D == null)
                return double.NaN;

            Point2D point2D_Closest = segmentable2D.Closest(point2D);

            List<Segment2D> segment2Ds = segmentable2D.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return double.NaN;

            if (inverted)
            {
                Segment2D segment2D_Temp = segment2Ds[0];
                segment2Ds.Reverse();
                segment2Ds.Remove(segment2D_Temp);
                segment2Ds.Insert(0, segment2D_Temp);

                segment2Ds.ForEach(x => x.Reverse());
            }

            List<double> lengths = new List<double>();
            int index = -1;
            double distance_Min = double.MaxValue;
            for (int i = 0; i < segment2Ds.Count; i++)
            {
                Segment2D segment2D_Temp = segment2Ds[i];

                if (segment2D_Temp == null)
                {
                    lengths.Add(0);
                    continue;
                }

                lengths.Add(segment2D_Temp.GetLength());

                if (distance_Min < tolerance)
                    continue;

                double distance_Temp = segment2D_Temp.Distance(point2D_Closest);
                if (distance_Temp < distance_Min)
                {
                    index = i;
                    distance_Min = distance_Temp;
                }
            }

            if (index == -1)
                return double.NaN;

            double length = lengths.Sum();
            if (length == 0)
                return 0;

            double distance = lengths.GetRange(0, index).Sum();

            Segment2D segment2D = segment2Ds[index];
            distance += segment2D.Start.Distance(segment2D.Closest(point2D_Closest));

            double result = distance / length;
            if (inverted)
                result = 1 - result;

            return result;
        }
    }
}