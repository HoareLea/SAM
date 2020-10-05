using SAM.Core;
using System.Collections.Generic;
using System.Linq;

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
            if (construction == null || materialLibrary == null)
                return false;

            List<ConstructionLayer> constructionLayers = construction.ConstructionLayers;
            if (constructionLayers == null)
                return false;

            foreach(ConstructionLayer constructionLayer in constructionLayers)
            {
                Material material = constructionLayer.Material(materialLibrary) as Material;
                if (material != null && material.MaterialType == materialType)
                    return true;
            }

            return false;
        }
    }
}