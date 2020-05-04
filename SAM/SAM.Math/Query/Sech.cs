namespace SAM.Math
{
    public static partial class Query
    {
        // Hyperbolic Secant
        //https://mathworld.wolfram.com/HyperbolicSecant.html
        public static double Sech(double angle)
        {
            return 2 / (System.Math.Exp(angle) + System.Math.Exp(-angle));
        }
    }
}