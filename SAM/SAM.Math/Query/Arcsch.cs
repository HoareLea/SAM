namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Hyperbolic Cosecant (Latin: Area cosecans hyperbolicus)
        //https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions#asinh
        //https://mathworld.wolfram.com/InverseHyperbolicCosecant.html
        public static double Arcsch(double x)
        {
            return System.Math.Log((System.Math.Sign(x) * System.Math.Sqrt(x * x + 1) + 1) / x);
        }
    }
}