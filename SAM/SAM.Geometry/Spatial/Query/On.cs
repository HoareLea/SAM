using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool On(this ISegmentable3D segmentable3D, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable3D == null || point3D == null)
                return false;

            return On(segmentable3D.GetSegments(), point3D, tolerance);
        }

        public static bool On(this IEnumerable<Segment3D> segment3Ds, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3Ds == null || point3D == null)
                return false;

            foreach (Segment3D segment3D in segment3Ds)
            {
                if (segment3D == null)
                    continue;

                if (segment3D.On(point3D, tolerance))
                    return true;
            }

            return false;
        }

        public static bool On(this IEnumerable<ISegmentable3D> segmentable3Ds, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable3Ds == null || point3D == null)
                return false;

            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                if (On(segmentable3D, point3D, tolerance))
                    return true;

            return false;
        }
    }
}