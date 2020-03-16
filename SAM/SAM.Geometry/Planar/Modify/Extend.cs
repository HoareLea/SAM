using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static List<Segment2D> Extend(this IEnumerable<Segment2D> segment2Ds, Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
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

                Segment2D segment2D_Temp = Extend(segment2D, polygon2D, tolerance);
                if (segment2D_Temp == null)
                    continue;

                result.Add(segment2D_Temp);
            }

            return result;
        }

        public static Segment2D Extend(this Segment2D segment2D, Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segment2D == null || polygon2D == null)
                return null;

            List<Point2D> point2Ds = Query.Intersections(segment2D.End, segment2D.Direction, polygon2D, false, true, tolerance);
            if (point2Ds == null || point2Ds.Count <= 1)
                return null;

            Point2D point2D_Start = segment2D.Start;
            Point2D point2D_End = segment2D.End;

            point2Ds.Add(point2D_Start);
            point2Ds.Add(point2D_End);

            Point2D point2D_1;
            Point2D point2D_2;

            Query.ExtremePoints(point2Ds, out point2D_1, out point2D_2);
            if (point2D_1 == null || point2D_2 == null)
                return null;

            Point2D point2D;

            if (point2D_Start.Distance(point2D_1) < point2D_Start.Distance(point2D_2))
                point2D = point2D_1;
            else
                point2D = point2D_2;

            Modify.SortByDistance(point2Ds, point2D);

            int index_Start = point2Ds.IndexOf(point2D_Start);
            int index_End = point2Ds.IndexOf(point2D_End);

            bool changed;

            changed = false;
            for(int i = index_Start - 1; i >= 0; i--)
            {
                point2D = Query.Mid(point2Ds[i], point2Ds[i + 1]);
                if(!polygon2D.Inside(point2D))
                {
                    index_Start = i + 1;
                    changed = true;
                    break;
                }
            }

            if (!changed)
                index_Start = 0;

            changed = false;
            for (int i = index_End + 1; i < point2Ds.Count - 1; i++)
            {
                point2D = Query.Mid(point2Ds[i], point2Ds[i + 1]);
                if (!polygon2D.Inside(point2D))
                {
                    index_Start = i - 1;
                    changed = true;
                    break;
                }
            }

            if (!changed)
                index_End = point2Ds.Count - 1;

            return new Segment2D(point2Ds[index_Start], point2Ds[index_End]);
        }
    }
}
