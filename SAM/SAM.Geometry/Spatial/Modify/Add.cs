using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static bool Add(this List<Point3D> point3Ds, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null || point3D == null)
                return false;

            Point3D point3D_Temp = point3Ds.Find(x => x.Distance(point3D) <= tolerance);
            if(point3D_Temp == null)
            {
                point3Ds.Add(point3D);
                return true;
            }

            return false;
        }
    }
}