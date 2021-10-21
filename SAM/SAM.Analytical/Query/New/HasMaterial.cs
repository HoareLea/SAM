using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool HasMaterial(this HostPartitionType hostPartitionType, IMaterial material)
        {
            return HasMaterial(hostPartitionType?.MaterialLayers, material);
        }

        public static bool HasMaterial(this IEnumerable<Architectural.MaterialLayer> materialLayers, IMaterial material)
        {
            if (materialLayers == null || material == null)
                return false;

            string name = material.Name;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            foreach(Architectural.MaterialLayer materialLayer in materialLayers)
            {
                if (name.Equals(materialLayer?.Name))
                    return true;
            }

            return false;
        }

        public static bool HasMaterial(this HostPartitionType hostPartitionType, MaterialLibrary materialLibrary, MaterialType materialType)
        {
            return HasMaterial(hostPartitionType?.MaterialLayers, materialLibrary, materialType);
        }

        public static bool HasMaterial(this OpeningType openingType, MaterialLibrary materialLibrary, MaterialType materialType)
        {
            return HasMaterial(openingType?.PaneMaterialLayers, materialLibrary, materialType) || HasMaterial(openingType?.FrameMaterialLayers, materialLibrary, materialType);
        }

        public static bool HasMaterial(this IEnumerable<Architectural.MaterialLayer> materialLayers, MaterialLibrary materialLibrary, MaterialType materialType)
        {
            if (materialLayers == null || materialLibrary == null)
                return false;

            foreach (ConstructionLayer constructionLayer in materialLayers)
            {
                Material material = constructionLayer.Material(materialLibrary) as Material;
                if (material != null && material.MaterialType == materialType)
                    return true;
            }

            return false;
        }
    }
}