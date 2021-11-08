using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Mesh ToGrasshopper(this Spatial.Mesh3D mesh3D)
        {
            return new GH_Mesh(Rhino.Convert.ToRhino(mesh3D));
        }
    }
}