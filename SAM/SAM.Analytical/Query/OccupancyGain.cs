namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double OccupancyGain(this Space space)
        {
            if (space == null)
                return double.NaN;

            double gain_1 = OccupancyLatentGain(space);

            double gain_2 = OccupancySensibleGain(space);

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