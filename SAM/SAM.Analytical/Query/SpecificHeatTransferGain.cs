namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double SpecificBuildingHeatTransferGain(this SpaceSimulationResult spaceSimulationResult)
        {
            if (spaceSimulationResult == null)
                return double.NaN;

            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.BuildingHeatTransfer, out double buildingHeatTransfer) || double.IsNaN(buildingHeatTransfer))
            {
                return double.NaN;
            }

            double area = double.NaN;
            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.Area, out area) || double.IsNaN(area))
            {
                return double.NaN;
            }

            if (area == 0)
            {
                return 0;
            }

            return buildingHeatTransfer / area;
        }
    }
}