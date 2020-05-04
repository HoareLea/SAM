using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Circle3D ToSAM(this Rhino.Geometry.Circle circle)
        {
            return new Spatial.Circle3D(circle.Plane.ToSAM(), circle.Radius);
        }

        public static Spatial.Circle3D ToSAM(this GH_Circle circle)
        {
            return circle.Value.ToSAM();
        }
    }
}