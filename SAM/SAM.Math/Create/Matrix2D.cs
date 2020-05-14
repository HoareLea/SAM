namespace SAM.Math
{
    public static partial class Create
    {
        public static Matrix2D Matrix2D(Matrix matrix)
        {
            if (matrix == null)
                return null;

            if (matrix.RowCount() != 2 || matrix.ColumnCount() != 2)
                return null;

            Matrix2D result = new Matrix2D();
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    result[i, j] = matrix[i, j];

            return result;
        }

        public static Matrix2D Matrix2D(double[,] values)
        {
            if (values == null)
                return null;

            if (values.GetLength(0) != 2 || values.GetLength(1) != 2)
                return null;

            Matrix2D result = new Matrix2D();
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    result[i, j] = values[i, j];

            return result;
        }
    }
}
