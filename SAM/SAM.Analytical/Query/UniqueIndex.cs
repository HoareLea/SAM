using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static int UniqueIndex(this AdjacencyCluster adjacencyCluster, Panel panel)
        {
            if (panel == null)
            {
                return -1;
            }

            List<Panel> panels = adjacencyCluster?.GetPanels();
            if (panels == null || panels.Count == 0)
            {
                return -1;
            }

            int index = panels.FindIndex(x => x.Guid == panel.Guid);
            if (index == -1)
            {
                return -1;
            }

            index++;

            return index;
        }
    }
}