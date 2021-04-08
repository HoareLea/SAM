using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D SelfIntersectionPoint2D(this ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D == null)
                return null;

            return SelfIntersectionPoint2Ds(segmentable2D, 1, tolerance)?.FirstOrDefault();
        }
    }
}