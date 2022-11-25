using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Construction> SetConstructionsDefaultPanelType(this AdjacencyCluster adjacencyCluster, bool updateNames = false)
        {
            if (adjacencyCluster == null)
                return null;

            List<Construction> constructions = adjacencyCluster.GetConstructions();
            if (constructions == null || constructions.Count == 0)
                return null;

            List<Construction> result = new List<Construction>();
            foreach (Construction construction in constructions)
            {
                List<Panel> panels = adjacencyCluster.GetPanels(construction);
                if (panels == null || panels.Count == 0)
                    continue;

                PanelType panelType_Construction = construction.PanelType();

                List<PanelType> panelTypes = panels.ConvertAll(x => x.PanelType);
                panelTypes = panelTypes.Distinct().ToList();

                if (panelTypes.Count == 1 && panelTypes[0].Equals(panelType_Construction))
                    continue;

                if(panelTypes.Contains(panelType_Construction))
                {
                    panels.RemoveAll(x => x.PanelType == panelType_Construction);
                    panelTypes.Remove(panelType_Construction);
                }

                
                foreach(PanelType panelType in panelTypes)
                {
                    Construction construction_PanelType = updateNames ? new Construction(construction, string.Format("{0} ({1})", construction.Name, Core.Query.Description(panelType))) : new Construction(construction, Guid.NewGuid());

                    construction_PanelType.SetValue(ConstructionParameter.DefaultPanelType, panelType.Text());
                    
                    List<Panel> panels_PanelType = panels.FindAll(x => x.PanelType == panelType);
                    foreach(Panel panel in panels_PanelType)
                    {
                        Panel panel_New = new Panel(panel, construction_PanelType);
                        adjacencyCluster.AddObject(panel_New);
                    }

                    result.Add(construction_PanelType);
                }

            }

            return result;
        }
    }
}
