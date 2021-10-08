using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static ArchitecturalModel ArchitecturalModel(this IEnumerable<ISegmentable2D> segmentable2Ds, double elevation_Min, double elevation_Max, double tolerance_Distance = Tolerance.Distance, double tolerance_Angle = Tolerance.Angle)
        {
            if (segmentable2Ds == null || double.IsNaN(elevation_Min) || double.IsNaN(elevation_Max))
                return null;

            Level level = Level(elevation_Min);

            Plane plane_Min = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Min)) as Plane;
            Plane plane_Max = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Max)) as Plane;

            ArchitecturalModel result = new ArchitecturalModel(null, null, null, new PlanarTerrain(plane_Min));

            Plane plane_Min_Flipped = new Plane(plane_Min);
            plane_Min_Flipped.FlipZ();

            Plane plane_Location = plane_Min.GetMoved(new Vector3D(0, 0, (elevation_Min + elevation_Max) / 2)) as Plane;

            List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segmentable2Ds, tolerance_Distance);
            if (polygon2Ds != null && polygon2Ds.Count != 0)
            {
                List<Tuple<Segment2D, HostPartition, Room>> tuples = new List<Tuple<Segment2D, HostPartition, Room>>();
                Dictionary<Room, List<HostPartition>> dictionary = new Dictionary<Room, List<HostPartition>>();

                for (int i = 0; i < polygon2Ds.Count; i++)
                {
                    Polygon2D polygon2D = polygon2Ds[i]?.SimplifyBySAM_Angle();
                    if (polygon2D == null)
                        polygon2D = polygon2Ds[i];

                    Room room = new Room(string.Format("Cell {0}", i + 1), plane_Location.Convert(polygon2D.GetInternalPoint2D()));
                    room.SetValue(RoomParameter.LevelName, level.Name);

                    double area = polygon2D.Area();

                    room.SetValue(RoomParameter.Area, area);
                    room.SetValue(RoomParameter.Volume, area * System.Math.Abs(elevation_Max - elevation_Min));

                    dictionary[room] = new List<HostPartition>();

                    List<Segment2D> segment2Ds = polygon2D.GetSegments();
                    if (segment2Ds == null || segment2Ds.Count < 3)
                        continue;

                    segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true);

                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        Segment3D segment3D_Min = plane_Min.Convert(segment2D);
                        Segment3D segment3D_Max = plane_Max.Convert(segment2D);

                        Polygon3D polygon3D = new Polygon3D(new Point3D[] { segment3D_Max[0], segment3D_Max[1], segment3D_Min[1], segment3D_Min[0] });
                        HostPartition hostPartition = HostPartition(new Face3D(polygon3D), null, tolerance_Angle);

                        tuples.Add(new Tuple<Segment2D, HostPartition, Room>(segment2D, hostPartition, room));
                    }

                    Polygon3D polygon3D_Min = plane_Min.Convert(polygon2D);
                    polygon3D_Min = plane_Min_Flipped.Convert(plane_Min_Flipped.Convert(polygon3D_Min));
                    if (polygon3D_Min != null)
                    {
                        HostPartition hostPartition = HostPartition(new Face3D(polygon3D_Min), null, tolerance_Angle);
                        if(hostPartition != null)
                        {
                            dictionary[room].Add(hostPartition);
                        }
                    }

                    Polygon3D polygon3D_Max = plane_Max.Convert(polygon2D);
                    if (polygon3D_Max != null)
                    {
                        HostPartition hostPartition = HostPartition(new Face3D(polygon3D_Max), null, tolerance_Angle);
                        if (hostPartition != null)
                        {
                            dictionary[room].Add(hostPartition);
                        }
                    }
                }

                while (tuples.Count > 0)
                {
                    Tuple<Segment2D, HostPartition, Room> tuple = tuples[0];
                    Segment2D segment2D = tuple.Item1;

                    List<Tuple<Segment2D, HostPartition, Room>> tuples_Temp = tuples.FindAll(x => segment2D.AlmostSimilar(x.Item1));
                    tuples.RemoveAll(x => tuples_Temp.Contains(x));

                    HostPartition hostPartition = tuple.Item2;

                    foreach (Tuple<Segment2D, HostPartition, Room> tuple_Temp in tuples_Temp)
                        dictionary[ tuple_Temp.Item3].Add(hostPartition);
                }

                foreach(KeyValuePair<Room, List<HostPartition>> keyValuePair in dictionary)
                {
                    result.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return result;
        }

        public static ArchitecturalModel ArchitecturalModel(this IEnumerable<Face3D> face3Ds, double elevation_Ground = 0, double tolerance = Tolerance.Distance)
        {
            if (face3Ds == null)
                return null;

            Plane plane_Default = Plane.WorldXY;
            plane_Default.Move(new Vector3D(0, 0, elevation_Ground));

            Dictionary<Face2D, Face3D> dictionary_Project = new Dictionary<Face2D, Face3D>();
            foreach (Face3D face3D in face3Ds)
            {
                Face3D face3D_Project = plane_Default.Project(face3D);
                if (face3D_Project == null)
                    continue;

                Face2D face2D = plane_Default.Convert(face3D_Project);
                if (face2D == null)
                    continue;

                dictionary_Project[face2D] = face3D;
            }

            if (dictionary_Project.Count == 0)
                return null;

            List<Face2D> face2Ds = Geometry.Planar.Query.Union(dictionary_Project.Keys);
            if (face2Ds == null)
                return null;

            Dictionary<Face2D, List<Face3D>> dictionary_Common = new Dictionary<Face2D, List<Face3D>>();
            foreach (KeyValuePair<Face2D, Face3D> keyValuePair_Project in dictionary_Project)
            {
                Face2D face2D = null;
                foreach (Face2D face2D_Temp in face2Ds)
                {
                    if (face2D_Temp.InRange(keyValuePair_Project.Key.GetInternalPoint2D(), tolerance))
                    {
                        face2D = face2D_Temp;
                        break;
                    }
                }

                if (face2D == null)
                    continue;

                if (!dictionary_Common.ContainsKey(face2D))
                    dictionary_Common[face2D] = new List<Face3D>();

                dictionary_Common[face2D].Add(keyValuePair_Project.Value);
            }

            List<Dictionary<Room, List<HostPartition>>> dictionaries_Room = new List<Dictionary<Room, List<HostPartition>>>();
            foreach (List<Face3D> face3Ds_Common in dictionary_Common.Values)
            {
                Dictionary<double, List<Face2D>> dictionary = new Dictionary<double, List<Face2D>>();
                foreach (Face3D face3D in face3Ds_Common)
                {
                    BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();
                    if (boundingBox3D == null)
                        continue;

                    double elevation = Core.Query.Round(boundingBox3D.Min.Z, tolerance);
                    plane_Default.Move(new Vector3D(0, 0, elevation));

                    Face2D face2D = plane_Default.Convert(plane_Default.Project(face3D));
                    if (face2D == null)
                        continue;

                    List<Face2D> face2Ds_Elevation = null;
                    if (!dictionary.TryGetValue(elevation, out face2Ds_Elevation))
                    {
                        face2Ds_Elevation = new List<Face2D>();
                        dictionary[elevation] = face2Ds_Elevation;
                    }

                    face2Ds_Elevation.Add(face2D);
                }

                List<double> elevations = dictionary.Keys.ToList();
                if (!elevations.Contains(elevation_Ground))
                {
                    elevations.Sort();
                    for (int i = 1; i < elevations.Count; i++)
                    {
                        if (elevation_Ground > elevations[i])
                            continue;

                        dictionary[elevation_Ground] = dictionary[elevations[i - 1]];
                        break;
                    }

                    if (!dictionary.ContainsKey(elevation_Ground))
                        dictionary[elevation_Ground] = dictionary[elevations.Last()];
                }

                Dictionary<Room, List<HostPartition>> dictionary_Room = Query.RoomDictionary(dictionary, dictionary.Keys.ToList().IndexOf(elevation_Ground), tolerance);
                if (dictionary_Room != null)
                    dictionaries_Room.Add(dictionary_Room);
            }

            ArchitecturalModel result = new ArchitecturalModel(null, null, null, PlanarTerrain(elevation_Ground));
            foreach (Dictionary<Room, List<HostPartition>> dictionary_Room in dictionaries_Room)
            {
                foreach (KeyValuePair<Room, List<HostPartition>> keyValuePair in dictionary_Room)
                {
                    result.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return result;
        }

        public static ArchitecturalModel ArchitecturalModel(this IEnumerable<Shell> shells, IEnumerable<IPartition> partitions, double groundElevation = 0, bool addMissingPartitions = true, double thinnessRatio = 0.01, double minArea = Tolerance.MacroDistance, double maxDistance = 0.1, double maxAngle = 0.0872664626, double silverSpacing = Tolerance.MacroDistance, double tolerance_Distance = Tolerance.Distance, double tolerance_Angle = Tolerance.Angle)
        {
            if(shells == null && partitions == null)
            {
                return null;
            }

            List<Tuple<Room, List<IPartition>>> tuples = new List<Tuple<Room, List<IPartition>>>();

            List<Tuple<Plane, Face3D, IPartition, BoundingBox3D, double>> tuples_Partition = new List<Tuple<Plane, Face3D, IPartition, BoundingBox3D, double>>();
            foreach (IPartition partition in partitions)
            {
                Face3D face3D = partition?.Face3D;
                if (face3D == null)
                    continue;

                Plane plane = face3D.GetPlane();
                if (plane == null)
                    continue;

                double area = face3D.GetArea();

                if (area < minArea || face3D.ThinnessRatio() < thinnessRatio) // Changed from tolerance_Distance to minArea
                    continue;

                tuples_Partition.Add(new Tuple<Plane, Face3D, IPartition, BoundingBox3D, double>(plane, face3D, partition, face3D.GetBoundingBox(tolerance_Distance), area));
            }

            tuples_Partition.Sort((x, y) => y.Item5.CompareTo(x.Item5));

            BoundingBox3D boundingBox3D_All = new BoundingBox3D(tuples_Partition.ConvertAll(x => x.Item4));

            List<Tuple<Point3D, IPartition, BoundingBox3D>> tuples_Partition_New = new List<Tuple<Point3D, IPartition, BoundingBox3D>>();

            List<Shell> shells_Temp = Enumerable.Repeat<Shell>(null, shells.Count()).ToList();
            Parallel.For(0, shells.Count(), (int i) =>
            {
                Shell shell = shells.ElementAt(i);

                BoundingBox3D boundingBox3D = shell?.GetBoundingBox();
                if (boundingBox3D == null)
                {
                    return;
                }

                if (!boundingBox3D_All.InRange(boundingBox3D))
                {
                    return;
                }

                shell = shell.RemoveInvalidFace3Ds(silverSpacing);
                if (shell == null)
                {
                    return;
                }

                Shell shell_Merge = shell.Merge(tolerance_Distance);
                if (shell_Merge == null)
                {
                    shell_Merge = new Shell(shell);
                }

                Shell shell_FixEdges = shell_Merge.FixEdges(tolerance_Distance);
                if(shell_FixEdges == null)
                {
                    shell_FixEdges = new Shell(shell_Merge);
                }

                shells_Temp[i] = shell_FixEdges;
            });

            shells_Temp.RemoveAll(x => x == null);
            List<IPartition> hostPartition_Merged = Query.MergeCoplanar(tuples_Partition.ConvertAll(x => x.Item3), maxDistance, true, minArea, tolerance_Distance);
            shells_Temp.FillFace3Ds(hostPartition_Merged.ConvertAll(x => x.Face3D), 0.1, maxDistance, maxAngle, silverSpacing, tolerance_Distance);
            shells_Temp.SplitCoplanarFace3Ds(tolerance_Angle, tolerance_Distance);

            shells_Temp = shells_Temp.Snap(shells, silverSpacing, tolerance_Distance);


            //Creating Elevations
            List<double> elevations = new List<double>();
            foreach (Shell shell_Temp in shells_Temp)
            {
                BoundingBox3D boundingBox3D = shell_Temp?.GetBoundingBox();
                if (boundingBox3D == null)
                {
                    continue;
                }

                double elevation = Core.Query.Round(boundingBox3D.Min.Z, silverSpacing);
                int index = elevations.FindIndex(x => elevation.AlmostEqual(x, silverSpacing));
                if (index == -1)
                {
                    elevations.Add(elevation);
                }
            }

            elevations.Sort();

            //Creating Levels
            List<Level> levels = new List<Level>();
            for (int i = 0; i < elevations.Count; i++)
            {
                levels.Add(new Level("Level " + i.ToString(), elevations[i]));
            }

            HashSet<Guid> guids = new HashSet<Guid>();

            //Creating Rooms and Partitions
            for (int i = 0; i < shells_Temp.Count; i++)
            {
                Shell shell_Temp = shells_Temp[i];

                BoundingBox3D boundingBox3D_Shell = shell_Temp.GetBoundingBox(maxDistance);
                if (boundingBox3D_Shell == null)
                {
                    continue;
                }

                List<Face3D> face3Ds = shell_Temp?.Face3Ds;
                if (face3Ds == null || face3Ds.Count == 0)
                {
                    continue;
                }

                Room room = new Room(string.Format("Cell {0}", i), shell_Temp.CalculatedInternalPoint3D(silverSpacing, tolerance_Distance));
                double elevation = shell_Temp.GetBoundingBox().Min.Z;

                List<Tuple<double, Level>> tuples_Level = levels.ConvertAll(x => new Tuple<double, Level>(System.Math.Abs(x.Elevation - elevation), x));
                tuples_Level.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                Level level = tuples_Level.FirstOrDefault()?.Item2;
                if (level != null)
                {
                    room.SetValue(RoomParameter.LevelName, level.Name);
                }

                double volume_Shell = shell_Temp.Volume(silverSpacing, tolerance_Distance);
                if (!double.IsNaN(volume_Shell))
                {
                    room.SetValue(RoomParameter.Volume, volume_Shell);
                }

                double area_Shell = shell_Temp.Area(silverSpacing, tolerance_Angle, tolerance_Distance, silverSpacing);
                if (!double.IsNaN(area_Shell))
                {
                    room.SetValue(RoomParameter.Area, area_Shell);
                }

                Tuple<Room, List<IPartition>> tuple = new Tuple<Room, List<IPartition>>(room, new List<IPartition>());
                tuples.Add(tuple);

                List<Tuple<Plane, Face3D, IPartition, BoundingBox3D, double>> tuples_hostPartition_Temp = tuples_Partition.FindAll(x => boundingBox3D_Shell.InRange(x.Item4, tolerance_Distance));
                
                foreach (Face3D face3D in face3Ds)
                {
                    Plane plane = face3D?.GetPlane();
                    if (plane == null)
                        continue;

                    if (face3D.ThinnessRatio() < thinnessRatio)
                    {
                        double area = face3D.GetArea();
                        if (area < minArea)
                        {
                            continue;
                        }
                    }

                    Point3D point3D_Internal = face3D.InternalPoint3D(tolerance_Distance);
                    if (point3D_Internal == null)
                    {
                        continue;
                    }

                    BoundingBox3D boundingBox3D_Face3D = face3D.GetBoundingBox(maxDistance);

                    IPartition partition_New = null;

                    List<Tuple<Point3D, IPartition, Face3D>> tuples_Face3D = tuples_Partition_New?.FindAll(x => boundingBox3D_Face3D.InRange(x.Item3, tolerance_Distance)).ConvertAll(x => new Tuple<Point3D, IPartition, Face3D>(x.Item1, x.Item2, x.Item2.Face3D));
                    if (tuples_Face3D != null && tuples_Face3D.Count != 0)
                    {
                        double area = face3D.GetArea();
                        tuples_Face3D.RemoveAll(x => !area.AlmostEqual(x.Item3.GetArea(), minArea));
                        if (tuples_Face3D != null && tuples_Face3D.Count != 0)
                        {
                            List<Tuple<Point3D, IPartition, Face3D, double>> tuples_Distance = tuples_Face3D.ConvertAll(x => new Tuple<Point3D, IPartition, Face3D, double>(x.Item1, x.Item2, x.Item3, System.Math.Min(x.Item3.Distance(point3D_Internal), face3D.Distance(x.Item1))));

                            if (tuples_Distance.Count > 1)
                                tuples_Distance.Sort((x, y) => x.Item4.CompareTo(y.Item4));

                            partition_New = tuples_Distance[0].Item4 < silverSpacing ? tuples_Distance[0].Item2 : null;
                        }
                    }

                    if (partition_New == null)
                    {
                        List<Tuple<Face2D, IPartition>> tuples_Face2D_All = new List<Tuple<Face2D, IPartition>>();
                        foreach (Tuple<Plane, Face3D, IPartition, BoundingBox3D, double> tuple_Partition in tuples_hostPartition_Temp)
                        {
                            if (!boundingBox3D_Face3D.InRange(tuple_Partition.Item4, maxDistance))
                                continue;

                            Plane plane_Partition = tuple_Partition.Item1;

                            if (plane_Partition.Normal.SmallestAngle(plane.Normal.GetNegated()) > maxAngle && plane_Partition.Normal.SmallestAngle(plane.Normal) > maxAngle)
                                continue;

                            double distance = tuple_Partition.Item2.Distance(face3D, tolerance_Distance: tolerance_Distance);

                            if (distance > maxDistance)
                                continue;

                            Face2D face2D = plane.Convert(plane.Project(tuple_Partition.Item2));
                            if (face2D == null)
                                continue;

                            tuples_Face2D_All.Add(new Tuple<Face2D, IPartition>(face2D, tuple_Partition.Item3));
                        }

                        if (tuples_Face2D_All != null && tuples_Face2D_All.Count != 0)
                        {
                            List<Tuple<Face2D, IPartition, double>> tuples_Face2D = tuples_Face2D_All.ConvertAll(x => new Tuple<Face2D, IPartition, double>(x.Item1, x.Item2, 0));

                            //Find By Face2D Intersection
                            Face2D face2D_Shell = plane.Convert(face3D);
                            for (int j = tuples_Face2D.Count - 1; j >= 0; j--)
                            {
                                List<Face2D> face2Ds_Intersection = Geometry.Planar.Query.Intersection(face2D_Shell, tuples_Face2D[j].Item1, tolerance_Distance);
                                face2Ds_Intersection?.RemoveAll(x => x == null || x.GetArea() <= minArea);

                                if (face2Ds_Intersection == null || face2Ds_Intersection.Count == 0)
                                {
                                    tuples_Face2D.RemoveAt(j);
                                }
                                else
                                {
                                    tuples_Face2D[j] = new Tuple<Face2D, IPartition, double>(tuples_Face2D[j].Item1, tuples_Face2D[j].Item2, face2Ds_Intersection.ConvertAll(x => x.GetArea()).Sum());
                                }
                            }

                            List<IPartition> partitions_New_Temp = null;

                            if (tuples_Face2D != null && tuples_Face2D.Count != 0)
                            {
                                tuples_Face2D.Sort((x, y) => y.Item3.CompareTo(x.Item3));

                                //Sorting by Face3D Normal
                                Vector3D normal = plane.Normal;
                                List<Tuple<Face2D, IPartition, double>> tuples_Face2D_Temp = tuples_Face2D.FindAll(x => x.Item2.Face3D.GetPlane().Normal.SameHalf(normal));
                                tuples_Face2D.RemoveAll(x => tuples_Face2D_Temp.Contains(x));
                                tuples_Face2D_Temp.AddRange(tuples_Face2D);
                                tuples_Face2D = tuples_Face2D_Temp;

                                partitions_New_Temp = tuples_Face2D_Temp?.ConvertAll(x => x.Item2);
                            }
                            else
                            {
                                //Find the closest hostPartition

                                Point3D point3D = face3D.GetInternalPoint3D(tolerance_Distance);
                                List<Tuple<Face2D, IPartition, double>> tuples_Face2D_Distance = tuples_Face2D_All.ConvertAll(x => new Tuple<Face2D, IPartition, double>(x.Item1, x.Item2, x.Item2.Face3D.Distance(point3D)));
                                tuples_Face2D_Distance.RemoveAll(x => x.Item3 > maxDistance);
                                tuples_Face2D_Distance.Sort((x, y) => x.Item3.CompareTo(y.Item3));

                                //Sorting by Face3D Normal
                                Vector3D normal = plane.Normal;
                                List<Tuple<Face2D, IPartition, double>> tuples_Face2D_Distance_Temp = tuples_Face2D_Distance.FindAll(x => x.Item2.Face3D.GetPlane().Normal.SameHalf(normal));
                                tuples_Face2D_Distance.RemoveAll(x => tuples_Face2D_Distance_Temp.Contains(x));
                                tuples_Face2D_Distance_Temp.AddRange(tuples_Face2D_Distance);
                                tuples_Face2D_Distance = tuples_Face2D_Distance_Temp;

                                partitions_New_Temp = tuples_Face2D_Distance?.ConvertAll(x => x.Item2);
                            }

                            if (partitions_New_Temp != null && partitions_New_Temp.Count != 0)
                            {
                                IPartition partition_New_Temp = partitions_New_Temp[0];

                                Guid guid = partition_New_Temp.Guid;
                                while (guids.Contains(guid))
                                {
                                    guid = Guid.NewGuid();
                                }

                                if(partition_New_Temp is AirPartition)
                                {
                                    partition_New = new AirPartition(guid, face3D);
                                }
                                else
                                {
                                    HostPartitionType hostPartitionType = (partition_New_Temp as HostPartition)?.SAMType as HostPartitionType;

                                    partition_New = HostPartition(guid, face3D, hostPartitionType, tolerance_Distance);

                                    for (int j = 1; j < partitions_New_Temp.Count; j++)
                                    {
                                        HostPartition hostPartition = partitions_New_Temp[j] as HostPartition;
                                        if(hostPartition == null)
                                        {
                                            continue;
                                        }

                                        List<IOpening> openings = hostPartition.Openings;
                                        if (openings == null)
                                        {
                                            continue;
                                        }

                                        foreach(IOpening opening in openings)
                                        {
                                            ((HostPartition)partition_New).AddOpening(opening);
                                        }
                                    }
                                }

                                tuple.Item2.Add(partition_New);

                                tuples_Partition_New.Add(new Tuple<Point3D, IPartition, BoundingBox3D>(point3D_Internal, partition_New, face3D.GetBoundingBox(tolerance_Distance)));
                            }
                        }
                    }

                    if (partition_New == null)
                    {
                        if (!addMissingPartitions)
                            continue;

                        partition_New = new AirPartition(face3D);
                        tuple.Item2.Add(partition_New);
                    }
                }
            }

            ArchitecturalModel result = new ArchitecturalModel(null, null, null, PlanarTerrain(groundElevation));
            foreach (Tuple<Room, List<IPartition>> tuple in tuples)
            {
                result.Add(tuple.Item1, tuple.Item2);
            }

            result.UpdateNormals(false, silverSpacing, tolerance_Distance);

            return result;
        }
    }
}