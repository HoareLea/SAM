using Grasshopper.Kernel.Types;
using System.Drawing.Drawing2D;

namespace SAM.Math.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Matrix ToRhino(this Math.Matrix matrix)
        {
            if (matrix == null)
                return null;

            int count_Rows = matrix.RowCount();
            int count_Columns = matrix.ColumnCount();

            Rhino.Geometry.Matrix result = new Rhino.Geometry.Matrix(count_Rows, count_Columns);
            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    result[i, j] = matrix[i, j];

            return result;
        }
    }
}