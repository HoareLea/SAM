using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Point ToGrasshopper(this Spatial.Point3D point3D)
        {
            return new GH_Point(Rhino.Convert.ToRhino(point3D));
        }

        public static GH_Point ToGrasshopper(this Planar.Point2D point2D)
        {
            return new GH_Point(Rhino.Convert.ToRhino(point2D));
        }
    }
}