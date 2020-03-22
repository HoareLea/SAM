using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Union of the sets A and B, denoted A ∪ B, is the set of all objects that are a member of A, or B, or both. The union of {1, 2, 3} and {2, 3, 4} is the set {1, 2, 3, 4}
        public static List<Segment2D> IntersectionSegment2Ds(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            Polygon2D[] polygon2Ds = new Polygon2D[] { polygon2D_1, polygon2D_2 };
            List<Polygon2D> polygon2Ds_Temp = new PointGraph2D(polygon2Ds, true).GetPolygon2Ds();
            if (polygon2Ds_Temp == null || polygon2Ds_Temp.Count == 0)
                return null;

            List<Segment2D> segment2Ds_2 = new List<Segment2D>();
            foreach (Polygon2D polygon2D in polygon2Ds_Temp)
            {
                List<Segment2D> segment2Ds_Temp = polygon2D.GetSegments();
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    continue;

                segment2Ds_2.AddRange(segment2Ds_Temp);
            }

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D_2 in segment2Ds_2)
            {
                if (polygon2D_2 == null)
                    continue;

                Point2D point2D = segment2D_2.Mid();

                bool on = true;
                foreach (Polygon2D polygon2D in polygon2Ds)
                {
                    if (!Query.On(polygon2D, point2D, tolerance))
                    {
                        on = false;
                        break;
                    }
                }

                if (on)
                    result.Add(segment2D_2);
            }

            return result;

        }
    }
}
