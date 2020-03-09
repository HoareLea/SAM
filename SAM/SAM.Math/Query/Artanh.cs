namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Hyperbolic Tangent (Latin: Area tangens hyperbolicus)
        //https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions#asinh
        //https://mathworld.wolfram.com/InverseHyperbolicTangent.html
        public static double Artanh(double x)
        {
            return System.Math.Log((1 + x) / (1 - x)) / 2;
        }
    }
}