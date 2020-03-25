using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    { 
        public static Dictionary<Point2D, Segment2D> IntersectionDictionary(this Point2D point2D, Vector2D vector2D, ISegmentable2D segmentable2D, bool keepDirection, bool removeColinear = true, double tolerance = Core.Tolerance.MicroDistance)
        {
            return IntersectionDictionary(point2D, vector2D, new ISegmentable2D[] { segmentable2D }, keepDirection, removeColinear, tolerance);
        }

        public static Dictionary<Point2D, Segment2D> IntersectionDictionary(this Point2D point2D, Vector2D vector2D, IEnumerable<ISegmentable2D> segmentable2Ds, bool keepDirection, bool removeColinear = true, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point2D == null || vector2D == null || segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach(ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    continue;

                segment2Ds.AddRange(segment2Ds_Temp);
            }

            Dictionary<Point2D, Segment2D> result = new Dictionary<Point2D, Segment2D>();

            Segment2D segment2D = new Segment2D(point2D, vector2D);
            foreach (Segment2D segment2D_Temp in segment2Ds)
            {
                Point2D point2D_closest_1;
                Point2D point2D_closest_2;

                if (removeColinear && segment2D.Colinear(segment2D_Temp))
                    continue;

                Point2D point2D_Intersection = segment2D.Intersection(segment2D_Temp, out point2D_closest_1, out point2D_closest_2);
                if (point2D_Intersection == null)
                    continue;

                if (point2D_closest_1 == null || point2D_closest_2 == null)
                {
                    result[point2D_Intersection] = segment2D_Temp;
                    continue;
                }

                if (point2D_closest_2 != null && segment2D_Temp.Distance(point2D_Intersection) < tolerance)
                {
                    if (keepDirection)
                        if (!vector2D.Unit.AlmostEqual(new Vector2D(point2D_closest_1, point2D_closest_2).Unit, tolerance))
                            continue;

                    result[point2D_Intersection] = segment2D_Temp;
                }

            }

            return result;
        }

    }
}
