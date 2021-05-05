using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Face2D Join(this Face2D face2D, IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face2D == null)
                return null;

            if (face2Ds == null || face2Ds.Count() == 0)
                return new Face2D(face2D);

            Point2D point2D = face2D.GetInternalPoint2D();
            if (point2D == null)
                return new Face2D(face2D);

            List<Face2D> face2Ds_Temp = new List<Face2D>(face2Ds);
            face2Ds_Temp.Add(face2D);

            face2Ds_Temp = face2Ds_Temp.Union(tolerance);
            if (face2Ds_Temp == null || face2Ds_Temp.Count == 0)
                return new Face2D(face2D);

            Face2D result = face2Ds_Temp.Find(x => x.Inside(point2D, tolerance));
            if (result == null)
                return new Face2D(face2D);

            return result;
        }

        public static Polyline2D Join(this Polyline2D polyline2D_1, Polyline2D polyline2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (polyline2D_1 == null || polyline2D_2 == null)
                return null;

            List<Segment2D> segment2Ds_1 = polyline2D_1.GetSegments();
            List<Segment2D> segment2Ds_2 = polyline2D_2.GetSegments();

            if (segment2Ds_1 == null || segment2Ds_2 == null || segment2Ds_1.Count == 0 || segment2Ds_2.Count == 0)
                return null;

            if (segment2Ds_1[0].Start.Distance(segment2Ds_2[0].Start) < tolerance)
            {
                segment2Ds_2[0] = new Segment2D(segment2Ds_1[0].Start, segment2Ds_2[0].End);
                segment2Ds_2.Reverse();
                segment2Ds_2.ForEach(x => x.Reverse());

                segment2Ds_2.AddRange(segment2Ds_1);
                return new Polyline2D(segment2Ds_2);
            }

            int count_1 = segment2Ds_1.Count;

            if (segment2Ds_1[count_1 - 1].End.Distance(segment2Ds_2[0].Start) < tolerance)
            {
                segment2Ds_2[0] = new Segment2D(segment2Ds_1[count_1 - 1].End, segment2Ds_2[0].End);

                segment2Ds_1.AddRange(segment2Ds_2);
                return new Polyline2D(segment2Ds_1);
            }

            int count_2 = segment2Ds_2.Count;

            if (segment2Ds_1[count_1 - 1].End.Distance(segment2Ds_2[count_2 - 1].End) < tolerance)
            {
                segment2Ds_2.Reverse();
                segment2Ds_2.ForEach(x => x.Reverse());
                segment2Ds_2[0] = new Segment2D(segment2Ds_1[count_1 - 1].End, segment2Ds_2[0].End);

                segment2Ds_1.AddRange(segment2Ds_2);
                return new Polyline2D(segment2Ds_1);
            }

            if (segment2Ds_1[0].Start.Distance(segment2Ds_2[count_2 - 1].End) < tolerance)
            {
                segment2Ds_2[count_2 - 1] = new Segment2D(segment2Ds_2[count_2 - 1].Start, segment2Ds_1[0].Start);

                segment2Ds_2.AddRange(segment2Ds_1);
                return new Polyline2D(segment2Ds_2);
            }

            return null;
        }

        public static List<Segment2D> Join(this IEnumerable<Segment2D> segment2Ds, Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
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

                Segment2D segment2D_Temp = Join(segment2D, polygon2D, tolerance);
                if (segment2D_Temp == null)
                    continue;

                result.Add(segment2D_Temp);
            }

            return result;
        }

        public static Segment2D Join(this Segment2D segment2D, Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
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
        }
    }
}