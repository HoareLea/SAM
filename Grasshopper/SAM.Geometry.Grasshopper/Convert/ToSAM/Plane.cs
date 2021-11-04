using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Plane ToSAM(this Rhino.Geometry.Plane plane)
        {
            return new Spatial.Plane(plane.Origin.ToSAM(), plane.XAxis.ToSAM(), plane.YAxis.ToSAM());
        }

        public static Spatial.Plane ToSAM(this GH_Plane plane)
        {
            return plane.Value.ToSAM();
        }
    }
}