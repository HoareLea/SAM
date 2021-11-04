using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Circle3D ToSAM(this GH_Circle circle)
        {
            return Rhino.Convert.ToSAM(circle.Value);
        }
    }
}