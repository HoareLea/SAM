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

                T partition_New = default;
                if(partition is AirPartition)
                {
                    partition_New = (T)(object)(new AirPartition(guid, face3D_Temp));
                }
                else if(partition is IHostPartition)
                {
                    partition_New = (T)Create.HostPartition(guid, face3D_Temp, ((IHostPartition)partition).Type());
                }

                if(partition_New == null)
                {
                    continue;
                }

                result.Add(partition_New);
            }

            if(partition is IHostPartition)
            {
                List<IOpening> openings = ((IHostPartition)partition).Openings;
                if(openings != null)
                {
                    foreach(IOpening opening in openings)
                    {
                        List<IOpening> openings_Split = opening.Split(face3Ds, tolerance_Distance: tolerance);
                        if(openings_Split != null)
                        {
                            foreach(IOpening opening_Split in openings_Split)
                            {
                                Point3D point3D =  opening_Split.Face3D.InternalPoint3D(tolerance);
                                if(point3D == null)
                                {
                                    continue;
                                }

                                for(int i=0; i < result.Count; i++)
                                {
                                    IHostPartition hostPartition = result[i] as IHostPartition;
                                    if(hostPartition == null)
                                    {
                                        continue;
                                    }

                                    if(hostPartition.Face3D.Distance(point3D, tolerance) > tolerance)
                                    {
                                        continue;
                                    }

                                    if(hostPartition.AddOpening(opening_Split, tolerance))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}