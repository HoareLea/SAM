using ClipperLib;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polygon2D> SelfIntersectionPolygon2Ds(this Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D == null)
                return null;

            List<List<IntPoint>> intPointsList = Clipper.SimplifyPolygon(Convert.ToClipper((ISegmentable2D)polygon2D, tolerance));
            if (intPointsList == null)
                return null;

            List<Polygon2D> polygon2Ds = new List<Polygon2D>();
            foreach (List<IntPoint> intPoints in intPointsList)
            {
                List<Point2D> point2Ds = intPoints.ToSAM(tolerance);
                polygon2Ds.Add(new Polygon2D(point2Ds));
            }

            return polygon2Ds;

            //if (!SelfIntersect(polygon2D))
            //    return null;

            //List<Polygon2D> result = new List<Polygon2D>();

            //Point2D point2D_Intersection = SelfIntersectionPoint2D(polygon2D, tolerance);
            //if (point2D_Intersection == null)
            //    return null;

            //Polygon2D polygon2D_Temp = new Polygon2D(polygon2D);
            //double parameter = polygon2D_Temp.GetParameter(point2D_Intersection, false, tolerance);

            ////TODO: Check implementation for the case where parameter is equal to 0 or 1
            //while (parameter == 1 || parameter == 0)
            //    polygon2D_Temp.Reorder(1);

            //Polygon2D polygon2D_Temp_Reversed = new Polygon2D(polygon2D_Temp);
            //polygon2D_Temp_Reversed.Reverse();

            //double parameter_Reversed = polygon2D_Temp_Reversed.GetParameter(point2D_Intersection, false, tolerance);

            //Polyline2D polyline2D = polygon2D_Temp.Trim(parameter) as Polyline2D;
            //Polyline2D polyline2D_Reversed = polygon2D_Temp_Reversed.Trim(parameter_Reversed) as Polyline2D;

            //List<Point2D> point2Ds_Reversed = polyline2D_Reversed.GetPoints();

            //point2Ds_Reversed.RemoveAt(0);
            //point2Ds_Reversed.Reverse();
            //point2Ds_Reversed.RemoveAt(0);

            //List<Point2D> point2Ds = polyline2D.GetPoints();
            //point2Ds.AddRange(point2Ds_Reversed);

            //result.Add(new Polygon2D(point2Ds));

            //List<Point2D> point2Ds_New = polygon2D.GetPoints();
            //if (point2Ds_New == null)
            //    return result;

            //point2Ds_New.RemoveAll(x => point2Ds.Contains(x));
            //if (point2Ds_New.Count == 0)
            //    return result;

            //if(!point2Ds_New[0].AlmostEquals(point2D_Intersection, tolerance))
            //    point2Ds_New.Insert(0, point2D_Intersection);

            //if (point2Ds_New.Last().AlmostEquals(point2D_Intersection, tolerance))
            //    point2Ds_New.RemoveAt(point2Ds_New.Count - 1);

            //Polygon2D polygon2D_New = new Polygon2D(point2Ds_New);

            //List<Polygon2D> polygon2Ds = SelfIntersectionPolygon2Ds(polygon2D_New, tolerance);
            //if (polygon2Ds != null && polygon2Ds.Count > 0)
            //    result.AddRange(polygon2Ds);
            //else
            //    result.Add(polygon2D_New);

            //return result;
        }
    }
}