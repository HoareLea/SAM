using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static bool Add(this List<Point2D> point2Ds, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (point2Ds == null || point2D == null)
                return false;

            Point2D point2D_Temp = point2Ds.Find(x => x.Distance(point2D) <= tolerance);
            if(point2D_Temp == null)
            {
                point2Ds.Add(point2D);
                return true;
            }

            return false;
        }
    }
}