namespace SAM.Math
{
    public static partial class Query
    {
        // Cotangent
        //https://mathworld.wolfram.com/Cotangent.html
        public static double Cotan(double angle)
        {
            return 1 / System.Math.Tan(angle);
        }
    }
}