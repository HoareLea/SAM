using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Polygon2D MovePoint2D(this Polygon2D polygon2D, int index, Point2D point2D)
        {
            if(index < 0 || point2D == null)
            {
                return null;
            }

            List<Point2D> point2Ds = polygon2D?.Points;
            if(point2Ds == null || point2Ds.Count == 0)
            {
                return null;
            }

            if(index >= point2Ds.Count)
            {
                return null;
            }

            point2Ds[index] = new Point2D(point2D);

            return new Polygon2D(point2Ds);
        }

        public static Polygon2D MovePoint2D(this Polygon2D polygon2D, int index, Vector2D vector2D)
        {
            if(index < 0 || vector2D == null)
            {
                return null;
            }

            List<Point2D> point2Ds = polygon2D?.Points;
            if (point2Ds == null || point2Ds.Count == 0)
            {
                return null;
            }

            if (index >= point2Ds.Count)
            {
                return null;
            }

            point2Ds[index] = point2Ds[index].GetMoved(vector2D);

            return new Polygon2D(point2Ds);
        }

        public static Polygon2D MovePoint2D(this Polygon2D polygon2D, Point2D point2D_Old, Point2D point2D_New)
        {
            if (point2D_Old == null || point2D_New == null)
            {
                return null;
            }

            List<Point2D> point2Ds = polygon2D?.Points;
            if (point2Ds == null || point2Ds.Count == 0)
            {
                return null;
            }

            double distance = double.MaxValue;
            int index = -1;
            for (int i = 0; i < point2Ds.Count; i++)
            {
                double distance_Temp = point2D_Old.Distance(point2Ds[i]);
                if(distance_Temp < distance)
                {
                    distance = distance_Temp;
                    index = i;
                }
            }

            if(index == -1)
            {
                return null;
            }

            point2Ds[index] = new Point2D(point2D_New);

            return new Polygon2D(point2Ds);
        }
    }
}