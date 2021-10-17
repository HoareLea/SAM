using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<AirPartition> AddAirPartitions(this ArchitecturalModel architecturalModel, IEnumerable<Plane> planes, IEnumerable<Room> rooms = null, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if(planes == null)
            {
                return null;
            }

            Dictionary<Guid, AirPartition> result = new Dictionary<Guid, AirPartition>();
            foreach (Plane plane in planes)
            {
                if (plane == null)
                {
                    continue;
                }

                List<AirPartition> airPartitions = AddAirPartitions(architecturalModel, plane, rooms, silverSpacing, tolerance_Angle, tolerance_Distance, tolerance_Snap);
                if (airPartitions != null && airPartitions.Count > 0)
                {
                    foreach(AirPartition airPartition in airPartitions)
                    {
                        if(airPartition == null)
                        {
                            continue;
                        }

                        result[airPartition.Guid] = airPartition;
                    }
                }

            }

            return result?.Values.ToList();
        }

        public static List<AirPartition> AddAirPartitions(this ArchitecturalModel architecturalModel, Plane plane, IEnumerable<Room> rooms = null, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if (architecturalModel == null || plane == null)
            {
                return null;
            }

            List<AirPartition> result = new List<AirPartition>();

            List<Room> rooms_Temp = architecturalModel.GetRooms();
            if (rooms_Temp == null || rooms_Temp.Count == 0)
            {
                return result;
            }

            if (rooms != null)
            {
                for (int i = rooms_Temp.Count - 1; i >= 0; i--)
                {
                    Guid guid = rooms_Temp[i].Guid;

                    bool exists = false;
                    foreach (Room room in rooms)
                    {
                        if (room.Guid == guid)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (exists)
                    {
                        continue;
                    }

                    rooms_Temp.RemoveAt(i);
                }
            }

            List<IPartition> partitions = architecturalModel.Partitions(plane, out List<IPartition> partitions_Existing, rooms_Temp, tolerance_Angle: tolerance_Angle, tolerance_Distance: tolerance_Distance, tolerance_Snap: tolerance_Snap);
            if (partitions == null || partitions.Count == 0)
            {
                return result;
            }

            List<AirPartition> partitions_Air = partitions.FindAll(x => x is AirPartition).ConvertAll(x => (AirPartition)x);
            if (partitions_Air == null || partitions_Air.Count == 0)
            {
                return result;
            }

            architecturalModel.Cut(plane, rooms_Temp, tolerance_Distance);

            List<Tuple<Room, Shell, List<Shell>>> tuples = new List<Tuple<Room, Shell, List<Shell>>>();
            foreach (Room room in rooms_Temp)
            {
                Shell shell = architecturalModel.GetShell(room);

                List<Shell> shells_Cut = shell?.Cut(plane, silverSpacing, tolerance_Angle, tolerance_Distance, tolerance_Snap);
                if (shells_Cut == null || shells_Cut.Count <= 1)
                {
                    continue;
                }

                shells_Cut.RemoveAll(x => x == null || x.GetBoundingBox() == null);
                if (shells_Cut.Count <= 1)
                {
                    continue;
                }

                tuples.Add(new Tuple<Room, Shell, List<Shell>>(room, shell, shells_Cut));
            }

            if (tuples == null || tuples.Count == 0)
            {
                return result;
            }

            List<Face3D> face3Ds_Existing = partitions_Existing?.ConvertAll(x => x.Face3D);

            List<Tuple<Room, List<Tuple<Room, List<IPartition>>>>> tuples_New = Enumerable.Repeat<Tuple<Room, List<Tuple<Room, List<IPartition>>>>>(null, tuples.Count).ToList();

            Parallel.For(0, tuples.Count, (int i) =>
            //for(int i=0; i < tuples.Count; i++)
            {
                Room room = tuples[i].Item1;

                List<IPartition> partitions_Room = architecturalModel.GetPartitions(room);
                if (partitions_Room == null || partitions_Room.Count == 0)
                {
                    return;
                    //continue;
                }

                List<Shell> shells = tuples[i].Item3;
                shells.Sort((x, y) => x.GetBoundingBox().Min.Z.CompareTo(y.GetBoundingBox().Min.Z));

                tuples_New[i] = new Tuple<Room, List<Tuple<Room, List<IPartition>>>>(room, new List<Tuple<Room, List<IPartition>>>());

                int index = 1;
                foreach (Shell shell in shells)
                {
                    shell.SplitCoplanarFace3Ds(face3Ds_Existing, tolerance_Snap, tolerance_Angle, tolerance_Angle, tolerance_Distance);
                    shell.Snap(tuples[i].Item2, tolerance_Snap, tolerance_Distance);

                    List<Face3D> face3Ds_Shell = shell.Face3Ds;
                    if (face3Ds_Shell == null || face3Ds_Shell.Count == 0)
                    {
                        continue;
                    }

                    Point3D point3D = shell.CalculatedInternalPoint3D(tolerance_Snap, tolerance_Distance);
                    if (point3D == null)
                    {
                        continue;
                    }

                    string name = room.Name;
                    if (name == null)
                    {
                        name = string.Empty;
                    }

                    name = string.Format("{0}_{1}", name, index);
                    index++;

                    Room room_New = new Room(Guid.NewGuid(), room, name, point3D);

                    List<IPartition> partitions_New = new List<IPartition>();
                    foreach (Face3D face3D_Shell in face3Ds_Shell)
                    {
                        IPartition partition_Face3D = partitions_Room.Face3DObjectsByFace3D(face3D_Shell, 0, tolerance_Snap, tolerance_Distance)?.FirstOrDefault();
                        if (partition_Face3D == null)
                        {
                            partition_Face3D = partitions_Air.Face3DObjectsByFace3D(face3D_Shell, 0, tolerance_Snap, tolerance_Distance)?.FirstOrDefault();
                            if (partition_Face3D == null)
                            {
                                partition_Face3D = partitions_Existing.Face3DObjectsByFace3D(face3D_Shell, 0, tolerance_Snap, tolerance_Distance)?.FirstOrDefault();
                            }
                        }

                        if (partition_Face3D == null)
                        {
                            continue;
                        }

                        partitions_New.Add(partition_Face3D);
                    }

                    tuples_New[i].Item2.Add(new Tuple<Room, List<IPartition>>(room_New, partitions_New));
                }

            });

            foreach (Tuple<Room, List<Tuple<Room, List<IPartition>>>> tuple in tuples_New)
            {
                if (tuple == null)
                {
                    continue;
                }

                Room room_Old = tuple.Item1;

                List<IJSAMObject> relatedObjects = architecturalModel.GetRelatedObjects(room_Old)?.FindAll(x => !(x is IPartition));
                architecturalModel.RemoveObject(room_Old);

                foreach (Tuple<Room, List<IPartition>> tuple_Room_New in tuple.Item2)
                {
                    Room room_New = tuple_Room_New.Item1;

                    architecturalModel.Add(room_New, tuple_Room_New.Item2);

                    if (relatedObjects != null)
                    {
                        foreach (IJSAMObject relatedObject in relatedObjects)
                        {
                            architecturalModel.AddRelation(room_New, relatedObject);
                        }
                    }
                }
            }

            return partitions_Air.FindAll(x => architecturalModel.Contains(x));

        }
    }
}