using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void Mirror(this List<Point2D> point2Ds, Segment2D segment2D)
        {
            if (point2Ds == null || segment2D == null)
                return;

            if (point2Ds.Count() == 0)
                return;

            for (int i = 0; i < point2Ds.Count; i++)
                point2Ds[i].Mirror(segment2D);
        }

        public static void Mirror(this List<Point2D> point2Ds, Point2D point2D)
        {
            if (point2Ds == null || point2D == null)
                return;

            if (point2Ds.Count() == 0)
                return;

            for (int i = 0; i < point2Ds.Count; i++)
                point2Ds[i].Mirror(point2D);
        }
    }
}