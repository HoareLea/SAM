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

        public static bool SimplifyByAngle(this List<Segment2D> segment2Ds, double maxAngle = Core.Tolerance.Angle)
        {
            if (segment2Ds == null)
                return false;

            List<PointGraph2D> pointGraph2Ds = new PointGraph2D(segment2Ds, false).Split();
            if (pointGraph2Ds == null || pointGraph2Ds.Count == 0)
                return false;

            List<Polyline2D> polyline2Ds = new PointGraph2D(segment2Ds, false).GetPolyline2Ds();
            if (polyline2Ds == null || polyline2Ds.Count == 0)
                return false;

            List<Segment2D> segment2Ds_Temp = new List<Segment2D>();
            foreach (Polyline2D polyline2D in polyline2Ds)
            {
                List<Point2D> point2Ds = polyline2D?.GetPoints();
                if (point2Ds == null)
                    continue;

                SimplifyBySAM_Angle(point2Ds, polyline2D.IsClosed(), maxAngle);

                segment2Ds_Temp.AddRange(Create.Segment2Ds(point2Ds, polyline2D.IsClosed()));
            }

            segment2Ds.Clear();
            segment2Ds.AddRange(segment2Ds_Temp);
            return true;
        }
    }
}