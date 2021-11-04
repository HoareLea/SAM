using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Brep ToGrasshopper(this Spatial.Shell shell, double tolerance = Core.Tolerance.MacroDistance)
        {
            Brep brep = Rhino.Convert.ToRhino(shell, tolerance);
            if (brep == null)
            {
                return null;
            }

            return new GH_Brep(brep);
        }

        public static GH_Brep ToGrasshopper_Brep(this Spatial.Face3D face3D, double tolerance = Core.Tolerance.MacroDistance)
        {
            Brep brep =Rhino.Convert.ToRhino_Brep(face3D, tolerance);
            if(brep == null)
            {
                return null;
            }

            return new GH_Brep(brep);
        }
    }
}