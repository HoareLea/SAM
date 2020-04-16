using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon2D> SelfIntersectionPolygon2Ds(this Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D == null)
                return null;

            if (!SelfIntersect(polygon2D))
                return null;

            List<Polygon2D> result = new List<Polygon2D>();

            Point2D point2D_Intersection = SelfIntersectionPoint2D(polygon2D, tolerance);

            Polygon2D polygon2D_Reversed = new Polygon2D(polygon2D);
            polygon2D_Reversed.Reverse();

            //TODO: Implementation for the case where parameter is equal to 0 or 1
            double parameter = polygon2D.GetParameter(point2D_Intersection);
            double parameter_Reversed = polygon2D_Reversed.GetParameter(point2D_Intersection);

            Polyline2D polyline2D = polygon2D.Trim(parameter) as Polyline2D;
            Polyline2D polyline2D_Reversed = polygon2D.Trim(parameter_Reversed) as Polyline2D;

            List<Point2D> point2Ds = polyline2D.GetPoints();
            List<Point2D> point2Ds_Reversed = polyline2D_Reversed.GetPoints();

            point2Ds_Reversed.RemoveAt(0);
            point2Ds_Reversed.Reverse();
            point2Ds_Reversed.RemoveAt(0);

            point2Ds.AddRange(point2Ds_Reversed);

            result.Add(new Polygon2D(point2Ds));

            List<Point2D> point2Ds_New = polygon2D.GetPoints();
            point2Ds_New.RemoveAll(x => point2Ds.Contains(x));
            if (!point2Ds_New.Contains(point2D_Intersection))
                point2Ds_New.Insert(0, point2D_Intersection);

            List<Polygon2D> polygon2Ds = SelfIntersectionPolygon2Ds(new Polygon2D(point2Ds_New), tolerance);
            if (polygon2Ds != null && polygon2Ds.Count > 0)
                result.AddRange(polygon2Ds);

            return result;

        }
    }
}
