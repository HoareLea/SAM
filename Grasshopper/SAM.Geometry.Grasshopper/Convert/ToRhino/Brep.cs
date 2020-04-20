using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

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

            List<Spatial.IClosedPlanar3D> edges = face3D.GetEdges();
            if (edges == null || edges.Count == 0)
                return null;

            return ToRhino_Brep(edges, tolerance);
        }

        public static Rhino.Geometry.Brep ToRhino_Brep(this IEnumerable<IClosed3D> closed3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (closed3Ds == null || closed3Ds.Count() == 0)
                return null;

            List<ICurve3D> curve3Ds = new List<ICurve3D>();
            foreach (IClosed3D closed3D in closed3Ds)
            {
                if (closed3D is Polygon3D)
                {

                    Polygon3D polygon3D = (Polygon3D)closed3D;

                    Plane plane = polygon3D.GetPlane();
                    Planar.Polygon2D polygon2D = plane.Convert(polygon3D);
                    List<Planar.Polygon2D> polygon2Ds = Planar.Query.Simplify(polygon2D, tolerance);
                    if (polygon2Ds == null)
                        curve3Ds.AddRange(polygon3D.GetCurves());
                    else
                        polygon2Ds.ConvertAll(x => plane.Convert(x)).ForEach(x => curve3Ds.AddRange(x.GetCurves()));
                }
                else if (closed3D is ICurvable3D)
                {
                    curve3Ds.AddRange(((ICurvable3D)(closed3D)).GetCurves());
                }
            }

            if (curve3Ds.Count == 0)
                return null;

            Rhino.Geometry.Brep[] breps = Rhino.Geometry.Brep.CreatePlanarBreps(curve3Ds.ToRhino_PolylineCurve(true), tolerance);

            if (breps == null || breps.Length == 0)
                return null;

            return breps[0];
        }
    }
}
