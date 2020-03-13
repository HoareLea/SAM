using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Point2D(this ISegmentable2D segmentable2D, double parameter)
        {
            if (parameter < 0 || parameter > 1 || segmentable2D == null)
                return null;

            List<Segment2D> segment2Ds = segmentable2D.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            if (parameter == 0)
                return segment2Ds.First().Start;

            if (parameter == 1)
                return segment2Ds.Last().End;

            double length = segment2Ds.ConvertAll(x => x.GetLength()).Sum();
            if (double.IsNaN(length))
                return null;

            length = length * parameter;

            if (length == 0)
                return segment2Ds.First().Start;

            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null)
                    continue;

                double length_Segment = segment2D.GetLength();
                if (length_Segment == 0)
                    continue;

                double length_Temp = length - length_Segment;
                if (length_Temp == 0)
                    return segment2D.End;

                if (length_Temp < 0)
                    return segment2D.GetPoint(length / segment2D.GetLength());
                    

                length = length_Temp;
            }

            return segment2Ds.Last().End;
        }
    }
}
