namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Cosecant
        //https://mathworld.wolfram.com/InverseCosecant.html
        public static double Arccosec(double angle)
        {
            return System.Math.Atan(System.Math.Sign(angle) / System.Math.Sqrt(angle * angle - 1));
        }
    }
}