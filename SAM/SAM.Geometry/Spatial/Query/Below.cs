namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Below(this Plane plane, Point3D point3D, double tolerance = 0)
        {
            if (point3D == null)
                return false;

            Vector3D normal = plane?.Normal;
            if (normal == null)
                return false;

            Point3D origin = plane.Origin;
            if (origin == null)
                return false;

            return (normal.X * (point3D.X - origin.X)) + (normal.Y * (point3D.Y - origin.Y)) + (normal.Z * (point3D.Z - origin.Z)) > 0 - tolerance;
        }
    }
}