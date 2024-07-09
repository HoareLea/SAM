using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool Replace(this AdjacencyCluster adjacencyCluster, ApertureConstruction apertureConstruction_Existing, ApertureConstruction apertureConstruction_New)
        {
            if (adjacencyCluster == null || apertureConstruction_Existing == null || apertureConstruction_New == null)
            {
                return false;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return false;
            }

            bool result = false;
            foreach (Panel panel in panels) 
            {
                List<Aperture> apertures = panel?.Apertures;
                if(apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                Panel panel_New = null;
                foreach (Aperture aperture in apertures)
                {
                    ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                    if(apertureConstruction.Guid != apertureConstruction_Existing.Guid)
                    {
                        continue;
                    }

                    Aperture aperture_New = new Aperture(aperture, apertureConstruction_New);

                    if(panel_New == null)
                    {
                        panel_New = new Panel(panel);
                    }

                    panel_New.RemoveAperture(aperture.Guid);
                    panel_New.AddAperture(aperture_New);
                }

                if(panel_New != null)
                {
                    result = true;
                    adjacencyCluster.AddObject(panel_New);
                }
            }

            return result;
        }

        public static bool Replace(this AdjacencyCluster adjacencyCluster, Construction construction_Existing, Construction construction_New)
        {
            if (adjacencyCluster == null || construction_Existing == null || construction_New == null)
            {
                return false;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
            {
                return false;
            }

            bool result = false;
            foreach (Panel panel in panels)
            {
                Construction construction = panel?.Construction;
                if(construction == null)
                {
                    continue;
                }

                if (construction_Existing.Guid != construction.Guid)
                {
                    continue;
                }

                Panel panel_New = new Panel(panel, construction_New);
                result = true;
                adjacencyCluster.AddObject(panel_New);
            }

            return result;
        }

        public static bool Replace(this AdjacencyCluster adjacencyCluster, Space space_Existing, Space space_New)
        {
            if (adjacencyCluster == null || space_Existing == null || space_New == null)
            {
                return false;
            }

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces == null || spaces.Count == 0)
            {
                return false;
            }

            bool result = false;
            foreach (Space space in spaces)
            {
                if(space == null)
                {
                    continue;
                }

                if(space.Guid != space_Existing.Guid)
                {
                    continue;
                }

                List<IJSAMObject> jSAMObjects = adjacencyCluster.GetRelatedObjects<IJSAMObject>(space);
                adjacencyCluster.Remove(new List<Space>() { space });
                adjacencyCluster.AddObject(space_New);
                jSAMObjects.ForEach(x => adjacencyCluster.AddRelation(space_New, x));
                return true;
            }

            return result;
        }

        public static bool Replace(this MaterialLibrary materialLibrary, IMaterial material_Existing, IMaterial material_New)
        {
            if (materialLibrary == null || material_Existing == null || material_New == null)
            {
                return false;
            }

            if(!materialLibrary.Contains(material_Existing))
            {
                return false;
            }

            materialLibrary.Remove(material_Existing);
            materialLibrary.Add(material_New);
            return true;
        }

        public static bool Replace(this AdjacencyCluster adjacencyCluster, IMaterial material_Existing, IMaterial material_New)
        {
            if (adjacencyCluster == null || material_Existing == null || material_New == null)
            {
                return false;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
            {
                return false;
            }

            bool result = false;
            foreach (Panel panel in panels)
            {
                Panel panel_New = null;

                Construction construction = panel?.Construction;
                if(construction != null)
                {
                    List<ConstructionLayer> constructionLayers = construction.ConstructionLayers;
                    if(constructionLayers != null)
                    {
                        for(int i = 0; i < constructionLayers.Count; i++)
                        {
                            if (constructionLayers[i].Name == material_Existing.Name)
                            {
                                if(panel_New == null)
                                {
                                    panel_New = new Panel(panel);
                                }

                                constructionLayers[i] = new ConstructionLayer(material_New.Name, constructionLayers[i].Thickness);
                            }
                        }
                    }

                    if(panel_New != null)
                    {
                        adjacencyCluster.AddObject(panel_New);
                    }
                }

                panel_New = null;

                List<Aperture> apertures = panel?.Apertures;
                if (apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                foreach (Aperture aperture in apertures)
                {
                    Aperture aperture_New = null;
                    
                    ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                    if (apertureConstruction != null)
                    {
                        List<ConstructionLayer> frameConstructionLayers = apertureConstruction.FrameConstructionLayers;
                        if (frameConstructionLayers != null)
                        {
                            for (int i = 0; i < frameConstructionLayers.Count; i++)
                            {
                                if (frameConstructionLayers[i].Name == material_Existing.Name)
                                {
                                    if (aperture_New == null)
                                    {
                                        aperture_New = new Aperture(aperture);
                                    }

                                    frameConstructionLayers[i] = new ConstructionLayer(material_New.Name, frameConstructionLayers[i].Thickness);
                                }
                            }
                        }

                        List<ConstructionLayer> paneConstructionLayers = apertureConstruction.PaneConstructionLayers;
                        if (paneConstructionLayers != null)
                        {
                            for (int i = 0; i < paneConstructionLayers.Count; i++)
                            {
                                if (paneConstructionLayers[i].Name == material_Existing.Name)
                                {
                                    if (aperture_New == null)
                                    {
                                        aperture_New = new Aperture(aperture);
                                    }

                                    paneConstructionLayers[i] = new ConstructionLayer(material_New.Name, paneConstructionLayers[i].Thickness);
                                }
                            }
                        }

                        if (aperture_New != null)
                        {
                            apertureConstruction = new ApertureConstruction(apertureConstruction, paneConstructionLayers, frameConstructionLayers);
                            aperture_New = new Aperture(aperture, apertureConstruction);
                        }
                    }


                    if (aperture_New == null)
                    {
                        continue;
                    }

                    if (panel_New == null)
                    {
                        panel_New = new Panel(panel);
                    }

                    panel_New.RemoveAperture(aperture.Guid);
                    panel_New.AddAperture(aperture_New);
                }

                if (panel_New != null)
                {
                    result = true;
                    adjacencyCluster.AddObject(panel_New);
                }
            }

            return result;
        }

        public static bool Replace(this ProfileLibrary profileLibrary, Profile profile_Existing, Profile profile_New)
        {
            if (profileLibrary == null || profile_Existing == null || profile_New == null)
            {
                return false;
            }

            if (!profileLibrary.Contains(profile_Existing))
            {
                return false;
            }

            profileLibrary.Remove(profile_Existing);
            profileLibrary.Add(profile_New);
            return true;
        }

        public static bool Replace(this AdjacencyCluster adjacencyCluster, Profile profile_Existing, Profile profile_New)
        {
            if (adjacencyCluster == null || profile_Existing == null || profile_New == null)
            {
                return false;
            }

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if(spaces == null)
            {
                return false;
            }

            bool result = false;
            foreach(Space space in spaces)
            {
                InternalCondition internalCondition = space?.InternalCondition;
                if(internalCondition == null)
                {
                    continue;
                }

                string profileName = internalCondition.GetProfileName(profile_Existing.ProfileType);
                if(profileName == profile_Existing.Name)
                {
                    continue;
                }

                internalCondition.SetProfileName(profile_Existing.ProfileType, profile_New.Name);

                Space space_New = new Space(space);
                space_New.InternalCondition = internalCondition;

                adjacencyCluster.AddObject(space_New);
                result = true;
            }

            return result;
        }
    }
}