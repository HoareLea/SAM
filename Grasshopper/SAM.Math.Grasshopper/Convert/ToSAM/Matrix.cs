using Grasshopper.Kernel.Types;

namespace SAM.Math.Grasshopper
{
    public static partial class Convert
    {
        public static Matrix ToSAM(this Rhino.Geometry.Matrix matrix)
        {
            if (matrix == null)
                return null;

            Matrix result = new Matrix(matrix.RowCount, matrix.ColumnCount);
            for (int i = 0; i < matrix.RowCount; i++)
                for (int j = 0; j < matrix.ColumnCount; j++)
                    result[i, j] = matrix[i, j];

            if(result.IsSquare())
            {
                if (matrix.RowCount == 2)
                    return Create.Matrix2D(result);

                if (matrix.RowCount == 3)
                    return Create.Matrix3D(result);

                if (matrix.RowCount == 4)
                    return Create.Matrix4D(result);
            }


            return result;
        }

        public static Matrix ToSAM(this GH_Matrix matrix)
        {
            return ToSAM(matrix.Value);
        }
    }
}