namespace SAM.Units
{
    public static partial class Convert
    {
        public static double ToDegrees(double value)
        {
            return value * Factor.RadiansToDegrees;
        }
    }
}