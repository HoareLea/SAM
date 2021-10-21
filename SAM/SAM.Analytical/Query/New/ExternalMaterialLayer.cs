using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Architectural.MaterialLayer ExternalMaterialLayer(this HostPartitionType hostPartitionType)
        {
            List<Architectural.MaterialLayer> materialLayers= hostPartitionType?.MaterialLayers;
            if (materialLayers == null || materialLayers.Count == 0)
                return null;

            return materialLayers.Last();
        }

        public static Architectural.MaterialLayer ExternalMaterialLayer(this IHostPartition hostPartition)
        {
            return ExternalMaterialLayer(hostPartition?.Type());
        }
    }
}