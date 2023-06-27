namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculated Exhaust Air Flow Per Person [m3/s/p] for Space. Sum of ExhaustAirFlowPerPerson, ExhaustAirFlowPerArea, ExhaustAirFlow and ExhaustAirChangesPerHour
        /// </summary>
        /// <param name="space">Space</param>
        /// <returns>Exhaust Air Flow Per Person [m3/s/p]</returns>
        public static double CalculatedExhaustAirFlowPerPerson(this Space space)
        {
            double ExhaustAirFlow = CalculatedExhaustAirFlow(space);
            if (double.IsNaN(ExhaustAirFlow))
            {
                return double.NaN;
            }

            double occupancy = CalculatedOccupancy(space);
            if (double.IsNaN(occupancy))
            {
                return double.NaN;
            }

            if (occupancy == 0)
            {
                return 0;
            }

            return ExhaustAirFlow / occupancy;
        }
    }
}