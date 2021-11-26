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
        public static List<AirPartition> AddAirPartitions(this BuildingModel buildingModel, IEnumerable<Plane> planes, IEnumerable<Space> spaces = null, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, double tolerance_Snap = Tolerance.MacroDistance)
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

                List<AirPartition> airPartitions = AddAirPartitions(buildingModel, plane, spaces, silverSpacing, tolerance_Angle, tolerance_Distance, tolerance_Snap);
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

        public static List<AirPartition> AddAirPartitions(this BuildingModel buildingModel, Plane plane, IEnumerable<Space> spaces = null, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, double tolerance_Snap = Tolerance.MacroDistance)
        {
            if (buildingModel == null || plane == null)
            {
                return null;
            }

            List<AirPartition> result = new List<AirPartition>();

            List<Space> spaces_Temp = buildingModel.GetSpaces();
            if (spaces_Temp == null || spaces_Temp.Count == 0)
            {
                return result;
            }

            if (spaces != null)
            {
                for (int i = spaces_Temp.Count - 1; i >= 0; i--)
                {
                    Guid guid = spaces_Temp[i].Guid;

                    bool exists = false;
                    foreach (Space space in spaces)
                    {
                        if (space.Guid == guid)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (exists)
                    {
                        continue;
                    }

                    spaces_Temp.RemoveAt(i);
                }
            }

            List<IPartition> partitions = buildingModel.Partitions(plane, out List<IPartition> partitions_Existing, spaces_Temp, tolerance_Angle: tolerance_Angle, tolerance_Distance: tolerance_Distance, tolerance_Snap: tolerance_Snap);
            if (partitions == null || partitions.Count == 0)
            {
                return result;
            }

            List<AirPartition> partitions_Air = partitions.FindAll(x => x is AirPartition).ConvertAll(x => (AirPartition)x);
            if (partitions_Air == null || partitions_Air.Count == 0)
            {
                return result;
            }

            buildingModel.Cut(plane, spaces_Temp, tolerance_Distance);

            List<Tuple<Space, Shell, List<Shell>>> tuples = new List<Tuple<Space, Shell, List<Shell>>>();
            foreach (Space space in spaces_Temp)
            {
                Shell shell = buildingModel.GetShell(space);

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

                tuples.Add(new Tuple<Space, Shell, List<Shell>>(space, shell, shells_Cut));
            }

            if (tuples == null || tuples.Count == 0)
            {
                return result;
            }

            List<Face3D> face3Ds_Existing = partitions_Existing?.ConvertAll(x => x.Face3D);

            List<Tuple<Space, List<Tuple<Space, List<IPartition>>>>> tuples_New = Enumerable.Repeat<Tuple<Space, List<Tuple<Space, List<IPartition>>>>>(null, tuples.Count).ToList();

            Parallel.For(0, tuples.Count, (int i) =>
            //for(int i=0; i < tuples.Count; i++)
            {
                Space space = tuples[i].Item1;

                List<IPartition> partitions_Room = buildingModel.GetPartitions(space);
                if (partitions_Room == null || partitions_Room.Count == 0)
                {
                    return;
                    //continue;
                }

                List<Shell> shells = tuples[i].Item3;
                shells.Sort((x, y) => x.GetBoundingBox().Min.Z.CompareTo(y.GetBoundingBox().Min.Z));

                tuples_New[i] = new Tuple<Space, List<Tuple<Space, List<IPartition>>>>(space, new List<Tuple<Space, List<IPartition>>>());

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

                    string name = space.Name;
                    if (name == null)
                    {
                        name = string.Empty;
                    }

                    name = string.Format("{0}_{1}", name, index);
                    index++;

                    Space space_New = new Space(Guid.NewGuid(), space, name, point3D);

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

                    tuples_New[i].Item2.Add(new Tuple<Space, List<IPartition>>(space_New, partitions_New));
                }

            });

            foreach (Tuple<Space, List<Tuple<Space, List<IPartition>>>> tuple in tuples_New)
            {
                if (tuple == null)
                {
                    continue;
                }

                Space space_Old = tuple.Item1;

                List<IJSAMObject> relatedObjects = buildingModel.GetRelatedObjects(space_Old)?.FindAll(x => !(x is IPartition));
                buildingModel.RemoveObject(space_Old);

                foreach (Tuple<Space, List<IPartition>> tuple_Room_New in tuple.Item2)
                {
                    Space space_New = tuple_Room_New.Item1;

                    buildingModel.Add(space_New, tuple_Room_New.Item2);

                    if (relatedObjects != null)
                    {
                        foreach (IJSAMObject relatedObject in relatedObjects)
                        {
                            buildingModel.AddRelation(space_New, relatedObject);
                        }
                    }
                }
            }

            return partitions_Air.FindAll(x => buildingModel.Contains(x));

        }
    }
}