namespace SAM.Math
{
    public static partial class Create
    {
        public static Matrix4D Matrix4D(Matrix matrix)
        {
            if (matrix == null)
                return null;

            if (matrix.RowCount() != 4 || matrix.ColumnCount() != 4)
                return null;

            Matrix4D result = new Matrix4D();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    result[i, j] = matrix[i, j];

            return result;
        }

        public static Matrix4D Matrix4D(double[,] values)
        {
            if (values == null)
                return null;

            if (values.GetLength(0) != 4 || values.GetLength(1) != 4)
                return null;

            Matrix4D result = new Matrix4D();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    result[i, j] = values[i, j];

            return result;
        }
    }
}
