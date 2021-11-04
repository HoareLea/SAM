using GH_IO.Types;
using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Point3D ToSAM(this GH_Point point)
        {
            return Rhino.Convert.ToSAM(point.Value);
        }

        public static Spatial.Point3D ToSAM(this GH_Point3D point3D)
        {
            return new Spatial.Point3D(point3D.x, point3D.y, point3D.z);
        }
    }
}