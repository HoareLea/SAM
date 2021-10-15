using SAM.Core;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static bool Transparent(this HostPartitionType hostPartitionType, MaterialLibrary materialLibrary = null)
        {
            MaterialType materialType = MaterialType(hostPartitionType?.MaterialLayers, materialLibrary);
            return materialType == Core.MaterialType.Transparent;
        }
    }
}