using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Intersect(this ISegmentable2D segmentable2D_1, ISegmentable2D segmentable2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D_1 == null || segmentable2D_2 == null)
                return false;

            List<Segment2D> segment2Ds_1 = segmentable2D_1.GetSegments();
            if (segment2Ds_1 == null)
                return false;

            List<Segment2D> segment2Ds_2 = segmentable2D_2.GetSegments();
            if (segment2Ds_2 == null)
                return false;

            HashSet<Point2D> point2Ds = new HashSet<Point2D>();
            foreach (Segment2D segment2D_1 in segment2Ds_1)
            {
                if (segment2D_1 == null)
                    continue;

                foreach (Segment2D segment2D_2 in segment2Ds_2)
                {
                    if (segment2D_2 == null)
                        continue;

                    Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, true, tolerance);
                    if (point2D_Intersection == null)
                        continue;

                    return true;
                }
            }

            return false;
        }

        public static bool Intersect(this ISegmentable2D segmentable2D, IEnumerable<Segment2D> segment2Ds)
        {
            if (segmentable2D == null || segment2Ds == null)
                return false;

            List<Segment2D> segment2Ds_Segmentable2D = segmentable2D.GetSegments();
            if (segment2Ds_Segmentable2D == null)
                return false;

            foreach (Segment2D segment2D in segment2Ds)
            {
                if (Intersect(segmentable2D, segment2D))
                    return true;
            }

            return false;
        }
    }
}