using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static MaterialType MaterialType(this IEnumerable<ConstructionLayer> constructionLayers, MaterialLibrary materialLibrary)
        {
            if (constructionLayers == null || materialLibrary == null)
                return Core.MaterialType.Undefined;

            bool gas = true;
            foreach (ConstructionLayer constructionLayer in constructionLayers)
            {
                IMaterial material = constructionLayer.Material(materialLibrary);
                if (material is OpaqueMaterial)
                    return Core.MaterialType.Opaque;

                if (material is TransparentMaterial)
                    gas = false;
            }

            if (gas)
                return Core.MaterialType.Gas;

            return Core.MaterialType.Transparent;
        }
    }
}