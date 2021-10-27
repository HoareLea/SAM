using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Architectural.Level> Levels(this AdjacencyCluster adjacencyCluster, bool includeOtherPanels = false, double tolerance = Core.Tolerance.MacroDistance)
        {
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if(panels == null)
            {
                return null;
            }

            if (!includeOtherPanels)
            {
                panels.RemoveAll(x => x.PanelType == PanelType.Air || x.PanelType.PanelGroup() == PanelGroup.Other);
            }
            
            return Architectural.Create.Levels(panels, tolerance);
        }
    }
}