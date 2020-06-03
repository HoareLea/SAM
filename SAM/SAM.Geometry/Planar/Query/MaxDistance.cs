using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static double MaxDistance(this IEnumerable<Point2D> point2Ds, out Point2D point2D_1, out Point2D point2D_2)
        {
            int aCount = point2Ds.Count();

            double aDistance = double.MinValue;
            point2D_1 = null;
            point2D_2 = null;

            for (int i = 0; i < aCount; i++)
            {
                Point2D point2D_Temp_1 = point2Ds.ElementAt(i);
                for (int j = 0; j < aCount - 1; j++)
                {
                    Point2D point2D_Temp_2 = point2Ds.ElementAt(j);
                    double aDistance_Temp = point2D_Temp_1.Distance(point2D_Temp_2);
                    if (aDistance_Temp > aDistance)
                    {
                        aDistance = aDistance_Temp;
                        point2D_1 = point2D_Temp_1;
                        point2D_2 = point2D_Temp_2;
                    }
                }
            }

            return aDistance;
        }
    }
}