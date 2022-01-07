using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Rectangle3D ToSAM(this GH_Rectangle rectangle)
        {
            return Rhino.Convert.ToSAM(rectangle.Value);
        }
    }
}