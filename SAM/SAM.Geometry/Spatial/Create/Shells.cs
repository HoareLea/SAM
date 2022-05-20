using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<Shell> Shells(this IEnumerable<ISegmentable2D> segmentable2Ds, double elevation_Min, double elevation_Max, double tolerance_Snap = Core.Tolerance.MacroDistance, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null || double.IsNaN(elevation_Min) || double.IsNaN(elevation_Max))
                return null;

            Plane plane_Min = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Min)) as Plane;
            Plane plane_Max = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Max)) as Plane;

            Plane plane_Min_Flipped = new Plane(plane_Min);
            plane_Min_Flipped.FlipZ();

            List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segmentable2Ds, tolerance_Distance);
            if (polygon2Ds == null)
                return null;

            List<Shell> result = new List<Shell>();

            if (polygon2Ds.Count == 0)
                return result;

            for (int i = 0; i < polygon2Ds.Count; i++)
            {
                Polygon2D polygon2D = polygon2Ds[i]?.SimplifyByAngle();
                if (polygon2D == null)
                    polygon2D = polygon2Ds[i];

                List<Segment2D> segment2Ds = polygon2D.GetSegments();
                if (segment2Ds == null || segment2Ds.Count < 3)
                    continue;

                segment2Ds = Planar.Query.Snap(segment2Ds, true, tolerance_Snap);

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

        public static List<Shell> Shells(this IEnumerable<Face3D> face3Ds, IEnumerable<double> elevations, IEnumerable<double> offsets, IEnumerable<double> auxiliaryElevations = null, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face3Ds == null || elevations == null)
                return null;

            if (elevations.Count() < 2)
            {
                return null;
            }

            HashSet<double> elevations_Unique = new HashSet<double>(elevations);
            if (auxiliaryElevations != null && auxiliaryElevations.Count() > 0)
            {
                foreach (double auxiliaryElevation in auxiliaryElevations)
                {
                    elevations_Unique.Add(auxiliaryElevation);
                }
            }

            List<double> elevations_All = new List<double>(elevations_Unique);
            elevations_All.Sort();

            int count = elevations_All.Count;

            List<Tuple<double, List<Face3D>>> tuples_Face3D_Bottom = Enumerable.Repeat<Tuple<double, List<Face3D>>>(null, count).ToList();
            List<Tuple<double, List<Face3D>>> tuples_Face3D_Top = Enumerable.Repeat<Tuple<double, List<Face3D>>>(null, count).ToList();
            Parallel.For(0, count, (int i) =>
            //for (int i=0; i < count; i++)
            {
                double elevation = elevations_All[i];

                //Bottom
                double offset = 0;
                int index = elevations.ToList().IndexOf(elevation);
                if (index != -1 && offsets != null && offsets.Count() > index)
                    offset = offsets.ElementAt(index);

                Plane plane = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation + offset)) as Plane;

                List<Face3D> face3Ds_Bottom = new List<Face3D>(face3Ds);
                face3Ds_Bottom.RemoveAll(x => x == null || x.Below(plane, tolerance_Distance));

                Dictionary<Face3D, List<ISegmentable2D>> dictionary = face3Ds_Bottom.SectionDictionary<ISegmentable2D>(plane, tolerance_Distance);

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (KeyValuePair<Face3D, List<ISegmentable2D>> keyValuePair in dictionary)
                {
                    foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                    {
                        segment2Ds.AddRange(segmentable2D.GetSegments());
                    }
                }

                List<Face2D> face2Ds = null;

                if (segment2Ds != null && segment2Ds.Count != 0)
                {
                    segment2Ds = Planar.Query.Split(segment2Ds, tolerance_Distance);
                    segment2Ds = Planar.Query.Snap(segment2Ds, true, snapTolerance);

                    List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);
                    if (polygon2Ds != null && polygon2Ds.Count != 0)
                    {
                        face2Ds = Planar.Create.Face2Ds(polygon2Ds, EdgeOrientationMethod.Opposite);
                        if (face2Ds != null && face2Ds.Count != 0)
                        {
                            List<IClosed2D> closed2Ds = Planar.Query.Holes(face2Ds);
                            closed2Ds?.ForEach(x => face2Ds.Add(new Face2D(x)));
                        }
                    }
                }

                plane = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

                tuples_Face3D_Bottom[i] = new Tuple<double, List<Face3D>>(elevation, face2Ds?.ConvertAll(x => plane.Convert(x)));

                if (i == 0)
                {
                    return;
                }

                //Top
                dictionary = face3Ds.SectionDictionary<ISegmentable2D>(plane, tolerance_Distance);

                segment2Ds = new List<Segment2D>();
                foreach (KeyValuePair<Face3D, List<ISegmentable2D>> keyValuePair in dictionary)
                {
                    foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                    {
                        segment2Ds.AddRange(segmentable2D.GetSegments());
                    }
                }

                face2Ds = null;

                if (segment2Ds != null && segment2Ds.Count != 0)
                {
                    segment2Ds = Planar.Query.Split(segment2Ds, tolerance_Distance);
                    segment2Ds = Planar.Query.Snap(segment2Ds, true, snapTolerance);

                    List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);
                    if (polygon2Ds != null && polygon2Ds.Count != 0)
                    {
                        face2Ds = Planar.Create.Face2Ds(polygon2Ds, EdgeOrientationMethod.Opposite);
                        if (face2Ds != null && face2Ds.Count != 0)
                        {
                            List<IClosed2D> closed2Ds = Planar.Query.Holes(face2Ds);
                            closed2Ds?.ForEach(x => face2Ds.Add(new Face2D(x)));
                        }
                    }
                }

                tuples_Face3D_Top[i] = new Tuple<double, List<Face3D>>(elevation, face2Ds?.ConvertAll(x => plane.Convert(x)));
            });

            List<Tuple<double, List<Shell>>> tuples_Shell = Enumerable.Repeat<Tuple<double, List<Shell>>>(null, count - 1).ToList();
            Parallel.For(0, count - 1, (int i) =>
            //for (int i = 0; i < count - 1; i++)
            {
                Tuple<double, List<Face3D>> tuple_Bottom = tuples_Face3D_Bottom[i];
                Tuple<double, List<Face3D>> tuple_Top = tuples_Face3D_Top[i + 1];

                List<Face3D> face3Ds_Temp = null;
                if (tuple_Bottom != null && tuple_Top != null && tuple_Bottom.Item2 != null && tuple_Top.Item2 != null)
                {
                    face3Ds_Temp = new List<Face3D>();
                    face3Ds_Temp.AddRange(tuple_Bottom.Item2);
                    face3Ds_Temp.AddRange(tuple_Top.Item2);
                }

                tuples_Shell[i] = new Tuple<double, List<Shell>>(tuple_Bottom.Item1, Shells_ByTopAndBottom(face3Ds_Temp, tolerance_Distance));
            });

            for (int i = 0; i < count - 1; i++)
            {
                double elevation_Bottom = tuples_Shell[i].Item1;
                if (!elevations.Contains(elevation_Bottom))
                {
                    continue;
                }

                Tuple<double, List<Shell>> tuple = tuples_Shell[i];
                if (tuple == null || tuple.Item2 == null || tuple.Item2.Count == 0)
                {
                    continue;
                }

                for (int j = 0; j < tuple.Item2.Count; j++)
                {
                    Shell shell = tuple.Item2[j];

                    for (int k = i + 1; k < count - 1; k++)
                    {
                        double elevation_Top = tuples_Shell[k].Item1;
                        if (elevations.Contains(elevation_Top))
                        {
                            break;
                        }

                        Tuple<double, List<Shell>> tuple_Temp = tuples_Shell[k];
                        if (tuple_Temp == null || tuple_Temp.Item2 == null || tuple_Temp.Item2.Count == 0)
                        {
                            continue;
                        }

                        List<Shell> shells_Union = new List<Shell>();
                        List<Shell> shells_Temp = new List<Shell>();
                        foreach (Shell shell_Temp in tuple_Temp.Item2)
                        {
                            List<Shell> shells_Union_Temp = shell.Union(shell_Temp, silverSpacing, tolerance_Angle, tolerance_Distance);
                            if (shells_Union_Temp != null && shells_Union_Temp.Count == 1)
                            {
                                shells_Union.Add(shells_Union_Temp[0]);
                                shells_Temp.Add(shell_Temp);
                            }
                        }

                        if (shells_Union != null && shells_Union.Count > 0)
                        {
                            if (shells_Union.Count > 1)
                            {
                                shell = shells_Union.Union(silverSpacing, tolerance_Angle, tolerance_Distance).First();
                            }
                            else
                            {
                                shell = shells_Union[0];
                            }

                            shells_Temp.ForEach(x => tuple_Temp.Item2.Remove(x));
                        }
                    }

                    tuple.Item2[j] = shell;
                }
            }

            List<Shell> result = new List<Shell>();
            for (int i = 0; i < count - 1; i++)
            {
                List<Shell> shells = tuples_Shell[i]?.Item2;
                if (shells == null || shells.Count == 0)
                {
                    continue;
                }

                foreach (Shell shell in shells)
                {
                    shell.OrientNormals(false, silverSpacing, tolerance_Distance);
                    result.Add(shell);
                }
            }

            return result;
        }

        public static List<Shell> Shells(this IEnumerable<Face3D> face3Ds, IEnumerable<double> elevations, double offset = 0.1, double thinnessRatio = 0.01, double minArea = Core.Tolerance.MacroDistance, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face3Ds == null || elevations == null)
            {
                return null;
            }

            List<BoundingBox3D> boundingBox3Ds = new List<BoundingBox3D>();
            foreach (Face3D face3D in face3Ds)
            {
                BoundingBox3D boundingBox3D_Face3D = face3D?.GetBoundingBox();
                if (boundingBox3D_Face3D == null)
                {
                    continue;
                }

                boundingBox3Ds.Add(boundingBox3D_Face3D);
            }

            BoundingBox3D boundingBox3D_face3Ds = new BoundingBox3D(boundingBox3Ds);

            List<double> elevations_Temp = new List<double>(elevations);
            elevations_Temp.Add(boundingBox3D_face3Ds.Max.Z);
            elevations_Temp.Add(boundingBox3D_face3Ds.Min.Z);
            Core.Modify.District(elevations_Temp, tolerance_Distance);

            List<List<Tuple<Shell, BoundingBox2D, Face2D, Plane>>> tuples_Elevation = Enumerable.Repeat<List<Tuple<Shell, BoundingBox2D, Face2D, Plane>>>(null, elevations.Count()).ToList();

            Parallel.For(0, elevations.Count(), (int i) =>
            {
                double sectionElevation = elevations.ElementAt(i) + offset;

                double maxElevation = Core.Query.Next(elevations, sectionElevation);
                if (double.IsNaN(maxElevation))
                {
                    return;
                }

                double minElevation = Core.Query.Previous(elevations, sectionElevation);
                if (double.IsNaN(minElevation))
                {
                    return;
                }

                Plane plane_Section = Plane(sectionElevation);

                Dictionary<Face3D, List<ISegmentable2D>> dictionary = face3Ds.SectionDictionary<ISegmentable2D>(plane_Section, tolerance_Distance);
                if (dictionary == null || dictionary.Count == 0)
                {
                    return;
                }

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (List<ISegmentable2D> segmentable2Ds in dictionary.Values)
                {
                    if (segmentable2Ds == null)
                    {
                        continue;
                    }

                    foreach (ISegmentable2D segmentable2D in segmentable2Ds)
                    {
                        List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                        if (segment2Ds_Temp == null)
                        {
                            continue;
                        }

                        segment2Ds.AddRange(segment2Ds_Temp);
                    }
                }

                if (segment2Ds == null || segment2Ds.Count == 0)
                {
                    return;
                }

                segment2Ds = Planar.Query.Split(segment2Ds, tolerance_Distance);
                segment2Ds = Planar.Query.Snap(segment2Ds, true, snapTolerance);

                List<Face2D> face2Ds_Section = Planar.Create.Face2Ds(segment2Ds, EdgeOrientationMethod.Undefined, tolerance_Distance);
                if (face2Ds_Section == null || face2Ds_Section.Count == 0)
                {
                    return;
                }

                List<IClosed2D> closed2Ds = Planar.Query.Holes(face2Ds_Section);
                if (closed2Ds != null && closed2Ds.Count > 0)
                    closed2Ds.ForEach(x => face2Ds_Section.Add(new Face2D(x)));

                Plane minPlane = Plane(minElevation);
                Vector3D vector3D = Spatial.Vector3D.WorldZ * (maxElevation - minElevation);

                tuples_Elevation[i] = new List<Tuple<Shell, BoundingBox2D, Face2D, Plane>>();
                foreach (Face2D face2D in face2Ds_Section)
                {
                    Shell shell = Shell(minPlane.Convert(face2D), vector3D, tolerance_Distance);
                    if (shell != null)
                    {
                        tuples_Elevation[i].Add(new Tuple<Shell, BoundingBox2D, Face2D, Plane>(shell, face2D.GetBoundingBox(), face2D, plane_Section));
                    }
                }
            });

            List<Tuple<Shell, BoundingBox2D, Face2D, Plane>> tuples = new List<Tuple<Shell, BoundingBox2D, Face2D, Plane>>();
            foreach (List<Tuple<Shell, BoundingBox2D, Face2D, Plane>> tuples_Temp in tuples_Elevation)
            {
                if (tuples_Temp == null || tuples_Temp.Count == 0)
                {
                    continue;
                }

                tuples.AddRange(tuples_Temp);
            }

            Plane plane = Spatial.Plane.WorldXY;

            List<Face2D> face2Ds = new List<Face2D>();
            List<Face3D> face3Ds_Temp = new List<Face3D>();

            foreach (Face3D face3D_Face3D in face3Ds)
            {
                if (face3D_Face3D == null)
                {
                    continue;
                }

                Face3D face3D_Project = plane.Project(face3D_Face3D);
                if (face3D_Project == null || !face3D_Project.IsValid())
                {
                    continue;
                }

                Face2D face2D = plane.Convert(face3D_Project);
                if (face2D == null || !face2D.IsValid() || face2D.GetArea() < minArea)
                {
                    continue;
                }

                face2Ds.Add(face2D);
                face3Ds_Temp.Add(face3D_Face3D);
            }

            List<ISegmentable2D> segmentable2Ds_Edges = face2Ds.Edges().FindAll(x => x is ISegmentable2D).ConvertAll(x => x as ISegmentable2D);
            List<Segment2D> segment2Ds_Edges = Planar.Query.Split(segmentable2Ds_Edges, tolerance_Distance);
            segment2Ds_Edges = Planar.Query.Snap(segment2Ds_Edges, true, snapTolerance);
            face2Ds = Planar.Create.Face2Ds(segment2Ds_Edges, EdgeOrientationMethod.Undefined, tolerance_Distance);

            face2Ds.RemoveAll(x => x == null || x.GetArea() <= minArea);
            List<Tuple<Face2D, BoundingBox2D>> tuples_All = face2Ds.ConvertAll(x => new Tuple<Face2D, BoundingBox2D>(x, x.GetBoundingBox()));

            List<Shell> result = new List<Shell>();
            while (tuples.Count > 0)
            {
                Tuple<Shell, BoundingBox2D, Face2D, Plane> tuple = tuples[0];

                Shell shell = tuple.Item1;
                if (shell == null)
                {
                    tuples.Remove(tuple);
                    continue;
                }

                List<Tuple<Face2D, BoundingBox2D>> tuples_Intersection = tuples_All.FindAll(x => tuple.Item2.InRange(x.Item2, tolerance_Distance));
                if (tuples_Intersection == null || tuples_Intersection.Count == 0)
                {
                    tuples.Remove(tuple);
                    continue;
                }

                Point3D point3D_Internal = shell.InternalPoint3D(silverSpacing, tolerance_Distance);
                if (point3D_Internal == null)
                {
                    tuples.Remove(tuple);
                    continue;
                }

                List<Face2D> face2Ds_Temp = tuples_Intersection.ConvertAll(x => x.Item1);
                Face2D face2D_Snap = tuple.Item3.Snap(face2Ds_Temp, snapTolerance, tolerance_Distance);
                if (face2D_Snap == null)
                {
                    face2D_Snap = tuple.Item3;
                }

                if (face2Ds_Temp.Find(x => x.Similar(face2D_Snap, snapTolerance)) == null)
                {
                    face2Ds_Temp.Add(face2D_Snap);
                    face2Ds_Temp = Planar.Query.Split(face2Ds_Temp, tolerance_Distance);
                }

                face2Ds_Temp.RemoveAll(x => x == null || !x.IsValid());
                face2Ds_Temp.RemoveAll(x => x.ThinnessRatio() < thinnessRatio || x.GetArea() < minArea);
                face2Ds_Temp.RemoveAll(x => !tuple.Item3.Inside(x.InternalPoint2D(tolerance_Distance)));
                if (face2Ds_Temp == null || face2Ds_Temp.Count == 0)
                {
                    tuples.Remove(tuple);
                    continue;
                }

                List<Point3D> point3Ds = face2Ds_Temp.ConvertAll(x => tuple.Item4.Convert(x)?.InternalPoint3D(tolerance_Distance));

                Point3D point3D_Up = null;
                Point3D point3D_Down = null;

                Vector3D up = Spatial.Vector3D.WorldZ;

                List<Shell> shells = new List<Shell>();
                foreach (Point3D point3D in point3Ds)
                {
                    List<Point3D> point3Ds_Intersection = point3D.Intersections(up, face3Ds_Temp, false, true, tolerance_Distance);
                    if (point3Ds == null || point3Ds.Count == 0)
                    {
                        continue;
                    }

                    Point3D point3D_Up_Temp = point3Ds_Intersection.FindAll(x => up.SameHalf(new Vector3D(point3D, x))).FirstOrDefault();
                    if (point3D_Up_Temp == null)
                    {
                        continue;
                    }

                    point3D_Up_Temp.Move(new Vector3D(0, 0, -snapTolerance));

                    Point3D point3D_Down_Temp = point3Ds_Intersection.FindAll(x => up.SameHalf(new Vector3D(x, point3D))).FirstOrDefault();
                    if (point3D_Down_Temp == null)
                    {
                        continue;
                    }

                    point3D_Down_Temp.Move(new Vector3D(0, 0, snapTolerance));

                    Segment3D segment3D = new Segment3D(point3D_Down_Temp, point3D_Up_Temp);

                    foreach (Tuple<Shell, BoundingBox2D, Face2D, Plane> tuple_Temp in tuples)
                    {
                        Shell shell_Temp = tuple_Temp.Item1;

                        if (shell_Temp == null || shells.Contains(shell_Temp))
                        {
                            continue;
                        }

                        List<Point3D> point3Ds_Intersection_Segment3D = shell_Temp?.Intersections(segment3D, tolerance_Distance);
                        if (point3Ds_Intersection_Segment3D == null || point3Ds_Intersection_Segment3D.Count == 0)
                        {
                            continue;
                        }

                        shells.Add(shell_Temp);
                    }

                    foreach (Shell shell_Temp in result)
                    {
                        if (shell_Temp == null || shells.Contains(shell_Temp))
                        {
                            continue;
                        }

                        List<Point3D> point3Ds_Intersection_Segment3D = shell_Temp?.Intersections(segment3D, tolerance_Distance);
                        if (point3Ds_Intersection_Segment3D == null || point3Ds_Intersection_Segment3D.Count == 0)
                        {
                            continue;
                        }

                        shells.Add(shell_Temp);
                    }

                    point3D_Up = point3D_Up_Temp;
                    point3D_Down = point3D_Down_Temp;
                }

                if (point3D_Up == null || !point3D_Up.IsValid() || point3D_Down == null || !point3D_Down.IsValid())
                {
                    tuples.Remove(tuple);
                    continue;
                }

                if (!shells.Contains(shell))
                {
                    shells.Add(shell);
                }

                if (shells.Count < 2)
                {
                    tuples.Remove(tuple);
                    result.Add(shell);
                    continue;
                }

                List<Shell> shells_Union = shells.Union(silverSpacing, tolerance_Angle, tolerance_Distance);
                if (shells_Union == null || shells_Union.Count == 0)
                {
                    tuples.Remove(tuple);
                    result.Add(shell);
                    continue;
                }

                Shell shell_Union = shells_Union.Find(x => x.Inside(point3D_Internal, silverSpacing, tolerance_Distance));
                if (shell_Union == null)
                {
                    tuples.Remove(tuple);
                    result.Add(shell);
                    continue;
                }

                foreach (Shell shell_Temp in shells)
                {
                    Point3D point3D_Internal_Temp = shell_Temp.InternalPoint3D(silverSpacing, tolerance_Distance);
                    if (shell_Union.Inside(point3D_Internal_Temp, silverSpacing, tolerance_Distance))
                    {
                        int index = tuples.FindIndex(x => x.Item1 == shell_Temp);
                        if (index != -1)
                        {
                            tuples.RemoveAt(index);
                        }
                        else
                        {
                            index = result.FindIndex(x => x == shell_Temp);
                            if (index != -1)
                            {
                                result.RemoveAt(index);
                            }
                        }
                    }
                }

                result.Add(shell_Union);
            }

            List<Tuple<BoundingBox3D, Face3D>> tuples_Face3D = face3Ds_Temp.ConvertAll(x => new Tuple<BoundingBox3D, Face3D>(x.GetBoundingBox(), x));

            List<bool> valids = Enumerable.Repeat(true, result.Count).ToList();
            Parallel.For(0, result.Count, (int i) =>
            //for(int i =0; i < result.Count; i++)
            {
                Shell shell = result[i];

                BoundingBox3D boundingBox3D_Shell = shell.GetBoundingBox();
                if (boundingBox3D_Shell == null)
                {
                    valids[i] = false;
                    return;
                    //continue;
                }

                Plane plane_Shell = Plane(boundingBox3D_Shell.Min.Z + offset);
                if (plane_Shell == null)
                {
                    valids[i] = false;
                    return;
                    //continue;
                }

                Face3D face3D_Shell = plane_Shell.Project(shell, tolerance_Distance);
                if (face3D_Shell == null)
                {
                    valids[i] = false;
                    return;
                    //continue;
                }

                Face2D face2D_Shell = plane_Shell.Convert(face3D_Shell);
                if (face2D_Shell == null)
                {
                    valids[i] = false;
                    return;
                    //continue;
                }

                List<Tuple<BoundingBox3D, Face3D>> tuples_Face3D_Temp = tuples_Face3D.FindAll(x => boundingBox3D_Shell.InRange(x.Item1, snapTolerance));
                if (tuples_Face3D_Temp == null || tuples_Face3D_Temp.Count == 0)
                {
                    valids[i] = false;
                    return;
                    //continue;
                }

                List<Face2D> face2Ds_Above = new List<Face2D>();
                List<Face2D> face2Ds_Below = new List<Face2D>();

                foreach (Tuple<BoundingBox3D, Face3D> tuple_Face3D in tuples_Face3D_Temp)
                {
                    Point3D point3D = tuple_Face3D?.Item2?.InternalPoint3D(tolerance_Distance);
                    if (point3D == null)
                    {
                        continue;
                    }

                    Face2D face2D = plane_Shell.Convert(plane_Shell.Project(tuple_Face3D.Item2));
                    if (face2D == null)
                    {
                        continue;
                    }

                    if (plane_Shell.Above(point3D, tolerance_Distance))
                    {
                        face2Ds_Above.Add(face2D);
                    }
                    else
                    {
                        face2Ds_Below.Add(face2D);
                    }
                }

                if (face2Ds_Above.Count == 0 || face2Ds_Below.Count == 0)
                {
                    valids[i] = false;
                    return;
                    //continue;
                }

                face2Ds_Above = face2Ds_Above.Union();
                face2Ds_Below = face2Ds_Below.Union();

                double area_Shell = face2D_Shell.GetArea();

                double area;

                area = 0;
                foreach (Face2D face2D in face2Ds_Above)
                {
                    List<Face2D> face2Ds_Intersection = face2D_Shell.Intersection(face2D, tolerance_Distance);
                    if (face2Ds_Intersection == null || face2Ds_Intersection.Count == 0)
                    {
                        continue;
                    }

                    area += face2Ds_Intersection.ConvertAll(x => x.GetArea()).FindAll(x => !double.IsNaN(x)).Sum();
                }

                if (area / area_Shell < 0.7)
                {
                    valids[i] = false;
                    return;
                    //continue;
                }

                area = 0;
                foreach (Face2D face2D in face2Ds_Below)
                {
                    List<Face2D> face2Ds_Intersection = face2D_Shell.Intersection(face2D, tolerance_Distance);
                    if (face2Ds_Intersection == null || face2Ds_Intersection.Count == 0)
                    {
                        continue;
                    }

                    area += face2Ds_Intersection.ConvertAll(x => x.GetArea()).FindAll(x => !double.IsNaN(x)).Sum();
                }

                if (area / area_Shell < 0.7)
                {
                    valids[i] = false;
                    return;
                    //continue;
                }

            });

            for (int i = result.Count - 1; i >= 0; i--)
            {
                if (!valids[i])
                {
                    result.RemoveAt(i);
                }
            }

            return result;
        }

        /// <summary>
        /// Create Shells by given Top and Bottom Face3Ds
        /// </summary>
        /// <param name="face3Ds">Top and Bottom Face3Ds</param>
        /// <param name="tolerance">tolerance</param>
        /// <returns>Shells</returns>
        public static List<Shell> Shells_ByTopAndBottom(this IEnumerable<Face3D> face3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face3Ds == null)
                return null;

            List<Face2D> face2Ds_All = new List<Face2D>();
            List<Tuple<double, List<Tuple<Face2D, BoundingBox2D>>>> tuples = new List<Tuple<double, List<Tuple<Face2D, BoundingBox2D>>>>();
            foreach (Face3D face3D in face3Ds)
            {
                BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();
                if (boundingBox3D == null)
                {
                    continue;
                }
                   
                double elevation_Min = boundingBox3D.Min.Z;
                Tuple<double, List<Tuple<Face2D, BoundingBox2D>>> tuple = tuples.Find(x => System.Math.Abs(x.Item1 - elevation_Min) < tolerance);
                if (tuple == null)
                {
                    tuple = new Tuple<double, List<Tuple<Face2D, BoundingBox2D>>>(elevation_Min, new List<Tuple<Face2D, BoundingBox2D>>());
                    tuples.Add(tuple);
                }

                Plane plane = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Min)) as Plane;
                Face2D face2D = plane.Convert(plane.Project(face3D));
                if (face2D == null)
                {
                    continue;
                }

                tuple.Item2.Add(new Tuple<Face2D, BoundingBox2D>(face2D, face2D.GetBoundingBox()));
                face2Ds_All.Add(face2D);
            }

            face2Ds_All = Planar.Query.Split(face2Ds_All);
            List<Point2D> point2Ds = face2Ds_All.ConvertAll(x => x?.GetInternalPoint2D());

            List<Shell> result = new List<Shell>();

            if (tuples == null || tuples.Count == 0)
                return result;

            tuples.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            for (int i = 0; i < tuples.Count - 1; i++)
            {
                Tuple<double, List<Tuple<Face2D, BoundingBox2D>>> tuple_Bottom = tuples[i];
                foreach (Tuple<Face2D, BoundingBox2D> tuple_Bottom_Face2D in tuple_Bottom.Item2)
                {
                    List<Point2D> point2Ds_Temp = point2Ds.FindAll(x => tuple_Bottom_Face2D.Item2.Inside(x, tolerance));
                    if (point2Ds_Temp == null || point2Ds_Temp.Count == 0)
                        continue;

                    Face2D face2D_Bottom = tuple_Bottom_Face2D.Item1;
                    point2Ds_Temp.RemoveAll(x => !face2D_Bottom.Inside(x, tolerance));
                    if (point2Ds_Temp == null || point2Ds_Temp.Count == 0)
                        continue;

                    List<Tuple<double, Face2D>> face2Ds_Top = new List<Tuple<double, Face2D>>();
                    foreach (Point2D point2D_Temp in point2Ds_Temp)
                    {
                        bool found = false;

                        for (int j = i + 1; j < tuples.Count; j++)
                        {
                            Tuple<double, List<Tuple<Face2D, BoundingBox2D>>> tuple_Top = tuples[j];
                            foreach (Tuple<Face2D, BoundingBox2D> tuple_Top_Face2D in tuple_Top.Item2)
                            {
                                if (!tuple_Top_Face2D.Item2.Inside(point2D_Temp, tolerance))
                                    continue;

                                Face2D face2D_Top = tuple_Top_Face2D.Item1;

                                if (!face2D_Top.Inside(point2D_Temp, tolerance))
                                    continue;

                                found = true;
                                if (face2Ds_Top.Find(x => x.Item2 == face2D_Top) == null)
                                    face2Ds_Top.Add(new Tuple<double, Face2D>(tuple_Top.Item1, face2D_Top));

                                break;
                            }

                            if (found)
                                break;
                        }
                    }

                    if (face2Ds_Top == null || face2Ds_Top.Count == 0)
                        continue;

                    List<Face2D> face2Ds_Top_Temp = new List<Face2D>(face2Ds_Top.ConvertAll(x => x.Item2));
                    face2Ds_Top_Temp.Add(face2D_Bottom);

                    face2Ds_Top_Temp = Planar.Query.Split(face2Ds_Top_Temp);
                    face2Ds_Top_Temp?.RemoveAll(x => !face2D_Bottom.Inside(x.InternalPoint2D(), tolerance));

                    if (face2Ds_Top_Temp == null || face2Ds_Top_Temp.Count == 0)
                        continue;

                    List<Face2D> face2Ds_Bottom_Temp = face2Ds_Top_Temp.Union();

                    List<Face3D> face3Ds_Shell = new List<Face3D>();
                    foreach(Face2D face2D_Bottom_Temp in face2Ds_Bottom_Temp)
                    {
                        Point2D point2D = face2D_Bottom_Temp.GetInternalPoint2D();
                        if (point2D == null)
                            continue;

                        double elevation_Top = face2Ds_Top.ConvertAll(x => x.Item1).Max();
                        int index = face2Ds_Top.FindIndex(x => x.Item2.Inside(point2D));
                        if (index != -1)
                            elevation_Top = face2Ds_Top[index].Item1;

                        double elevation_Bottom = tuple_Bottom.Item1;
                         
                        Plane plane_Top = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Top)) as Plane;
                        Plane plane_Bottom = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Bottom)) as Plane;
                        Plane plane_Bottom_Flipped = new Plane(plane_Bottom);
                        plane_Bottom_Flipped.FlipZ();

                        //Add Side Face3Ds
                        List<Segment2D> segment2Ds = new List<Segment2D>();
                        foreach (IClosed2D closed2D in face2D_Bottom_Temp.Edge2Ds)
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

                        foreach (Segment2D segment2D in segment2Ds)
                        {
                            Segment3D segment3D_Top = plane_Top.Convert(segment2D);
                            Segment3D segment3D_Bottom = plane_Bottom.Convert(segment2D);

                            Polygon3D polygon3D = Polygon3D(new Point3D[] { segment3D_Top[0], segment3D_Top[1], segment3D_Bottom[1], segment3D_Bottom[0] }, tolerance);
                            if (polygon3D == null)
                                continue;

                            Face3D face3D = new Face3D(polygon3D);
                            face3Ds_Shell.Add(face3D);
                        }

                        //Add Top Face3Ds
                        foreach (Face2D face2D_Top_Temp in face2Ds_Top_Temp)
                        {
                            Face3D face3D_Top = plane_Top.Convert(face2D_Top_Temp);
                            if (face3D_Top == null)
                                continue;

                            face3Ds_Shell.Add(face3D_Top);
                        }

                        //Add Bottom Face3D
                        Face3D face3D_Bottom = plane_Top.Convert(face2D_Bottom_Temp);
                        if (face3D_Bottom == null)
                            continue;

                        Face2D face2D_Bottom_Flipped = plane_Bottom_Flipped.Convert(face3D_Bottom);
                        face3D_Bottom = plane_Bottom_Flipped.Convert(face2D_Bottom_Flipped);

                        face3Ds_Shell.Add(face3D_Bottom);
                    }

                    if(face3Ds_Shell != null && face3Ds_Shell.Count > 0)
                    {
                        Shell shell = new Shell(face3Ds_Shell);
                        Shell shell_Merge = shell.Merge(tolerance);
                        if (shell_Merge != null)
                        {
                            shell = shell_Merge;
                        }

                        if (shell != null)
                        {
                            result.Add(shell);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Method creates Shells based on IFace3DObjects and given offset from level.
        /// </summary>
        /// <param name="face3Ds">Face3DObjects</param>
        /// <param name="offset">Offset from Level</param>
        /// <param name="snapTolerance">Snap Tolerance</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of Shells</returns>
        public static List<Shell> Shells_ByOffset_Old(this IEnumerable<Face3D> face3Ds, double offset, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (face3Ds == null)
                return null;

            List<Shell> result = new List<Shell>();
            if (face3Ds.Count() < 2)
                return result;

            Dictionary<double, List<Face3D>> elevationDictionary = face3Ds.ElevationDictionary(tolerance);

            List<Face3D> face3Ds_Temp = new List<Face3D>();
            Dictionary<double, List<Tuple<Face3D, List<Segment2D>>>> dictionary = new Dictionary<double, List<Tuple<Face3D, List<Segment2D>>>>();
            foreach (KeyValuePair<double, List<Face3D>> keyValuePair in elevationDictionary)
            {
                List<Tuple<Face3D, List<Segment2D>>> tuples = new List<Tuple<Face3D, List<Segment2D>>>();

                Plane plane = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, keyValuePair.Key + offset)) as Plane;

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (Face3D face3D in keyValuePair.Value)
                {
                    if (face3D == null)
                    {
                        continue;
                    }

                    PlanarIntersectionResult planarIntersectionResult = plane.PlanarIntersectionResult(new Face3D(face3D.GetExternalEdge3D()));
                    if (planarIntersectionResult == null)
                        continue;

                    List<ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<ISegmentable2D>();
                    if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                        continue;

                    Tuple<Face3D, List<Segment2D>> tuple_Panel = new Tuple<Face3D, List<Segment2D>>(face3D, new List<Segment2D>());
                    foreach (ISegmentable2D segmentable2D in segmentable2Ds_Temp)
                    {
                        segment2Ds.AddRange(segmentable2D.GetSegments());
                        tuple_Panel.Item2.AddRange(segmentable2D.GetSegments());
                    }

                    tuples.Add(tuple_Panel);
                }

                if (segment2Ds == null || segment2Ds.Count == 0)
                    continue;

                segment2Ds = Planar.Query.Split(segment2Ds, tolerance);
                segment2Ds = Planar.Query.Snap(segment2Ds, true, snapTolerance);

                List<Face2D> face2Ds = Planar.Create.Face2Ds(segment2Ds, EdgeOrientationMethod.Undefined, tolerance);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                List<IClosed2D> closed2Ds = Planar.Query.Holes(face2Ds);
                if (closed2Ds != null && closed2Ds.Count > 0)
                    closed2Ds.ForEach(x => face2Ds.Add(new Face2D(x)));

                if (tuples != null && tuples.Count > 0)
                    dictionary[keyValuePair.Key] = tuples;

                plane = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, keyValuePair.Key)) as Plane;

                face3Ds_Temp.AddRange(face2Ds.ConvertAll(x => new Face3D(plane, x)));
            }

            List<Face3D> face3Ds_Top = Query.TopFace3Ds(face3Ds_Temp);
            if (face3Ds_Top == null || face3Ds_Top.Count == 0)
                return result;

            foreach (Face3D face3D in face3Ds_Top)
            {
                Plane plane = face3D?.GetPlane();
                if (plane == null)
                    continue;

                if (!dictionary.TryGetValue(plane.Origin.Z, out List<Tuple<Face3D, List<Segment2D>>> tuples))
                    continue;

                IEnumerable<ISegmentable2D> segmentable2Ds = face3D.Edge2Ds?.FindAll(x => x is ISegmentable2D).Cast<ISegmentable2D>();
                if (segmentable2Ds == null || segmentable2Ds.Count() == 0)
                    continue;

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (ISegmentable2D segmentable2D_Temp in segmentable2Ds)
                    segment2Ds.AddRange(segmentable2D_Temp.GetSegments());

                BoundingBox2D boundingBox2D = Planar.Create.BoundingBox2D(segment2Ds);

                List<Face3D> face3Ds_Face3D = new List<Face3D>();
                foreach (Tuple<Face3D, List<Segment2D>> tuple in tuples)
                {
                    bool include = false;
                    foreach (Segment2D segment2D in tuple.Item2)
                    {
                        Point2D point2D = segment2D.Mid();

                        if (boundingBox2D.InRange(point2D, tolerance))
                        {
                            foreach (Segment2D segment2D_Temp in segment2Ds)
                            {
                                if (segment2D_Temp.On(point2D, tolerance))
                                {
                                    include = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (segment2Ds.Find(x => segment2D.On(x.Mid(), tolerance)) != null)
                                include = true;
                        }

                        if (include)
                            break;
                    }

                    if (include)
                        face3Ds_Face3D.Add(tuple.Item1);
                }

                if (face3Ds_Face3D == null || face3Ds_Face3D.Count == 0)
                    continue;

                double elevation_Max = face3Ds_Face3D.ConvertAll(x => x.GetBoundingBox().Max.Z).Max();

                if (System.Math.Abs(elevation_Max - plane.Origin.Z) < tolerance)
                    continue;

                face3Ds_Temp.Add(face3D.GetMoved(new Vector3D(0, 0, elevation_Max - plane.Origin.Z)) as Face3D);
            }

            //return Geometry.Spatial.Create.Shells_Depreciated(face3Ds, snapTolerance, tolerance);
            return Shells_ByTopAndBottom(face3Ds_Temp, tolerance);
        }

        /// <summary>
        /// Method creates Shells based on IFace3DObjects and given offset from level.
        /// </summary>
        /// <param name="face3Ds">Face3DObjects</param>
        /// <param name="offset">Offset from Level</param>
        /// <param name="snapTolerance">Snap Tolerance</param>
        /// <param name="tolerance_Angle">Angle Tolerance</param>
        /// <param name="tolerance_Distance">Distance Tolerance</param>
        /// <returns>List of Shells</returns>
        public static List<Shell> Shells_ByOffset(this IEnumerable<Face3D> face3Ds, double offset, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face3Ds == null)
                return null;

            List<Shell> result = new List<Shell>();
            if (face3Ds.Count() < 2)
                return result;

            Dictionary<double, List<Face3D>> elevationDictionary = face3Ds.ElevationDictionary(tolerance_Distance);

            List<Tuple<double, List<Face3D>>> tuples = new List<Tuple<double, List<Face3D>>>();
            foreach (KeyValuePair<double, List<Face3D>> keyValuePair in elevationDictionary)
            {
                tuples.Add(new Tuple<double, List<Face3D>>(keyValuePair.Key, keyValuePair.Value));
            }

            List<List<Shell>> shellsList = Enumerable.Repeat<List<Shell>>(null, tuples.Count).ToList();

            Parallel.For(0, tuples.Count, (int i) =>
            {
                Tuple<double, List<Face3D>> tuple = tuples[i];

                Plane plane_Bottom = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, tuple.Item1 + offset)) as Plane;
                Dictionary<Face3D, List<Segment2D>> dictionary_Bottom = tuple.Item2.SectionDictionary<Segment2D>(plane_Bottom, tolerance_Angle, tolerance_Distance);

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (KeyValuePair<Face3D, List<Segment2D>> keyValuePair_Face3D in dictionary_Bottom)
                {
                    if (keyValuePair_Face3D.Value == null)
                    {
                        continue;
                    }

                    segment2Ds.AddRange(keyValuePair_Face3D.Value);
                }

                segment2Ds = Planar.Query.Split(segment2Ds, tolerance_Distance);
                segment2Ds = Planar.Query.Snap(segment2Ds, true, snapTolerance);

                List<Face2D> face2Ds = Planar.Create.Face2Ds(segment2Ds, EdgeOrientationMethod.Undefined, tolerance_Distance);
                if (face2Ds == null || face2Ds.Count == 0)
                {
                    return;
                }

                List<IClosed2D> closed2Ds = Planar.Query.Holes(face2Ds);
                if (closed2Ds != null && closed2Ds.Count > 0)
                    closed2Ds.ForEach(x => face2Ds.Add(new Face2D(x)));

                plane_Bottom = Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, tuple.Item1)) as Plane;

                List<Face3D> face3Ds_Bottom = face2Ds.ConvertAll(x => new Face3D(plane_Bottom, x));

                Vector3D vector3D = new Vector3D(0, 0, tuple.Item2.ConvertAll(x => x.GetBoundingBox().Max.Z - tuple.Item1 ).Max());

                List<Shell> shells = face3Ds_Bottom.ConvertAll(x => Shell(x, vector3D, tolerance_Distance));
                if (shells == null || shells.Count == 0)
                {
                    return;
                }

                shellsList[i] = shells;
            });

            List<List<Shell>> shellsList_Split = Enumerable.Repeat<List<Shell>>(null, shellsList.Count).ToList();
            Parallel.For(0, shellsList.Count, (int i) => 
            {
                List<Shell> shells = shellsList[i];
                if (shells == null || shells.Count == 0)
                {
                    return;
                }

                List<Shell> shells_Split = new List<Shell>();

                if (i > 0)
                {
                    List<Shell> shells_Temp = shellsList[i - 1];
                    if (shells_Temp != null)
                    {
                        shells_Split.AddRange(shells_Temp);
                    }
                }

                if (i < shellsList.Count - 1)
                {
                    List<Shell> shells_Temp = shellsList[i + 1];
                    if (shells_Temp != null)
                    {
                        shells_Split.AddRange(shells_Temp);
                    }
                }

                shellsList_Split[i] = new List<Shell>();
                for (int j = 0; j < shells.Count; j++)
                {
                    Shell shell = new Shell(shells[i]);
                    foreach (Shell shell_Split in shells_Split)
                    {
                        shell.SplitFace3Ds(shell_Split, snapTolerance, tolerance_Angle, tolerance_Distance);
                    }
                    shellsList_Split[i].Add(shell);
                }
            });

            foreach(List<Shell> shells in shellsList_Split)
            {
                if(shells == null || shells.Count == 0)
                {
                    continue;
                }

                result.AddRange(shells);
            }

            return result;
        }

        public static List<Shell> Shells<N>(this IEnumerable<N> face3DObjects, IEnumerable<double> elevations, double offset = 0.1, double thinnessRatio = 0.01, double minArea = Core.Tolerance.MacroDistance, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where N: IFace3DObject
        {
            return Shells(face3DObjects?.ToList().ConvertAll(x => x?.Face3D), elevations, offset, thinnessRatio, minArea, snapTolerance, silverSpacing, tolerance_Angle, tolerance_Distance);
        }

        public static List<Shell> Shells<N>(this IEnumerable<N> face3DObjects, IEnumerable<double> elevations, IEnumerable<double> offsets, IEnumerable<double> auxiliaryElevations = null, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where N: IFace3DObject
        {
            return Shells(face3DObjects?.ToList().ConvertAll(x => x?.Face3D), elevations, offsets, auxiliaryElevations, snapTolerance, silverSpacing, tolerance_Angle, tolerance_Distance);
        }
    }
}