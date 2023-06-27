namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculated Supply Air Changes Per Hour [ACH] for Space. Sum of SupplyAirFlowPerPerson, SupplyAirFlowPerArea, SupplyAirFlow and SupplyAirChangesPerHour
        /// </summary>
        /// <param name="space">Space</param>
        /// <returns>Supply Air Changes Per Hour [ACH]</returns>
        public static double CalculatedSupplyAirChangesPerHour(this Space space)
        {
            double supplyAirFlow = CalculatedSupplyAirFlow(space);
            if (double.IsNaN(supplyAirFlow))
            {
                return double.NaN;
            }

            if (!space.TryGetValue(SpaceParameter.Volume, out double volume) || double.IsNaN(volume))
            {
                return double.NaN;
            }

            if (volume == 0)
            {
                return 0;
            }

            return supplyAirFlow / volume * 3600;
        }
    }
}