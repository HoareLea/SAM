namespace SAM.Math
{
    public static partial class Convert
    {
        public static MathNet.Numerics.Polynomial ToMathNet(this PolynomialEquation polynomialEquation)
        {
            if (polynomialEquation == null)
                return null;

            return new MathNet.Numerics.Polynomial(polynomialEquation.Coefficients);
        }
    }
}