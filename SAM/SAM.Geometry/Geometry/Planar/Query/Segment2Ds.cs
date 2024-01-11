using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Segment2D> Segment2Ds(this IEnumerable<Segment2D> segment2Ds, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || point2D == null)
                return null;

            HashSet<Segment2D> segment2Ds_Temp = new HashSet<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds_Temp)
            {
                if (segment2D[0].Distance(point2D) < tolerance)
                {
                    segment2Ds_Temp.Add(segment2D);
                    continue;
                }

                if (segment2D[1].Distance(point2D) < tolerance)
                {
                    segment2Ds_Temp.Add(segment2D);
                    continue;
                }
            }

            return segment2Ds_Temp.ToList();
        }

        public static List<Segment2D> Segment2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds = segmentable2D?.GetSegments();
                if (segment2Ds == null)
                    continue;

                foreach (Segment2D segment2D in segment2Ds)
                {
                    if (segment2D == null)
                        continue;

                    result.Add(segment2D);
                }
            }

            return result;
        }
    }
}