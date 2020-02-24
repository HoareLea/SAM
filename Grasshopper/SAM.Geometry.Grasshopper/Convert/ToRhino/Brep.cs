using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel.Types;

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

        public static Rhino.Geometry.Brep ToRhino_Brep(this Spatial.Face3D face3D, double tolerance = Tolerance.MicroDistance)
        {
            if (face3D == null)
                return null;

            List<Spatial.IClosedPlanar3D> edges = face3D.GetEdges();
            List<Rhino.Geometry.PolylineCurve> polylineCurves = edges.ConvertAll(x => ((Spatial.ICurvable3D)x).GetCurves().ToRhino_PolylineCurve(true));

            Rhino.Geometry.Brep[] breps = Rhino.Geometry.Brep.CreatePlanarBreps(polylineCurves, tolerance);
            if (breps != null && breps.Length > 0)
                return breps[0];

            return null;

            //Rhino.Geometry.Brep[] breps = Rhino.Geometry.Brep.CreatePlanarBreps(((Spatial.ICurvable3D)face3D.GetExternalEdge()).GetCurves().ToRhino_PolylineCurve(), tolerance);
            //if (breps == null || breps.Length == 0)
            //    return null;

            //List<Spatial.IClosedPlanar3D> closedPlanar3Ds = face3D.GetInternalEdges();
            //if (closedPlanar3Ds != null && closedPlanar3Ds.Count > 0)
            //{
            //    List<Rhino.Geometry.Brep> breps_Internal = new List<Rhino.Geometry.Brep>();
            //    foreach (Spatial.IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
            //    {
            //        Rhino.Geometry.Brep[] breps_Temp = Rhino.Geometry.Brep.CreatePlanarBreps(((Spatial.ICurvable3D)closedPlanar3D).GetCurves().ToRhino_PolylineCurve(), tolerance);
            //        if (breps_Temp != null && breps_Temp.Length > 0)
            //            breps_Internal.AddRange(breps_Temp);
            //    }

            //    foreach (Rhino.Geometry.Brep brep_Internal in breps_Internal)
            //    {
            //        breps = Rhino.Geometry.Brep.CreateBooleanDifference(breps[0], brep_Internal, tolerance);
            //        if (breps == null)
            //            break;
            //    }
            //}

            //if (breps != null && breps.Length > 0)
            //    return breps[0];

            //return null;
        }
    }
}
