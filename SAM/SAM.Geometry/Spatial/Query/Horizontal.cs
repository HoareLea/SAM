
namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Horizontal(this Plane plane, double tolerance = Core.Tolerance.Angle)
        {
            if (plane == null)
                return false;

            return plane.BaseZ.SmallestAngle(Plane.Base.BaseZ) < tolerance;
        }
    }
}
