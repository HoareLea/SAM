using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void RemoveExternalSpaces(this AdjacencyCluster adjacencyCluster, IEnumerable<ExternalSpace> externalSpaces)
        {
            if (adjacencyCluster == null)
            {
                return;
            }

            foreach (ExternalSpace externalSpace in externalSpaces)
            {
                List<ExternalPanel> externalPanels = adjacencyCluster.GetRelatedObjects<ExternalPanel>(externalSpace);
                if (externalPanels != null && externalPanels.Count != 0)
                {
                    foreach (ExternalPanel externalPanel in externalPanels)
                    {
                        List<ExternalSpace> externalSpaces_ExternalPanel = adjacencyCluster.GetRelatedObjects<ExternalSpace>(externalPanel);
                        if (externalSpaces_ExternalPanel != null && externalSpaces_ExternalPanel.Count > 1)
                        {
                            continue;
                        }

                        adjacencyCluster.RemoveObject<ExternalPanel>(externalPanel.Guid);
                    }
                }

                adjacencyCluster.RemoveObject<ExternalSpace>(externalSpace.Guid);
            }
        }

    }
}