using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool SelfIntersect(this ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment2D> segment2Ds = segmentable2D?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return false;

            PointGraph2D pointGraph2D = new PointGraph2D(segment2Ds, true, tolerance);
            for (int i = 0; i < pointGraph2D.Count; i++)
            {
                int count = pointGraph2D.ConnectionsCount(i);
                if (count > 2)
                    return true;
            }

            return false;
        }
    }
}