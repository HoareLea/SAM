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
                    result = Planar.Point2D.Mid(point2Ds[i], point2Ds[j]);
                    if (On(segment2Ds, result, tolerance))
                        continue;

                    if (Inside(point2Ds, result))
                        return result;
                }
            }

            return null;
        }
    }
}