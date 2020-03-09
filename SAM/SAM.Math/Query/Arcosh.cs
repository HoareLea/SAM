namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Hyperbolic Cosine  (Latin: Area cosinus hyperbolicus)
        //https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions#asinh
        //https://mathworld.wolfram.com/InverseHyperbolicCosine.html
        public static double Arcosh(double x)
        {
            return System.Math.Log(x + System.Math.Sqrt(x * x - 1));
        }
    }
}