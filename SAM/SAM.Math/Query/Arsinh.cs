namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Hyperbolic Sine (Latin: Area sinus hyperbolicus)
        //https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions#asinh
        //https://mathworld.wolfram.com/InverseHyperbolicSine.html
        public static double Arsinh(double x)
        {
            return System.Math.Log(x + System.Math.Sqrt(x * x + 1));
        }
    }
}