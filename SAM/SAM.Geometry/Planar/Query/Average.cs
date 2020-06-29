using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Average(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null || point2Ds.Count() == 0)
                return null;

            int count = 0;

            double x = 0;
            double y = 0;
            foreach(Point2D point2D in point2Ds)
            {
                count++;
                x += point2D.X;
                y += point2D.Y;
            }

            return new Point2D(x / count, y / count);
        }
    }
}