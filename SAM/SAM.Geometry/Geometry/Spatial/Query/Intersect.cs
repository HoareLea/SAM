using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Intersect(this Plane plane, BoundingBox3D boundingBox3D, double tolerance = Core.Tolerance.Distance)
        {
            if(plane == null)
            {
                return false;
            }

            List<Point3D> point3Ds = boundingBox3D?.GetPoints();
            if(point3Ds == null || point3Ds.Count == 0)
            {
                return false;
            }

            bool above = plane.Above(point3Ds[0], tolerance);
            for(int i = 1; i < point3Ds.Count; i++)
            {
                bool above_Temp = plane.Above(point3Ds[i], tolerance);
                if(above != above_Temp)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Intersect(this Face3D face3D, Point3D point3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if(face3D == null || point3D == null || vector3D == null)
            {
                return false;
            }

            PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, point3D, vector3D, tolerance);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
            {
                return false;
            }

            Point3D point3D_Intersection = planarIntersectionResult.GetGeometry3Ds<Point3D>()?.FirstOrDefault();

            return point3D_Intersection != null && point3D_Intersection.IsValid();
        }
    }
}