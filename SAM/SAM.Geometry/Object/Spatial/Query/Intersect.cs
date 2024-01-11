using SAM.Geometry.Spatial;
using System.Linq;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static bool Intersect(this Plane plane, IFace3DObject face3DObject, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (plane == null)
                return false;

            Face3D face3D = face3DObject?.Face3D;
            if (face3D == null)
                return false;

            PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D, tolerance_Angle, tolerance_Distance);
            if (planarIntersectionResult == null)
                return false;

            return planarIntersectionResult.Intersecting;
        }

        public static bool Intersect(this IFace3DObject face3DObject, Point3D point3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            return Geometry.Spatial.Query.Intersect(face3DObject?.Face3D, point3D, vector3D, tolerance);
        }
    }
}