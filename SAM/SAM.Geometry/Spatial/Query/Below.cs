namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Below(this Plane plane, Point3D point3D, double tolerance = 0)
        {
            if (point3D == null || plane == null)
                return false;

            return !Above(plane, point3D, tolerance) && !plane.On(point3D, tolerance);
        }
    }
}