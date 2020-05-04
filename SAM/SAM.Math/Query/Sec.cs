namespace SAM.Math
{
    public static partial class Query
    {
        // Secant
        //https://mathworld.wolfram.com/Secant.html
        public static double Sec(double angle)
        {
            return 1 / System.Math.Cos(angle);
        }
    }
}