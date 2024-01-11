using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool SelfIntersect(this ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.Distance)
        {
            return segmentable2D.SelfIntersectionPoint2Ds(1, tolerance)?.FirstOrDefault() != null;
        }
    }
}