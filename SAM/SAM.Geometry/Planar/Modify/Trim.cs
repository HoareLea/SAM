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

        public static Segment2D Trim(this Segment2D segment2D, Polygon2D polygon2D, bool inside, double tolerance = Tolerance.MicroDistance)
        {
            if (segment2D == null || polygon2D == null)
                return null;

            Point2D point2D_Start = segment2D.Start;
            Point2D point2D_End = segment2D.End;

            Vector2D vector2D = segment2D.Direction;

            bool inside_Start = polygon2D.Inside(point2D_Start);
            bool inside_End = polygon2D.Inside(point2D_End);

            if (inside_Start != inside_End)
            {
                Point2D point2D = point2D_Start;
                if (inside_End == inside)
                {
                    point2D = point2D_End;
                    vector2D.Negate();
                }

                List<Point2D> point2Ds = Query.Intersections(point2D, vector2D, polygon2D, tolerance);
                if (point2Ds == null && point2Ds.Count == 0)
                    return new Segment2D(point2D_Start, point2D_End);

                Modify.SortByDistance(point2Ds, point2D);
                if (inside_End == inside)
                    point2D_End = point2Ds[0];
                else
                    point2D_Start = point2Ds[0];
            }
            else
            {
                List<Point2D> point2Ds;

                point2Ds = Query.Intersections(point2D_End, vector2D, polygon2D, tolerance);
                if (point2Ds != null && point2Ds.Count > 0)
                {
                    Modify.SortByDistance(point2Ds, point2D_End);
                    point2D_End = point2Ds[0];
                }

                vector2D.Negate();
                point2Ds = Query.Intersections(point2D_Start, vector2D, polygon2D, tolerance);
                if (point2Ds != null && point2Ds.Count > 0)
                {
                    Modify.SortByDistance(point2Ds, point2D_Start);
                    point2D_Start = point2Ds[0];
                }
            }

            return new Segment2D(point2D_Start, point2D_End);
        }

        public static List<Segment2D> Trim(IEnumerable<Segment2D> segment2Ds, Polygon2D polygon2D, bool inside, double tolerance = Tolerance.MicroDistance)
        {
            if (segment2Ds == null || polygon2D == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D_Temp in segment2Ds)
            {
                Segment2D segment2D = Trim(segment2D_Temp, polygon2D, inside, tolerance);
                result.Add(segment2D);
            }

            return result;
        }

    }
}
