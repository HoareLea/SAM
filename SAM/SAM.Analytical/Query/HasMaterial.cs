using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool HasMaterial(this Construction construction, IMaterial material)
        {
            return HasMaterial(construction.ConstructionLayers, material);
        }

        public static bool HasMaterial(this IEnumerable<ConstructionLayer> constructionLayers, IMaterial material)
        {
            if (constructionLayers == null || material == null)
                return false;

            string name = material.Name;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            foreach(ConstructionLayer constructionLayer in constructionLayers)
            {
                if (name.Equals(constructionLayer?.Name))
                    return true;
            }

            return false;
        }

        public static bool HasMaterial(this Construction construction, MaterialLibrary materialLibrary, MaterialType materialType)
        {
            return HasMaterial(construction.ConstructionLayers, materialLibrary, materialType);
        }

        public static bool HasMaterial(this ApertureConstruction apertureConstruction, MaterialLibrary materialLibrary, MaterialType materialType)
        {
            return HasMaterial(apertureConstruction.PaneConstructionLayers, materialLibrary, materialType) || HasMaterial(apertureConstruction.FrameConstructionLayers, materialLibrary, materialType);
        }

        public static bool HasMaterial(this IEnumerable<ConstructionLayer> constructionLayers, MaterialLibrary materialLibrary, MaterialType materialType)
        {
            if (constructionLayers == null || materialLibrary == null)
                return false;

            foreach (ConstructionLayer constructionLayer in constructionLayers)
            {
                Material material = constructionLayer.Material(materialLibrary) as Material;
                if (material != null && material.MaterialType == materialType)
                    return true;
            }

            return false;
        }
    }
}