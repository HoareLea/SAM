using System.Linq;
using System.Collections.Generic;


namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<int> IndexesOfClosest(this IEnumerable<Segment2D> segment2Ds, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || point2D == null)
                return null;

            List<int> result = new List<int>();
            for(int i=0; i < segment2Ds.Count(); i++)
            {
                Segment2D segment2D = segment2Ds.ElementAt(i);
                if (segment2D == null)
                    continue;

                double distance = segment2D.Closest(point2D).Distance(point2D);
                if (distance <= tolerance)
                    result.Add(i);
            }

            return result;
        }

        public static List<int> IndexesOfClosest(this ISegmentable2D segmentable2D, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return IndexesOfClosest(segmentable2D?.GetSegments(), point2D, tolerance);
        }
    }
}
