using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {

        public static Spatial.Vector3D ToSAM(this GH_Vector vector)
        {
            return Rhino.Convert.ToSAM(vector.Value);
        }
    }
}