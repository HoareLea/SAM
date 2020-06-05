using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Min(this Point2D point2D_1, Point2D point2D_2)
        {
            return new Point2D(System.Math.Min(point2D_1.X, point2D_2.X), System.Math.Min(point2D_1.Y, point2D_2.Y));
        }

        public static Point2D Min(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null || point2Ds.Count() == 0)
                return null;

            double aX = double.MaxValue;
            double aY = double.MaxValue;
            foreach (Point2D point2D in point2Ds)
            {
                if (aX > point2D.X)
                    aX = point2D.X;
                if (aY > point2D.Y)
                    aY = point2D.Y;
            }

            return new Point2D(aX, aY);
        }
    }
}