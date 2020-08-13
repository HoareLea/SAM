using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Panels(this AdjacencyCluster adjacencyCluster, Construction construction)
        {
            if (adjacencyCluster == null || construction == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null)
                return null;

            Guid guid = construction.Guid;

            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
                if (panel.SAMTypeGuid.Equals(guid))
                    result.Add(panel);

            return result;
        }
    }
}