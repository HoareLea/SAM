using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculated Exhaust Air Changes Per Hour [ACH] for Space. Sum of ExhaustAirFlowPerPerson, ExhaustAirFlowPerArea, ExhaustAirFlow and ExhaustAirChangesPerHour
        /// </summary>
        /// <param name="space">Space</param>
        /// <returns>Exhaust Air Changes Per Hour [ACH]</returns>
        public static double CalculatedExhaustAirChangesPerHour(this Space space)
        {
            double supplyAirFlow = CalculatedExhaustAirFlow(space);
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