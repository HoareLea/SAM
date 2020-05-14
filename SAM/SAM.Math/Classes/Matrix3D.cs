using Newtonsoft.Json.Linq;

namespace SAM.Math
{
    public class Matrix3D : Matrix, ISquareMatrix
    {
        public Matrix3D(JObject jObject) 
            : base(jObject)
        {
        }

        public Matrix3D(Matrix3D matrix3D)
            : base(matrix3D)
        {

        }

        public Matrix3D()
            : base(3, 3)
        {

        }

        public override Matrix Clone()
        {
            return new Matrix3D(this);
        }

        public static Matrix3D GetIdentity()
        {
            Matrix3D matrix3D = new Matrix3D();
            for (int i = 0; i < 3; i++)
                matrix3D[i, i] = 1;

            return matrix3D;
        }

        public static Matrix3D operator *(Matrix3D matrix3D_1, Matrix3D matrix3D_2)
        {
            if (matrix3D_1 == null || matrix3D_2 == null)
                return null;

            Matrix matrix = ((Matrix)matrix3D_1) * ((Matrix)matrix3D_2);
            if (matrix == null)
                return null;

            return Create.Matrix3D(matrix);
        }

        public static Matrix3D operator *(Matrix3D matrix3D, double value)
        {
            if (matrix3D == null)
                return null;

            Matrix matrix = (Matrix)matrix3D * value;
            if (matrix == null)
                return null;

            return Create.Matrix3D(matrix);
        }

        public static Matrix3D operator +(Matrix3D matrix3D, double value)
        {
            if (matrix3D == null)
                return null;

            Matrix matrix = (Matrix)matrix3D + value;
            if (matrix == null)
                return null;

            return Create.Matrix3D(matrix);
        }

        public static Matrix3D operator +(Matrix3D matrix3D_1, Matrix3D matrix3D_2)
        {
            if (matrix3D_1 == null || matrix3D_2 == null)
                return null;

            Matrix matrix = ((Matrix)matrix3D_1) + ((Matrix)matrix3D_2);
            if (matrix == null)
                return null;

            return Create.Matrix3D(matrix);
        }

        public static Matrix3D operator -(Matrix3D matrix3D_1, Matrix3D matrix3D_2)
        {
            if (matrix3D_1 == null || matrix3D_2 == null)
                return null;

            Matrix matrix = ((Matrix)matrix3D_1) + ((Matrix)matrix3D_2);
            if (matrix == null)
                return null;

            return Create.Matrix3D(matrix);
        }

        public static Matrix3D operator -(Matrix3D matrix3D, double value)
        {
            if (matrix3D == null)
                return null;

            Matrix matrix = (Matrix)matrix3D + (-value);
            if (matrix == null)
                return null;


            return Create.Matrix3D(matrix);
        }

        public static explicit operator Matrix3D(double[,] values)
        {
            return Create.Matrix3D(values);
        }
    }
}
