namespace SAM.Math
{
    public static partial class Query
    {
        // Hyperbolic Tangent
        //https://mathworld.wolfram.com/HyperbolicTangent.html
        public static double Tanh(double angle)
        {
            return (System.Math.Exp(angle) - System.Math.Exp(-angle)) / (System.Math.Exp(angle) + System.Math.Exp(-angle));
        }
    }
}