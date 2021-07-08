using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Brep ToRhino(this Shell shell, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (shell == null)
                return null;

            List<Face3D> face3Ds = shell.Face3Ds;
            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            List<Rhino.Geometry.Brep> breps = new List<Rhino.Geometry.Brep>();
            foreach(Face3D face3D in face3Ds)
            {
                Rhino.Geometry.Brep brep = face3D.ToRhino_Brep(tolerance);
                if (brep == null)
                    continue;

                breps.Add(brep);
            }

            if (breps == null || breps.Count == 0)
                return null;

            //adjsuted to make sure breps are closed in future see if we can close with higher tolerance
            Rhino.Geometry.Brep[] result = Rhino.Geometry.Brep.JoinBreps(breps, 0.1);
            if (result == null || result.Length == 0)
                return null;

            return result[0];
        }

        /// <summary>
        /// Creates Rhino surface brep from planar polyline points
        /// </summary>
        /// <param name="points">Rhino points</param>
        /// <returns>Rhino Brep</returns>
        public static Rhino.Geometry.Brep ToRhino_Brep(this IEnumerable<Rhino.Geometry.Point3d> points)
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

            IClosedPlanar3D externalEdge3D = face3D.GetExternalEdge3D();
            if(externalEdge3D == null || !externalEdge3D.IsValid())
            {
                return null;
            }

            List<IClosedPlanar3D> edges = new List<IClosedPlanar3D>() { externalEdge3D };

            List<IClosedPlanar3D> internalEdge3Ds = face3D.GetInternalEdge3Ds();
            if (internalEdge3Ds != null && internalEdge3Ds.Count != 0)
            {
                internalEdge3Ds.RemoveAll(x => x == null || !x.IsValid());
                
                Orientation orientation_Main = externalEdge3D.Orinetation();
                if (orientation_Main != Orientation.Undefined && orientation_Main != Orientation.Collinear)
                {
                    for (int i = 0; i < internalEdge3Ds.Count; i++)
                    {
                        Orientation orientation = internalEdge3Ds[i].Orinetation();
                        if (orientation != Orientation.Undefined && orientation != Orientation.Collinear)
                        {
                            internalEdge3Ds[i].Reverse();
                        }
                    }
                }

                edges.AddRange(internalEdge3Ds);
            }

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