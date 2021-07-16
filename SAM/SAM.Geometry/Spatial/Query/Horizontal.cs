namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Horizontal(this Plane plane, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null)
                return false;

            return System.Math.Abs(System.Math.Abs(plane.Normal.DotProduct(Plane.WorldXY.AxisZ)) - 1) < tolerance;
        }

        public static bool Horizontal(this IClosedPlanar3D closedPlanar3D, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane = closedPlanar3D?.GetPlane();
            if(plane == null)
            {
                return false;
            }

            return Horizontal(plane, tolerance);
        }
    }
}