using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<T> FixEdges<T>(this T partition, double tolerance = Core.Tolerance.Distance) where T : IPartition
        {
            if(partition == null)
            {
                return null;
            }

            Face3D face3D = partition.Face3D;
            if(face3D == null)
            {
                return null;
            }

            List<Face3D> face3Ds = face3D.FixEdges(tolerance);
            if(face3Ds == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(Face3D face3D_Temp in face3Ds)
            {
                System.Guid guid = partition.Guid;
                while(result.Find(x => x.Guid == guid) != null)
                {
                    guid = System.Guid.NewGuid();
                }

                IPartition partition_New = Create.Partition(partition, guid, face3D_Temp, tolerance);
                if(partition_New is T)
                {
                    result.Add((T)partition_New);
                }
            }

            return result;
        }
    }
}