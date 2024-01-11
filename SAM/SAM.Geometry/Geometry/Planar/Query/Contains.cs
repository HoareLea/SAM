using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Contains(this IEnumerable<Point2D> point2Ds, Point2D point2D, double tolerance)
        {
            if (point2Ds == null || point2D == null)
                return false;

            foreach (Point2D poin2D_Temp in point2Ds)
            {
                if (poin2D_Temp != null && poin2D_Temp.AlmostEquals(point2D, tolerance))
                    return true;
            }

            return false;
        }
    }
}