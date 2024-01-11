using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Union of the sets A and B, denoted A ∪ B, is the set of all objects that are a member of A, or B, or both. The union of {1, 2, 3} and {2, 3, 4} is the set {1, 2, 3, 4}
        public static List<Segment2D> IntersectionSegment2Ds(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, bool sort = true, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            return IntersectionSegment2Ds(polygon2D_1, new Polygon2D[] { polygon2D_2 }, sort, tolerance);
        }

        public static List<Segment2D> IntersectionSegment2Ds(this Polygon2D polygon2D, IEnumerable<Polygon2D> polygon2Ds, bool sort = true, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon2D == null || polygon2Ds == null)
                return null;

            if (polygon2Ds.Count() == 0)
                return new List<Segment2D>();

            List<Polygon2D> polygon2Ds_Temp_1 = new List<Polygon2D>() { polygon2D };
            polygon2Ds_Temp_1.AddRange(polygon2Ds);

            List<Polygon2D> polygon2Ds_Temp_2 = Create.Polygon2Ds(polygon2Ds_Temp_1, tolerance);//new PointGraph2D(polygon2Ds_Temp_1, true).GetPolygon2Ds();
            if (polygon2Ds_Temp_2 == null || polygon2Ds_Temp_2.Count == 0)
                return null;

            List<Segment2D> segment2Ds_2 = new List<Segment2D>();
            foreach (Polygon2D polygon2D_Temp in polygon2Ds_Temp_2)
            {
                List<Segment2D> segment2Ds_Temp = polygon2D_Temp.GetSegments();
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    continue;

                segment2Ds_2.AddRange(segment2Ds_Temp);
            }

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D_2 in segment2Ds_2)
            {
                if (polygon2Ds == null)
                    continue;

                Point2D point2D = segment2D_2.Mid();
                if (!polygon2D.On(point2D, tolerance))
                    continue;

                bool on = false;
                foreach (Polygon2D polygon2D_Temp in polygon2Ds)
                {
                    if (On(polygon2D_Temp, point2D, tolerance))
                    {
                        on = true;
                        break;
                    }
                }

                if (on)
                    result.Add(segment2D_2);
            }

            if (sort)
            {
                Dictionary<int, List<Segment2D>> dictionary = new Dictionary<int, List<Segment2D>>();

                List<Segment2D> segment2Ds = polygon2D.GetSegments();
                foreach (Segment2D segment2D in result)
                {
                    Point2D point2D = segment2D.Mid();

                    int index = IndexOfClosestSegment2D(segment2Ds, point2D);
                    if (index == -1)
                        continue;

                    List<Segment2D> segment2Ds_Temp;
                    if (!dictionary.TryGetValue(index, out segment2Ds_Temp))
                    {
                        segment2Ds_Temp = new List<Segment2D>();
                        dictionary[index] = segment2Ds_Temp;
                    }

                    segment2Ds_Temp.Add(segment2D);
                }

                List<int> indexes = dictionary.Keys.ToList();
                indexes.Sort();

                result = new List<Segment2D>();
                foreach (int index in indexes)
                {
                    List<Segment2D> segment2Ds_Temp = dictionary[index];
                    Modify.SortByDistance(segment2Ds_Temp, segment2Ds[index].Start);
                    result.AddRange(segment2Ds_Temp);
                }
            }

            return result;
        }
    }
}