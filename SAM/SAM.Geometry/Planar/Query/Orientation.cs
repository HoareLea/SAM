using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Orientation Orientation(this Point2D point2D_1, Point2D point2D_2, Point2D point2D_3)
        {
            double aDeterminant = Determinant(point2D_1, point2D_2, point2D_3);

            if (aDeterminant == 0)
                return Geometry.Orientation.Collinear;

            if (aDeterminant > 0)
                return Geometry.Orientation.Clockwise;
            else
                return Geometry.Orientation.CounterClockwise;
        }

        public static Orientation Orientation(this IEnumerable<Point2D> point2Ds, bool convexHull = true)
        {
            if (point2Ds == null || point2Ds.Count() == 0)
                return Geometry.Orientation.Undefined;

            List<Point2D> point2Ds_Temp = new List<Point2D>(point2Ds);
            if (point2Ds_Temp == null || point2Ds_Temp.Count < 3)
                return Geometry.Orientation.Undefined;

            if (convexHull)
            {
                List<Point2D> point2Ds_ConvexHull = ConvexHull(point2Ds);

                //ConvexHull may have different orientation so needs to remove unnecessary points from existing point2Ds
                if (point2Ds_ConvexHull != null && point2Ds_ConvexHull.Count > 0)
                {
                    List<Point2D> point2Ds_ConvexHull_Temp = new List<Point2D>(point2Ds);
                    point2Ds_ConvexHull_Temp.RemoveAll(x => point2Ds_ConvexHull.Contains(x));
                    point2Ds_Temp.RemoveAll(x => point2Ds_ConvexHull_Temp.Contains(x));
                }
            }

            point2Ds_Temp.Add(point2Ds_Temp[0]);
            point2Ds_Temp.Add(point2Ds_Temp[1]);

            for (int i = 0; i < point2Ds_Temp.Count - 2; i++)
            {
                Orientation orientation = Orientation(point2Ds_Temp[i], point2Ds_Temp[i + 1], point2Ds_Temp[i + 2]);
                if (orientation != Geometry.Orientation.Collinear && orientation != Geometry.Orientation.Undefined)
                    return orientation;
            }

            return Geometry.Orientation.Undefined;
        }
    }
}