namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculated Exhaust Air Flow Per Area [m3/s/m2] for Space. Sum of ExhaustAirFlowPerPerson, ExhaustAirFlowPerArea, ExhaustAirFlow and ExhaustAirChangesPerHour
        /// </summary>
        /// <param name="space">Space</param>
        /// <returns>Exhaust Air Flow Per Area [m3/s/m2]</returns>
        public static double CalculatedExhaustAirFlowPerArea(this Space space)
        {
            double ExhaustAirFlow = CalculatedExhaustAirFlow(space);
            if (double.IsNaN(ExhaustAirFlow))
            {
                return double.NaN;
            }

            if (!space.TryGetValue(SpaceParameter.Area, out double area) || double.IsNaN(area))
            {
                return double.NaN;
            }

            if (area == 0)
            {
                return 0;
            }

            return ExhaustAirFlow / area;
        }
    }
}