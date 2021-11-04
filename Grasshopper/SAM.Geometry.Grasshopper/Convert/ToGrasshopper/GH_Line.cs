using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Line ToGrasshopper(this Spatial.Segment3D segment3D)
        {
            return new GH_Line(Rhino.Convert.ToRhino(segment3D));
        }

        public static GH_Line ToGrasshopper(this Planar.Segment2D segment2D)
        {
            return new GH_Line(Rhino.Convert.ToRhino(segment2D));
        }
    }
}