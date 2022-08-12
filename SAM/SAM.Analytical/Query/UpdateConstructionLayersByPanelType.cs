using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel UpdateConstructionLayersByPanelType(this AnalyticalModel analyticalModel, ConstructionLibrary constructionLibrary = null, ApertureConstructionLibrary apertureConstructionLibrary = null, MaterialLibrary materialLibrary = null, bool emptyOnly = true)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if (panels == null)
            {
                return null;
            }

            if (constructionLibrary == null)
                constructionLibrary = ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);

            if (materialLibrary == null)
                materialLibrary = ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary);

            MaterialLibrary materialLibrary_AnalyticalModel = analyticalModel.MaterialLibrary;

            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                if (panel == null)
                    continue;

                Construction construction_Old = panel.Construction;

                Construction construction_New = null;
                if (emptyOnly)
                {
                    if (construction_Old == null || !construction_Old.HasConstructionLayers())
                        construction_New = constructionLibrary.GetConstructions(panel.PanelType).FirstOrDefault();
                }
                else
                {
                    construction_New = constructionLibrary.GetConstructions(panel.PanelType).FirstOrDefault();
                }

                bool updated = false;

                if (construction_New != null)
                {
                    IEnumerable<IMaterial> materials_Temp = Materials(construction_New, materialLibrary);
                    if (materials_Temp != null)
                    {
                        foreach (IMaterial material in materials_Temp)
                            if (!materialLibrary_AnalyticalModel.Contains(material))
                                materialLibrary_AnalyticalModel.Add(material);
                    }

                    construction_New = new Construction(construction_Old, construction_New.ConstructionLayers);

                    panel = Create.Panel(panel, construction_New);
                    updated = true;
                }

                if (panel.HasApertures)
                {
                    panel = Create.Panel(panel);
                    foreach (Aperture aperture in panel.Apertures)
                    {
                        Aperture aperture_Old = panel.GetAperture(aperture.Guid);
                        if (aperture_Old == null)
                            continue;

                        ApertureConstruction apertureConstruction_Old = aperture_Old.ApertureConstruction;

                        ApertureConstruction apertureConstruction_New = null;
                        if (emptyOnly)
                        {
                            if (apertureConstruction_Old == null || !apertureConstruction_Old.HasPaneConstructionLayers())
                                apertureConstruction_New = apertureConstructionLibrary.GetApertureConstructions(apertureConstruction_Old.ApertureType, panel.PanelType).FirstOrDefault();
                        }
                        else
                        {
                            apertureConstruction_New = apertureConstructionLibrary.GetApertureConstructions(apertureConstruction_Old.ApertureType, panel.PanelType).FirstOrDefault();
                        }

                        if (apertureConstruction_New != null)
                        {
                            IEnumerable<IMaterial> materials_Temp = Materials(apertureConstruction_New, materialLibrary);
                            if (materials_Temp != null)
                            {
                                foreach (IMaterial material in materials_Temp)
                                    if (!materialLibrary_AnalyticalModel.Contains(material))
                                        materialLibrary_AnalyticalModel.Add(material);
                            }

                            apertureConstruction_New = new ApertureConstruction(apertureConstruction_Old, apertureConstruction_New.PaneConstructionLayers, apertureConstruction_New.FrameConstructionLayers);

                            Aperture aperture_New = new Aperture(aperture_Old, apertureConstruction_New);

                            if (aperture_New == null)
                                continue;

                            panel.RemoveAperture(aperture_Old.Guid);
                            panel.AddAperture(aperture_New);
                            updated = true;
                        }
                    }
                }

                if (updated)
                    adjacencyCluster.AddObject(panel);

            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster, materialLibrary_AnalyticalModel, analyticalModel.ProfileLibrary);
        }
    }
}