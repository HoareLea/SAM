using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static bool SimplifyByAngle(this List<Point3D> point3Ds, bool closed, double maxAngle = Core.Tolerance.Angle)
        {
            if (point3Ds == null || point3Ds.Count < 3)
                return false;

            List<Point3D> point3Ds_Temp = new List<Point3D>(point3Ds);

            bool removed = true;
            while (removed)
            {
                removed = false;

                int count = point3Ds_Temp.Count;

                for (int i = 0; i < count - 2; i++)
                {
                    Point3D point3D_1 = point3Ds_Temp[i];
                    Point3D point3D_2 = point3Ds_Temp[i + 1];
                    Point3D point3D_3 = point3Ds_Temp[i + 2];
                    if (Query.SmallestAngle(point3D_1, point3D_2, point3D_3) < maxAngle)
                    {
                        point3Ds_Temp.RemoveAt(i + 1);
                        if ((point3Ds_Temp.Count < 3 && closed) || (point3Ds_Temp.Count < 2))
                            return false;

                        removed = true;
                        break;
                    }
                }

                if (closed)
                {
                    count = point3Ds_Temp.Count;

                    Point3D point3D_1;
                    Point3D point3D_2;
                    Point3D point3D_3;

                    point3D_1 = point3Ds_Temp[count - 2];
                    point3D_2 = point3Ds_Temp[count - 1];
                    point3D_3 = point3Ds_Temp[0];
                    if (Query.SmallestAngle(point3D_1, point3D_2, point3D_3) < maxAngle)
                    {
                        point3Ds_Temp.RemoveAt(count - 1);
                        if (point3Ds_Temp.Count < 3)
                            return false;

                        removed = true;
                        continue;
                    }

                    point3D_1 = point3Ds_Temp[count - 1];
                    point3D_2 = point3Ds_Temp[0];
                    point3D_3 = point3Ds_Temp[1];
                    if (Query.SmallestAngle(point3D_1, point3D_2, point3D_3) < maxAngle)
                    {
                        point3Ds_Temp.RemoveAt(0);
                        if (point3Ds_Temp.Count < 3)
                            return false;

                        removed = true;
                        continue;
                    }
                }
            }

            point3Ds.Clear();
            point3Ds.AddRange(point3Ds_Temp);
            return true;
        }
    }
}