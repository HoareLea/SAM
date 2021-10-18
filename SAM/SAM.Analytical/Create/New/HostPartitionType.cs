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
    }
}
