using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculated Supply Air Flow [m3/s] for Space. Sum of SupplyAirFlowPerPerson, SupplyAirFlowPerArea, SupplyAirFlow and SupplyAirChangesPerHour
        /// </summary>
        /// <param name="space">Space</param>
        /// <returns>Supply Air Flow [m3/s]</returns>
        public static double CalculatedSupplyAirFlow(this Space space)
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
                if (!double.IsNaN(area) && area > 0)
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
                if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.SupplyAirFlowPerPerson, out double airFlowPerPerson))
                {
                    airFlow_1 = airFlowPerPerson * occupancy;
                }
            }

            double airFlow_2 = double.NaN;
            if (!double.IsNaN(area) && area > 0)
            {
                if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.SupplyAirFlowPerArea, out double airFlowPerArea))
                {
                    airFlow_2 = airFlowPerArea * area;
                }
            }

            if (!internalCondition.TryGetValue(Analytical.InternalConditionParameter.SupplyAirFlow, out double airFlow_3))
            {
                airFlow_3 = double.NaN;
            }

            double airFlow_4 = double.NaN;
            if (space.TryGetValue(SpaceParameter.Volume, out double volume) && !double.IsNaN(volume) && volume > 0)
            {
                if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.SupplyAirChangesPerHour, out double supplyAirChangesPerHour) && !double.IsNaN(supplyAirChangesPerHour) && supplyAirChangesPerHour > 0)
                {
                    airFlow_4 = (supplyAirChangesPerHour * volume) / 3600;
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

        /// <summary>
        /// Calculated Supply Air Flow [m3/s] for whole Zone. Sum of SupplyAirFlowPerPerson, SupplyAirFlowPerArea, SupplyAirFlow and SupplyAirChangesPerHour for all the spaces in Zone
        /// </summary>
        /// <param name="adjacencyCluster">AdjacencyCluster</param>
        /// <param name="zone">Zone</param>
        /// <returns>Supply Air Flow [m3/s]</returns>
        public static double CalculatedSupplyAirFlow(this AdjacencyCluster adjacencyCluster, Zone zone)
        {
            if (adjacencyCluster == null || zone == null)
            {
                return double.NaN;
            }

            double result = 0;

            List<Space> spaces = adjacencyCluster.GetSpaces(zone);
            if(spaces == null || spaces.Count == 0)
            {
                return result;
            }

            foreach(Space space in spaces)
            {
                double value = space.CalculatedSupplyAirFlow();
                if(double.IsNaN(value))
                {
                    continue;
                }

                result += value;
            }

            return result;
        }

        /// <summary>
        /// Calculated Supply Air Flow [m3/s] for Space including AirSupplyMethod for the system. Sum of SupplyAirFlowPerPerson, SupplyAirFlowPerArea, SupplyAirFlow and SupplyAirChangesPerHour for Outside AirSupplyMethod of the VentilationSystemType
        /// </summary>
        /// <param name="adjacencyCluster">AdjacencyCluster</param>
        /// <param name="space">Space</param>
        /// <returns>Supply Air Flow [m3/s]</returns>
        public static double CalculatedSupplyAirFlow(this AdjacencyCluster adjacencyCluster, Space space)
        {
            if (adjacencyCluster == null || space == null)
            {
                return double.NaN;
            }

            double result = space.CalculatedSupplyAirFlow();
            if (double.IsNaN(result))
            {
                return double.NaN;
            }

            AirSupplyMethod airSupplyMethod = AirSupplyMethod(adjacencyCluster, space, out VentilationSystemType ventilationSystemType);
            switch (airSupplyMethod)
            {
                case Analytical.AirSupplyMethod.Outside:
                    return result;

                case Analytical.AirSupplyMethod.Total:
                    if (space.TryGetValue(SpaceParameter.DesignHeatingLoad, out double designHeatingLoad) && space.TryGetValue(SpaceParameter.DesignCoolingLoad, out double designCoolingLoad))
                    {
                        if (ventilationSystemType.TryGetValue(VentilationSystemTypeParameter.TemperatureDifference, out double temperatureDifference))
                        {
                            double supplyAirFlow_Load = System.Math.Max(designCoolingLoad, designHeatingLoad) / 1.2 / 1.005 / temperatureDifference / 1000;

                            return System.Math.Max(result, supplyAirFlow_Load);
                        }
                    }
                    break;
            }

            return result;
        }
    }
}