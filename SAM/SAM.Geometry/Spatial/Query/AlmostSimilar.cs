namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool AlmostSimilar(this Vector3D vector3D_1, Vector3D vector3D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (vector3D_1 == vector3D_2)
                return true;

            if (vector3D_1 == null || vector3D_2 == null)
                return false;

            Vector3D vector3D_3 = new Vector3D(vector3D_2);
            vector3D_3.Negate();

            return vector3D_1.AlmostEqual(vector3D_2, tolerance) || vector3D_1.AlmostEqual(vector3D_3, tolerance);
        }
    }
}
