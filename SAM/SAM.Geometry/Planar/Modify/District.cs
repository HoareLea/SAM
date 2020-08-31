using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void District(this List<Point2D> point2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point2Ds == null)
                return;

            List<Point2D> point2Ds_Temp = new List<Point2D>(point2Ds);
            List<Point2D> point2Ds_Result = new List<Point2D>();
            while(point2Ds_Temp.Count != 0)
            {
                Point2D point2D = point2Ds_Temp.First();
                point2Ds_Result.Add(point2D);

                List<Point2D> point2Ds_ToRemove = point2Ds_Temp.FindAll(x => x.Distance(point2D) <= tolerance);
                foreach (Point2D point2D_ToRemove in point2Ds_ToRemove)
                    point2Ds_Temp.Remove(point2D_ToRemove);
            }

            point2Ds.Clear();
            point2Ds.AddRange(point2Ds_Result);
        }
    }
}