using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel UpdateConstructionsByConstructionManager(this AnalyticalModel analyticalModel, ConstructionManager constructionManager)
        {
            if (analyticalModel == null || constructionManager == null)
            {
                return null;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if (adjacencyCluster == null)
            {
                return null;
            }

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;
            if(materialLibrary == null)
            {
                materialLibrary = new MaterialLibrary("Default MaterialLibrary");
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels != null && panels.Count != 0)
            {
                for (int i = 0; i < panels.Count; i++)
                {
                    Panel panel = panels[i];
                    
                    Construction construction = panel?.Construction;
                    if(construction == null)
                    {
                        continue;
                    }

                    List<Construction> constructions = constructionManager.GetConstructions(construction.Name);
                    if(constructions == null || constructions.Count == 0)
                    {
                        continue;
                    }

                    construction = constructions[0];

                    if(constructions.Count > 1)
                    {
                        foreach(Construction construction_Temp in constructions)
                        {
                            if(!construction_Temp.TryGetValue(ConstructionParameter.DefaultPanelType, out string string_PanelType) || string.IsNullOrWhiteSpace(string_PanelType))
                            {
                                continue;
                            }

                            PanelType panelType = Core.Query.Enum<PanelType>(string_PanelType);
                            if(panelType == panel.PanelType)
                            {
                                construction = construction_Temp;
                                break;
                            }
                        }
                    }

                    if(construction == null)
                    {
                        continue;
                    }

                    panel = new Panel(panel, construction);

                    adjacencyCluster.AddObject(panel);

                    List<ConstructionLayer> constructionLayers = construction.ConstructionLayers;
                    if(constructionLayers != null || constructionLayers.Count == 0)
                    {
                        continue;
                    }

                    foreach(ConstructionLayer constructionLayer in constructionLayers)
                    {
                        IMaterial material = constructionManager.GetMaterial(constructionLayer.Name);
                        if(material == null)
                        {
                            continue;
                        }

                        materialLibrary.Add(material);
                    }
                }
            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary, analyticalModel.ProfileLibrary);
        }
    }
}