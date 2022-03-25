using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Panel, ConstructionLayer> ExternalConstructionLayerDictionary(this Space space, AdjacencyCluster adjacencyCluster, double silverSpacing = Tolerance.MacroDistance, double tolerance = Tolerance.Distance)
        {
            List<Panel> panels = adjacencyCluster?.UpdateNormals(space, false, silverSpacing, tolerance);
            if (panels == null || panels.Count == 0)
                return null;

            Dictionary<Panel, ConstructionLayer> result = new Dictionary<Panel, ConstructionLayer>();
            foreach(Panel panel in panels)
            {
                Panel panel_Temp = adjacencyCluster.GetObject<Panel>(panel.Guid);
                if (panel_Temp == null)
                    continue;

                ConstructionLayer constructionLayer = LastConstructionLayer(panel_Temp, panel.Normal);
                result[panel_Temp] = constructionLayer;
            }

            return result;
        }
    }
}