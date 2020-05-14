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

        public static Matrix3D Matrix3D(double[,] values)
        {
            if (values == null)
                return null;

            if (values.GetLength(0) != 3 || values.GetLength(1) != 3)
                return null;

            Matrix3D result = new Matrix3D();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i, j] = values[i, j];

            return result;
        }
    }
}
