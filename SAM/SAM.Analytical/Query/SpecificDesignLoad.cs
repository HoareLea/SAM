namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double SpecificDesignLoad(this SpaceSimulationResult spaceSimulationResult)
        {
            if (spaceSimulationResult == null)
                return double.NaN;

            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.DesignLoad, out double load) || double.IsNaN(load))
                return double.NaN;

            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.Area, out double area) || double.IsNaN(area))
                return double.NaN;

            if (area == 0)
                return 0;

            return load / area;
        }
    }
}