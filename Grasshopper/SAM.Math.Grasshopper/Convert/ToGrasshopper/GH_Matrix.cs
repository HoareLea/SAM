using Grasshopper.Kernel.Types;
using System.Drawing.Drawing2D;

namespace SAM.Math.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Matrix ToGrasshopper(this Matrix matrix)
        {
            return new GH_Matrix(matrix?.ToRhino());
        }
    }
}