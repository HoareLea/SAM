using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<T> Systems<T>(this AdjacencyCluster adjacencyCluster, Space space) where T : MechanicalSystem
        {
            if(adjacencyCluster == null || space == null)
            {
                return null;
            }

            return adjacencyCluster.GetRelatedObjects<T>(space);
        }

        public static List<T> Systems<T>(this AnalyticalModel analyticalModel, Space space) where T : MechanicalSystem
        {
            if (analyticalModel == null || space == null)
            {
                return null;
            }

            return analyticalModel.AdjacencyCluster?.GetRelatedObjects<T>(space);
        }
    }
}