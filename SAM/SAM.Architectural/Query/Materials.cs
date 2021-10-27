using SAM.Core;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static List<IMaterial> Materials(this IEnumerable<MaterialLayer> materialLayers, MaterialLibrary materialLibrary)
        {
            if (materialLayers == null || materialLibrary == null)
            {
                return null;
            }

            List<IMaterial> result = new List<IMaterial>();
            foreach(MaterialLayer materialLayer in materialLayers)
            {
                IMaterial material = materialLayer?.Material(materialLibrary);
                if(material != null && result.Find(x => x.Name == material.Name) == null)
                {
                    result.Add(material);
                }
            }

            return result;
        }
    }
}