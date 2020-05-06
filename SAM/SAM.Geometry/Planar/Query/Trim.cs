using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Segment2D> Trim(this IEnumerable<Segment2D> segment2Ds, double length, bool trim_Start = true, bool trim_End = true)
        {
            if (segment2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                Segment2D segment2D_Temp = Trim(segment2D, length, trim_Start, trim_End);
                if (segment2D_Temp != null)
                    result.Add(segment2D_Temp);
            }

            return result;
        }

        public static List<Segment2D> Trim(this IEnumerable<Segment2D> segment2Ds, double length, int index)
        {
            if (segment2Ds == null || (index != 0 && index != 1))
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                Segment2D segment2D_Temp = Trim(segment2D, length, index);
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
            if (trim_Start)
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

        public static Segment2D Trim(this Segment2D segment2D, double distance, int index)
        {
            if (segment2D == null || (index != 0 && index != 1))
                return null;

            if (distance == 0)
                return new Segment2D(segment2D);

            Vector2D vector2D = segment2D.Direction * distance;

            Point2D point2D_Start = segment2D.Start;
            Point2D point2D_End = segment2D.End;
            if (index == 0)
            {
                point2D_Start.Move(vector2D);
            }
            else if (index == 1)
            {
                vector2D.Negate();
                point2D_End.Move(vector2D);
            }

            Segment2D result = new Segment2D(point2D_Start, point2D_End);
            if (!result.Direction.AlmostEqual(segment2D.Direction))
                return null;

            return result;
        }

        public static List<Segment2D> Trim(this Segment2D segment2D, Polygon2D polygon2D, bool includeOnEdge = false, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null || polygon2D == null)
                return null;

            List<Point2D> point2Ds = Intersections(segment2D.End, segment2D.Direction, polygon2D, false, true, false, true, tolerance);
            if (point2Ds == null || point2Ds.Count <= 1)
                return null;

            Point2D point2D_1;
            Point2D point2D_2;

            ExtremePoints(point2Ds, out point2D_1, out point2D_2);
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
                if (segment2D_Temp.GetLength() > tolerance)
                {
                    Point2D point2D_Mid = segment2D_Temp.Mid();
                    if (!includeOnEdge && !polygon2D.On(point2D_Mid, tolerance) && polygon2D.Inside(point2D_Mid))
                        result.Add(segment2D_Temp);
                    else if (includeOnEdge && (polygon2D.On(point2D_Mid, tolerance) || polygon2D.Inside(point2D_Mid)))
                        result.Add(segment2D_Temp);
                }
            }

            return result;
        }

        public static List<Segment2D> Trim(IEnumerable<Segment2D> segment2Ds, Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || polygon2D == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D_Temp in segment2Ds)
            {
                List<Segment2D> segment2Ds_Temp = Trim(segment2D_Temp, polygon2D, true, tolerance);
                if (segment2Ds_Temp != null)
                {
                    segment2Ds_Temp.RemoveAll(x => x == null);
                    result.AddRange(segment2Ds_Temp);
                }
            }

            return result;
        }

        public static ISegmentable2D Trim(ISegmentable2D segmentable2D, double parameter, bool inverted = false)
        {
            if (segmentable2D == null || parameter < 0 || parameter > 1)
                return null;

            if (parameter == 0)
                return null;

            if (parameter == 1)
                return segmentable2D.Clone() as ISegmentable2D;

            List<Point2D> point2Ds = segmentable2D.GetPoints();
            if (point2Ds == null || point2Ds.Count < 2)
                return null;

            bool closed = segmentable2D is IClosed2D;

            if (inverted)
                Modify.Reverse(point2Ds, closed);

            List<Segment2D> segment2Ds = Create.Segment2Ds(point2Ds, closed);
            if (segment2Ds == null || segment2Ds.Count() == 0)
                return null;

            double length = segment2Ds.ConvertAll(x => x.GetLength()).Sum();
            if (double.IsNaN(length))
                return null;

            if (length == 0)
                return null;

            length = length * parameter;

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null)
                    continue;

                double length_Segment = segment2D.GetLength();
                if (length_Segment == 0)
                    continue;

                double length_Temp = length - length_Segment;
                if (length_Temp == 0)
                {
                    result.Add(segment2D);
                    return new Polyline2D(result);
                }

                if (length_Temp < 0)
                {
                    Point2D point2D = segment2D.GetPoint(length / segment2D.GetLength());
                    if (point2D == null)
                        return null;

                    result.Add(new Segment2D(segment2D[0], point2D));
                    return new Polyline2D(result);
                }

                result.Add(segment2D);
                length = length_Temp;
            }

            return new Polyline2D(result);
        }
    }
}