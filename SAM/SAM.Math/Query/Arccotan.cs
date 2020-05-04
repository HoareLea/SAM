namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Cotangent
        //https://mathworld.wolfram.com/InverseCotangent.html
        public static double Arccotan(double angle)
        {
            return 2 * System.Math.Atan(1) - System.Math.Atan(angle);
        }
    }
}