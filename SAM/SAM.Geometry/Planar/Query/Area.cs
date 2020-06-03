using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static double Area(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null)
                return double.NaN;

            if (point2Ds.Count() < 3)
                return 0;

            List<Point2D> point2DList = new List<Point2D>(point2Ds);
            if (!point2DList[point2DList.Count - 1].Equals(point2DList[0]))
                point2DList.Add(point2DList[0]);

            return System.Math.Abs(point2Ds.Take(point2DList.Count - 1).Select((p, i) => (point2DList[i + 1].X - p.X) * (point2DList[i + 1].Y + p.Y)).Sum() / 2);
        }
    }
}