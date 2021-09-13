using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<System.Guid> RemoveInvalidAirPanels(this AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null)
                return null;

            List<System.Guid> guids = new List<System.Guid>();

            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return guids;
            }

            foreach(Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }

                if(panel.Construction != null && panel.PanelType != PanelType.Air)
                {
                    continue;
                }

                List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                if(spaces != null && spaces.Count > 1)
                {
                    continue;
                }

                guids.Add(panel.Guid);
            }

            return adjacencyCluster.Remove(typeof(Panel), guids);
        }
    }
}