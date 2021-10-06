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
                List<Tuple<Segment2D, HostBuildingElement, Room>> tuples = new List<Tuple<Segment2D, HostBuildingElement, Room>>();
                Dictionary<Room, List<HostBuildingElement>> dictionary = new Dictionary<Room, List<HostBuildingElement>>();

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

                    dictionary[room] = new List<HostBuildingElement>();

                    List<Segment2D> segment2Ds = polygon2D.GetSegments();
                    if (segment2Ds == null || segment2Ds.Count < 3)
                        continue;

                    segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true);

                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        Segment3D segment3D_Min = plane_Min.Convert(segment2D);
                        Segment3D segment3D_Max = plane_Max.Convert(segment2D);

                        Polygon3D polygon3D = new Polygon3D(new Point3D[] { segment3D_Max[0], segment3D_Max[1], segment3D_Min[1], segment3D_Min[0] });
                        HostBuildingElement hostBuildingElement = HostBuildingElement(new Face3D(polygon3D), null, tolerance_Angle);

                        tuples.Add(new Tuple<Segment2D, HostBuildingElement, Room>(segment2D, hostBuildingElement, room));
                    }

                    Polygon3D polygon3D_Min = plane_Min.Convert(polygon2D);
                    polygon3D_Min = plane_Min_Flipped.Convert(plane_Min_Flipped.Convert(polygon3D_Min));
                    if (polygon3D_Min != null)
                    {
                        HostBuildingElement hostBuildingElement = HostBuildingElement(new Face3D(polygon3D_Min), null, tolerance_Angle);
                        if(hostBuildingElement != null)
                        {
                            dictionary[room].Add(hostBuildingElement);
                        }
                    }

                    Polygon3D polygon3D_Max = plane_Max.Convert(polygon2D);
                    if (polygon3D_Max != null)
                    {
                        HostBuildingElement hostBuildingElement = HostBuildingElement(new Face3D(polygon3D_Max), null, tolerance_Angle);
                        if (hostBuildingElement != null)
                        {
                            dictionary[room].Add(hostBuildingElement);
                        }
                    }
                }

                while (tuples.Count > 0)
                {
                    Tuple<Segment2D, HostBuildingElement, Room> tuple = tuples[0];
                    Segment2D segment2D = tuple.Item1;

                    List<Tuple<Segment2D, HostBuildingElement, Room>> tuples_Temp = tuples.FindAll(x => segment2D.AlmostSimilar(x.Item1));
                    tuples.RemoveAll(x => tuples_Temp.Contains(x));

                    HostBuildingElement hostBuildingElement = tuple.Item2;

                    foreach (Tuple<Segment2D, HostBuildingElement, Room> tuple_Temp in tuples_Temp)
                        dictionary[ tuple_Temp.Item3].Add(hostBuildingElement);
                }

                foreach(KeyValuePair<Room, List<HostBuildingElement>> keyValuePair in dictionary)
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

            List<Dictionary<Room, List<HostBuildingElement>>> dictionaries_Room = new List<Dictionary<Room, List<HostBuildingElement>>>();
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

                Dictionary<Room, List<HostBuildingElement>> dictionary_Room = Query.RoomDictionary(dictionary, dictionary.Keys.ToList().IndexOf(elevation_Ground), tolerance);
                if (dictionary_Room != null)
                    dictionaries_Room.Add(dictionary_Room);
            }

            ArchitecturalModel result = new ArchitecturalModel(null, null, null, PlanarTerrain(elevation_Ground));
            foreach (Dictionary<Room, List<HostBuildingElement>> dictionary_Room in dictionaries_Room)
            {
                foreach (KeyValuePair<Room, List<HostBuildingElement>> keyValuePair in dictionary_Room)
                {
                    result.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return result;
        }

        public static ArchitecturalModel ArchitecturalModel(this IEnumerable<Shell> shells, IEnumerable<HostBuildingElement> hostBuildingElements, double thinnessRatio = 0.01, double minArea = Tolerance.MacroDistance, double maxDistance = 0.1, double maxAngle = 0.0872664626, double silverSpacing = Tolerance.MacroDistance, double tolerance_Distance = Tolerance.Distance, double tolerance_Angle = Tolerance.Angle)
        {
            if(shells == null && hostBuildingElements == null)
            {
                return null;
            }

            List<Tuple<Room, HostBuildingElement>> tuples = new List<Tuple<Room, HostBuildingElement>>();

            List<Tuple<Plane, Face3D, HostBuildingElement, BoundingBox3D, double>> tuples_HostBuildingElement = new List<Tuple<Plane, Face3D, HostBuildingElement, BoundingBox3D, double>>();
            foreach (HostBuildingElement hostBuildingElement in hostBuildingElements)
            {
                Face3D face3D = hostBuildingElement?.Face3D;
                if (face3D == null)
                    continue;

                Plane plane = face3D.GetPlane();
                if (plane == null)
                    continue;

                double area = face3D.GetArea();

                if (area < minArea || face3D.ThinnessRatio() < thinnessRatio) // Changed from tolerance_Distance to minArea
                    continue;

                tuples_HostBuildingElement.Add(new Tuple<Plane, Face3D, HostBuildingElement, BoundingBox3D, double>(plane, face3D, hostBuildingElement, face3D.GetBoundingBox(tolerance_Distance), area));
            }

            tuples_HostBuildingElement.Sort((x, y) => y.Item5.CompareTo(x.Item5));

            BoundingBox3D boundingBox3D_All = new BoundingBox3D(tuples_HostBuildingElement.ConvertAll(x => x.Item4));

            int count = 1;

            List<Tuple<Point3D, HostBuildingElement, BoundingBox3D>> tuples_HostBuildingElement_New = new List<Tuple<Point3D, HostBuildingElement, BoundingBox3D>>();

            List<Shell> shells_Temp = Enumerable.Repeat<Shell>(null, shells.Count()).ToList();
            Parallel.For(0, shells.Count(), (int i) =>
            //for(int i=0; i < shells.Count(); i++)
            {
                Shell shell = shells.ElementAt(i);

                BoundingBox3D boundingBox3D = shell?.GetBoundingBox();
                if (boundingBox3D == null)
                {
                    //continue;
                    return;
                }

                if (!boundingBox3D_All.InRange(boundingBox3D))
                {
                    //continue;
                    return;
                }

                shell = shell.RemoveInvalidFace3Ds(silverSpacing);
                if (shell == null)
                {
                    //continue;
                    return;
                }

                Shell shell_Merge = shell.Merge(tolerance_Distance);
                if (shell_Merge == null)
                {
                    shell_Merge = new Shell(shell);
                }

                shells_Temp[i] = shell_Merge;
            });

            shells_Temp.RemoveAll(x => x == null);
            List<HostBuildingElement> HostBuildingElement_Merged = Query.MergeCoplanar(tuples_HostBuildingElement.ConvertAll(x => x.Item3), maxDistance, true, minArea, tolerance_Distance);
            shells_Temp.FillFace3Ds(HostBuildingElement_Merged.ConvertAll(x => x.Face3D), 0.1, maxDistance, maxAngle, silverSpacing, tolerance_Distance);
            shells_Temp.SplitCoplanarFace3Ds(tolerance_Angle, tolerance_Distance);

            throw new NotImplementedException();
        }
    }
}