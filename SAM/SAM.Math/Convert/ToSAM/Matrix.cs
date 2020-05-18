using System.Collections.Generic;

namespace SAM.Math
{
    public static partial class Convert
    {
        public static  Matrix ToSAM(this MathNet.Numerics.LinearAlgebra.Matrix<double> matrix)
        {
            if (matrix == null)
                return null;

            return new Matrix(matrix.ToArray());
        }
    }
}