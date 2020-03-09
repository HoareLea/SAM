using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void SortByDistance(this List<Point2D> point2Ds, Point2D point2D)
        {
            if (point2Ds == null || point2D == null)
                return;

            point2Ds.RemoveAll(x => x == null);

            point2Ds.Sort((x, y) => point2D.Distance(x).CompareTo(point2D.Distance(y)));
        }
    }
}
