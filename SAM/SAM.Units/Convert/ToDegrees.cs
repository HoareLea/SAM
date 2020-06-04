namespace SAM.Units
{
    public static partial class Convert
    {
        private const double factor = 180 / System.Math.PI;


        public static double ToDegrees(double value)
        {
            return value * factor;
        }
    }
}