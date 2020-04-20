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
        public static GH_Surface ToGrasshopper(this Surface surface)
        {
            IClosed3D closed3D = surface.GetExternalEdge();

            if(closed3D is ICurvable3D)
            {
                Rhino.Geometry.Brep[] breps = null;

                List<ICurve3D> curve3Ds = null;

                if (closed3D is IClosedPlanar3D)
                {
                    if(closed3D is Polygon3D)
                    {
                        Polygon3D polygon3D = (Polygon3D)closed3D;

                        Plane plane = polygon3D.GetPlane();
                        Planar.Polygon2D polygon2D = plane.Convert(polygon3D);
                        List<Planar.Polygon2D> polygon2Ds = Planar.Query.Simplify(polygon2D);
                        if(polygon2Ds == null)
                        {
                            curve3Ds = polygon3D.GetCurves();
                        }
                        else
                        {
                            curve3Ds = new List<ICurve3D>();
                            polygon2Ds.ConvertAll(x => plane.Convert(x)).ForEach(x => curve3Ds.AddRange(x.GetCurves()));
                        }
                    }
                    else
                    {
                        curve3Ds = ((ICurvable3D)(closed3D)).GetCurves();
                    }

                    breps = Rhino.Geometry.Brep.CreatePlanarBreps(curve3Ds.ToRhino_PolylineCurve(true), Core.Tolerance.Distance);
                }
                else
                {

                }

                if (breps != null && breps.Length == 0)
                    return new GH_Surface(breps[0]);
            }



            return null;
        }

        public static GH_Surface ToGrasshopper(this Spatial.Face3D face, double tolerance = Core.Tolerance.Distance)
        {
            return new GH_Surface(ToRhino_Brep(face, tolerance));
        }
    }
}
