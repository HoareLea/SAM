namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedOccupancyGain(this Space space)
        {
            if (space == null)
                return double.NaN;

            double gain_1 = CalculatedOccupancyLatentGain(space);

            double gain_2 = CalculatedOccupancySensibleGain(space);

            if (double.IsNaN(gain_1) && double.IsNaN(gain_2))
                return double.NaN;

            if (double.IsNaN(gain_1))
                return gain_2;

            if (double.IsNaN(gain_2))
                return gain_1;

            return gain_1 + gain_2;
        }
    }
}