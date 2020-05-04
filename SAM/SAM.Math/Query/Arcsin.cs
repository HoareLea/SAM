namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Sine
        public static double Arcsin(double angle)
        {
            return System.Math.Atan(angle / System.Math.Sqrt(-angle * angle + 1));
        }
    }
}