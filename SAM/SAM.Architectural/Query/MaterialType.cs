using SAM.Core;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static MaterialType MaterialType(this IEnumerable<MaterialLayer> materialLayers, MaterialLibrary materialLibrary)
        {
            if (materialLayers == null || materialLibrary == null)
                return Core.MaterialType.Undefined;

            bool gas = true;
            foreach (MaterialLayer materialLayer in materialLayers)
            {
                IMaterial material = materialLayer.Material(materialLibrary);
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