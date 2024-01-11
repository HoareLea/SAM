using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static bool Add(this List<Point2D> point2Ds, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (point2Ds == null || point2D == null)
            {
                return false;
            }

            foreach(Point2D point2D_Temp in point2Ds)
            {
                if(point2D_Temp == null)
                {
                    continue;
                }

                if(System.Math.Abs(point2D_Temp.X - point2D.X) > tolerance)
                {
                    continue;
                }

                if (System.Math.Abs(point2D_Temp.Y - point2D.Y) > tolerance)
                {
                    continue;
                }

                if(point2D_Temp.Distance(point2D) <= tolerance)
                {
                    return false;
                }
            }

            point2Ds.Add(point2D);
            return true;
        }

        public static bool Add(this List<Point2D> point2Ds, double x, double y)
        {
            if (point2Ds == null)
                return false;

            point2Ds.Add(new Point2D(x, y));
            return true;
        }
    }
}