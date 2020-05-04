using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Plane ToGrasshopper(this Spatial.Plane plane)
        {
            return new GH_Plane(plane.ToRhino());
        }
    }
}