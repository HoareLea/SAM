using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static List<Segment2D> Trim(this IEnumerable<Segment2D> segment2Ds, double distance, bool trim_Start = true, bool trim_End = true)
        {
            if (segment2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach(Segment2D segment2D in segment2Ds)
            {
                Segment2D segment2D_Temp = Trim(segment2D, distance, trim_Start, trim_End);
                if (segment2D_Temp != null)
                    result.Add(segment2D_Temp);
            }

            return result;
        }

        public static Segment2D Trim(this Segment2D segment2D, double distance, bool trim_Start = true, bool trim_End = true)
        {
            if (segment2D == null)
                return null;

            if (!trim_Start && !trim_End)
                return new Segment2D(segment2D);

            if (distance == 0)
                return new Segment2D(segment2D);

            Vector2D vector2D = segment2D.Direction * distance;

            Point2D point2D_Start = segment2D.Start;
            if(trim_Start)
                point2D_Start.Move(vector2D);

            vector2D.Negate();

            Point2D point2D_End = segment2D.End;
            if (trim_End)
                point2D_End.Move(vector2D);

            Segment2D result = new Segment2D(point2D_Start, point2D_End);
            if (!result.Direction.AlmostEqual(segment2D.Direction))
                return null;

            return result;
        }

        public static List<Segment2D> Trim(this Segment2D segment2D, Polygon2D polygon2D, double tolerance = Tolerance.MicroDistance)
        {
            if (segment2D == null || polygon2D == null)
                return null;

            List<Point2D> point2Ds = Query.Intersections(segment2D.End, segment2D.Direction, polygon2D, false, true, tolerance);
            if (point2Ds == null || point2Ds.Count <= 1)
                return null;

            Point2D point2D_1;
            Point2D point2D_2;

            Query.ExtremePoints(point2Ds, out point2D_1, out point2D_2);
            if (point2D_1 == null || point2D_2 == null)
                return null;

            Point2D point2D;

            Point2D point2D_Start = segment2D.Start;

            if (point2D_Start.Distance(point2D_1) < point2D_Start.Distance(point2D_2))
                point2D = point2D_1;
            else
                point2D = point2D_2;

            Modify.SortByDistance(point2Ds, point2D);

            List<Segment2D> result = new List<Segment2D>();
            for (int i = 0; i < point2Ds.Count - 1; i++)
            {
                Segment2D segment2D_Temp = new Segment2D(point2Ds[i], point2Ds[i + 1]);
                if (segment2D_Temp.Length > tolerance)
                {
                    if (!polygon2D.On(segment2D_Temp.Mid(), tolerance))
                        result.Add(segment2D_Temp);
                }

            }

            return result;
        }

        public static List<Segment2D> Trim(IEnumerable<Segment2D> segment2Ds, Polygon2D polygon2D, double tolerance = Tolerance.MicroDistance)
        {
            if (segment2Ds == null || polygon2D == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D_Temp in segment2Ds)
            {
                List<Segment2D> segment2Ds_Temp = Trim(segment2D_Temp, polygon2D, tolerance);
                if (segment2Ds_Temp != null)
                {
                    segment2Ds_Temp.RemoveAll(x => x == null);
                    result.AddRange(segment2Ds_Temp);
                }
                    
            }

            return result;
        }

    }
}
