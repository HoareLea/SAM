namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double OccupancySensibleGain(this Space space)
        {
            if (space == null)
                return double.NaN;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                return double.NaN;

            double result = double.NaN;
            if (internalCondition.TryGetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, out result) && !double.IsNaN(result))
            {
                double occupancy = CalculatedOccupancy(space);
                if (double.IsNaN(occupancy))
                    return double.NaN;

                result = result * occupancy;
            }

            return result;
        }
    }
}