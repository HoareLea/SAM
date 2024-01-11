namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Math.Matrix3D Matrix3D(this Point3D point3D)
        {
            if (point3D == null)
            {
                return null;
            }

            Math.Matrix3D result = new Math.Matrix3D();
            result[0, 0] = point3D.X;
            result[1, 1] = point3D.Y;
            result[2, 2] = point3D.Z;

            return result;
        }

        public static Math.Matrix3D Matrix3D(this Vector3D vector3D)
        {
            if (vector3D == null)
            {
                return null;
            }

            Math.Matrix3D result = new Math.Matrix3D();
            result[0, 0] = vector3D.X;
            result[1, 1] = vector3D.Y;
            result[2, 2] = vector3D.Z;

            return result;
        }
    }
}