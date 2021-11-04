using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Segment3D ToSAM(this GH_Line line)
        {
            return Rhino.Convert.ToSAM(line.Value);
        }
    }
}