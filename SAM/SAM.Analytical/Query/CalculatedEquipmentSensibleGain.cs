namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedEquipmentSensibleGain(this Space space)
        {
            if (space == null)
                return double.NaN;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                return double.NaN;

            double gain_1 = double.NaN;
            if(internalCondition.TryGetValue(Analytical.InternalConditionParameter.EquipmentSensibleGainPerArea, out gain_1) && !double.IsNaN(gain_1))
            {
                double area = double.NaN;
                if (!space.TryGetValue(SpaceParameter.Area, out area) || double.IsNaN(area))
                    return double.NaN;

                gain_1 = gain_1 * area;
            }

            double gain_2 = double.NaN;
            internalCondition.TryGetValue(Analytical.InternalConditionParameter.EquipmentSensibleGain, out gain_2);

            double gain_3 = double.NaN;
            if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.EquipmentSensibleGainPerPerson, out gain_3) && !double.IsNaN(gain_3))
            {
                double occupancy = CalculatedOccupancy(space);
                if (double.IsNaN(occupancy))
                {
                    return double.NaN;
                }

                gain_3 = gain_3 * occupancy;
            }

            if (double.IsNaN(gain_1) && double.IsNaN(gain_2) && double.IsNaN(gain_3))
            {
                return double.NaN;
            }

            if (double.IsNaN(gain_1))
            {
                gain_1 = 0;
            }


            if (double.IsNaN(gain_2))
            {
                gain_2 = 0;
            }

            if (double.IsNaN(gain_3))
            {
                gain_3 = 0;
            }

            return gain_1 + gain_2 + gain_3;
        }
    }
}