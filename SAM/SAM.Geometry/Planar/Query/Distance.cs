using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static double Distance(this ISegmentable2D segmentable2D_1, ISegmentable2D segmentable2D_2)
        {
            if (segmentable2D_1 == null || segmentable2D_2 == null)
                return double.NaN;

            List<Segment2D> segment2Ds_1 = segmentable2D_1.GetSegments();
            if (segment2Ds_1 == null || segment2Ds_1.Count == 0)
                return double.NaN;

            List<Segment2D> segment2Ds_2 = segmentable2D_2.GetSegments();
            if (segment2Ds_2 == null || segment2Ds_2.Count == 0)
                return double.NaN;

            double result = double.MaxValue;
            foreach (Segment2D segment2D_1 in segment2Ds_1)
            {
                foreach (Segment2D segment2D_2 in segment2Ds_2)
                {
                    double distance = segment2D_1.Distance(segment2D_2);
                    if (distance == 0)
                        return 0;

                    if (distance < result)
                        result = distance;
                }
            }

            return result;
        }

        public static double Distance(this ISegmentable2D segmentable2D_1, Point2D point2D)
        {
            if (segmentable2D_1 == null || point2D == null)
                return double.NaN;

            List<Segment2D> segment2Ds_1 = segmentable2D_1.GetSegments();
            if (segment2Ds_1 == null || segment2Ds_1.Count == 0)
                return double.NaN;

            double result = double.MaxValue;
            foreach (Segment2D segment2D_1 in segment2Ds_1)
            {
                double distance = segment2D_1.Distance(point2D);
                if (distance == 0)
                    return 0;

                if (distance < result)
                    result = distance;
            }

            return result;
        }
    }
}