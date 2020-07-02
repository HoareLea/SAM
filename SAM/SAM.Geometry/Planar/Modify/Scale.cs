using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void Scale(this List<Point2D> point2Ds, double factor)
        {
            if (point2Ds == null || point2Ds.Count < 3)
                return;

            point2Ds.ForEach(x => x.Scale(factor));
        }

        public static void Scale(this List<Point2D> point2Ds, Point2D point2D, double factor)
        {
            if (point2Ds == null)
                return;

            if (point2Ds.Count == 0)
                return;

            for (int i = 0; i < point2Ds.Count; i++)
                point2Ds[i].Scale(point2D, factor);
        }
    }
}