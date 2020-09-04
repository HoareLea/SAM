using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool SimplifyBySAM_Angle(this List<Point2D> point2Ds, bool closed, double maxAngle = Core.Tolerance.Angle)
        {
            if (point2Ds == null || point2Ds.Count < 3)
                return false;

            List<Point2D> point2Ds_Temp = new List<Point2D>(point2Ds);

            bool removed = true;
            while (removed)
            {
                removed = false;

                int count = point2Ds_Temp.Count;

                for (int i = 0; i < count - 2; i++)
                {
                    Point2D point2D_1 = point2Ds_Temp[i];
                    Point2D point2D_2 = point2Ds_Temp[i + 1];
                    Point2D point2D_3 = point2Ds_Temp[i + 2];
                    if (SmallestAngle(point2D_1, point2D_2, point2D_3) < maxAngle)
                    {
                        point2Ds_Temp.RemoveAt(i + 1);
                        if ((point2Ds_Temp.Count < 3 && closed) || (point2Ds_Temp.Count < 2))
                            return false;

                        removed = true;
                        break;
                    }
                }

                if (closed)
                {
                    count = point2Ds_Temp.Count;

                    Point2D point2D_1;
                    Point2D point2D_2;
                    Point2D point2D_3;

                    point2D_1 = point2Ds_Temp[count - 2];
                    point2D_2 = point2Ds_Temp[count - 1];
                    point2D_3 = point2Ds_Temp[0];
                    if (SmallestAngle(point2D_1, point2D_2, point2D_3) < maxAngle)
                    {
                        point2Ds_Temp.RemoveAt(count - 1);
                        if (point2Ds_Temp.Count < 3)
                            return false;

                        removed = true;
                        continue;
                    }

                    point2D_1 = point2Ds_Temp[count - 1];
                    point2D_2 = point2Ds_Temp[0];
                    point2D_3 = point2Ds_Temp[1];
                    if (SmallestAngle(point2D_1, point2D_2, point2D_3) < maxAngle)
                    {
                        point2Ds_Temp.RemoveAt(0);
                        if (point2Ds_Temp.Count < 3)
                            return false;

                        removed = true;
                        continue;
                    }
                }
            }

            point2Ds.Clear();
            point2Ds.AddRange(point2Ds_Temp);
            return true;
        }

        public static Polygon2D SimplifyBySAM_Angle(this Polygon2D polygon2D, double maxAngle = Core.Tolerance.Angle)
        {
            if (polygon2D == null)
                return null;

            List<Point2D> point2Ds = polygon2D.Points;
            if (point2Ds == null || point2Ds.Count < 3)
                return new Polygon2D(polygon2D);

            SimplifyBySAM_Angle(point2Ds, true, maxAngle);
            if (point2Ds == null || point2Ds.Count < 3)
                return new Polygon2D(polygon2D);

            return new Polygon2D(point2Ds);
        }

        public static Polyline2D SimplifyBySAM_Angle(this Polyline2D polyline2D, double maxAngle = Core.Tolerance.Angle)
        {
            if (polyline2D == null)
                return null;

            List<Point2D> point2Ds = polyline2D.Points;
            if (point2Ds == null || point2Ds.Count < 3)
                return new Polyline2D(polyline2D);

            SimplifyBySAM_Angle(point2Ds, true, maxAngle);
            if (point2Ds == null || point2Ds.Count < 3)
                return new Polyline2D(polyline2D);

            return new Polyline2D(point2Ds, polyline2D.IsClosed());
        }
    }
}