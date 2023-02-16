using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedSupplyAirFlow(this Space space)
        {
            if (space == null)
                return double.NaN;

            InternalCondition internalCondition = space.InternalCondition;

            if (internalCondition == null)
                return double.NaN;

            double area;
            if (!space.TryGetValue(SpaceParameter.Area, out area))
                area = double.NaN;

            double occupancy;
            if (!space.TryGetValue(SpaceParameter.Occupancy, out occupancy))
                occupancy = double.NaN;

            if (double.IsNaN(occupancy))
            {
                if (!double.IsNaN(area))
                {
                    double areaPerPerson = double.NaN;
                    if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.AreaPerPerson, out areaPerPerson))
                        occupancy = area / areaPerPerson;
                }
            }

            double airFlow_1 = double.NaN;
            if (!double.IsNaN(occupancy))
            {
                double airFlowPerPerson = double.NaN;
                if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.SupplyAirFlowPerPerson, out airFlowPerPerson))
                    airFlow_1 = airFlowPerPerson * occupancy;
            }

            double airFlow_2 = double.NaN;
            if (!double.IsNaN(area))
            {
                double airFlowPerArea = double.NaN;
                if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.SupplyAirFlowPerArea, out airFlowPerArea))
                    airFlow_2 = airFlowPerArea * area;
            }

            double airFlow_3 = double.NaN;
            if (!internalCondition.TryGetValue(Analytical.InternalConditionParameter.SupplyAirFlow, out airFlow_3))
                airFlow_3 = double.NaN;

            if (double.IsNaN(airFlow_1) && double.IsNaN(airFlow_2) && double.IsNaN(airFlow_3))
                return double.NaN;

            double result = 0;

            if (!double.IsNaN(airFlow_1))
                result += airFlow_1;

            if (!double.IsNaN(airFlow_2))
                result += airFlow_2;

            if (!double.IsNaN(airFlow_3))
                result += airFlow_3;

            return result;
        }

        public static double CalculatedSupplyAirFlow(this AdjacencyCluster adjacencyCluster, Zone zone)
        {
            if (adjacencyCluster == null || zone == null)
                return double.NaN;

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