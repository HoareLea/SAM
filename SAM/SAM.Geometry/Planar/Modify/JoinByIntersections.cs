using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static bool JoinByIntersections(this List<Segment2D> segment2Ds, bool close = false, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (segment2Ds == null || segment2Ds.Count < 2)
                return false;

            List<Segment2D> result = new List<Segment2D>();
            result.Add(segment2Ds[0]);
            for (int i = 1; i < segment2Ds.Count; i++)
            {
                Segment2D segment_Previous = result.Last();
                Segment2D segment = segment2Ds[i];

                Point2D point2D_Intersection = segment_Previous.Intersection(segment, false, tolerance);
                if (point2D_Intersection == null)
                {
                    result.Add(new Segment2D(segment_Previous[0], segment[0]));
                    result.Add(new Segment2D(segment[0], segment[1]));
                }
                else
                {
                    result[result.Count - 1] = new Segment2D(segment_Previous[0], point2D_Intersection);
                    result.Add(new Segment2D(point2D_Intersection, segment[1]));
                }
            }

            if (close)
            {
                Segment2D segment_Previous = result[result.Count - 1];
                Segment2D segment = result[0];

                Point2D point2D_Intersection = segment.Intersection(segment_Previous, false, tolerance); 
                if (point2D_Intersection == null)
                {
                    result.Add(new Segment2D(segment_Previous[1], segment[0]));
                }
                else
                {
                    result[result.Count - 1] = new Segment2D(segment_Previous[0], point2D_Intersection); 
                    result[0] = new Segment2D(point2D_Intersection, segment[1]);
                }
            }

            segment2Ds.Clear();
            segment2Ds.AddRange(result);

            return true;
        }
    }
}