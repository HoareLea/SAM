using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Dictionary<Point2D, Segment2D> IntersectionDictionary(this Point2D point2D, Vector2D vector2D, ISegmentable2D segmentable2D, bool keepDirection, bool removeCollinear = true, bool sort = true, bool selfIntersection = false, double tolerance = Core.Tolerance.Distance)
        {
            return IntersectionDictionary(point2D, vector2D, new ISegmentable2D[] { segmentable2D }, keepDirection, removeCollinear, sort, selfIntersection, tolerance);
        }

        public static Dictionary<Point2D, Segment2D> IntersectionDictionary(this Point2D point2D, Vector2D vector2D, IEnumerable<ISegmentable2D> segmentable2Ds, bool keepDirection, bool removeCollinear = true, bool sort = true, bool selfIntersection = false, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null || vector2D == null || segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
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

                if (removeCollinear && segment2D.Collinear(segment2D_Temp))
                    continue;

                Point2D point2D_Intersection = segment2D.Intersection(segment2D_Temp, out point2D_closest_1, out point2D_closest_2, tolerance);
                if (point2D_Intersection == null)
                    continue;

                if (!selfIntersection && point2D_Intersection.Distance(point2D) < tolerance)
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

            if (sort)
            {
                List<Point2D> point2Ds = result.Keys.ToList();
                Modify.SortByDistance(point2Ds, point2D);

                Dictionary<Point2D, Segment2D> dictionary_Temp = new Dictionary<Point2D, Segment2D>();
                foreach (Point2D point2D_Temp in point2Ds)
                    dictionary_Temp[point2D_Temp] = result[point2D_Temp];

                result = dictionary_Temp;
            }

            return result;
        }
    }
}