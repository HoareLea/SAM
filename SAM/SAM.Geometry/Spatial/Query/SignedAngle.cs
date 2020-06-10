namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double SignedAngle(this Vector3D vector3D_1, Vector3D vector3D_2, Vector3D normal)
        {
            if (vector3D_1 == null || vector3D_2 == null || normal == null)
                return double.NaN;

            double angle = vector3D_1.Angle(vector3D_2);

            Vector3D crossProduct = vector3D_1.CrossProduct(vector3D_2);
            if (crossProduct.DotProduct(normal) < 0)
                return -angle;
            else
                return angle;
        }
    }
}