using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polyline2D> Cut(this Polyline2D polyline2D, Point2D point2D_1, Point2D point2D_2)
        {
            if (polyline2D == null || point2D_1 == null || point2D_2 == null)
                return null;

            Point2D point2D_1_Closest = polyline2D.Closest(point2D_1, true);
            Point2D point2D_2_Closest = polyline2D.Closest(point2D_2, true);
            if (point2D_1_Closest.Equals(point2D_2_Closest))
            {
                Polyline2D polyline2D_Temp = new Polyline2D(polyline2D);
                polyline2D_Temp.InsertClosest(point2D_1_Closest);
                int index = polyline2D_Temp.IndexOfClosestPoint2D(point2D_1);
                //polyline2D_Temp.Reorder(index); //DODO: Check if this line is necessary

                return new List<Polyline2D>() { polyline2D_Temp };
            }

            double parameter_1 = polyline2D.GetParameter(point2D_1_Closest);
            double parameter_2 = polyline2D.GetParameter(point2D_2_Closest);

            if (parameter_1 > parameter_2)
            {
                Point2D point2D = point2D_1_Closest;
                point2D_1_Closest = point2D_2_Closest;
                point2D_2_Closest = point2D;

                double parameter = parameter_1;
                parameter_1 = parameter_2;
                parameter_2 = parameter;
            }

            List<Polyline2D> result = new List<Polyline2D>();

            if (parameter_1 == 0)
            {
                if (1 - parameter_2 < parameter_2)
                {
                    result.Add(polyline2D.Trim(parameter_2) as Polyline2D);
                    return result;
                }

                Polyline2D polyline2D_Temp = new Polyline2D(polyline2D);
                polyline2D_Temp.Reverse();
                polyline2D_Temp = polyline2D_Temp.Trim(1 - parameter_2) as Polyline2D;
                polyline2D_Temp.Reverse();

                result.Add(polyline2D_Temp);
                return result;
            }

            result.Add(polyline2D.Trim(parameter_1) as Polyline2D);

            Polyline2D polyline2D_Reversed = new Polyline2D(polyline2D);
            polyline2D_Reversed.Reverse();

            if (parameter_2 != 1)
                result.Add(polyline2D_Reversed.Trim(1 - parameter_2) as Polyline2D);

            return result;
        }

        public static List<Face2D> Cut(this Face2D face2D, IEnumerable<Segment2D> segment2Ds, double tolerance = Tolerance.Distance)
        {
            if (face2D == null || segment2Ds == null || segment2Ds.Count() == 0)
                return null;

            List<IClosed2D> closed2Ds = face2D.Edge2Ds;
            if (closed2Ds == null || closed2Ds.Count == 0)
                return null;

            List<Segment2D> segment2Ds_Temp = new List<Segment2D>();
            foreach (IClosed2D closed2D in closed2Ds)
            {
                if (closed2D == null || !(closed2D is ISegmentable2D))
                    continue;

                segment2Ds_Temp.AddRange(((ISegmentable2D)closed2D).GetSegments());
            }

            if (segment2Ds_Temp.Count < 2)
                return null;

            segment2Ds_Temp.AddRange(segment2Ds);

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segment2Ds_Temp, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            return Create.Face2Ds(polygon2Ds);

        }
    }
}