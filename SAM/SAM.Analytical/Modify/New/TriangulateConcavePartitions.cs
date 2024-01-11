using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<IPartition> TriangulateConcavePartitions(this BuildingModel buildingModel, out List<IPartition> triangulatedPartitions, double tolerance = Tolerance.Distance)
        {
            triangulatedPartitions = null;

            List<IPartition> partitions = buildingModel?.GetObjects<IPartition>(x => x != null && Geometry.Object.Spatial.Query.Concave(x));
            if(partitions == null)
            {
                return null;
            }

            triangulatedPartitions = new List<IPartition>();
            List<IPartition> result = new List<IPartition>();
            foreach (IPartition partition in partitions)
            {
                List<IPartition> partitions_Triangulate = partition.Triangulate(tolerance);

                if (partitions_Triangulate == null || partitions_Triangulate.Count == 0)
                {
                    continue;
                }

                triangulatedPartitions.Add(partition);

                List<IJSAMObject> relatedObjects = buildingModel.GetRelatedObjects(partition);

                foreach (IPartition partition_Triangulate in partitions_Triangulate)
                {
                    if (partition_Triangulate == null)
                    {
                        continue;
                    }

                    result.Add(partition_Triangulate);

                    buildingModel.Add(partition_Triangulate);

                    if (relatedObjects != null && relatedObjects.Count > 0)
                        foreach (IJSAMObject relatedObject in relatedObjects)
                        {
                            buildingModel.AddRelation(partition_Triangulate, relatedObject);
                        }
                }
            }

            return result;
        }
    }
}