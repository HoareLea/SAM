using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool On(this ISegmentable2D segmentable2D, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D == null || point2D == null)
                return false;

            return On(segmentable2D.GetSegments(), point2D, tolerance);
        }

        public static bool On(this IEnumerable<Segment2D> segment2Ds, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || point2D == null)
                return false;

            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null)
                    continue;

                if (segment2D.On(point2D, tolerance))
                    return true;
            }

            return false;
        }

        public static bool On(this IEnumerable<ISegmentable2D> segmentable2Ds, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null || point2D == null)
                return false;

            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
                if (On(segmentable2D, point2D, tolerance))
                    return true;

            return false;
        }
    }
}