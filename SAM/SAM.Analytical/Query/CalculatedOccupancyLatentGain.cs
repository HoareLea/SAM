namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedOccupancyLatentGain(this Space space)
        {
            if (space == null)
                return double.NaN;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                return double.NaN;

            double gain_1 = double.NaN;
            if(internalCondition.TryGetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, out gain_1) && !double.IsNaN(gain_1))
            {
                double occupancy = space.CalculatedOccupancy();
                if(!double.IsNaN(occupancy))
                {
                    gain_1 = gain_1 * occupancy;
                }
            }

            return gain_1;
        }
    }
}