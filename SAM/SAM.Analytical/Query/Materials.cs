using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IEnumerable<IMaterial> Materials(this IEnumerable<Panel> panels, MaterialLibrary materialLibrary)
        {
            if (panels == null || materialLibrary == null)
                return null;

            Dictionary<Guid, Construction> dictionary_Construction = new Dictionary<Guid, Construction>();
            Dictionary<Guid, ApertureConstruction> dictionary_ApertureConstruction = new Dictionary<Guid, ApertureConstruction>();
            foreach (Panel panel in panels)
            {
                Guid guid_Construction = panel.SAMTypeGuid;
                if (!dictionary_Construction.ContainsKey(guid_Construction))
                    dictionary_Construction[guid_Construction] = panel.Construction;

                List<Aperture> apertures = panel.Apertures;
                if (apertures != null && apertures.Count != 0)
                {
                    foreach (Aperture aperture in apertures)
                    {
                        Guid guid_ApertureConstruction = aperture.SAMTypeGuid;
                        if (!dictionary_ApertureConstruction.ContainsKey(guid_ApertureConstruction))
                            dictionary_ApertureConstruction[guid_ApertureConstruction] = aperture.ApertureConstruction;
                    }
                }
            }

            Dictionary<string, IMaterial> dictionary_Materials = new Dictionary<string, IMaterial>();

            foreach (Construction construction in dictionary_Construction.Values)
            {
                List<ConstructionLayer> constructionLayers = construction.ConstructionLayers;
                if (constructionLayers == null)
                    continue;

                foreach (ConstructionLayer constructionLayer in constructionLayers)
                {
                    IMaterial material = constructionLayer.Material(materialLibrary);
                    if (material == null || dictionary_Materials.ContainsKey(material.Name))
                        continue;

                    dictionary_Materials[material.Name] = material;
                }
            }

            foreach (ApertureConstruction apertureConstruction in dictionary_ApertureConstruction.Values)
            {
                List<ConstructionLayer> constructionLayers;

                constructionLayers = apertureConstruction.PaneConstructionLayers;
                if (constructionLayers != null)
                {
                    foreach (ConstructionLayer constructionLayer in constructionLayers)
                    {
                        IMaterial material = constructionLayer.Material(materialLibrary);
                        if (material == null || dictionary_Materials.ContainsKey(material.Name))
                            continue;

                        dictionary_Materials[material.Name] = material;
                    }
                }

                constructionLayers = apertureConstruction.FrameConstructionLayers;
                if (constructionLayers != null)
                {
                    foreach (ConstructionLayer constructionLayer in constructionLayers)
                    {
                        IMaterial material = constructionLayer.Material(materialLibrary);
                        if (material == null || dictionary_Materials.ContainsKey(material.Name))
                            continue;

                        dictionary_Materials[material.Name] = material;
                    }
                }
            }

            return dictionary_Materials.Values;
        }

        public static IEnumerable<IMaterial> Materials(this AdjacencyCluster adjacencyCluster, MaterialLibrary materialLibrary)
        {
            return Materials(adjacencyCluster?.GetPanels(), materialLibrary);
        }
    }
}