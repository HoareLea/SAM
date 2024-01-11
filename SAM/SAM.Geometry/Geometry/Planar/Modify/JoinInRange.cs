using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static bool JoinInRange(this List<Segment2D> segment2Ds, bool close = false, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (segment2Ds == null || segment2Ds.Count < 2)
                return false;

            List<Segment2D> result = new List<Segment2D>();
            result.Add(segment2Ds[0]);
            for (int i = 1; i < segment2Ds.Count; i++)
            {
                Segment2D segment2D_Previous = result.Last();
                Segment2D segment2D = segment2Ds[i];

                Point2D point2D_Previous = segment2D_Previous[1];
                Point2D point2D = segment2D[0];

                if(point2D_Previous.Distance(point2D) <= tolerance)
                {
                    Point2D point2D_Mid = point2D_Previous.Mid(point2D);
                    result[result.Count - 1] = new Segment2D(segment2D_Previous[0], point2D_Mid);
                    result.Add(new Segment2D(point2D_Mid, segment2D[1]));
                }
                else
                {
                    result.Add(new Segment2D(segment2D_Previous[0], segment2D[0]));
                    result.Add(new Segment2D(segment2D[0], segment2D[1]));
                }
            }

            if (close)
            {
                Segment2D segment2D_Previous = result[result.Count - 1];
                Segment2D segment2D = result[0];

                Point2D point2D_Previous = segment2D_Previous[1];
                Point2D point2D = segment2D[0];

                if (point2D_Previous.Distance(point2D) <= tolerance)
                {
                    Point2D point2D_Mid = point2D_Previous.Mid(point2D);
                    result[result.Count - 1] = new Segment2D(segment2D_Previous[0], point2D_Mid);
                    result[0] = new Segment2D(point2D_Mid, segment2D[1]);
                }
            }

            segment2Ds.Clear();
            segment2Ds.AddRange(result);

            return true;
        }
    }
}