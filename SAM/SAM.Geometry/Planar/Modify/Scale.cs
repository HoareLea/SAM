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
    }
}