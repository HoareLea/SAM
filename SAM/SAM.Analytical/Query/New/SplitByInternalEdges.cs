using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<IPartition> SplitByInternalEdges(this IPartition partition, double tolerance = Core.Tolerance.Distance)
        {
            Face3D face3D = partition?.Face3D;
            if (face3D == null)
                return null;

            List<Face3D> face3Ds = face3D.SplitByInternalEdges(tolerance);
            if (face3Ds == null)
                return null;

            List<IPartition> result = new List<IPartition>();
            for(int i=0; i < face3Ds.Count; i++)
            {
                System.Guid guid = i == 0 ? partition.Guid : System.Guid.NewGuid();

                IPartition partition_Temp = Create.Partition(partition, guid, face3Ds[i], tolerance);
                if(partition_Temp == null)
                {
                    continue;
                }

                result.Add(partition_Temp);
            }

            return result;
        }
    }
}