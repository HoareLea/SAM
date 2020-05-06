using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Segment2D> Extend(this IEnumerable<Segment2D> segment2Ds, Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
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

                Segment2D segment2D_Temp = Extend(segment2D, polygon2D, tolerance);
                if (segment2D_Temp == null)
                    continue;

                result.Add(segment2D_Temp);
            }

            return result;
        }

        public static Segment2D Extend(this Segment2D segment2D, Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null || polygon2D == null)
                return null;

            Point2D point2D_Segment2D_Start = segment2D.Start;
            Point2D point2D_Segment2D_End = segment2D.End;

            Vector2D vector2D;

            Point2D point2D_Start = null;

            vector2D = TraceFirst(point2D_Segment2D_Start, segment2D.Direction.GetNegated(), polygon2D);
            if (vector2D != null)
                point2D_Start = point2D_Segment2D_Start.GetMoved(vector2D);

            if (point2D_Start == null && polygon2D.On(point2D_Segment2D_Start, tolerance))
                point2D_Start = point2D_Segment2D_Start;

            Point2D point2D_End = null;

            vector2D = TraceFirst(point2D_Segment2D_End, segment2D.Direction, polygon2D);
            if (vector2D != null)
                point2D_End = point2D_Segment2D_End.GetMoved(vector2D);

            if (point2D_End == null && polygon2D.On(point2D_Segment2D_End, tolerance))
                point2D_End = point2D_Segment2D_End;

            if (point2D_Start == null && point2D_End == null)
                return null;

            if (point2D_Start == null)
                point2D_Start = point2D_Segment2D_Start;

            if (point2D_End == null)
                point2D_End = point2D_Segment2D_End;

            return new Segment2D(point2D_Start, point2D_End);

            //List<Point2D> point2Ds = Query.Intersections(segment2D.End, segment2D.Direction, polygon2D, false, true, false, tolerance);
            //if (point2Ds == null || point2Ds.Count <= 1)
            //    return null;

            //Point2D point2D_Start = segment2D.Start;
            //Point2D point2D_End = segment2D.End;

            //point2Ds.Add(point2D_Start);
            //point2Ds.Add(point2D_End);

            //Point2D point2D_1;
            //Point2D point2D_2;

            //Query.ExtremePoints(point2Ds, out point2D_1, out point2D_2);
            //if (point2D_1 == null || point2D_2 == null)
            //    return null;

            //Point2D point2D;

            //if (point2D_Start.Distance(point2D_1) < point2D_Start.Distance(point2D_2))
            //    point2D = point2D_1;
            //else
            //    point2D = point2D_2;

            //Modify.SortByDistance(point2Ds, point2D);

            //int index_Start = point2Ds.IndexOf(point2D_Start);
            //int index_End = point2Ds.IndexOf(point2D_End);

            //bool changed;

            //changed = false;
            //for(int i = index_Start - 1; i >= 0; i--)
            //{
            //    point2D = Query.Mid(point2Ds[i], point2Ds[i + 1]);
            //    if(!polygon2D.Inside(point2D))
            //    {
            //        index_Start = i + 1;
            //        changed = true;
            //        break;
            //    }
            //}

            //if (!changed)
            //    index_Start = 0;

            //changed = false;
            //for (int i = index_End + 1; i < point2Ds.Count - 1; i++)
            //{
            //    point2D = Query.Mid(point2Ds[i], point2Ds[i + 1]);
            //    if (!polygon2D.Inside(point2D))
            //    {
            //        index_Start = i - 1;
            //        changed = true;
            //        break;
            //    }
            //}

            //if (!changed)
            //    index_End = point2Ds.Count - 1;

            //return new Segment2D(point2Ds[index_Start], point2Ds[index_End]);
        }

        public static Segment2D Extend(this Segment2D segment2D, double distance, bool extend_Start = true, bool extend_End = true)
        {
            if (segment2D == null)
                return null;

            if (!extend_Start && !extend_End)
                return new Segment2D(segment2D);

            if (distance == 0)
                return new Segment2D(segment2D);

            Vector2D vector2D = segment2D.Direction * distance;

            Point2D point2D_End = segment2D.End;
            if (extend_End)
                point2D_End.Move(vector2D);

            vector2D.Negate();

            Point2D point2D_Start = segment2D.Start;
            if (extend_Start)
                point2D_Start.Move(vector2D);

            return new Segment2D(point2D_Start, point2D_End);
        }

        public static Polyline2D Extend(this Polyline2D polyline2D, ISegmentable2D segmentable2D, bool extend_Start = true, bool extend_End = true)
        {
            if (polyline2D == null || segmentable2D == null)
                return null;

            if (polyline2D.IsClosed() || (!extend_End && !extend_Start))
                return new Polyline2D(polyline2D);

            List<Segment2D> segment2Ds = polyline2D.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            if (extend_Start)
            {
                Segment2D segment2D = segment2Ds[0];

                Point2D point2D = segment2D.Start;
                Vector2D vector2D = segment2D.Direction;
                vector2D.Negate();

                List<Vector2D> vector2Ds = Trace(point2D, vector2D, new ISegmentable2D[] { segmentable2D }, 0);
                if (vector2Ds != null && vector2Ds.Count > 0)
                    segment2Ds[0] = new Segment2D(point2D.GetMoved(vector2Ds[0]), segment2D.End);
            }

            if (extend_End)
            {
                Segment2D segment2D = segment2Ds[segment2Ds.Count - 1];

                Point2D point2D = segment2D.End;
                Vector2D vector2D = segment2D.Direction;

                List<Vector2D> vector2Ds = Trace(point2D, vector2D, new ISegmentable2D[] { segmentable2D }, 0);
                if (vector2Ds != null && vector2Ds.Count > 0)
                    segment2Ds[segment2Ds.Count - 1] = new Segment2D(segment2D.End, point2D.GetMoved(vector2Ds[0]));
            }

            return new Polyline2D(segment2Ds);
        }
    }
}