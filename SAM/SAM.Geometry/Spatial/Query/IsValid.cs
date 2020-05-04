namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool IsValid(this Vector3D vector3D)
        {
            if (vector3D == null)
                return false;

            double x = vector3D.X;
            double y = vector3D.Y;
            double z = vector3D.Z;

            if (double.IsNaN(x) || double.IsNaN(x) || double.IsNaN(x))
                return false;

            if (x == 0 && y == 0 && z == 0)
                return false;

            return true;
        }
    }
}