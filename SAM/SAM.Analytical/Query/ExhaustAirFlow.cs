namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double ExhaustAirFlow(this Space space)
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
                    double occupancyPerArea = double.NaN;
                    if (internalCondition.TryGetValue(InternalConditionParameter.OccupancyPerArea, out occupancyPerArea))
                        occupancy = occupancyPerArea * area;
                }
            }

            double airFlow_1 = double.NaN;
            if (!double.IsNaN(occupancy))
            {
                double airFlowPerPerson = double.NaN;
                if (internalCondition.TryGetValue(InternalConditionParameter.ExhaustAirFlowPerPerson, out airFlowPerPerson))
                    airFlow_1 = airFlowPerPerson * occupancy;
            }

            double airFlow_2 = double.NaN;
            if (!double.IsNaN(area))
            {
                double airFlowPerArea = double.NaN;
                if (internalCondition.TryGetValue(InternalConditionParameter.ExhaustAirFlowPerArea, out airFlowPerArea))
                    airFlow_2 = airFlowPerArea * area;
            }

            double airFlow_3 = double.NaN;
            if (!internalCondition.TryGetValue(InternalConditionParameter.ExhaustAirFlow, out airFlow_3))
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
    }
}