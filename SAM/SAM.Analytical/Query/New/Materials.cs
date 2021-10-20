using SAM.Core;
using System.Collections.Generic;

using SAM.Architectural;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<IMaterial> Materials(this HostPartitionType hostPartitionType, MaterialLibrary materialLibrary)
        {
            if (hostPartitionType == null || materialLibrary == null)
            {
                return null;
            }

            List<MaterialLayer> materialLayers = hostPartitionType.MaterialLayers;
            if(materialLayers == null)
            {
                return null;
            }

            List<IMaterial> result = new List<IMaterial>();
            foreach(MaterialLayer materialLayer in materialLayers)
            {
                IMaterial material = materialLayer.Material(materialLibrary);
                result.Add(material);
            }

            return result;
        }
    }
}