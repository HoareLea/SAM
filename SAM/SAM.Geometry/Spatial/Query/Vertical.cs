namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Vertical(this Plane plane, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null)
                return false;

            return System.Math.Abs(plane.Normal.DotProduct(Plane.WorldXY.AxisZ)) < tolerance;
        }
    }
}