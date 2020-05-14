using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Math
{
    public static partial class Create
    {
        public static Matrix3D Matrix3D(Matrix matrix)
        {
            if (matrix == null)
                return null;

            if (matrix.RowCount() != 3 || matrix.ColumnCount() != 3)
                return null;

            Matrix3D result = new Matrix3D();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i, j] = matrix[i, j];

            return result;
        }
    }
}
