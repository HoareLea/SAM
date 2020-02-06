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
        public static GH_Surface ToGrasshopper(this Spatial.Surface surface)
        {
            Spatial.IClosed3D closed3D = surface.GetBoundary();

            if(closed3D is Spatial.ICurvable3D)
            {
                List<Spatial.ICurve3D> curve3Ds = ((Spatial.ICurvable3D)(closed3D)).GetCurves();

                Rhino.Geometry.Brep[] breps = null;

                if (closed3D is Spatial.IClosedPlanar3D)
                {
                    breps = Rhino.Geometry.Brep.CreatePlanarBreps(curve3Ds.ToRhino_PolylineCurve(), Tolerance.MicroDistance);

                }
                else
                {

                }

                if (breps != null && breps.Length == 0)
                    return new GH_Surface(breps[0]);
            }



            return null;
        }

        public static GH_Surface ToGrasshopper(this Spatial.Face face)
        {
            Spatial.IClosed3D closed3D = face.GetBoundary();

            if (closed3D is Spatial.ICurvable3D)
            {
                List<Spatial.ICurve3D> curve3Ds = ((Spatial.ICurvable3D)(closed3D)).GetCurves();

                Rhino.Geometry.Brep[] breps = Rhino.Geometry.Brep.CreatePlanarBreps(curve3Ds.ToRhino_PolylineCurve(), Tolerance.MicroDistance);
                if (breps != null && breps.Length > 0)
                    return new GH_Surface(breps[0]);
            }

            return null;
        }
    }
}
