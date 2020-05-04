namespace SAM.Math
{
    public static partial class Query
    {
        // Cosecant
        //https://mathworld.wolfram.com/Cosecant.html
        public static double Cosec(double angle)
        {
            return 1 / System.Math.Sin(angle);
        }
    }
}