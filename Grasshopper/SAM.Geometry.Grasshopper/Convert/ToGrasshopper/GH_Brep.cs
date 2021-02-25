using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Brep ToGrasshopper(this Spatial.Shell shell, double tolerance = Core.Tolerance.MacroDistance)
        {
            return new GH_Brep(shell?.ToRihno(tolerance));
        }
    }
}