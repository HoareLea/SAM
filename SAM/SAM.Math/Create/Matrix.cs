using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Math
{
    public static partial class Create
    {
        public static Matrix Matrix(Matrix matrix)
        {
            if (matrix == null)
                return null;

            if(matrix.IsSquare())
            {
                int count = matrix.RowCount();

                if (count == 2)
                    return Matrix2D(matrix);

                if (count == 3)
                    return Matrix3D(matrix);

                if (count == 4)
                    return Matrix4D(matrix);
            }

            return new Matrix(matrix);
        }

        public static Matrix Matrix(JObject jObject)
        {
            if (jObject == null)
                return null;

            return Core.Query.IJSAMObject(jObject) as Matrix;
        }
    }
}
