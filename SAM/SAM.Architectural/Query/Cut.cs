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
    }
}