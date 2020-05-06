using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Segment2D> ExtendOrTrim(this IEnumerable<Segment2D> segment2Ds, Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || polygon2D == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();

            if (segment2Ds.Count() == 0)
                return result;

            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null)
                    continue;

                Segment2D segment2D_Temp = ExtendOrTrim(segment2D, polygon2D, tolerance);
                if (segment2D_Temp == null)
                    continue;

                result.Add(segment2D_Temp);
            }

            return result;
        }

        public static Segment2D ExtendOrTrim(this Segment2D segment2D, Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null || polygon2D == null)
                return null;

            Point2D point2D_Segment2D_Start = segment2D.Start;
            Point2D point2D_Segment2D_End = segment2D.End;

            List<Point2D> point2Ds = Intersections(polygon2D, segment2D, tolerance);
            Vector2D vector2D_Start = TraceFirst(point2D_Segment2D_Start, segment2D.Direction.GetNegated(), polygon2D);
            if (vector2D_Start != null)
                point2Ds.Add(point2D_Segment2D_Start.GetMoved(vector2D_Start));

            Vector2D vector2D_End = TraceFirst(point2D_Segment2D_End, segment2D.Direction, polygon2D);
            if (vector2D_End != null)
                point2Ds.Add(point2D_Segment2D_End.GetMoved(vector2D_End));

            if (point2Ds.Count == 0)
                return null;

            ExtremePoints(point2Ds, out point2D_Segment2D_Start, out point2D_Segment2D_End);
            if (point2D_Segment2D_Start == null || point2D_Segment2D_End == null)
                return null;

            if (point2D_Segment2D_Start.Distance(segment2D.GetStart()) > point2D_Segment2D_End.Distance(segment2D.GetStart()))
            {
                Point2D point2D = point2D_Segment2D_Start;
                point2D_Segment2D_Start = point2D_Segment2D_End;
                point2D_Segment2D_End = point2D;
            }

            Modify.SortByDistance(point2Ds, point2D_Segment2D_End);

            int index;

            index = -1;
            for (int i = 0; i < point2Ds.Count - 1; i++)
            {
                Point2D point2D_Mid = point2Ds[i].Mid(point2Ds[i + 1]);
                if (polygon2D.Inside(point2D_Mid) || polygon2D.On(point2D_Mid))
                    break;

                index = i;
            }

            if (index != -1)
                point2Ds = point2Ds.GetRange(index + 1, point2Ds.Count - index - 1);

            if (point2Ds == null || point2Ds.Count < 2)
                return null;

            point2Ds.Reverse();

            index = -1;
            for (int i = 0; i < point2Ds.Count - 1; i++)
            {
                Point2D point2D_Mid = point2Ds[i].Mid(point2Ds[i + 1]);
                if (polygon2D.Inside(point2D_Mid) || polygon2D.On(point2D_Mid))
                    break;

                index = i;
            }

            if (point2Ds == null || point2Ds.Count < 2)
                return null;

            return new Segment2D(point2Ds.First(), point2Ds.Last());

            //Vector2D vector2D_Start = Query.TraceFirst(point2D_Segment2D_Start, segment2D.Direction.GetNegated(), polygon2D);
            //if(vector2D_Start == null)
            //    vector2D_Start = Query.TraceFirst(point2D_Segment2D_Start, segment2D.Direction, polygon2D);

            //Vector2D vector2D_End = Query.TraceFirst(point2D_Segment2D_End, segment2D.Direction, polygon2D);
            //if (vector2D_End == null)
            //    vector2D_End = Query.TraceFirst(point2D_Segment2D_End, segment2D.Direction.GetNegated(), polygon2D);

            //if (vector2D_Start == null && vector2D_End == null)
            //    return null;

            //Point2D point2D_Start = null;

            //if (vector2D_Start != null)
            //    point2D_Start = point2D_Segment2D_Start.GetMoved(vector2D_Start);

            //if (point2D_Start == null && polygon2D.On(point2D_Segment2D_Start, tolerance))
            //    point2D_Start = point2D_Segment2D_Start;

            //Point2D point2D_End = null;

            //if (vector2D_End != null)
            //    point2D_End = point2D_Segment2D_End.GetMoved(vector2D_End);

            //if (point2D_End == null && polygon2D.On(point2D_Segment2D_End, tolerance))
            //    point2D_End = point2D_Segment2D_End;

            //if (point2D_Start != null && point2D_End != null && point2D_Start.AlmostEquals(point2D_End, tolerance))
            //{
            //    Point2D point2D = point2D_Start;

            //    if (point2D.Distance(point2D_Segment2D_Start) < point2D.Distance(point2D_Segment2D_End))
            //        return new Segment2D(point2D, point2D_Segment2D_End);
            //    else
            //        return new Segment2D(point2D_Segment2D_Start, point2D);
            //}
            //else
            //{
            //    if (point2D_Start == null)
            //        point2D_Start = point2D_Segment2D_Start;

            // if (point2D_End == null) point2D_End = point2D_Segment2D_End;

            //    return new Segment2D(point2D_Start, point2D_End);
            //}
        }
    }
}