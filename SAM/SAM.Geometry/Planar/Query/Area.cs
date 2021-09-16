using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static double Area(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null)
            {
                return double.NaN;
            }
                
            List<Point2D> point2Ds_Temp = new List<Point2D>(point2Ds);
            if (point2Ds_Temp.FindIndex(x => x == null) != -1)
            {
                return double.NaN;
            }

            int count = point2Ds_Temp.Count;

            if (count < 3)
            {
                return 0;
            }

            if (!point2Ds_Temp[count - 1].Equals(point2Ds_Temp[0]))
                point2Ds_Temp.Add(point2Ds_Temp[0]);

            return System.Math.Abs(point2Ds.Take(count - 1).Select((p, i) => (point2Ds_Temp[i + 1].X - p.X) * (point2Ds_Temp[i + 1].Y + p.Y)).Sum() / 2);
        }
    }
}