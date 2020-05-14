using Newtonsoft.Json.Linq;

namespace SAM.Math
{
    public class Matrix4D : Matrix, ISquareMatrix
    {
        public Matrix4D(JObject jObject) 
            : base(jObject)
        {
        }

        public Matrix4D(Matrix4D matrix4D)
            : base(matrix4D)
        {

        }

        public Matrix4D()
            : base(4, 4)
        {

        }

        public override Matrix Clone()
        {
            return new Matrix4D(this);
        }

        public static Matrix4D GetIdentity()
        {
            Matrix4D matrix4D = new Matrix4D();
            for (int i = 0; i < 4; i++)
                matrix4D[i, i] = 1;

            return matrix4D;
        }

        public static Matrix4D GetUnset()
        {
            return Create.Matrix4D(GetUnset(4, 4));
        }

        public static Matrix4D operator *(Matrix4D matrix4D_1, Matrix4D matrix4D_2)
        {
            if (matrix4D_1 == null || matrix4D_2 == null)
                return null;

            Matrix matrix = ((Matrix)matrix4D_1) * ((Matrix)matrix4D_2);
            if (matrix == null)
                return null;

            return Create.Matrix4D(matrix);
        }

        public static Matrix4D operator *(Matrix4D matrix4D, double value)
        {
            if (matrix4D == null)
                return null;

            Matrix matrix = (Matrix)matrix4D * value;
            if (matrix == null)
                return null;

            return Create.Matrix4D(matrix);
        }

        public static Matrix4D operator +(Matrix4D matrix4D, double value)
        {
            if (matrix4D == null)
                return null;

            Matrix matrix = (Matrix)matrix4D + value;
            if (matrix == null)
                return null;

            return Create.Matrix4D(matrix);
        }

        public static Matrix4D operator +(Matrix4D matrix4D_1, Matrix4D matrix4D_2)
        {
            if (matrix4D_1 == null || matrix4D_2 == null)
                return null;

            Matrix matrix = ((Matrix)matrix4D_1) + ((Matrix)matrix4D_2);
            if (matrix == null)
                return null;

            return Create.Matrix4D(matrix);
        }

        public static Matrix4D operator -(Matrix4D matrix4D_1, Matrix4D matrix4D_2)
        {
            if (matrix4D_1 == null || matrix4D_2 == null)
                return null;

            Matrix matrix = ((Matrix)matrix4D_1) + ((Matrix)matrix4D_2);
            if (matrix == null)
                return null;

            return Create.Matrix4D(matrix);
        }

        public static Matrix4D operator -(Matrix4D matrix4D, double value)
        {
            if (matrix4D == null)
                return null;

            Matrix matrix = (Matrix)matrix4D + (-value);
            if (matrix == null)
                return null;


            return Create.Matrix4D(matrix);
        }

        public static explicit operator Matrix4D(double[,] values)
        {
            return Create.Matrix4D(values);
        }
    }
}
