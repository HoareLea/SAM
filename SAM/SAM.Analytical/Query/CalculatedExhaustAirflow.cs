using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculated Exhaust Air Flow [m3/s] for Space. Sum of ExhaustAirFlowPerPerson, ExhaustAirFlowPerArea, ExhaustAirFlow and ExhaustAirChangesPerHour
        /// </summary>
        /// <param name="space">Space</param>
        /// <returns>Exhaust Air Flow [m3/s]</returns>
        public static double CalculatedExhaustAirFlow(this Space space)
        {
            InternalCondition internalCondition = space?.InternalCondition;
            if (internalCondition == null)
            {
                return double.NaN;
            }

            if (!space.TryGetValue(SpaceParameter.Area, out double area))
            {
                area = double.NaN;
            }

            if (!space.TryGetValue(SpaceParameter.Occupancy, out double occupancy))
            {
                occupancy = double.NaN;
            }

            if (double.IsNaN(occupancy))
            {
                if (!double.IsNaN(area))
                {
                    if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.AreaPerPerson, out double areaPerPerson))
                    {
                        occupancy = area / areaPerPerson;
                    }
                }
            }

            double airFlow_1 = double.NaN;
            if (!double.IsNaN(occupancy) && occupancy > 0)
            {
                if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.ExhaustAirFlowPerPerson, out double airFlowPerPerson))
                {
                    airFlow_1 = airFlowPerPerson * occupancy;
                }
            }

            double airFlow_2 = double.NaN;
            if (!double.IsNaN(area) && area > 0)
            {
                if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.ExhaustAirFlowPerArea, out double airFlowPerArea))
                {
                    airFlow_2 = airFlowPerArea * area;
                }
            }

            if (!internalCondition.TryGetValue(Analytical.InternalConditionParameter.ExhaustAirFlow, out double airFlow_3))
            {
                airFlow_3 = double.NaN;
            }

            double airFlow_4 = double.NaN;
            if (space.TryGetValue(SpaceParameter.Volume, out double volume) && !double.IsNaN(volume) && volume > 0)
            {
                if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.ExhaustAirChangesPerHour, out double exhaustAirChangesPerHour) && !double.IsNaN(exhaustAirChangesPerHour) && exhaustAirChangesPerHour > 0)
                {
                    airFlow_4 = (exhaustAirChangesPerHour * volume) / 3600;
                }
            }

            if (double.IsNaN(airFlow_1) && double.IsNaN(airFlow_2) && double.IsNaN(airFlow_3) && double.IsNaN(airFlow_4))
            {
                return double.NaN;
            }

            double result = 0;

            if (!double.IsNaN(airFlow_1))
            {
                result += airFlow_1;
            }

            if (!double.IsNaN(airFlow_2))
            {
                result += airFlow_2;
            }

            if (!double.IsNaN(airFlow_3))
            {
                result += airFlow_3;
            }

            if (!double.IsNaN(airFlow_4))
            {
                result += airFlow_4;
            }

            return result;
        }

        public static double CalculatedExhaustAirFlow(this AdjacencyCluster adjacencyCluster, Zone zone)
        {
            if (adjacencyCluster == null || zone == null)
            {
                return double.NaN;
            }


            if (adjacencyCluster == null || zone == null)
            {
                return double.NaN;
            }

            double result = 0;

            List<Space> spaces = adjacencyCluster.GetSpaces(zone);
            if (spaces == null || spaces.Count == 0)
            {
                return result;
            }

            foreach (Space space in spaces)
            {
                double value = space.CalculatedExhaustAirFlow();
                if (double.IsNaN(value))
                {
                    continue;
                }

                result += value;
            }

            return result;
        }
    }
}