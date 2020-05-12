using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Overlap(this Plane plane_1, Plane plane_2, double tolerance = Core.Tolerance.Distance)
        {
            if (plane_1 == plane_2)
                return true;

            return plane_1.Coplanar(plane_2, tolerance);
        }

        public static bool Overlap(this Face3D face3D_1, Face3D face3D_2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face3D_1 == face3D_2)
                return true;

            Plane plane_1 = face3D_1?.GetPlane();
            if (plane_1 == null)
                return false;
            
            Plane plane_2 = face3D_2.GetPlane();
            if (plane_2 == null)
                return false;

            if (!plane_1.Overlap(plane_2, tolerance_Distance))
                return false;

            PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(face3D_1, face3D_2, tolerance_Angle, tolerance_Distance);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return false;

            List<ISAMGeometry2D> geometry2Ds = planarIntersectionResult.Geometry2Ds;
            if (geometry2Ds == null || geometry2Ds.Count == 0)
                return false;

            return geometry2Ds.Find(x => x is IClosed2D) != null;
        }
    }
}