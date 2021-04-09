using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Polyline2D Polyline2D(this IEnumerable<ISegmentable2D> segmentable2Ds, bool split = true, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null || segmentable2Ds.Count() == 0)
                return null;

            if (segmentable2Ds.Count() == 1)
            {
                ISegmentable2D segmentable2D = segmentable2Ds.ElementAt(0);
                return new Polyline2D(segmentable2D.GetPoints(), segmentable2D is IClosed2D);
            }

            List<Segment2D> segment2Ds = null;
            if(split)
            {
                segment2Ds = segmentable2Ds.Split(tolerance);
            }
            else
            {
                foreach(ISegmentable2D segmentable2D in segmentable2Ds)
                {
                    List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                    if (segment2Ds_Temp == null)
                        continue;

                    segment2Ds.AddRange(segment2Ds_Temp);
                }
            }

            List<Point2D> point2Ds = Query.UniquePoint2Ds(segment2Ds, tolerance);

            Point2D point2D = null;
            foreach(Point2D point2D_Temp in point2Ds)
            {
                List<Segment2D> segment2Ds_Closest = segment2Ds.FindAll(x => x[0].Distance(point2D_Temp) <= tolerance || x[1].Distance(point2D_Temp) <= tolerance);
                if (segment2Ds_Closest != null && segment2Ds_Closest.Count == 1)
                    point2D = point2D_Temp;
            }

            if (point2D == null)
                point2D = segment2Ds[0][0];

            List<Point2D> point2Ds_Result = new List<Point2D>() { point2D };
            while(point2D != null)
            {
                List<Segment2D> segment2Ds_Closest = segment2Ds.FindAll(x => x[0].Distance(point2D) <= tolerance || x[1].Distance(point2D) <= tolerance);
                if (segment2Ds_Closest == null || segment2Ds_Closest.Count == 0)
                {
                    break;
                }

                segment2Ds_Closest.RemoveAll(x => x.GetLength() < tolerance);

                List<Point2D> point2Ds_Temp = Query.UniquePoint2Ds(segment2Ds_Closest, tolerance);
                if (point2Ds == null || point2Ds.Count == 0)
                {
                    break;
                }
                    

                foreach(Point2D point2D_Result in point2Ds_Result)
                {
                    point2Ds_Temp.RemoveAll(x => x.Distance(point2D_Result) <= tolerance);
                }

                if (point2Ds_Temp.Count == 0)
                {
                    break;
                }

                point2Ds_Result.Add(point2Ds_Temp[0]);
                point2D = point2Ds_Temp[0];
            }

            if (point2Ds_Result == null || point2Ds_Result.Count < 2)
            {
                return null;
            }

            return new Polyline2D(point2Ds_Result);
        }
    }
}