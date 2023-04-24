using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<MechanicalSystem> MechanicalSystems(this AdjacencyCluster adjacencyCluster, Space space)
        {
            if(adjacencyCluster == null || space == null)
            {
                return null;
            }

            List<MechanicalSystem> result = adjacencyCluster.GetRelatedObjects<MechanicalSystem>(space);

            return result;
        }

        public static List<MechanicalSystem> MechanicalSystems(this AdjacencyCluster adjacencyCluster, Space space, MechanicalSystemCategory mechanicalSystemCategory)
        {
            List<MechanicalSystem> mechanicalSystems = MechanicalSystems(adjacencyCluster, space);
            if(mechanicalSystems == null || mechanicalSystems.Count == 0)
            {
                return null;
            }

            return mechanicalSystems.FindAll(x => x.MechanicalSystemCategory() == mechanicalSystemCategory);
        }
    }
}