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
                Face3D face3D_Temp = face3Ds[i];

                System.Guid guid = System.Guid.NewGuid();
                if (i == 0)
                    guid = partition.Guid;

                IPartition partition_Temp = null;
                if (partition is IHostPartition)
                {
                    partition_Temp = Create.HostPartition(guid, face3D_Temp, ((IHostPartition)partition).Type(), tolerance);
                }
                else
                {
                    partition_Temp = new AirPartition(guid, face3D_Temp);
                }

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