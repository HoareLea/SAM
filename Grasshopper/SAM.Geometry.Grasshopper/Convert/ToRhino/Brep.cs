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

        public static Rhino.Geometry.Brep ToRhino_Brep(this Spatial.Face3D face)
        {
            Spatial.IClosed3D closed3D = face.ToClosedPlanar3D();
            if (closed3D is Spatial.Polygon3D)
            {
                //return Rhino.Geometry.Brep.CreateEdgeSurface(((Spatial.Polygon3D)closed3D).GetSegments().ConvertAll(x => new Rhino.Geometry.LineCurve(x.ToRhino())));

                Rhino.Geometry.Brep[] breps = Rhino.Geometry.Brep.CreatePlanarBreps(((Spatial.Polygon3D)closed3D).ToRhino_PolylineCurve(), Tolerance.MicroDistance);
                if (breps != null && breps.Length > 0)
                    return breps[0];

                //List<Rhino.Geometry.LineCurve> lineCurves = new List<Rhino.Geometry.LineCurve>();

                //List<Rhino.Geometry.Point3d> points = ((Spatial.Polygon3D)closed3D).GetPoints().ConvertAll(x => x.ToRhino());

                //Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane(points[0], points[1], points[2]);
                //points = points.ConvertAll(x => plane.ClosestPoint(x));

                //for(int i=1; i < points.Count; i++)
                //    lineCurves.Add(new Rhino.Geometry.LineCurve(points[i - 1], points[i]));

                //lineCurves.Add(new Rhino.Geometry.LineCurve(points.Last(), points.First()));

                //return Rhino.Geometry.Brep.CreateEdgeSurface(lineCurves);
            }
                

            return null;
        }
    }
}
