using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel UpdateApertureConstructions(this AnalyticalModel analyticalModel, ConstructionManager constructionManager)
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

                    List<Aperture> apertures = panel?.Apertures;
                    if(apertures == null || apertures.Count == 0)
                    {
                        continue;
                    }

                    panel = new Panel(panel);

                    bool updated = false;

                    foreach(Aperture aperture in apertures)
                    {
                        ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                        if(apertureConstruction == null)
                        {
                            continue;
                        }

                        List<ApertureConstruction> apertureConstructions = constructionManager.GetApertureConstructions(apertureConstruction.ApertureType, apertureConstruction.Name);
                        if (apertureConstructions == null || apertureConstructions.Count == 0)
                        {
                            continue;
                        }

                        apertureConstruction = apertureConstructions[0];

                        if (apertureConstructions.Count > 1)
                        {
                            foreach (ApertureConstruction apertureConstruction_Temp in apertureConstructions)
                            {
                                if (!apertureConstruction_Temp.TryGetValue(ApertureConstructionParameter.DefaultPanelType, out string string_PanelType) || string.IsNullOrWhiteSpace(string_PanelType))
                                {
                                    continue;
                                }

                                PanelType panelType = Core.Query.Enum<PanelType>(string_PanelType);
                                if (panelType == panel.PanelType)
                                {
                                    apertureConstruction = apertureConstruction_Temp;
                                    break;
                                }
                            }
                        }

                        if (apertureConstruction == null)
                        {
                            continue;
                        }

                        if(!panel.RemoveAperture(aperture.Guid))
                        {
                            continue;
                        }

                        panel.AddAperture(new Aperture(aperture, apertureConstruction));
                        updated = true;

                        List<ConstructionLayer> constructionLayers = null;

                        constructionLayers = apertureConstruction.FrameConstructionLayers;
                        if (constructionLayers != null || constructionLayers.Count != 0)
                        {
                            foreach (ConstructionLayer constructionLayer in constructionLayers)
                            {
                                IMaterial material = constructionManager.GetMaterial(constructionLayer.Name);
                                if (material == null)
                                {
                                    continue;
                                }

                                materialLibrary.Add(material);
                            }
                        }

                        constructionLayers = apertureConstruction.PaneConstructionLayers;
                        if (constructionLayers != null || constructionLayers.Count != 0)
                        {
                            foreach (ConstructionLayer constructionLayer in constructionLayers)
                            {
                                IMaterial material = constructionManager.GetMaterial(constructionLayer.Name);
                                if (material == null)
                                {
                                    continue;
                                }

                                materialLibrary.Add(material);
                            }
                        }
                    }

                    if (!updated)
                    {
                        continue;
                    }

                    adjacencyCluster.AddObject(panel);
                }
            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary, analyticalModel.ProfileLibrary);
        }
    }
}