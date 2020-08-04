using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<ApertureConstruction> ApertureConstructions(this AnalyticalModel analyticalModel, ApertureType apertureType = Analytical.ApertureType.Undefined, PanelType panelType = Analytical.PanelType.Undefined)
        {
            if (analyticalModel == null)
                return null;

            return ApertureConstructions(analyticalModel.AdjacencyCluster, apertureType, panelType);
        }

        public static List<ApertureConstruction> ApertureConstructions(this AdjacencyCluster adjacencyCluster, ApertureType apertureType = Analytical.ApertureType.Undefined, PanelType panelType = Analytical.PanelType.Undefined)
        {
            if (adjacencyCluster == null)
                return null;

            return ApertureConstructions(adjacencyCluster.GetPanels(), apertureType, panelType);
        }

        public static List<ApertureConstruction> ApertureConstructions(this IEnumerable<Panel> panels, ApertureType apertureType = Analytical.ApertureType.Undefined, PanelType panelType = Analytical.PanelType.Undefined)
        {
            if (panels == null)
                return null;

            Dictionary<Guid, ApertureConstruction> dictionary = new Dictionary<Guid, ApertureConstruction>();
            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                if (panelType != Analytical.PanelType.Undefined && panelType != panel.PanelType)
                    continue;

                List<Aperture> apertures = panel.Apertures;
                if (apertures == null || apertures.Count == 0)
                    continue;

                foreach(Aperture aperture in apertures)
                {
                    if (aperture == null)
                        continue;

                    ApertureType apertureType_Temp = aperture.ApertureType;

                    if (apertureType != Analytical.ApertureType.Undefined && apertureType != apertureType_Temp)
                        continue;

                    ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                    dictionary[apertureConstruction.Guid] = apertureConstruction;
                }
            }

            return dictionary.Values.ToList();
        }
    }
}