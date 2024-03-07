using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Polygon2D Simplify(this Circle2D circle2D, int density)
        {
            if (circle2D == null)
            {
                return null;
            }

            double factor = 2 * System.Math.PI / density;

            List<Point2D> point2Ds = new List<Point2D>();
            for (int i = 0; i <= density; i++)
            {
                double value = i * factor;

                Point2D point2D = circle2D.GetPoint2D(value);
                if (point2D == null)
                {
                    continue;
                }

                point2Ds.Add(point2D);
            }

            if (point2Ds == null || point2Ds.Count < 2)
            {
                return null;
            }

            return new Polygon2D(point2Ds);
        }
    }
}