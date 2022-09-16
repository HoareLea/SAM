using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool IsPerimeter(this AdjacencyCluster adjacencyCluster, Space space, bool apertureCheck = true, IEnumerable<PanelGroup> panelGroups = null)
        {
            if(adjacencyCluster == null || space == null)
            {
                return false;
            }

            Space space_Temp = adjacencyCluster.GetObject<Space>(space.Guid);
            if(space_Temp == null)
            {
                return false;
            }

            List<Panel> panels = adjacencyCluster.GetPanels(space_Temp);
            if(panels == null || panels.Count == 0)
            {
                return false;
            }

            foreach (Panel panel in panels)
            {
                if (!adjacencyCluster.ExposedToSun(panel))
                {
                    continue;
                }

                if(panelGroups != null && !panelGroups.Contains(panel.PanelGroup))
                {
                    continue;
                }
                
                if (apertureCheck)
                {
                    List<Aperture> apertures = panel.Apertures;

                    if (apertures == null || apertures.Count == 0)
                    {
                        continue;
                    }
                }

                return true;
            }

            return false;

        }
    }
}