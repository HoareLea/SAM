using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static void ExtremePoints(this IEnumerable<Point3D> point3Ds, out Point3D point3D_1, out Point3D point3D_2)
        {
            ExtremePoints(point3Ds, out point3D_1, out point3D_2, out double distance);
        }

        public static void ExtremePoints(this IEnumerable<Point3D> point3Ds, out Point3D point3D_1, out Point3D point3D_2, out double distance)
        {
            point3D_1 = null;
            point3D_2 = null;
            distance = double.NaN;

            if (point3Ds == null)
                return;

            int count = point3Ds.Count();

            if (count < 2)
                return;

            double distance_Max = double.MinValue;
            for (int i = 0; i < count - 1; i++)
            {
                Point3D point3D_1_Temp = point3Ds.ElementAt(i);
                for (int j = i + 1; j < count; j++)
                {
                    Point3D point3D_2_Temp = point3Ds.ElementAt(j);

                    double distance_Temp = point3D_1_Temp.Distance(point3D_2_Temp);
                    if (distance_Max < distance_Temp)
                    {
                        point3D_1 = point3D_1_Temp;
                        point3D_2 = point3D_2_Temp;
                        distance_Max = distance_Temp;
                    }
                }
            }

            if(distance_Max != double.MinValue)
            {
                distance = distance_Max;
            }
        }
    }
}