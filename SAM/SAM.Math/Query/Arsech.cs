namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Hyperbolic Secant (Latin: Area secans hyperbolicus)
        //https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions#asinh
        //https://mathworld.wolfram.com/InverseHyperbolicSecant.html
        public static double Arsech(double x)
        {
            return System.Math.Log((System.Math.Sqrt(-x * x + 1) + 1) / x);
        }
    }
}