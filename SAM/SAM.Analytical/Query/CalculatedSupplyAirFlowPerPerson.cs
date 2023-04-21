using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculated Supply Air Flow Per Person [m3/s/p] for Space. Sum of SupplyAirFlowPerPerson, SupplyAirFlowPerArea, SupplyAirFlow and SupplyAirChangesPerHour
        /// </summary>
        /// <param name="space">Space</param>
        /// <returns>Supply Air Flow Per Person [m3/s/p]</returns>
        public static double CalculatedSupplyAirFlowPerPerson(this Space space)
        {
            double supplyAirFlow = CalculatedSupplyAirFlow(space);
            if (double.IsNaN(supplyAirFlow))
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

            return supplyAirFlow / occupancy;
        }
    }
}