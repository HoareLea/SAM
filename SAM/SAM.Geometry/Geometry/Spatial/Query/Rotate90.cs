namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Vector3D Rotate90(this Vector3D vector3D, bool clockwise = true)
        {
            if (vector3D == null)
                return null;

            if (clockwise)
                return new Vector3D(vector3D.Z, 0, -vector3D.X);

            return new Vector3D(-vector3D.Z, 0, vector3D.X);
        }
    }
}