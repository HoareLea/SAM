using SAM.Core;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool InRange(this BoundingBox3D boundingBox3D, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (boundingBox3D == null || point3D == null)
                return false;

            return boundingBox3D.Inside(point3D, true, tolerance);
        }

        public static bool InRange(this BoundingBox3D boundingBox3D_1, BoundingBox3D boundingBox3D_2, double tolerance = Tolerance.Distance)
        {
            if (boundingBox3D_1 == null || boundingBox3D_2 == null)
                return false;

            return boundingBox3D_1.Inside(boundingBox3D_2) || boundingBox3D_1.Intersect(boundingBox3D_2) || boundingBox3D_1.On(boundingBox3D_2.Min) || boundingBox3D_1.On(boundingBox3D_2.Max);
        }

        public static bool InRange(this BoundingBox3D boundingBox3D, Segment3D segment3D, double tolerance = Tolerance.Distance)
        {
            if (boundingBox3D == null || segment3D == null)
                return false;

            if (boundingBox3D.Inside(segment3D[0]) || boundingBox3D.Inside(segment3D[1]) || boundingBox3D.On(segment3D[0], tolerance) || boundingBox3D.On(segment3D[1], tolerance))
                return true;

            if (boundingBox3D.Intersect(segment3D, tolerance))
                return true;

            return false;
        }
    }
}