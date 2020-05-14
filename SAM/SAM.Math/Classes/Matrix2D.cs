using Newtonsoft.Json.Linq;

namespace SAM.Math
{
    public class Matrix2D : Matrix, ISquareMatrix
    {
        public Matrix2D(JObject jObject) 
            : base(jObject)
        {
        }

        public Matrix2D(Matrix2D matrix2D)
            : base(matrix2D)
        {

        }

        public Matrix2D()
            : base(2, 2)
        {

        }

        public override Matrix Clone()
        {
            return new Matrix2D(this);
        }

        public static Matrix2D GetIdentity()
        {
            Matrix2D matrix2D = new Matrix2D();
            for (int i = 0; i < 2; i++)
                matrix2D[i, i] = 1;

            return matrix2D;
        }

        public static Matrix2D operator *(Matrix2D matrix2D_1, Matrix2D matrix2D_2)
        {
            if (matrix2D_1 == null || matrix2D_2 == null)
                return null;

            Matrix matrix = ((Matrix)matrix2D_1) * ((Matrix)matrix2D_2);
            if (matrix == null)
                return null;

            return Create.Matrix2D(matrix);
        }

        public static Matrix2D operator *(Matrix2D matrix2D, double value)
        {
            if (matrix2D == null)
                return null;

            Matrix matrix = (Matrix)matrix2D * value;
            if (matrix == null)
                return null;

            return Create.Matrix2D(matrix);
        }

        public static Matrix2D operator +(Matrix2D matrix2D, double value)
        {
            if (matrix2D == null)
                return null;

            Matrix matrix = (Matrix)matrix2D + value;
            if (matrix == null)
                return null;

            return Create.Matrix2D(matrix);
        }

        public static Matrix2D operator +(Matrix2D matrix2D_1, Matrix2D matrix2D_2)
        {
            if (matrix2D_1 == null || matrix2D_2 == null)
                return null;

            Matrix matrix = ((Matrix)matrix2D_1) + ((Matrix)matrix2D_2);
            if (matrix == null)
                return null;

            return Create.Matrix2D(matrix);
        }

        public static Matrix2D operator -(Matrix2D matrix2D_1, Matrix2D matrix2D_2)
        {
            if (matrix2D_1 == null || matrix2D_2 == null)
                return null;

            Matrix matrix = ((Matrix)matrix2D_1) + ((Matrix)matrix2D_2);
            if (matrix == null)
                return null;

            return Create.Matrix2D(matrix);
        }

        public static Matrix2D operator -(Matrix2D matrix2D, double value)
        {
            if (matrix2D == null)
                return null;

            Matrix matrix = (Matrix)matrix2D + (-value);
            if (matrix == null)
                return null;


            return Create.Matrix2D(matrix);
        }

        public static explicit operator Matrix2D(double[,] values)
        {
            return Create.Matrix2D(values);
        }
    }
}
