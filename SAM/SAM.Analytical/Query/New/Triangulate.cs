using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<T> Triangulate<T>(this T partition, double tolerance = Core.Tolerance.Distance) where T : IPartition
        {
            Face3D face3D = partition?.Face3D;
            if (face3D == null)
                return null;

            List<Face3D> face3Ds = face3D.Triangulate(tolerance)?.ConvertAll(x => new Face3D(x));
            if (face3Ds == null)
                return null;

            List<T> result = new List<T>();
            for(int i=0; i < face3Ds.Count; i++)
            {
                Face3D face3D_Temp = face3Ds[i];

                System.Guid guid = System.Guid.NewGuid();
                if (i == 0)
                    guid = partition.Guid;

                T partition_New = Create.Partition(partition, guid, face3D_Temp, tolerance);
                if(partition_New == null)
                {
                    continue;
                }

                result.Add(partition_New);
            }

            return result;
        }
    }
}