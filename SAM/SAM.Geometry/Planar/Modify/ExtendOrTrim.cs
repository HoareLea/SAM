using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static List<Segment2D> ExtendOrTrim(this IEnumerable<Segment2D> segment2Ds, Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || polygon2D == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();

            if (segment2Ds.Count() == 0)
                return result;

            foreach(Segment2D segment2D in segment2Ds)
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

            Vector2D vector2D_Start = Query.TraceFirst(point2D_Segment2D_Start, segment2D.Direction.GetNegated(), polygon2D);
            if(vector2D_Start == null)
                vector2D_Start = Query.TraceFirst(point2D_Segment2D_Start, segment2D.Direction, polygon2D);

            Vector2D vector2D_End = Query.TraceFirst(point2D_Segment2D_End, segment2D.Direction, polygon2D);
            if (vector2D_End == null)
                vector2D_End = Query.TraceFirst(point2D_Segment2D_End, segment2D.Direction.GetNegated(), polygon2D);

            if (vector2D_Start == null && vector2D_End == null)
                return null;

            Point2D point2D_Start = null;

            if (vector2D_Start != null)
                point2D_Start = point2D_Segment2D_Start.GetMoved(vector2D_Start);

            if (point2D_Start == null && polygon2D.On(point2D_Segment2D_Start, tolerance))
                point2D_Start = point2D_Segment2D_Start;

            Point2D point2D_End = null;

            if (vector2D_End != null)
                point2D_End = point2D_Segment2D_End.GetMoved(vector2D_End);

            if (point2D_End == null && polygon2D.On(point2D_Segment2D_End, tolerance))
                point2D_End = point2D_Segment2D_End;

            if (point2D_Start != null && point2D_End != null && point2D_Start.AlmostEquals(point2D_End, tolerance))
            {
                Point2D point2D = point2D_Start;

                if (point2D.Distance(point2D_Segment2D_Start) < point2D.Distance(point2D_Segment2D_End))
                    return new Segment2D(point2D, point2D_Segment2D_End);
                else
                    return new Segment2D(point2D_Segment2D_Start, point2D);
            }
            else
            {
                if (point2D_Start == null)
                    point2D_Start = point2D_Segment2D_Start;

                if (point2D_End == null)
                    point2D_End = point2D_Segment2D_End;

                return new Segment2D(point2D_Start, point2D_End);
            }
        }
    }
}
