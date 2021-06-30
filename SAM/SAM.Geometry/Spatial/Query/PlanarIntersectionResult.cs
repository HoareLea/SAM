using SAM.Core;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static PlanarIntersectionResult PlanarIntersectionResult(this Plane plane, Segment3D segment3D, double tolerance = Tolerance.Distance)
        {
            return Create.PlanarIntersectionResult(plane, segment3D, tolerance);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(this Plane plane, IClosedPlanar3D closedPlanar3D, double tolerance = Tolerance.Distance)
        {
            if (closedPlanar3D is Face3D)
                return PlanarIntersectionResult(plane, (Face3D)closedPlanar3D, tolerance);

            return Create.PlanarIntersectionResult(plane, closedPlanar3D, tolerance);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(this Plane plane, Face3D face3D, double tolerance = Tolerance.Distance)
        {
            return Create.PlanarIntersectionResult(plane, face3D, tolerance);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(this Plane plane_1, Plane plane_2, double tolerance = Tolerance.Distance)
        {
            return Create.PlanarIntersectionResult(plane_1, plane_2, tolerance);
        }
    }
}