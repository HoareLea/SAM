namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Parallel(this Vector3D vector3D_1, Vector3D vector3D_2, double tolerance = Core.Tolerance.Angle)
        {
            if (vector3D_1 == vector3D_2)
                return true;

            if (vector3D_1 == null || vector3D_2 == null)
                return false;

            double d = vector3D_1.Unit.DotProduct(vector3D_2.Unit);
            return System.Math.Abs(d) >= System.Math.Cos(tolerance);
        }

        public static bool Parallel(this Segment3D segment3D_1, Segment3D segment3D_2, double tolerance = Core.Tolerance.Angle)
        {
            return Parallel(segment3D_1?.Direction, segment3D_2?.Direction, tolerance);
        }
    }
}