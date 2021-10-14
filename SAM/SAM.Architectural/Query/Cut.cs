using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static List<IPartition> Cut(this IPartition partition, double elevation, double tolerance = Tolerance.Distance)
        {
            if (partition == null || double.IsNaN(elevation))
                return null;

            Plane plane = Geometry.Spatial.Create.Plane(elevation);
            if (plane == null)
                return null;

            return Cut(partition, plane, tolerance);
        }
        
        public static List<IPartition> Cut(this IPartition partition, Plane plane, double tolerance = Tolerance.Distance)
        {
            if (plane == null)
                return null;

            Face3D face3D = partition?.Face3D;
            if (face3D == null)
                return null;

            List<IPartition> result = new List<IPartition>();
            
            List<Face3D> face3Ds = Geometry.Spatial.Query.Cut(face3D, plane, tolerance);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                result.Add(partition.Clone());
                return result;
            }

            foreach(Face3D face3D_New in face3Ds)
            {
                if (face3D_New == null)
                    continue;

                IPartition partition_New = null;
                if(partition is IHostPartition)
                {
                    partition_New = Create.HostPartition(face3D_New, ((IHostPartition)partition).Type());
                }
                else
                {
                    partition_New = new AirPartition(face3D_New);
                }

                if(partition_New != null)
                {
                    result.Add(partition_New);
                }
            }

            return result;
        }

        public static List<IPartition> Cut(this IPartition partition, IEnumerable<Plane> planes, double tolerance = Tolerance.Distance)
        {
            if (partition == null || planes == null)
                return null;

            List<IPartition> result = new List<IPartition>() { partition.Clone() };

            if (planes.Count() == 0)
                return result;

            foreach (Plane plane in planes)
            {
                Dictionary<System.Guid, IPartition> dictionary = new Dictionary<System.Guid, IPartition>();
                foreach (IPartition partition_Temp in result)
                {
                    List<IPartition> panels_Temp = Cut(partition_Temp, plane, tolerance);
                    if (panels_Temp != null)
                        panels_Temp.ForEach(x => dictionary[x.Guid] = x);
                }

                result = dictionary.Values.ToList();
            }

            return result;
        }

        public static List<IPartition> Cut(this IPartition partition, IEnumerable<double> elevations, double tolerance = Tolerance.Distance)
        {
            if (partition == null || elevations == null)
                return null;

            List<Plane> planes = elevations.ToList().ConvertAll(x => Geometry.Spatial.Create.Plane(x));
            if (planes == null)
                return null;

            return Cut(partition, planes, tolerance);
        }

        public static List<IPartition> Cut(this ArchitecturalModel architecturalModel, Plane plane, IEnumerable<Room> rooms = null, double tolerance = Tolerance.Distance)
        {
            if (architecturalModel == null || plane == null)
            {
                return null;
            }

            List<IPartition> partitions = null;
            if (rooms == null || rooms.Count() == 0)
            {
                partitions = architecturalModel.GetPartitions();
            }
            else
            {
                partitions = new List<IPartition>();
                foreach (Room room in rooms)
                {
                    List<IPartition> partitions_Room = architecturalModel.GetPartitions(room);
                    if (partitions_Room == null || partitions_Room.Count == 0)
                    {
                        continue;
                    }

                    foreach (IPartition partition_Room in partitions_Room)
                    {
                        if (partitions.Find(x => x.Guid == partition_Room.Guid) == null)
                        {
                            partitions.Add(partition_Room);
                        }
                    }
                }
            }

            if (partitions == null || partitions.Count == 0)
            {
                return null;
            }

            List<IPartition> result = new List<IPartition>();
            foreach (IPartition partition in partitions)
            {
                List<IPartition> partitions_Cut = partition.Cut(plane, tolerance);
                if (partitions_Cut != null && partitions_Cut.Count > 1)
                {
                    List<object> relatedObjects = architecturalModel.GetRelatedObjects(partition);
                    if (architecturalModel.RemoveObject(partition))
                    {
                        foreach (IPartition partition_Cut in partitions_Cut)
                        {
                            architecturalModel.Add(partition_Cut);
                            relatedObjects?.ForEach(x => architecturalModel.AddRelation(partition_Cut, x));
                        }
                    }

                    result.AddRange(partitions_Cut);
                }
            }

            return result;
        }
    }
}