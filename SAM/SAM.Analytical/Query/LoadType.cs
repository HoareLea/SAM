namespace SAM.Analytical
{
    public static partial class Query
    {
        public static LoadType LoadType(this SurfaceSimulationResult surfaceSimulationResult)
        {
            if (surfaceSimulationResult == null)
                return Analytical.LoadType.Undefined;

            if (!surfaceSimulationResult.TryGetValue(SurfaceSimulationResultParameter.LoadType, out string text) || string.IsNullOrWhiteSpace(text))
                return Analytical.LoadType.Undefined;

            return Core.Query.Enum<LoadType>(text);
        }

        public static LoadType LoadType(this ZoneSimulationResult zoneSimulationResult)
        {
            if (zoneSimulationResult == null)
                return Analytical.LoadType.Undefined;

            if (!zoneSimulationResult.TryGetValue(ZoneSimulationResultParameter.LoadType, out string text) || string.IsNullOrWhiteSpace(text))
                return Analytical.LoadType.Undefined;

            return Core.Query.Enum<LoadType>(text);
        }

        public static LoadType LoadType(this SpaceSimulationResult spaceSimulationResult)
        {
            if (spaceSimulationResult == null)
                return Analytical.LoadType.Undefined;

            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.LoadType, out string text) || string.IsNullOrWhiteSpace(text))
                return Analytical.LoadType.Undefined;

            return Core.Query.Enum<LoadType>(text);
        }

        public static LoadType LoadType(this AdjacencyClusterSimulationResult adjacencyClusterSimulationResult)
        {
            if (adjacencyClusterSimulationResult == null)
                return Analytical.LoadType.Undefined;

            if (!adjacencyClusterSimulationResult.TryGetValue(AdjacencyClusterSimulationResultParameter.LoadType, out string text) || string.IsNullOrWhiteSpace(text))
                return Analytical.LoadType.Undefined;

            return Core.Query.Enum<LoadType>(text);
        }
    }
}