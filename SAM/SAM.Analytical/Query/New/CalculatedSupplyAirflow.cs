namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedSupplyAirflow(this BuildingModel buildingModel, Zone zone)
        {
            if (buildingModel == null || zone == null)
                return double.NaN;

            return buildingModel.Sum(zone, SpaceParameter.SupplyAirFlow);
        }
    }
}