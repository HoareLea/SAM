using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Max(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null || point2Ds.Count() == 0)
                return null;

            double aX = double.MinValue;
            double aY = double.MinValue;
            foreach (Point2D point in point2Ds)
            {
                if (aX < point.X)
                    aX = point.X;
                if (aY < point.Y)
                    aY = point.Y;
            }

            return new Point2D(aX, aY);
        }

        public static Point2D Max(this Point2D point2D_1, Point2D point2D_2)
        {
            return new Point2D(System.Math.Max(point2D_1.X, point2D_2.X), System.Math.Max(point2D_1.Y, point2D_2.Y));
        }
    }
}