namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedOutsideSupplyAirflow(this ArchitecturalModel architecturalModel, Zone zone)
        {
            if (architecturalModel == null || zone == null)
                return double.NaN;

            return architecturalModel.Sum(zone, SpaceParameter.OutsideSupplyAirFlow);
        }
    }
}