using NetTopologySuite.Geometries;
using SAM.Geometry;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<IPartition> MergeCoplanar(this IEnumerable<IPartition> partitions, double offset, bool validateHostPartitionType = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (partitions == null)
                return null;

            return MergeCoplanar(partitions?.ToList(), offset, out List<IPartition> redundantPartitions, validateHostPartitionType, minArea, tolerance);
        }

        public static List<IPartition> MergeCoplanar(this IEnumerable<IPartition> partitions, double offset, out List<IPartition> redundantPartitions, bool validateHostPartitionType = true, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            redundantPartitions = null;

            if (partitions == null)
                return null;

            List<IPartition> partitions_Temp = partitions.ToList();

            partitions_Temp.Sort((x, y) => y.Face3D.GetArea().CompareTo(x.Face3D.GetArea()));

            List<IPartition> result = new List<IPartition>(partitions);
            HashSet<Guid> guids = new HashSet<Guid>();

            redundantPartitions = new List<IPartition>();

            while (partitions_Temp.Count > 0)
            {
                IPartition partition = partitions_Temp[0];
                partitions_Temp.RemoveAt(0);

                Plane plane = partition.Face3D.GetPlane();
                if (plane == null)
                    continue;

                HostPartitionType hostPartitionType = (partition as IHostPartition)?.Type();

                List<IPartition> partitions_Offset = new List<IPartition>();
                foreach (IPartition partition_Temp in partitions_Temp)
                {
                    if(partition.GetType() != partition_Temp?.GetType())
                    {
                        continue;
                    }

                    if(validateHostPartitionType && hostPartitionType != null)
                    {
                        HostPartitionType hostPartitionType_Temp = (partition_Temp as IHostPartition)?.Type();
                        if (hostPartitionType_Temp != null && hostPartitionType != null)
                        {
                            if (!hostPartitionType_Temp.Name.Equals(hostPartitionType.Name))
                                continue;
                        }
                        else if (!(hostPartitionType_Temp == null && hostPartitionType == null))
                        {
                            continue;
                        }
                    }

                    Plane plane_Temp = partition_Temp?.Face3D.GetPlane();
                    if (plane == null)
                        continue;

                    if (!plane.Coplanar(plane_Temp, tolerance))
                        continue;

                    double distance = plane.Distance(plane_Temp, tolerance);
                    if (distance > offset)
                        continue;

                    partitions_Offset.Add(partition_Temp);
                }

                if (partitions_Offset == null || partitions_Offset.Count == 0)
                    continue;

                partitions_Offset.Add(partition);

                List<Tuple<Polygon, IPartition>> tuples_Polygon = new List<Tuple<Polygon, IPartition>>();
                List<Point2D> point2Ds = new List<Point2D>(); //Snap Points
                foreach (IPartition partition_Temp in partitions_Offset)
                {
                    Face3D face3D = partition_Temp.Face3D;
                    foreach (IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                    {
                        ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                        if (segmentable3D == null)
                            continue;

                        segmentable3D.GetPoints()?.ForEach(x => Geometry.Planar.Modify.Add(point2Ds, plane.Convert(x), tolerance));
                    }

                    Face2D face2D = plane.Convert(plane.Project(face3D));

                    //tuples_Polygon.Add(new Tuple<Polygon, Panel>(face2D.ToNTS(), panel_Temp));
                    tuples_Polygon.Add(new Tuple<Polygon, IPartition>(face2D.ToNTS(tolerance), partition_Temp));
                }

                List<Polygon> polygons_Temp = tuples_Polygon.ConvertAll(x => x.Item1);
                Geometry.Planar.Modify.RemoveAlmostSimilar_NTS(polygons_Temp, tolerance);

                polygons_Temp = Geometry.Planar.Query.Union(polygons_Temp);
                foreach (Polygon polygon in polygons_Temp)
                {
                    if (polygon.Area < minArea)
                        continue;

                    List<Tuple<Polygon, IPartition>> tuples_Partition = tuples_Polygon.FindAll(x => polygon.Contains(x.Item1.InteriorPoint));
                    if (tuples_Partition == null || tuples_Partition.Count == 0)
                        continue;

                    tuples_Partition.Sort((x, y) => y.Item1.Area.CompareTo(x.Item1.Area));

                    foreach (Tuple<Polygon, IPartition> tuple in tuples_Partition)
                    {
                        result.Remove(tuple.Item2);
                        partitions_Temp.Remove(tuple.Item2);
                    }

                    IPartition partition_Old = tuples_Partition.First().Item2;
                    tuples_Partition.RemoveAt(0);
                    redundantPartitions.AddRange(tuples_Partition.ConvertAll(x => x.Item2));

                    if (partition_Old == null)
                        continue;

                    Polygon polygon_Temp = Geometry.Planar.Query.SimplifyBySnapper(polygon, tolerance);
                    polygon_Temp = Geometry.Planar.Query.SimplifyByTopologyPreservingSimplifier(polygon_Temp, tolerance);

                    Face2D face2D = polygon_Temp.ToSAM(minArea, Core.Tolerance.MicroDistance)?.Snap(point2Ds, tolerance);
                    if (face2D == null)
                        continue;

                    Face3D face3D = new Face3D(plane, face2D);
                    Guid guid = partition_Old.Guid;
                    if (guids.Contains(guid))
                    {
                        guid = Guid.NewGuid();
                    }

                    IPartition partition_New = Create.Partition(partition_Old, guid, face3D, tolerance);
                    if(partition_New is IHostPartition)
                    {
                        //Adding Openings from redundant Panels
                        foreach (IPartition partition_Redundant in tuples_Partition.ConvertAll(x => x.Item2))
                        {
                            IHostPartition hostPartition = partition_Redundant as IHostPartition;
                            if (hostPartition == null)
                                continue;

                            List<IOpening> openings = hostPartition.GetOpenings();
                            if (openings == null || openings.Count == 0)
                                continue;

                            openings.ForEach(x => ((IHostPartition)partition_New).AddOpening(x, tolerance));
                        }
                    }

                    if(partition_New != null)
                    {
                        result.Add(partition_New);
                    }
                }
            }

            return result;
        }
    }
}