using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Centroid(this IEnumerable<Point2D> point2Ds)
        {
            double aArea = 0;
            double aX = 0;
            double aY = 0;

            for (int i = 0, j = point2Ds.Count() - 1; i < point2Ds.Count(); j = i++)
            {
                Point2D aPoint2D_1 = point2Ds.ElementAt(i);
                Point2D aPoint2D_2 = point2Ds.ElementAt(j);

                double aArea_Temp = aPoint2D_1.X * aPoint2D_2.Y - aPoint2D_2.X * aPoint2D_1.Y;
                aArea += aArea_Temp;
                aX += (aPoint2D_1.X + aPoint2D_2.X) * aArea_Temp;
                aY += (aPoint2D_1.Y + aPoint2D_2.Y) * aArea_Temp;
            }

            if (aArea == 0)
                return null;

            aArea *= 3;
            return new Point2D(aX / aArea, aY / aArea);
        }
    }
}