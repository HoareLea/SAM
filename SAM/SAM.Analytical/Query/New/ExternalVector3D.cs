using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Vector3D ExternalVector3D(this BuildingModel buildingModel, Space space, IPartition partition, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (buildingModel == null || partition == null || space == null)
                return null;
            
            List<IPartition> partitions = buildingModel.GetPartitions(space);
            if(partitions == null || partitions.Count == 0)
            {
                return null;
            }

            IPartition partition_Temp = partitions.Find(x => x.Guid == partition.Guid);
            if(partition_Temp == null)
            {
                return null;
            }

            Face3D face3D = partition.Face3D;
            if (face3D == null)
                return null;

            Shell shell = buildingModel.GetShell(space);
            if (shell == null)
                return null;

            return shell.Normal(face3D.InternalPoint3D(), true, silverSpacing, tolerance);
        }
    }
}