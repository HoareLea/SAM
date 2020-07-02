using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D InternalPoint2D(this IClosed2D closed2D, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D == null)
                return null;

            if (!(closed2D is ISegmentable2D))
                throw new NotImplementedException();

            List<Point2D> point2Ds = ((ISegmentable2D)closed2D).GetPoints();
            if (point2Ds == null || point2Ds.Count < 3)
                return null;

            Point2D result = Query.Centroid(point2Ds);
            if (Inside(point2Ds, result))
                return result;

            List<Segment2D> segment2Ds = Create.Segment2Ds(point2Ds, true);

            int count = point2Ds.Count;
            for (int i = 0; i < count - 2; i++)
            {
                for (int j = 1; j < count - 1; j++)
                {
                    result = Planar.Query.Mid(point2Ds[i], point2Ds[j]);
                    if (On(segment2Ds, result, tolerance))
                        continue;

                    if (Inside(point2Ds, result))
                        return result;
                }
            }

            return null;
        }

        public static Point2D InternalPoint2D(IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point2Ds == null || point2Ds.Count() < 3)
                return null;

            Point2D result = Query.Centroid(point2Ds);

            if (result != null && Query.Inside(point2Ds, result))
                return result;

            List<Point2D> point2Ds_List = new List<Point2D>(point2Ds);
            point2Ds_List.Add(point2Ds_List[0]);
            point2Ds_List.Add(point2Ds_List[1]);

            int count = point2Ds_List.Count;

            List<Segment2D> segments = Create.Segment2Ds(point2Ds, true);
            for (int i = 0; i < count - 2; i++)
            {
                for (int j = i + 1; j < count - 1; j++)
                {
                    for (int k = j + 1; k < count; k++)
                    {
                        Point2D point2D_1 = point2Ds_List[i];
                        Point2D point2D_2 = point2Ds_List[j];
                        Point2D point2D_3 = point2Ds_List[k];

                        Segment2D segment2D = new Segment2D(point2D_1, point2D_3);
                        if (segment2D.On(point2D_2, tolerance))
                            continue;

                        result = segment2D.Mid();
                        if (Query.Inside(point2Ds, result) && !Query.On(segments, result))
                            return result;
                    }
                }
            }

            return null;
        }
    }
}