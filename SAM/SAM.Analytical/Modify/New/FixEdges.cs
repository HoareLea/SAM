using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void FixEdges(this ArchitecturalModel architecturalModel, double tolerance = Core.Tolerance.Distance)
        {
            if (architecturalModel == null)
            {
                return;
            }

            List<IPartition> partitions = architecturalModel.GetPartitions();
            if (partitions == null || partitions.Count == 0)
            {
                return ;
            }

            List<List<IPartition>> partitionsList = Enumerable.Repeat<List<IPartition>>(null, partitions.Count).ToList();

            Parallel.For(0, partitions.Count, (int i) =>
            {
                partitionsList[i] = partitions[i].FixEdges(tolerance);
            });

            for(int i=0; i < partitions.Count; i++)
            {
                List<IPartition> partitions_Temp = partitionsList[i];
                if(partitions_Temp == null || partitions_Temp.Count == 0)
                {
                    continue;
                }

                if (partitions_Temp.Count == 1)
                {
                    architecturalModel.Add(partitions_Temp[0]);
                    continue;
                }

                List<Core.IJSAMObject> relatedObjects = architecturalModel.GetRelatedObjects(partitions[i]);

                foreach (IPartition partition_FixEdge in partitions_Temp)
                {
                    architecturalModel.Add(partition_FixEdge);
                    if (relatedObjects != null)
                    {
                        foreach (Core.IJSAMObject relatedObject in relatedObjects)
                        {
                            architecturalModel.AddRelation(partition_FixEdge, relatedObject);
                        }
                    }
                }
            }
        }
    }
}