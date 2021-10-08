using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Method creates Shells based on IFace3DObjects and given offset from level.
        /// </summary>
        /// <param name="face3DObjects">Face3DObjects</param>
        /// <param name="offset">Offset from Level</param>
        /// <param name="snapTolerance">Snap Tolerance</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>List of Shells</returns>
        public static List<Shell> Shells(this IEnumerable<IFace3DObject> face3DObjects, double offset = 0.1, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (face3DObjects == null)
                return null;

            List<Shell> result = new List<Shell>();
            if (face3DObjects.Count() < 2)
                return result;

            Dictionary<double, List<IFace3DObject>> elevationDictionary = face3DObjects.ElevationDictionary<IFace3DObject>(tolerance);

            List<Face3D> face3Ds = new List<Face3D>();
            Dictionary<double, List<Tuple<IFace3DObject, List<Segment2D>>>> dictionary = new Dictionary<double, List<Tuple<IFace3DObject, List<Segment2D>>>>();
            foreach (KeyValuePair<double, List<IFace3DObject>> keyValuePair in elevationDictionary)
            {

                List<Tuple<IFace3DObject, List<Segment2D>>> tuples = new List<Tuple<IFace3DObject, List<Segment2D>>>();

                Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, keyValuePair.Key + offset)) as Plane;

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (IFace3DObject face3DObject in keyValuePair.Value)
                {
                    Face3D face3D = face3DObject.Face3D;
                    if (face3D == null)
                        continue;

                    face3D = new Face3D(face3D.GetExternalEdge3D());

                    PlanarIntersectionResult planarIntersectionResult = plane.PlanarIntersectionResult(face3D);
                    if (planarIntersectionResult == null)
                        continue;

                    List<ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<ISegmentable2D>();
                    if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                        continue;

                    Tuple<IFace3DObject, List<Segment2D>> tuple_Panel = new Tuple<IFace3DObject, List<Segment2D>>(face3DObject, new List<Segment2D>());
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

                List<Face2D> face2Ds = Planar.Create.Face2Ds(segment2Ds, tolerance);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                List<IClosed2D> closed2Ds = Planar.Query.Holes(face2Ds);
                if (closed2Ds != null && closed2Ds.Count > 0)
                    closed2Ds.ForEach(x => face2Ds.Add(new Face2D(x)));

                if (tuples != null && tuples.Count > 0)
                    dictionary[keyValuePair.Key] = tuples;

                plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, keyValuePair.Key)) as Plane;

                face3Ds.AddRange(face2Ds.ConvertAll(x => new Face3D(plane, x)));
            }

            List<Face3D> face3Ds_Top = TopFace3Ds(face3Ds);
            if (face3Ds_Top == null || face3Ds_Top.Count == 0)
                return result;

            foreach (Face3D face3D in face3Ds_Top)
            {
                Plane plane = face3D?.GetPlane();
                if (plane == null)
                    continue;

                if (!dictionary.TryGetValue(plane.Origin.Z, out List<Tuple<IFace3DObject, List<Segment2D>>> tuples))
                    continue;

                IEnumerable<ISegmentable2D> segmentable2Ds = face3D.Edge2Ds?.FindAll(x => x is ISegmentable2D).Cast<ISegmentable2D>();
                if (segmentable2Ds == null || segmentable2Ds.Count() == 0)
                    continue;

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (ISegmentable2D segmentable2D_Temp in segmentable2Ds)
                    segment2Ds.AddRange(segmentable2D_Temp.GetSegments());

                BoundingBox2D boundingBox2D = Planar.Create.BoundingBox2D(segment2Ds);

                List<IFace3DObject> face3DObject_Face3D = new List<IFace3DObject>();
                foreach (Tuple<IFace3DObject, List<Segment2D>> tuple in tuples)
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
                        face3DObject_Face3D.Add(tuple.Item1);
                }

                if (face3DObject_Face3D == null || face3DObject_Face3D.Count == 0)
                    continue;

                double elevation_Max = face3DObject_Face3D.ConvertAll(x => x.Face3D.GetBoundingBox().Max.Z).Max();

                if (System.Math.Abs(elevation_Max - plane.Origin.Z) < tolerance)
                    continue;

                face3Ds.Add(face3D.GetMoved(new Vector3D(0, 0, elevation_Max - plane.Origin.Z)) as Face3D);
            }

            //return Geometry.Spatial.Create.Shells_Depreciated(face3Ds, snapTolerance, tolerance);
            return Create.Shells(face3Ds, tolerance);
        }

        public static List<Shell> Shells(this IEnumerable<IFace3DObject> face3DObjects, IEnumerable<double> elevations, IEnumerable<double> offsets, IEnumerable<double> auxiliaryElevations = null, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face3DObjects == null || elevations == null)
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
            System.Threading.Tasks.Parallel.For(0, count, (int i) =>
            //for (int i=0; i < count; i++)
            {
                double elevation = elevations_All[i];

                //Bottom
                double offset = 0;
                int index = elevations.ToList().IndexOf(elevation);
                if (index != -1 && offsets != null && offsets.Count() > index)
                    offset = offsets.ElementAt(index);

                Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation + offset)) as Plane;

                List<IFace3DObject> face3DObject_Bottom = new List<IFace3DObject>(face3DObjects);
                face3DObject_Bottom.RemoveAll(x => x == null || x.Below(plane, tolerance_Distance));

                Dictionary<IFace3DObject, List<ISegmentable2D>> dictionary = face3DObject_Bottom.SectionDictionary<IFace3DObject, ISegmentable2D>(plane, tolerance_Distance);

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (KeyValuePair<IFace3DObject, List<ISegmentable2D>> keyValuePair in dictionary)
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

                plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

                tuples_Face3D_Bottom[i] = new Tuple<double, List<Face3D>>(elevation, face2Ds?.ConvertAll(x => plane.Convert(x)));

                if (i == 0)
                {
                    return;
                }

                //Top
                dictionary = face3DObjects.SectionDictionary<IFace3DObject, ISegmentable2D>(plane, tolerance_Distance);

                segment2Ds = new List<Segment2D>();
                foreach (KeyValuePair<IFace3DObject, List<ISegmentable2D>> keyValuePair in dictionary)
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
            System.Threading.Tasks.Parallel.For(0, count - 1, (int i) =>
            //for (int i = 0; i < count - 1; i++)
            {
                Tuple<double, List<Face3D>> tuple_Bottom = tuples_Face3D_Bottom[i];
                Tuple<double, List<Face3D>> tuple_Top = tuples_Face3D_Top[i + 1];

                List<Face3D> face3Ds = null;
                if (tuple_Bottom != null && tuple_Top != null && tuple_Bottom.Item2 != null && tuple_Top.Item2 != null)
                {
                    face3Ds = new List<Face3D>();
                    face3Ds.AddRange(tuple_Bottom.Item2);
                    face3Ds.AddRange(tuple_Top.Item2);
                }

                tuples_Shell[i] = new Tuple<double, List<Shell>>(tuple_Bottom.Item1, Create.Shells(face3Ds, tolerance_Distance));
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

        public static List<Shell> Shells(this IEnumerable<IFace3DObject> face3DObjects, IEnumerable<double> elevations, double offset = 0.1, double thinnessRatio = 0.01, double minArea = Core.Tolerance.MacroDistance, double snapTolerance = Core.Tolerance.MacroDistance, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face3DObjects == null || elevations == null)
            {
                return null;
            }

            List<BoundingBox3D> boundingBox3Ds = new List<BoundingBox3D>();
            foreach (IFace3DObject face3DObject in face3DObjects)
            {
                BoundingBox3D boundingBox3D_Face3DObject = face3DObject?.Face3D.GetBoundingBox();
                if (boundingBox3D_Face3DObject == null)
                {
                    continue;
                }

                boundingBox3Ds.Add(boundingBox3D_Face3DObject);
            }

            BoundingBox3D boundingBox3D_face3DObjects = new BoundingBox3D(boundingBox3Ds);

            List<double> elevations_Temp = new List<double>(elevations);
            elevations_Temp.Add(boundingBox3D_face3DObjects.Max.Z);
            elevations_Temp.Add(boundingBox3D_face3DObjects.Min.Z);
            Core.Modify.District(elevations_Temp, tolerance_Distance);

            List<List<Tuple<Shell, BoundingBox2D, Face2D, Plane>>> tuples_Elevation = Enumerable.Repeat<List<Tuple<Shell, BoundingBox2D, Face2D, Plane>>>(null, elevations.Count()).ToList();

            System.Threading.Tasks.Parallel.For(0, elevations.Count(), (int i) =>
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

                Plane plane_Section = Create.Plane(sectionElevation);

                Dictionary<IFace3DObject, List<ISegmentable2D>> dictionary = face3DObjects.SectionDictionary<IFace3DObject, ISegmentable2D>(plane_Section, tolerance_Distance);
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

                List<Face2D> face2Ds_Section = Planar.Create.Face2Ds(segment2Ds, tolerance_Distance);
                if (face2Ds_Section == null || face2Ds_Section.Count == 0)
                {
                    return;
                }

                List<IClosed2D> closed2Ds = Planar.Query.Holes(face2Ds_Section);
                if (closed2Ds != null && closed2Ds.Count > 0)
                    closed2Ds.ForEach(x => face2Ds_Section.Add(new Face2D(x)));

                Plane minPlane = Create.Plane(minElevation);
                Vector3D vector3D = Vector3D.WorldZ * (maxElevation - minElevation);

                tuples_Elevation[i] = new List<Tuple<Shell, BoundingBox2D, Face2D, Plane>>();
                foreach (Face2D face2D in face2Ds_Section)
                {
                    Shell shell = Create.Shell(minPlane.Convert(face2D), vector3D, tolerance_Distance);
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

            Plane plane = Plane.WorldXY;

            List<Face2D> face2Ds = new List<Face2D>();
            List<Face3D> face3Ds = new List<Face3D>();

            foreach (IFace3DObject face3DObject in face3DObjects)
            {
                Face3D face3D_Face3DObject = face3DObject?.Face3D;
                if (face3D_Face3DObject == null)
                {
                    continue;
                }

                Face3D face3D_Project = plane.Project(face3D_Face3DObject);
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
                face3Ds.Add(face3D_Face3DObject);
            }

            List<ISegmentable2D> segmentable2Ds_Edges = face2Ds.Edges().FindAll(x => x is ISegmentable2D).ConvertAll(x => x as ISegmentable2D);
            List<Segment2D> segment2Ds_Edges = Planar.Query.Split(segmentable2Ds_Edges, tolerance_Distance);
            segment2Ds_Edges = Planar.Query.Snap(segment2Ds_Edges, true, snapTolerance);
            face2Ds = Planar.Create.Face2Ds(segment2Ds_Edges, tolerance_Distance);

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

                Vector3D up = Vector3D.WorldZ;

                List<Shell> shells = new List<Shell>();
                foreach (Point3D point3D in point3Ds)
                {
                    List<Point3D> point3Ds_Intersection = point3D.Intersections(up, face3Ds, false, true, tolerance_Distance);
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

            List<Tuple<BoundingBox3D, Face3D>> tuples_Face3D = face3Ds.ConvertAll(x => new Tuple<BoundingBox3D, Face3D>(x.GetBoundingBox(), x));

            List<bool> valids = Enumerable.Repeat(true, result.Count).ToList();
            System.Threading.Tasks.Parallel.For(0, result.Count, (int i) =>
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

                Plane plane_Shell = Create.Plane(boundingBox3D_Shell.Min.Z + offset);
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
                if(!valids[i])
                {
                    result.RemoveAt(i);
                }
            }

            return result;
        }
    }
}