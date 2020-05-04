namespace SAM.Math
{
    public static partial class Query
    {
        // Hyperbolic Cotangent
        //https://mathworld.wolfram.com/HyperbolicCotangent.html
        public static double Coth(double angle)
        {
            return (System.Math.Exp(angle) + System.Math.Exp(-angle)) / (System.Math.Exp(angle) - System.Math.Exp(-angle));
        }
    }
}