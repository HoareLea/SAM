namespace SAM.Units
{
    public static partial class Query
    {
        public static double ToDegrees(double value)
        {
            return value * 180 / System.Math.PI;
        }
    }
}
