namespace SAM.Math.Rhino
{
    /// <summary>
    /// Provides methods to convert between different matrix types.
    /// </summary>
    public static partial class Convert
    {
        /// <summary>
        /// Converts a SAM.Math.Matrix to a Rhino.Geometry.Matrix.
        /// </summary>
        /// <param name="matrix">The SAM.Math.Matrix instance to convert.</param>
        /// <returns>A new Rhino.Geometry.Matrix instance with the same values as the input matrix, or null if the input matrix is null.</returns>
        public static global::Rhino.Geometry.Matrix ToRhino(this Matrix matrix)
        {
            // If the input matrix is null, return null
            if (matrix == null)
                return null;

            // Get the number of rows and columns in the input matrix
            int count_Rows = matrix.RowCount();
            int count_Columns = matrix.ColumnCount();

            // Create a new Rhino.Geometry.Matrix with the same dimensions as the input matrix
            global::Rhino.Geometry.Matrix result = new global::Rhino.Geometry.Matrix(count_Rows, count_Columns);

            // Iterate through each element in the input matrix
            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    // Copy the value from the input matrix to the new Rhino.Geometry.Matrix
                    result[i, j] = matrix[i, j];

            // Return the new Rhino.Geometry.Matrix with the same values as the input matrix
            return result;
        }
    }
}
