namespace SAM.Math
{
    public static partial class Query
    {
        // Hyperbolic Sine
        //https://mathworld.wolfram.com/HyperbolicSine.html
        public static double Sinh(double angle)
        {
            return (System.Math.Exp(angle) - System.Math.Exp(-angle)) / 2;
        }
    }
}