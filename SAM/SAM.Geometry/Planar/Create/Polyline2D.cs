using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Polyline2D Polyline2D(this IEnumerable<Segment2D> segment2Ds, bool split = true, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || segment2Ds.Count() == 0)
                return null;

            if (segment2Ds.Count() == 1)
                return new Polyline2D(new List<Point2D>() { segment2Ds.ElementAt(0).Start, segment2Ds.ElementAt(0).End });

            List<Point2D> point2Ds = Query.Point2Ds(segment2Ds, tolerance);
            List<Segment2D> segment2Ds_Temp = new List<Segment2D>(segment2Ds);

            Point2D point2D = null;
            foreach(Point2D point2D_Temp in point2Ds)
            {
                List<Segment2D> segment2Ds_Closest = segment2Ds_Temp.FindAll(x => x[0].Distance(point2D_Temp) <= tolerance || x[1].Distance(point2D_Temp) <= tolerance);
                if (segment2Ds_Closest != null && segment2Ds_Closest.Count == 1)
                    point2D = point2D_Temp;
            }

            if (point2D == null)
                point2D = segment2Ds_Temp[0][0];

            List<Point2D> point2Ds_Result = new List<Point2D>() { point2D };
            while(point2D != null)
            {
                List<Segment2D> segment2Ds_Closest = segment2Ds_Temp.FindAll(x => x[0].Distance(point2D) <= tolerance || x[1].Distance(point2D) <= tolerance);
                if (segment2Ds_Closest == null || segment2Ds_Closest.Count == 0)
                    break;

                segment2Ds_Closest.RemoveAll(x => x.GetLength() < tolerance);

                List<Point2D> point2Ds_Temp = Query.Point2Ds(segment2Ds_Closest, tolerance);
                if (point2Ds == null || point2Ds.Count == 0)
                    break;

                foreach(Point2D point2D_Result in point2Ds_Result)
                    point2Ds_Temp.RemoveAll(x => x.Distance(point2D_Result) <= tolerance);

                if (point2Ds_Temp.Count == 0)
                    break;

                point2Ds_Result.Add(point2Ds_Temp[0]);
                point2D = point2Ds_Temp[0];
            }

            if (point2Ds_Result == null || point2Ds_Result.Count < 2)
                return null;

            return new Polyline2D(point2Ds_Result);

            //return new PointGraph2D(segment2Ds_Result, split, tolerance).GetPolyline2D();
        }
    }
}