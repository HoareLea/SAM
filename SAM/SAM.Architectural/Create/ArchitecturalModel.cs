using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static ArchitecturalModel ArchitecturalModel(this IEnumerable<ISegmentable2D> segmentable2Ds, double elevation_Min, double elevation_Max, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Angle = Core.Tolerance.Angle)
        {
            if (segmentable2Ds == null || double.IsNaN(elevation_Min) || double.IsNaN(elevation_Max))
                return null;

            ArchitecturalModel architecturalModel = new ArchitecturalModel();

            Level level = Level(elevation_Min);

            Plane plane_Min = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Min)) as Plane;
            Plane plane_Max = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Max)) as Plane;

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
                    architecturalModel.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return architecturalModel;
        }
    }
}