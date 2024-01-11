namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Perpendicular(this Vector3D vector3D_1, Vector3D vector3D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (vector3D_1 == vector3D_2)
                return false;

            if (vector3D_1 == null || vector3D_2 == null)
                return false;

            return System.Math.Abs(vector3D_1.GetNormalized().DotProduct(vector3D_2.GetNormalized())) <= tolerance;
        }

        public static bool Perpendicular(this Plane plane_1, Plane plane_2, double tolerance = Core.Tolerance.Distance)
        {
            return Perpendicular(plane_1?.Normal, plane_2?.Normal, tolerance);
        }
    }
}