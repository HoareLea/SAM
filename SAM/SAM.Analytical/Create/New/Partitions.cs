using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<IPartition> Partitions(this ArchitecturalModel architecturalModel, Plane plane, out List<IPartition> existingPartitions, IEnumerable<Room> rooms = null, HostPartitionType hostPartitionType = null, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            existingPartitions = null;

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

            Dictionary<IPartition, List<ISegmentable2D>> dictionary = partitions.SectionDictionary<IPartition, ISegmentable2D>(plane, tolerance_Distance);
            if (dictionary == null || dictionary.Count == 0)
            {
                return null;
            }

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (List<ISegmentable2D> segmentable2Ds in dictionary.Values)
            {
                List<Segment2D> segment2Ds_Temp = Geometry.Planar.Query.Segment2Ds(segmentable2Ds);
                if (segment2Ds_Temp != null)
                {
                    segment2Ds.AddRange(segment2Ds_Temp);
                }
            }

            segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance_Distance);
            segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, tolerance_Snap);

            List<Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(segment2Ds);
            if (face2Ds == null || face2Ds.Count == 0)
            {
                return null;
            }

            existingPartitions = new List<IPartition>();

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (Face2D face2D in face2Ds)
            {
                Face3D face3D = plane.Convert(face2D);
                if (face3D == null)
                {
                    continue;
                }

                List<Face2D> face2Ds_Difference = new List<Face2D>() { face2D };

                List<IPartition> partitions_Face3D = Geometry.Spatial.Query.Face3DObjectsByFace3D(partitions, face3D, tolerance_Distance, tolerance_Snap, tolerance_Angle, tolerance_Distance);
                if (partitions_Face3D != null && partitions_Face3D.Count != 0)
                {
                    existingPartitions.AddRange(partitions_Face3D);

                    foreach (IPartition partition_Face3D in partitions_Face3D)
                    {
                        if (face2Ds_Difference == null || face2Ds_Difference.Count == 0)
                        {
                            break;
                        }

                        Face2D face2D_Panel = plane.Convert(plane.Project(partition_Face3D.Face3D));
                        if (face2D_Panel == null)
                        {
                            continue;
                        }

                        int count = face2Ds_Difference.Count;
                        for (int i = count - 1; i >= 0; i--)
                        {
                            Face2D face2D_Difference = face2Ds_Difference[i];
                            List<Face2D> face2Ds_Difference_Temp = face2D_Difference.Difference(face2D_Panel, tolerance_Distance);
                            if (face2Ds_Difference_Temp == null || face2Ds_Difference_Temp.Count == 0)
                            {
                                continue;
                            }

                            face2Ds_Difference.RemoveAt(i);
                            face2Ds_Difference.AddRange(face2Ds_Difference_Temp);
                        }
                    }
                }

                if (face2Ds_Difference == null || face2Ds_Difference.Count == 0)
                {
                    continue;
                }

                for (int i = 0; i < face2Ds_Difference.Count; i++)
                {
                    face2Ds_Difference[i] = face2Ds_Difference[i].Snap(face2Ds, tolerance_Snap, tolerance_Distance);
                    face3Ds.Add(plane.Convert(face2Ds_Difference[i]));
                }
            }

            List<IPartition> result = new List<IPartition>();

            if (face3Ds == null || face3Ds.Count == 0)
            {
                return result;
            }

            foreach(Face3D face3D in face3Ds)
            {
                IPartition partition = null;

                if(hostPartitionType == null)
                {
                    partition = new AirPartition(face3D);
                }
                else
                {
                    partition = HostPartition(face3D, hostPartitionType, tolerance_Angle);
                }

                if(partition != null)
                {
                    result.Add(partition);
                }
            }

            return result;
        }
    }
}