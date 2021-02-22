using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<Shell> Shells(this IEnumerable<ISegmentable2D> segmentable2Ds, double elevation_Min, double elevation_Max, double elevation_Ground = 0, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null || double.IsNaN(elevation_Min) || double.IsNaN(elevation_Max))
                return null;

            Plane plane_Min = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Min)) as Plane;
            Plane plane_Max = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Max)) as Plane;

            Plane plane_Min_Flipped = new Plane(plane_Min);
            plane_Min_Flipped.FlipZ();

            List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segmentable2Ds, tolerance);
            if (polygon2Ds == null)
                return null;

            List<Shell> result = new List<Shell>();

            if (polygon2Ds.Count == 0)
                return result;

            for (int i = 0; i < polygon2Ds.Count; i++)
            {
                Polygon2D polygon2D = polygon2Ds[i]?.SimplifyBySAM_Angle();
                if (polygon2D == null)
                    polygon2D = polygon2Ds[i];

                List<Segment2D> segment2Ds = polygon2D.GetSegments();
                if (segment2Ds == null || segment2Ds.Count < 3)
                    continue;

                segment2Ds = Planar.Query.Snap(segment2Ds, true, tolerance);


                List<Face3D> face3Ds = new List<Face3D>();
                foreach (Segment2D segment2D in segment2Ds)
                {
                    Segment3D segment3D_Min = plane_Min.Convert(segment2D);
                    Segment3D segment3D_Max = plane_Max.Convert(segment2D);

                    Polygon3D polygon3D = new Polygon3D(new Point3D[] { segment3D_Max[0], segment3D_Max[1], segment3D_Min[1], segment3D_Min[0] });

                    face3Ds.Add(new Face3D(polygon3D));
                }

                Polygon3D polygon3D_Min = plane_Min.Convert(polygon2D);
                polygon3D_Min = plane_Min_Flipped.Convert(plane_Min_Flipped.Convert(polygon3D_Min));
                if (polygon3D_Min != null)
                    face3Ds.Add(new Face3D(polygon3D_Min));

                Polygon3D polygon3D_Max = plane_Max.Convert(polygon2D);
                if (polygon3D_Max != null)
                    face3Ds.Add(new Face3D(polygon3D_Max));

                Shell shell = new Shell(face3Ds);
                result.Add(shell);
            }

            return result;
        }

        public static List<Shell> Shells(this IEnumerable<Face3D> face3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face3Ds == null)
                return null;

            Plane plane_Default = Spatial.Plane.WorldXY;

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

            List<Face2D> face2Ds = Planar.Query.Union(dictionary_Project.Keys);
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

            List<Shell> result = new List<Shell>();
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

                List<Shell> shells = Shells(dictionary, tolerance);
                if (shells != null)
                    result.AddRange(shells);
            }

            return result;
        }

        private static List<Shell> Shells(this Dictionary<double, List<Face2D>> face2Ds, double tolerance = Core.Tolerance.Distance)
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

                List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segmentable2Ds, tolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                List<Face2D> face2Ds_New = Geometry.Planar.Create.Face2Ds(polygon2Ds, true);

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

            Plane plane = Spatial.Plane.WorldXY;

            List<Shell> result = new List<Shell>();

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

                double elevation_Location = elevation_Bottom + Core.Tolerance.MacroDistance * 2;

                List<Face3D> face3Ds = new List<Face3D>();
                foreach (Face2D face2D in face2Ds_Top)
                {
                    Point3D location = plane_Bottom.Convert(face2D.GetInternalPoint2D());
                    location = new Point3D(location.X, location.Y, elevation_Location);

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

                    segment2Ds = Planar.Query.Snap(segment2Ds, true, tolerance);

                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        Segment3D segment3D_Top = plane_Top.Convert(segment2D);
                        Segment3D segment3D_Bottom = plane_Bottom.Convert(segment2D);

                        Polygon3D polygon3D = Polygon3D(new Point3D[] { segment3D_Top[0], segment3D_Top[1], segment3D_Bottom[1], segment3D_Bottom[0] }, tolerance); //new Polygon3D(new Point3D[] { segment3D_Top[0], segment3D_Top[1], segment3D_Bottom[1], segment3D_Bottom[0] });
                        if (polygon3D == null)
                            continue;

                        Face3D face3D = new Face3D(polygon3D);
                        face3Ds.Add(face3D);
                    }

                    List<Face2D> face2Ds_Space_Top = tuple_Top.Item2.FindAll(x => face2D.On(x.GetInternalPoint2D()) || face2D.Inside(x.GetInternalPoint2D()));
                    foreach (Face2D face2D_Top in face2Ds_Space_Top)
                    {
                        Face3D face3D_Top = plane_Top.Convert(face2D_Top);
                        if (face3D_Top == null)
                            continue;

                        face3Ds.Add(face3D_Top);
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

                            face3Ds.Add(face3D_Bottom);
                        }
                    }

                    Shell shell = new Shell(face3Ds);
                    result.Add(shell);
                }
            }

            return result;
        }
    }
}