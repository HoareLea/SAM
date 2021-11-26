using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<IPartition> SplitByInternalEdges(this BuildingModel buildingModel, double tolerance = Core.Tolerance.Distance)
        {
            if (buildingModel == null)
                return null;

            List<IPartition> result = new List<IPartition>();

            List<IPartition> partitions = buildingModel.GetPartitions();
            if(partitions != null && partitions.Count != 0)
            {
                foreach(IPartition partition in partitions)
                {
                    List<IPartition> partitions_Split = partition.SplitByInternalEdges(tolerance);
                    if (partitions_Split == null || partitions_Split.Count < 2)
                    {
                        continue;
                    }
                    List<Core.IJSAMObject> relatedObjects = buildingModel.GetRelatedObjects(partition); 

                    foreach(IPartition partition_Split in partitions_Split)
                    {
                        buildingModel.Add(partition_Split);
                        result.Add(partition_Split);

                        if(relatedObjects != null && relatedObjects.Count > 0)
                        {
                            foreach (Core.IJSAMObject relatedObject in relatedObjects)
                            {
                                buildingModel.AddRelation(partition_Split, relatedObject);
                            }

                        }
                    }
                }
            }

            return result;
        }
    }
}