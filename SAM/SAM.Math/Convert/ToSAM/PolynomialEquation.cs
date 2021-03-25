using System.Collections.Generic;

namespace SAM.Math
{
    public static partial class Convert
    {
        public static PolynomialEquation ToSAM(this MathNet.Numerics.Polynomial polynomial)
        {
            if (polynomial == null)
                return null;

            return new PolynomialEquation(polynomial.Coefficients);
        }
    }
}