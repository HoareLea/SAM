using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void Move(this List<Point2D> point2Ds, Vector2D vector2D)
        {
            if (point2Ds == null || vector2D == null)
                return;

            if (point2Ds.Count == 0)
                return;

            for (int i = 0; i < point2Ds.Count; i++)
                point2Ds[i].Move(vector2D);
        }
    }
}