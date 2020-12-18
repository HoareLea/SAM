namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedOccupancySensibleGain(this Space space)
        {
            if (space == null)
                return double.NaN;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                return double.NaN;

            double gain_1 = double.NaN;
            if (internalCondition.TryGetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, out gain_1) && !double.IsNaN(gain_1))
            {
                double occupancy = CalculatedOccupancy(space);
                if (double.IsNaN(occupancy))
                    return double.NaN;

                gain_1 = gain_1 * occupancy;
            }

            double gain_2 = double.NaN;
            internalCondition.TryGetValue(InternalConditionParameter.OccupancySensibleGain, out gain_2);

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