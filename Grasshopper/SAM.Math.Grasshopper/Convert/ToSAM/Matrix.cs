using Grasshopper.Kernel.Types;

namespace SAM.Math.Grasshopper
{
    public static partial class Convert
    {

        public static Matrix ToSAM(this GH_Matrix matrix)
        {
            return Rhino.Convert.ToSAM(matrix?.Value);
        }
    }
}