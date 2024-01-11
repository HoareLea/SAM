using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static double MinDistance(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null)
                return double.NaN;

            int count = point2Ds.Count();

            double result = double.MaxValue;

            for (int i = 0; i < count - 1; i++)
            {
                Point2D point2D_1 = point2Ds.ElementAt(i);
                for (int j = i + 1; j < count; j++)
                {
                    Point2D point2D_2 = point2Ds.ElementAt(j);

                    double distance = point2D_1.Distance(point2D_2);
                    if (distance < result)
                        result = distance;
                }
            }

            return result;
        }
    }
}