using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Plane ToSAM(this GH_Plane plane)
        {
            return Rhino.Convert.ToSAM(plane.Value);
        }
    }
}