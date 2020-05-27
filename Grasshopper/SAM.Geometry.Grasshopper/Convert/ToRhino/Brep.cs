using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        //Create surfaace from planar polyline points
        public static Rhino.Geometry.Brep ToRihno_Brep(this IEnumerable<Rhino.Geometry.Point3d> points)
        {
            List<Rhino.Geometry.Point3d> pointList = new List<Rhino.Geometry.Point3d>(points);

            //---Not necessary---
            Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane(pointList[0], pointList[1], pointList[2]);
            points = pointList.ConvertAll(x => plane.ClosestPoint(x));
            //-------------------

            List<Rhino.Geometry.LineCurve> lineCurves = new List<Rhino.Geometry.LineCurve>();

            // Create Line Curve
            for (int i = 1; i < pointList.Count; i++)
                lineCurves.Add(new Rhino.Geometry.LineCurve(pointList[i - 1], pointList[i]));

            lineCurves.Add(new Rhino.Geometry.LineCurve(pointList.Last(), points.First()));

            //Ceate Surface
            return Rhino.Geometry.Brep.CreateEdgeSurface(lineCurves);
        }

        public static Rhino.Geometry.Brep ToRhino_Brep(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null)
                return null;

            List<IClosedPlanar3D> edges = face3D.GetEdges();
            if (edges == null || edges.Count == 0)
                return null;

            return ToRhino_Brep(edges, tolerance);
        }

        public static Rhino.Geometry.Brep ToRhino_Brep(this IEnumerable<IClosed3D> closed3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (closed3Ds == null || closed3Ds.Count() == 0)
                return null;

            List<Rhino.Geometry.PolylineCurve> polylineCurves = new List<Rhino.Geometry.PolylineCurve>();
            foreach (IClosed3D closed3D in closed3Ds)
            {
                if (closed3D is Polygon3D)
                {
                    Polygon3D polygon3D = (Polygon3D)closed3D;

                    Plane plane = polygon3D.GetPlane();
                    Planar.Polygon2D polygon2D = plane.Convert(polygon3D);
                    List<Planar.Polygon2D> polygon2Ds = Planar.Query.SimplifyByClipper(polygon2D, tolerance);
                    if (polygon2Ds == null)
                        polylineCurves.Add(polygon3D.GetCurves().ToRhino_PolylineCurve());
                    else
                        polygon2Ds.ConvertAll(x => plane.Convert(x)).ForEach(x => polylineCurves.Add(x.GetSegments().ToRhino_PolylineCurve()));
                }
                else if (closed3D is ICurvable3D)
                {
                    polylineCurves.Add(((ICurvable3D)(closed3D)).GetCurves().ToRhino_PolylineCurve());
                }
            }

            if (polylineCurves.Count == 0)
                return null;

            Rhino.Geometry.Brep[] breps = Rhino.Geometry.Brep.CreatePlanarBreps(polylineCurves, tolerance);

            if (breps == null || breps.Length == 0)
                return null;

            if (breps.Length == 1)
                return breps[0];

            List<Rhino.Geometry.Brep> brepList = breps.ToList();
            brepList.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));

            Rhino.Geometry.Brep brep = brepList.Last();
            brepList.Remove(brep);

            breps = Rhino.Geometry.Brep.CreateBooleanIntersection(new List<Rhino.Geometry.Brep> { brep }, brepList, tolerance);
            if (breps == null || breps.Length == 0)
                return null;

            return breps[0];
        }
    }
}