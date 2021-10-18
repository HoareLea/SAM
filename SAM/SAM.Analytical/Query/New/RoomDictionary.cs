using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

using SAM.Architectural;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Room, List<IHostPartition>> RoomDictionary(this Dictionary<double, List<Face2D>> face2Ds, double tolerance_Distance = Tolerance.Distance, double tolerance_Angle = Tolerance.Angle)
        {
            if (face2Ds == null)
                return null;

            if (face2Ds.Count() < 2)
                return null;

            List<double> elevations = face2Ds.Keys.ToList();
            elevations.Sort((x, y) => y.CompareTo(x));

            List<Tuple<double, List<Face2D>>> tuples = new List<Tuple<double, List<Face2D>>>();

            tuples.Add(new Tuple<double, List<Face2D>>(elevations[0], face2Ds[elevations[0]]));

            for (int i = 1; i < elevations.Count - 1; i++)
            {
                double elevation_Top = elevations[i - 1];
                double elevation_Bottom = elevations[i];

                List<Face2D> face2Ds_Top = face2Ds[elevation_Top];
                List<Face2D> face2Ds_Bottom = face2Ds[elevation_Bottom];

                List<IClosed2D> closed2Ds = new List<IClosed2D>();

                if (face2Ds_Top != null && face2Ds_Top.Count > 0)
                    face2Ds_Top.ConvertAll(x => x.Edge2Ds).ForEach(x => closed2Ds.AddRange(x));

                if (face2Ds_Bottom != null && face2Ds_Bottom.Count > 0)
                    face2Ds_Bottom.ConvertAll(x => x.Edge2Ds).ForEach(x => closed2Ds.AddRange(x));

                List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();
                foreach (IClosed2D closed2D in closed2Ds)
                {
                    ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                    if (segmentable2D == null)
                        continue;

                    segmentable2Ds.Add(segmentable2D);
                }

                List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segmentable2Ds, tolerance_Distance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                List<Face2D> face2Ds_New = Geometry.Planar.Create.Face2Ds(polygon2Ds, Geometry.EdgeOrientationMethod.Opposite);

                List<Face2D> face2Ds_All = new List<Face2D>();
                face2Ds_All.AddRange(face2Ds_Top);
                face2Ds_All.AddRange(face2Ds_Bottom);

                List<IClosed2D> internalEdge2Ds = new List<IClosed2D>();
                foreach (Face2D face2D_New in face2Ds_New)
                {
                    List<IClosed2D> internalEdge2Ds_New = face2D_New.InternalEdge2Ds;
                    if (internalEdge2Ds_New == null || internalEdge2Ds_New.Count == 0)
                        continue;

                    foreach (IClosed2D closed2D in internalEdge2Ds_New)
                    {
                        Point2D point2D = closed2D?.GetInternalPoint2D();
                        if (point2D == null)
                            continue;

                        if (face2Ds_All.Find(x => x.Inside(point2D)) == null)
                            continue;

                        internalEdge2Ds.Add(closed2D);
                    }
                }

                internalEdge2Ds.RemoveAll(x => x is Polygon2D && face2Ds_New.Find(y => y.ExternalEdge2D is Polygon2D && ((Polygon2D)y.ExternalEdge2D).Similar((Polygon2D)x)) != null);

                face2Ds_New.AddRange(internalEdge2Ds.ConvertAll(x => new Face2D(x)));

                tuples.Add(new Tuple<double, List<Face2D>>(elevation_Bottom, face2Ds_New));
            }

            tuples.Add(new Tuple<double, List<Face2D>>(elevations[elevations.Count - 1], face2Ds[elevations[elevations.Count - 1]]));

            Plane plane = Plane.WorldXY;

            Dictionary<Room, List<IHostPartition>> result = new Dictionary<Room, List<IHostPartition>>();

            List<Tuple<Point3D, IHostPartition, Room>> tuples_Point3D = new List<Tuple<Point3D, IHostPartition, Room>>();
            for (int i = 1; i < tuples.Count; i++)
            {
                Tuple<double, List<Face2D>> tuple_Top = tuples[i - 1];
                Tuple<double, List<Face2D>> tuple_Bottom = tuples[i];

                double elevation_Top = tuple_Top.Item1;
                double elevation_Bottom = tuple_Bottom.Item1;

                Plane plane_Top = plane.GetMoved(new Vector3D(0, 0, elevation_Top)) as Plane;
                Plane plane_Bottom = plane.GetMoved(new Vector3D(0, 0, elevation_Bottom)) as Plane;
                Plane plane_Bottom_Flipped = new Plane(plane_Bottom);
                plane_Bottom_Flipped.FlipZ();

                List<Face2D> face2Ds_Top = face2Ds[elevation_Top];

                double elevation_Location = (elevation_Bottom + elevation_Top) / 2;

                Level level = Architectural.Create.Level(elevation_Bottom);

                int count = 1;
                foreach (Face2D face2D in face2Ds_Top)
                {
                    Point3D location = plane_Bottom.Convert(face2D.GetInternalPoint2D());
                    location = new Point3D(location.X, location.Y, elevation_Location);

                    Room room = new Room(string.Format("Cell {0}.{1}", tuples.Count - i, count), location);
                    room.SetValue(RoomParameter.LevelName, level.Name);
                    count++;

                    List<Segment2D> segment2Ds = new List<Segment2D>();
                    foreach (IClosed2D closed2D in face2D.Edge2Ds)
                    {
                        ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                        if (segmentable2D == null)
                            segmentable2D = closed2D as ISegmentable2D;

                        if (segmentable2D == null)
                            continue;

                        segment2Ds.AddRange(segmentable2D.GetSegments());
                    }

                    if (segment2Ds == null || segment2Ds.Count < 3)
                        continue;

                    segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true);

                    IHostPartition hostBuilidngElement;

                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        Segment3D segment3D_Top = plane_Top.Convert(segment2D);
                        Segment3D segment3D_Bottom = plane_Bottom.Convert(segment2D);

                        Polygon3D polygon3D = Geometry.Spatial.Create.Polygon3D(new Point3D[] { segment3D_Top[0], segment3D_Top[1], segment3D_Bottom[1], segment3D_Bottom[0] }, tolerance_Distance); //new Polygon3D(new Point3D[] { segment3D_Top[0], segment3D_Top[1], segment3D_Bottom[1], segment3D_Bottom[0] });
                        if (polygon3D == null)
                            continue;

                        hostBuilidngElement = Create.HostPartition(new Face3D(polygon3D), null, tolerance_Angle);
                        if (hostBuilidngElement != null)
                        {
                            tuples_Point3D.Add(new Tuple<Point3D, IHostPartition, Room>(Geometry.Spatial.Query.Mid(plane_Bottom.Convert(segment2D)), hostBuilidngElement, room));
                        }
                    }

                    List<Face2D> face2Ds_Space_Top = tuple_Top.Item2.FindAll(x => face2D.On(x.GetInternalPoint2D()) || face2D.Inside(x.GetInternalPoint2D()));
                    foreach (Face2D face2D_Top in face2Ds_Space_Top)
                    {
                        Face3D face3D_Top = plane_Top.Convert(face2D_Top);
                        if (face3D_Top == null)
                            continue;

                        hostBuilidngElement = Create.HostPartition(face3D_Top, null, tolerance_Angle);
                        if (hostBuilidngElement != null)
                        {
                            tuples_Point3D.Add(new Tuple<Point3D, IHostPartition, Room>(face3D_Top.InternalPoint3D(), hostBuilidngElement, room));
                        }
                    }

                    List<Face2D> face2Ds_Space_Bottom = tuple_Bottom.Item2.FindAll(x => face2D.On(x.GetInternalPoint2D()) || face2D.Inside(x.GetInternalPoint2D()));
                    if (face2Ds_Space_Bottom != null && face2Ds_Space_Bottom.Count != 0)
                    {
                        foreach (Face2D face2D_Bottom in face2Ds_Space_Bottom)
                        {
                            Face3D face3D_Bottom = plane_Bottom.Convert(face2D_Bottom);
                            if (face3D_Bottom == null)
                                continue;

                            Face2D face2D_Bottom_Flipped = plane_Bottom_Flipped.Convert(face3D_Bottom);
                            face3D_Bottom = plane_Bottom_Flipped.Convert(face2D_Bottom_Flipped);

                            hostBuilidngElement = Create.HostPartition(face3D_Bottom, null, tolerance_Angle);
                            if (hostBuilidngElement != null)
                            {
                                tuples_Point3D.Add(new Tuple<Point3D, IHostPartition, Room>(face3D_Bottom.InternalPoint3D(), hostBuilidngElement, room));
                            }
                        }
                    }

                    double area = face2D.GetArea();
                    double height = elevation_Top - elevation_Bottom;
                    double volume = area * height;

                    ParameterSet parameterSet_Space = new ParameterSet(typeof(Room).Assembly);
                    room.Add(parameterSet_Space);

                    room.SetValue(RoomParameter.Area, area);
                    room.SetValue(RoomParameter.Volume, volume);
                    result[room] = new List<IHostPartition>();
                }
            }

            while (tuples_Point3D.Count > 0)
            {
                Tuple<Point3D, IHostPartition, Room> tuple = tuples_Point3D[0];
                Point3D point3D = tuple.Item1;

                List<Tuple<Point3D, IHostPartition, Room>> tuples_Temp = tuples_Point3D.FindAll(x => point3D.AlmostEquals(x.Item1));
                tuples_Point3D.RemoveAll(x => tuples_Temp.Contains(x));

                IHostPartition hostPartition = tuple.Item2;

                foreach (Tuple<Point3D, IHostPartition, Room> tuple_Temp in tuples_Temp)
                {
                    result[tuple_Temp.Item3].Add(hostPartition);
                }
            }

            return result;

        }
    }
}