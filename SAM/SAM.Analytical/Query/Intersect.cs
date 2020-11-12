using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Intersect(this Panel panel, Plane plane, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (plane == null)
                return false;

            Face3D face3D = panel?.GetFace3D();
            if (face3D == null)
                return false;

            PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(plane, face3D, tolerance_Angle, tolerance_Distance);
            if (planarIntersectionResult == null)
                return false;

            return planarIntersectionResult.Intersecting;
        }
    }
}