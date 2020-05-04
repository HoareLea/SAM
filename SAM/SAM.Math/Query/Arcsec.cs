namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Secant
        //https://mathworld.wolfram.com/InverseSecant.html
        public static double Arcsec(double x)
        {
            return 2 * System.Math.Atan(1) - System.Math.Atan(System.Math.Sign(x) / System.Math.Sqrt(x * x - 1));
        }
    }
}