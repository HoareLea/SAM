using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void MergeCoplanarPartitionsBySpace(this BuildingModel buildingModel, Space space, bool validateHostPartitionType = true, double tolerance = Core.Tolerance.Distance)
        {
            List<IPartition> partitions = buildingModel?.GetPartitions(space);
            if(partitions == null || partitions.Count == 0)
            {
                return;
            }

            List<IPartition> partitions_MergeCoplanar = partitions.MergeCoplanar(tolerance, out List<IPartition> redundantPartitions, validateHostPartitionType, tolerance, tolerance);
            if(partitions_MergeCoplanar == null || partitions_MergeCoplanar.Count == 0)
            {
                return;
            }

            if(redundantPartitions != null && redundantPartitions.Count != 0)
            {
                foreach(IPartition partition in redundantPartitions)
                {
                    buildingModel.RemoveObject(partition);
                }
            }

            foreach(IPartition partition in partitions_MergeCoplanar)
            {
                buildingModel.Add(partition);
            }

        }
    }
}