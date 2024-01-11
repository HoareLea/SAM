using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static void ExtremePoints(this IEnumerable<Point2D> point2Ds, out Point2D point2D_1, out Point2D point2D_2)
        {
            point2D_1 = null;
            point2D_2 = null;

            if (point2Ds == null)
                return;

            int count = point2Ds.Count();

            if (count < 2)
                return;

            double distance_Max = double.MinValue;
            for (int i = 0; i < count - 1; i++)
            {
                Point2D point2D_1_Temp = point2Ds.ElementAt(i);
                for (int j = i + 1; j < count; j++)
                {
                    Point2D point2D_2_Temp = point2Ds.ElementAt(j);

                    double distance = point2D_1_Temp.Distance(point2D_2_Temp);
                    if (distance_Max < distance)
                    {
                        point2D_1 = point2D_1_Temp;
                        point2D_2 = point2D_2_Temp;
                        distance_Max = distance;
                    }
                }
            }
        }
    }
}