using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {       
        public static Mesh3D ToSAM(this GH_Mesh ghMesh)
        {
            return Rhino.Convert.ToSAM(ghMesh?.Value);
        }
    }
}