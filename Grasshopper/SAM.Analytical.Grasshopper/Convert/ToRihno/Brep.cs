using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Types;
using Rhino;
using SAM.Geometry;
using SAM.Geometry.Grasshopper;

namespace SAM.Analytical.Grasshopper
{
    // Add new class
    //class GH_Panel : GH_Goo<Panel>
    //{
    //    //drawPeview
    //}

    public static partial class Convert
    {
        public static Rhino.Geometry.Brep ToRhino(this PlanarBoundary3D planarBoundary3D)
        {
            if (planarBoundary3D == null)
                return null;
            
            Geometry.Spatial.Face3D face3D = planarBoundary3D.GetFace();
            if (face3D == null)
                return null;

            Rhino.Geometry.Brep[] breps = Rhino.Geometry.Brep.CreatePlanarBreps(((Geometry.Spatial.ICurvable3D)face3D.ToClosedPlanar3D()).GetCurves().ToRhino_PolylineCurve(), Tolerance.MicroDistance);
            if (breps == null || breps.Length == 0)
                return null;

            List<Geometry.Spatial.IClosedPlanar3D> closedPlanar3Ds = planarBoundary3D.GetInternalClosedPlanar3Ds();
            if(closedPlanar3Ds != null && closedPlanar3Ds.Count > 0)
            {
                List<Rhino.Geometry.Brep> breps_Internal = new List<Rhino.Geometry.Brep>();
                foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
                {
                    Rhino.Geometry.Brep[] breps_Temp = Rhino.Geometry.Brep.CreatePlanarBreps(((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves().ToRhino_PolylineCurve(), Tolerance.MicroDistance);
                    if (breps_Temp != null && breps_Temp.Length > 0)
                        breps_Internal.AddRange(breps_Temp);
                }

                foreach (Rhino.Geometry.Brep brep_Internal in breps_Internal)
                {
                    breps = Rhino.Geometry.Brep.CreateBooleanDifference(breps[0], brep_Internal, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    if (breps == null)
                        break;
                }
                    


                //RhinoDoc.ActiveDoc.ModelAbsoluteTolerance = Tolerance.MicroDistance;
                //breps = Rhino.Geometry.Brep.CreateBooleanDifference(breps, breps_Internal, Tolerance.MacroDistance);
            }

            if (breps != null && breps.Length > 0)
                return breps[0];

            return null;
        }
    }
}
