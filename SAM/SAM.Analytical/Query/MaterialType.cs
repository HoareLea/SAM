using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static MaterialType MaterialType(this IEnumerable<ConstructionLayer> constructionLayers, MaterialLibrary materialLibrary)
        {
            if (constructionLayers == null || materialLibrary == null)
            {
                return Core.MaterialType.Undefined;
            }

            HashSet<MaterialType> materialTypes = new HashSet<MaterialType>();
            foreach (ConstructionLayer constructionLayer in constructionLayers)
            {
                IMaterial material = constructionLayer.Material(materialLibrary);
                if(material == null)
                {
                    return Core.MaterialType.Undefined;
                }

                MaterialType materialType = material.MaterialType();
                if(materialType == Core.MaterialType.Undefined)
                {
                    return Core.MaterialType.Undefined;
                }

                materialTypes.Add(materialType);
            }

            if(materialTypes.Contains(Core.MaterialType.Opaque))
            {
                return Core.MaterialType.Opaque;
            }

            if (materialTypes.Contains(Core.MaterialType.Transparent))
            {
                return Core.MaterialType.Transparent;
            }

            if (materialTypes.Contains(Core.MaterialType.Gas))
            {
                return Core.MaterialType.Gas;
            }

            return Core.MaterialType.Undefined;
        }
    }
}