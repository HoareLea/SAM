namespace SAM.Units
{
    public static partial class Query
    {
        public static double ToRadians(double value)
        {
            return value * System.Math.PI / 180;
        }
    }
}
