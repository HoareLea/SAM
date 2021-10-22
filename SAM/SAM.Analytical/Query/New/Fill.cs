using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<IPartition> Fill(this Face3D face3D, IEnumerable<IPartition> partitions, double offset = 0.1, double tolerance_Area = Core.Tolerance.MacroDistance, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3D == null || partitions == null)
            {
                return null;
            }

            List<Tuple<IPartition, Face3D, Point3D>> tuples_Partition = new List<Tuple<IPartition, Face3D, Point3D>>();
            foreach(IPartition partition in partitions)
            {
                Face3D face3D_Partition = partition?.Face3D;
                if(face3D_Partition == null)
                {
                    continue;
                }

                Point3D point3D = face3D_Partition.InternalPoint3D(tolerance_Distance);
                if(point3D == null)
                {
                    continue;
                }

                tuples_Partition.Add(new Tuple<IPartition, Face3D, Point3D>(partition, face3D_Partition, point3D));
            }

            List<IPartition> result = new List<IPartition>();
            if(tuples_Partition == null || tuples_Partition.Count == 0)
            {
                return result;
            }

            List<Face3D> face3Ds = Geometry.Spatial.Query.Fill(face3D, tuples_Partition.ConvertAll(x => x.Item2), offset, tolerance_Area, tolerance_Distance);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return result;
            }

            foreach(Face3D face3D_Temp in face3Ds)
            {
                Plane plane = face3D_Temp.GetPlane();
                if(plane == null)
                {
                    continue;
                }

                BoundingBox3D boundingBox3D = face3D_Temp.GetBoundingBox();
                if(boundingBox3D == null)
                {
                    continue;
                }

                List<Tuple<IPartition, double>> tuples_Distance = new List<Tuple<IPartition, double>>();
                foreach(Tuple<IPartition, Face3D, Point3D> tuple in tuples_Partition)
                {
                    Point3D point3D = plane.Project(tuple.Item3);
                    if(point3D == null)
                    {
                        continue;
                    }

                    if(!boundingBox3D.InRange(point3D, tolerance_Distance) || !face3D_Temp.Inside(point3D, tolerance_Distance))
                    {
                        continue;
                    }

                    tuples_Distance.Add(new Tuple<IPartition, double>(tuple.Item1, tuple.Item3.Distance(point3D)));
                }

                if(tuples_Distance == null || tuples_Distance.Count == 0)
                {
                    continue;
                }

                if(tuples_Distance.Count > 1)
                {
                    tuples_Distance.Sort((x, y) => x.Item2.CompareTo(y.Item2));
                }

                IPartition partition = tuples_Distance[0].Item1;

                IPartition partition_New = Create.Partition(partition, partition.Guid, face3D_Temp, tolerance_Distance);
                if(partition_New != null)
                {
                    partition = partition_New;
                }

                if(partition == null)
                {
                    continue;
                }

                result.Add(partition);
            }

            return result;
        }
    }
}