namespace SAM.Math
{
    public static partial class Query
    {
        // Logarithm to base N
        //https://mathworld.wolfram.com/Logarithm.html
        public static double LogN(double x, double n)
        {
            return System.Math.Log(x) / System.Math.Log(n);
        }
    }
}