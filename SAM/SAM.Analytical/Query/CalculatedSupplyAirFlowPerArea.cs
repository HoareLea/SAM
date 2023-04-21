using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculated Supply Air Flow Per Area [m3/s/m2] for Space. Sum of SupplyAirFlowPerPerson, SupplyAirFlowPerArea, SupplyAirFlow and SupplyAirChangesPerHour
        /// </summary>
        /// <param name="space">Space</param>
        /// <returns>Supply Air Flow Per Area [m3/s/m2]</returns>
        public static double CalculatedSupplyAirFlowPerArea(this Space space)
        {
            double supplyAirFlow = CalculatedSupplyAirFlow(space);
            if (double.IsNaN(supplyAirFlow))
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

            return supplyAirFlow / area;
        }
    }
}