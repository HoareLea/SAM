using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Brep ToRhino(this Shell shell, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (shell == null)
                return null;

            return ToRhino(shell.Face3Ds, tolerance);
        }

        public static global::Rhino.Geometry.Brep ToRhino(this IEnumerable<Face3D> face3Ds, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (face3Ds == null)
                return null;

            List<global::Rhino.Geometry.Brep> breps = new List<global::Rhino.Geometry.Brep>();
            foreach (Face3D face3D in face3Ds)
            {
                global::Rhino.Geometry.Brep brep = face3D.ToRhino_Brep(Core.Tolerance.Distance);
                if (brep == null)
                    continue;

                breps.Add(brep);
            }

            if (breps == null || breps.Count == 0)
                return null;

            double unitScale = Query.UnitScale();

            //adjsuted to make sure breps are closed in future see if we can close with higher tolerance
            global::Rhino.Geometry.Brep[] result = global::Rhino.Geometry.Brep.JoinBreps(breps, unitScale * tolerance); //previous tolerance 0.1
            if (result == null || result.Length == 0)
                return null;

            return result[0];
        }

        /// <summary>
        /// Creates Rhino surface brep from planar polyline points
        /// </summary>
        /// <param name="points">Rhino points</param>
        /// <returns>Rhino Brep</returns>
        public static global::Rhino.Geometry.Brep ToRhino_Brep(this IEnumerable<global::Rhino.Geometry.Point3d> points)
        {
            List<global::Rhino.Geometry.Point3d> pointList = new List<global::Rhino.Geometry.Point3d>(points);

            //---Not necessary---
            global::Rhino.Geometry.Plane plane = new global::Rhino.Geometry.Plane(pointList[0], pointList[1], pointList[2]);
            points = pointList.ConvertAll(x => plane.ClosestPoint(x));
            //-------------------

            List<global::Rhino.Geometry.LineCurve> lineCurves = new List<global::Rhino.Geometry.LineCurve>();

            // Create Line Curve
            for (int i = 1; i < pointList.Count; i++)
                lineCurves.Add(new global::Rhino.Geometry.LineCurve(pointList[i - 1], pointList[i]));

            lineCurves.Add(new global::Rhino.Geometry.LineCurve(pointList.Last(), points.First()));

            //Ceate Surface
            return global::Rhino.Geometry.Brep.CreateEdgeSurface(lineCurves);
        }

        public static global::Rhino.Geometry.Brep ToRhino_Brep(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null)
            {
                return null;
            }

            return ToRhino_Brep(face3D?.GetExternalEdge3D(), face3D?.GetInternalEdge3Ds(), tolerance);

            //IClosedPlanar3D externalEdge3D = face3D.GetExternalEdge3D();
            //if(externalEdge3D == null || !externalEdge3D.IsValid())
            //{
            //    return null;
            //}

            //List<IClosedPlanar3D> edges = new List<IClosedPlanar3D>() { externalEdge3D };

            //List<IClosedPlanar3D> internalEdge3Ds = face3D.GetInternalEdge3Ds();
            //if (internalEdge3Ds != null && internalEdge3Ds.Count != 0)
            //{
            //    internalEdge3Ds.RemoveAll(x => x == null || !x.IsValid());

            //    Orientation orientation_Main = externalEdge3D.Orientation();
            //    if (orientation_Main != Orientation.Undefined && orientation_Main != Orientation.Collinear)
            //    {
            //        for (int i = 0; i < internalEdge3Ds.Count; i++)
            //        {
            //            Orientation orientation = internalEdge3Ds[i].Orientation();
            //            if (orientation != Orientation.Undefined && orientation != Orientation.Collinear)
            //            {
            //                internalEdge3Ds[i].Reverse();
            //            }
            //        }
            //    }

            //    edges.AddRange(internalEdge3Ds);
            //}

            //return ToRhino_Brep(edges, tolerance);
        }

        public static global::Rhino.Geometry.Brep ToRhino_Brep(this Triangle3D triangle3D, double tolerance = Core.Tolerance.Distance)
        {
            if(triangle3D == null || !triangle3D.IsValid() || triangle3D.GetArea() < tolerance)
            {
                return null;
            }

            List<global::Rhino.Geometry.PolylineCurve> polylineCurves = new List<global::Rhino.Geometry.PolylineCurve>();
            polylineCurves.Add(triangle3D.GetSegments().ToRhino_PolylineCurve());

            if (polylineCurves.Count == 0)
            {
                return null;
            }

            double unitScale = Query.UnitScale();

            global::Rhino.Geometry.Brep[] breps = global::Rhino.Geometry.Brep.CreatePlanarBreps(polylineCurves, unitScale * tolerance);
            if(breps == null || breps.Length == 0)
            {
                return null;
            }

            return breps[0];
        }

        public static global::Rhino.Geometry.Brep ToRhino_Brep(this IClosedPlanar3D externalEdge3D, IEnumerable<IClosedPlanar3D> internalEdge3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if(externalEdge3D == null)
            {
                return null;
            }

            Plane plane = externalEdge3D.GetPlane();

            IClosed2D externalEdge2D = plane.Convert(externalEdge3D);

            List<IClosed2D> internalEdge2Ds = null;
            if(internalEdge3Ds != null)
            {
                internalEdge2Ds = new List<IClosed2D>();

                foreach (IClosedPlanar3D internalEdge3D in internalEdge3Ds)
                {
                    IClosed2D internalEdge2D = plane.Convert(plane.Project(internalEdge3D));
                    if(internalEdge2D != null)
                    {
                        internalEdge2Ds.Add(internalEdge2D);
                    }
                }
            }

            List<Face3D> face3Ds = Spatial.Create.Face3Ds(externalEdge2D, internalEdge2Ds, plane, tolerance : tolerance);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            List<IClosed3D> closed3Ds = new List<IClosed3D>();
            foreach (Face3D face3D in face3Ds)
            {
                List<Face3D> face3Ds_FixEdges = face3D.FixEdges();
                if(face3Ds_FixEdges == null || face3Ds_FixEdges.Count == 0)
                {
                    face3Ds_FixEdges = new List<Face3D>() { face3D };
                }

                foreach(Face3D face3D_FixEdge in face3Ds_FixEdges)
                {
                    IClosedPlanar3D externalEdge3D_Face3D = face3D_FixEdge?.GetExternalEdge3D();
                    if(externalEdge3D_Face3D != null)
                    {
                        closed3Ds.Add(externalEdge3D_Face3D);
                    }

                    List<IClosedPlanar3D> internalEdge3Ds_Face3D = face3D_FixEdge?.GetInternalEdge3Ds();
                    if(internalEdge3Ds_Face3D != null)
                    {
                        foreach(IClosedPlanar3D internalEdge3D_Face3D in internalEdge3Ds_Face3D)
                        {
                            if(internalEdge3D_Face3D == null)
                            {
                                continue;
                            }

                            closed3Ds.Add(internalEdge3D_Face3D);
                        }
                    }

                }
            }

            return ToRhino_Brep(closed3Ds, tolerance);

        }

        public static global::Rhino.Geometry.Brep ToRhino_Brep(this IEnumerable<IClosed3D> closed3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (closed3Ds == null || closed3Ds.Count() == 0)
            {
                return null;
            }

            double unitScale = Query.UnitScale();

            List<global::Rhino.Geometry.PolylineCurve> polylineCurves = new List<global::Rhino.Geometry.PolylineCurve>();
            foreach (IClosed3D closed3D in closed3Ds)
            {
                if (closed3D is Polygon3D)
                {
                    Polygon3D polygon3D = (Polygon3D)closed3D;

                    Plane plane = polygon3D.GetPlane();
                    Polygon2D polygon2D = plane.Convert(polygon3D);
                    List<Polygon2D> polygon2Ds = Planar.Query.FixEdges(polygon2D, tolerance);
                    if (polygon2Ds == null)
                    {
                        polylineCurves.Add(polygon3D.GetCurves().ToRhino_PolylineCurve());
                    }
                    else
                    {
                        polygon2Ds.ConvertAll(x => plane.Convert(x)).ForEach(x => polylineCurves.Add(x.GetSegments().ToRhino_PolylineCurve()));
                    }
                }
                else if (closed3D is ICurvable3D)
                {
                    polylineCurves.Add(((ICurvable3D)(closed3D)).GetCurves().ToRhino_PolylineCurve());
                }
            }

            if (polylineCurves.Count == 0)
                return null;

            global::Rhino.Geometry.Brep[] breps = global::Rhino.Geometry.Brep.CreatePlanarBreps(polylineCurves, unitScale * tolerance);

            if (breps == null || breps.Length == 0)
                return null;

            if (breps.Length == 1)
                return breps[0];

            List<global::Rhino.Geometry.Brep> brepList = breps.ToList();
            brepList.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));

            global::Rhino.Geometry.Brep brep = brepList.Last();
            brepList.Remove(brep);

            breps = global::Rhino.Geometry.Brep.CreateBooleanIntersection(new List<global::Rhino.Geometry.Brep> { brep }, brepList, unitScale * tolerance);
            if (breps == null || breps.Length == 0)
                return null;

            return breps[0];
        }
    }
}