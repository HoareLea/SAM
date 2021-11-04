namespace SAM.Math.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Matrix ToRhino(this Matrix matrix)
        {
            if (matrix == null)
                return null;

            int count_Rows = matrix.RowCount();
            int count_Columns = matrix.ColumnCount();

            global::Rhino.Geometry.Matrix result = new global::Rhino.Geometry.Matrix(count_Rows, count_Columns);
            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    result[i, j] = matrix[i, j];

            return result;
        }
    }
}