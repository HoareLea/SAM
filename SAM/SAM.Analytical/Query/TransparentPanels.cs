using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> TransparentPanels(this AnalyticalModel analyticalModel)
        {
            if (analyticalModel == null)
                return null;

            return TransparentPanels(analyticalModel.AdjacencyCluster, analyticalModel.MaterialLibrary);
        }

        public static List<Panel> TransparentPanels(this AdjacencyCluster adjacencyCluster, MaterialLibrary materialLibrary = null)
        {
            return TransparentPanels(adjacencyCluster?.GetPanels(), materialLibrary);
        }

        public static List<Panel> TransparentPanels(this IEnumerable<Panel> panels, MaterialLibrary materialLibrary = null)
        {
            if (panels == null)
                return null;

            List<Panel> result = new List<Panel>();

            foreach (Panel panel in panels)
            {
                Construction construction_Panel = panel.Construction;
                MaterialType materialType = MaterialType(construction_Panel?.ConstructionLayers, materialLibrary);
                if (materialType == Core.MaterialType.Undefined && panel.PanelType == Analytical.PanelType.CurtainWall)
                    materialType = Core.MaterialType.Transparent;

                if (materialType != Core.MaterialType.Transparent)
                    continue;

                result.Add(panel);
            }

            return result;
        }
    }
}