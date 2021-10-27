using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<T> Cut<T>(this T partition, double elevation, double tolerance = Tolerance.Distance) where T : IPartition
        {
            if (partition == null || double.IsNaN(elevation))
                return null;

            Plane plane = Geometry.Spatial.Create.Plane(elevation);
            if (plane == null)
                return null;

            return Cut<T>(partition, plane, tolerance);
        }
        
        public static List<T> Cut<T>(this T partition, Plane plane, double tolerance = Tolerance.Distance) where T : IPartition
        {
            if (plane == null)
                return null;

            Face3D face3D = partition?.Face3D;
            if (face3D == null)
                return null;

            List<T> result = new List<T>();
            
            List<Face3D> face3Ds = Geometry.Spatial.Query.Cut(face3D, plane, tolerance);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                result.Add(partition.Clone());
                return result;
            }

            for(int i =0; i < face3Ds.Count; i++)
            {
                System.Guid guid = i == 0 ? partition.Guid : System.Guid.NewGuid();
                
                T partition_New = Create.Partition(partition, guid, face3Ds[i], tolerance);
                if(partition_New == null)
                {
                    continue;
                }

                result.Add(partition_New);
            }

            return result;
        }

        public static List<T> Cut<T>(this T partition, IEnumerable<Plane> planes, double tolerance = Tolerance.Distance) where T : IPartition
        {
            if (partition == null || planes == null)
                return null;

            List<T> result = new List<T>() { partition.Clone() };

            if (planes.Count() == 0)
                return result;

            foreach (Plane plane in planes)
            {
                Dictionary<System.Guid, T> dictionary = new Dictionary<System.Guid, T>();
                foreach (T partition_Temp in result)
                {
                    List<T> partitions_Temp = Cut(partition_Temp, plane, tolerance);
                    if (partitions_Temp != null)
                        partitions_Temp.ForEach(x => dictionary[x.Guid] = x);
                }

                result = dictionary.Values.ToList();
            }

            return result;
        }

        public static List<T> Cut<T>(this T partition, IEnumerable<double> elevations, double tolerance = Tolerance.Distance) where T: IPartition
        {
            if (partition == null || elevations == null)
                return null;

            List<Plane> planes = elevations.ToList().ConvertAll(x => Geometry.Spatial.Create.Plane(x));
            if (planes == null)
                return null;

            return Cut(partition, planes, tolerance);
        }

        public static List<T> Cut<T>(this ArchitecturalModel architecturalModel, Plane plane, IEnumerable<Space> spaces = null, double tolerance = Tolerance.Distance) where T : IPartition
        {
            if (architecturalModel == null || plane == null)
            {
                return null;
            }

            List<T> partitions = null;
            if (spaces == null || spaces.Count() == 0)
            {
                partitions = architecturalModel.GetPartitions<T>();
            }
            else
            {
                partitions = new List<T>();
                foreach (Space space in spaces)
                {
                    List<T> partitions_Space = architecturalModel.GetPartitions<T>(space);
                    if (partitions_Space == null || partitions_Space.Count == 0)
                    {
                        continue;
                    }

                    foreach (T partition_Space in partitions_Space)
                    {
                        if (partitions.Find(x => x.Guid == partition_Space.Guid) == null)
                        {
                            partitions.Add(partition_Space);
                        }
                    }
                }
            }

            if (partitions == null || partitions.Count == 0)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (T partition in partitions)
            {
                List<T> partitions_Cut = partition.Cut(plane, tolerance);
                if (partitions_Cut != null && partitions_Cut.Count > 1)
                {
                    List<IJSAMObject> relatedObjects = architecturalModel.GetRelatedObjects(partition);
                    if (architecturalModel.RemoveObject(partition))
                    {
                        foreach (T partition_Cut in partitions_Cut)
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

        public static List<IPartition> Cut(this ArchitecturalModel architecturalModel, Plane plane, IEnumerable<Space> spaces = null, double tolerance = Tolerance.Distance)
        {
            return Cut<IPartition>(architecturalModel, plane, spaces, tolerance);
        }
    }
}