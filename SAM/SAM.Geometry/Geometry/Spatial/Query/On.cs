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

        public static bool On(this Plane plane, ISegmentable3D segmentable3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || segmentable3D == null)
                return false;

            List<Point3D> point3Ds = segmentable3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
                return false;

            foreach(Point3D point3D in point3Ds)
            {
                if (point3D == null)
                    continue;

                if (!plane.On(point3D, tolerance))
                    return false;
            }

            return true;
        }

        public static bool On(this Plane plane, IPlanar3D planar3D, double tolerance = Core.Tolerance.Distance)
        {
            if(plane == null || planar3D == null)
            {
                return false;
            }

            Plane plane_Planar3D = planar3D.GetPlane();
            if(plane_Planar3D == null)
            {
                return false;
            }

            return Coplanar(plane, plane_Planar3D, tolerance) && plane.On(plane_Planar3D.Origin, tolerance);
        }
    }
}