using System.Collections.Generic;

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
    }
}