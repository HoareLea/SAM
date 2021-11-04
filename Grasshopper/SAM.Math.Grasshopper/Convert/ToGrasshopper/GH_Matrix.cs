using Grasshopper.Kernel.Types;

namespace SAM.Math.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Matrix ToGrasshopper(this Matrix matrix)
        {
            return new GH_Matrix(Rhino.Convert.ToRhino(matrix));
        }
    }
}