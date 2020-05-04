namespace SAM.Math
{
    public static partial class Query
    {
        // Hyperbolic Cosine
        public static double Cosh(double angle)
        {
            return (System.Math.Exp(angle) + System.Math.Exp(-angle)) / 2;
        }
    }
}