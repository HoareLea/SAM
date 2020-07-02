using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> Clone(this IEnumerable<Point2D> point2Ds)
        {
            List<Point2D> result = new List<Point2D>();
            foreach (Point2D point2D in point2Ds)
                result.Add(new Point2D(point2D));

            return result;
        }
    }
}