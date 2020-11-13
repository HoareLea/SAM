namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Between(this Plane plane_1, Plane plane_2, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane_1 == null || plane_2 == null || point3D == null)
                return false;

            return (plane_1.Above(point3D, tolerance) && plane_2.Below(point3D, tolerance)) || (plane_2.Above(point3D, tolerance) && plane_1.Below(point3D, tolerance));
        }
    }
}