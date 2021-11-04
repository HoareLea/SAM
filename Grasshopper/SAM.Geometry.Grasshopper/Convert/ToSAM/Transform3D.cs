using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Transform3D ToSAM_Transform3D(this GH_Matrix matrix)
        {
            return Rhino.Convert.ToSAM_Transform3D(matrix.Value);
        }

        public static Spatial.Transform3D ToSAM(this GH_Transform transform)
        {
            return Rhino.Convert.ToSAM(transform.Value);
        }
    }
}