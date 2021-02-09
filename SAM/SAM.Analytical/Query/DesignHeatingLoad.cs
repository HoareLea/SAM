using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double DesignHeatingLoad(this AdjacencyCluster adjacencyCluster, ZoneSimulationResult zoneSimulationResult)
        {
            if (adjacencyCluster == null || zoneSimulationResult == null)
                return double.NaN;
            
            return DesignHeatingLoad(adjacencyCluster, adjacencyCluster.GetRelatedObjects<Zone>(zoneSimulationResult)?.FirstOrDefault());
        }


        public static double DesignHeatingLoad(this AdjacencyCluster adjacencyCluster, Zone zone)
        {
            List<Space> spaces = adjacencyCluster?.GetSpaces(zone);
            if (spaces == null || spaces.Count == 0)
                return double.NaN;

            double result = 0;
            foreach(Space space in spaces)
            {
                if (space == null)
                    continue;

                if (!space.TryGetValue(SpaceParameter.DesignHeatingLoad, out double load) || double.IsNaN(load))
                    continue;

                result += load;
            }

            return result;
        }
    }
}