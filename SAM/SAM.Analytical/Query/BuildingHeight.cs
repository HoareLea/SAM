using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double BuildingHeight(this AdjacencyCluster adjacencyCluster)
        {
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if(panels == null)
            {
                return double.NaN;
            }

            panels.RemoveAll(x => x.PanelType == Analytical.PanelType.Shade);
            panels.RemoveAll(x => x.PanelType == Analytical.PanelType.Air);
            panels.RemoveAll(x => x.PanelType == Analytical.PanelType.WallInternal);
            panels.RemoveAll(x => x.PanelType == Analytical.PanelType.FloorInternal);

            return panels.ConvertAll(x => x.GetBoundingBox().Max.Z).Max();
        }
    }
}