using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Create
    {
        public static Shell Shell(this GH_Mesh mesh, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (mesh == null)
                return null;

            return Rhino.Create.Shell(mesh.Value, tolerance_Angle, tolerance_Distance);
        }
    }
}