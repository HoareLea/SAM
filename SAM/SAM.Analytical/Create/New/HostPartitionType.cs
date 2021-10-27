using System.Collections.Generic;

using SAM.Architectural;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static HostPartitionType HostPartitionType(this HostPartitionCategory hostPartitionCategory, string name, IEnumerable<MaterialLayer> materialLayers)
        {
            if(name == null || materialLayers == null || hostPartitionCategory == HostPartitionCategory.Undefined)
            {
                return null;
            }

            switch(hostPartitionCategory)
            {
                case HostPartitionCategory.Floor:
                    return new FloorType(name, materialLayers);

                case HostPartitionCategory.Roof:
                    return new RoofType(name, materialLayers);

                case HostPartitionCategory.Wall:
                    return new WallType(name, materialLayers);
            }

            return null;
        }

        public static HostPartitionType HostPartitionType(this HostPartitionType hostPartitionType, string name)
        {
            if (hostPartitionType == null || name == null)
            {
                return null;
            }

            if(hostPartitionType is WallType)
            {
                return new WallType((WallType)hostPartitionType, name);
            }

            if (hostPartitionType is RoofType)
            {
                return new RoofType((RoofType)hostPartitionType, name);
            }

            if (hostPartitionType is FloorType)
            {
                return new FloorType((FloorType)hostPartitionType, name);
            }

            return null;
        }
    }
}
