using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {       
        public static Shell ToSAM_Shell(this GH_Extrusion ghExtrusion)
        {
            return Rhino.Convert.ToSAM_Shell(ghExtrusion?.Value);
        }
    }
}