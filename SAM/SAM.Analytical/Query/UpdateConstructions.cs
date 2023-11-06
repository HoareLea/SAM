using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel UpdateConstructions(this AnalyticalModel analyticalModel, ConstructionManager constructionManager)
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

            List<Construction> constructions = null;

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

                    constructions = constructionManager.GetConstructions(construction.Name);
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
                    if(constructionLayers == null || constructionLayers.Count == 0)
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

            constructions = adjacencyCluster.GetObjects<Construction>();
            if(constructions != null && constructions.Count != 0)
            {
                foreach(Construction construction in constructions)
                {
                    if(construction == null)
                    {
                        continue;
                    }

                    List<Construction> constructions_Temp = constructionManager.GetConstructions(construction.Name);
                    if (constructions_Temp == null || constructions_Temp.Count == 0)
                    {
                        continue;
                    }

                    Construction construction_Temp = constructions_Temp[0];

                    if (constructions_Temp.Count > 1)
                    {
                        if (construction.TryGetValue(ConstructionParameter.DefaultPanelType, out string string_PanelType) || string.IsNullOrWhiteSpace(string_PanelType))
                        {
                            PanelType panelType_Temp = Core.Query.Enum<PanelType>(string_PanelType);

                            foreach (Construction construction_Temp_Temp in constructions_Temp)
                            {
                                if (!construction_Temp_Temp.TryGetValue(ConstructionParameter.DefaultPanelType, out string_PanelType) || string.IsNullOrWhiteSpace(string_PanelType))
                                {
                                    continue;
                                }

                                PanelType panelType = Core.Query.Enum<PanelType>(string_PanelType);
                                if (panelType == panelType_Temp)
                                {
                                    construction_Temp = construction_Temp_Temp;
                                    break;
                                }
                            }
                        }
                    }

                    if(construction_Temp == null)
                    {
                        continue;
                    }

                    construction_Temp = new Construction(construction.Guid, construction_Temp, construction.Name);
                    adjacencyCluster.AddObject(construction_Temp);
                }
            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary, analyticalModel.ProfileLibrary);
        }
    }
}