using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Order of materials from inside to outside following the TAS approach.
        /// </summary>
        /// <param name="panels"></param>
        /// <param name="materialLibrary"></param>
        /// <returns></returns>
        public static IEnumerable<IMaterial> Materials(this IEnumerable<Panel> panels, MaterialLibrary materialLibrary)
        {
            if (panels == null || materialLibrary == null)
                return null;

            Dictionary<Guid, Construction> dictionary_Construction = new Dictionary<Guid, Construction>();
            Dictionary<Guid, ApertureConstruction> dictionary_ApertureConstruction = new Dictionary<Guid, ApertureConstruction>();
            foreach (Panel panel in panels)
            {
                Guid guid_Construction = panel.TypeGuid;
                if (!dictionary_Construction.ContainsKey(guid_Construction))
                    dictionary_Construction[guid_Construction] = panel.Construction;

                List<Aperture> apertures = panel.Apertures;
                if (apertures != null && apertures.Count != 0)
                {
                    foreach (Aperture aperture in apertures)
                    {
                        Guid guid_ApertureConstruction = aperture.TypeGuid;
                        if (!dictionary_ApertureConstruction.ContainsKey(guid_ApertureConstruction))
                            dictionary_ApertureConstruction[guid_ApertureConstruction] = aperture.ApertureConstruction;
                    }
                }
            }

            Dictionary<string, IMaterial> dictionary_Materials = new Dictionary<string, IMaterial>();

            foreach (Construction construction in dictionary_Construction.Values)
            {
                List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
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

        public static IEnumerable<IMaterial> Materials(this Construction construction, MaterialLibrary materialLibrary, MaterialType materialType = Core.MaterialType.Undefined)
        {
            if (construction == null || materialLibrary == null)
                return null;

            List<ConstructionLayer> constructionLayers = construction.ConstructionLayers;
            if (constructionLayers == null)
                return null;

            List<IMaterial> result = new List<IMaterial>();
            foreach(ConstructionLayer constructionLayer in constructionLayers)
            {
                Material material = constructionLayer.Material(materialLibrary) as Material;
                if(material != null)
                {
                    if (materialType == Core.MaterialType.Undefined || materialType == material.MaterialType)
                        result.Add(material);
                }
            }

            return result;
        }

        /// <summary>
        /// Order of materials from inside to outside following the TAS approach.
        /// </summary>
        /// <param name="apertureConstruction"></param>
        /// <param name="materialLibrary"></param>
        /// <param name="materialType"></param>
        /// <returns></returns>
        public static IEnumerable<IMaterial> Materials(this ApertureConstruction apertureConstruction, MaterialLibrary materialLibrary, MaterialType materialType = Core.MaterialType.Undefined)
        {
            if (apertureConstruction == null || materialLibrary == null)
                return null;

            List<ConstructionLayer> constructionLayers = null;
            List<ConstructionLayer> constructionLayers_Temp = null;

            constructionLayers_Temp = apertureConstruction.PaneConstructionLayers;
            if (constructionLayers_Temp != null)
                constructionLayers = constructionLayers_Temp;

            constructionLayers_Temp = apertureConstruction.FrameConstructionLayers;
            if (constructionLayers_Temp != null)
            {
                if (constructionLayers == null)
                    constructionLayers = new List<ConstructionLayer>();

                constructionLayers.AddRange(constructionLayers_Temp);
            }

            if (constructionLayers == null)
                return null;

            List<IMaterial> result = new List<IMaterial>();
            foreach (ConstructionLayer constructionLayer in constructionLayers)
            {
                Material material = constructionLayer.Material(materialLibrary) as Material;
                if (material != null)
                {
                    if (materialType == Core.MaterialType.Undefined || materialType == material.MaterialType)
                        result.Add(material);
                }
            }

            return result;
        }

        /// <summary>
        /// Order of materials from inside to outside following the TAS approach.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="construction"></param>
        /// <param name="materialLibrary"></param>
        /// <returns></returns>
        public static IEnumerable<T> Materials<T>(this Construction construction, MaterialLibrary materialLibrary) where T: IMaterial
        {
            if (construction == null || materialLibrary == null)
                return null;

            List<ConstructionLayer> constructionLayers = construction.ConstructionLayers;
            if (constructionLayers == null)
                return null;

            List<T> result = new List<T>();
            foreach (ConstructionLayer constructionLayer in constructionLayers)
            {
                IMaterial material = constructionLayer.Material(materialLibrary);
                if (material is T)
                    result.Add((T)material);
            }

            return result;
        }
    }
}