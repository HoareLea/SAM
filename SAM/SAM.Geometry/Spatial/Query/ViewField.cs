using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static void ViewField<T>(this IEnumerable<T> face3DObjects, Vector3D viewDirection, out List<LinkedFace3D> linkedFace3Ds_Hidden, out List<LinkedFace3D> linkedFace3Ds_Visible, bool hidden = true, bool visible = true, double tolerance_Area = Tolerance.MacroDistance, double tolerance_Snap = Tolerance.MacroDistance, double tolerance_Distance = Tolerance.Distance) where T : IFace3DObject
        {
            linkedFace3Ds_Hidden = null;
            linkedFace3Ds_Visible = null;

            if (face3DObjects == null || viewDirection == null || !viewDirection.IsValid())
            {
                return;
            }

            if (visible)
            {
                linkedFace3Ds_Visible = new List<LinkedFace3D>();
            }

            if (hidden)
            {
                linkedFace3Ds_Hidden = new List<LinkedFace3D>();
            }

            List<LinkedFace3D> linkedFace3Ds = new List<LinkedFace3D>();
            foreach (T face3DObject in face3DObjects)
            {
                if (face3DObject == null)
                {
                    continue;
                }

                if (face3DObject is LinkedFace3D)
                {
                    linkedFace3Ds.Add((LinkedFace3D)(object)face3DObject);
                    continue;
                }

                LinkedFace3D linkedFace3D = Create.LinkedFace3D(face3DObject);
                if (linkedFace3D != null)
                {
                    linkedFace3Ds.Add(linkedFace3D);
                }
            }

            if (linkedFace3Ds.Count() < 2)
            {
                LinkedFace3D linkedFace3D = linkedFace3Ds.ElementAt(0);
                if (linkedFace3D != null && linkedFace3Ds_Visible != null)
                {
                    linkedFace3Ds_Visible.Add(new LinkedFace3D(linkedFace3D));
                }

                return;
            }

            BoundingBox3D boundingBox3D = Create.BoundingBox3D(linkedFace3Ds);
            if (boundingBox3D == null || !boundingBox3D.IsValid())
            {
                return;
            }

            Vector3D vector3D = new Vector3D(viewDirection).Unit * boundingBox3D.Min.Distance(boundingBox3D.Max);
            Point3D point3D = boundingBox3D.GetCentroid().GetMoved(vector3D.GetNegated()) as Point3D;

            Line3D line3D = new Line3D(point3D, vector3D);
            Plane plane = new Plane(point3D, vector3D.Unit);

            Vector3D vector3D_Ray = 2 * vector3D;

            List<Tuple<LinkedFace3D, Planar.BoundingBox2D, Planar.Face2D>> tuples = new List<Tuple<LinkedFace3D, Planar.BoundingBox2D, Planar.Face2D>>();
            foreach (LinkedFace3D linkedFace3D in linkedFace3Ds)
            {
                Face3D face3D = linkedFace3D?.Face3D;
                if (face3D == null)
                {
                    continue;
                }

                Face3D face3D_Project = plane.Project(face3D, vector3D, tolerance_Distance);
                if (face3D_Project == null || !face3D_Project.IsValid())
                {
                    continue;
                }

                Planar.Face2D face2D = plane.Convert(face3D_Project);
                if (face2D == null || face2D.GetArea() < tolerance_Area)
                {
                    continue;
                }

                tuples.Add(new Tuple<LinkedFace3D, Planar.BoundingBox2D, Planar.Face2D>(linkedFace3D, face2D.GetBoundingBox(), face2D));
            }

            List<Tuple<LinkedFace3D, List<Planar.Face2D>>> tuples_Hidden = Enumerable.Repeat<Tuple<LinkedFace3D, List<Planar.Face2D>>>(null, tuples.Count).ToList();
            List<Tuple<LinkedFace3D, List<Planar.Face2D>>> tuples_Visible = Enumerable.Repeat<Tuple<LinkedFace3D, List<Planar.Face2D>>>(null, tuples.Count).ToList();

            System.Threading.Tasks.Parallel.For(0, tuples.Count, (int i) =>
            //for (int i =0; i < tuples.Count; i++)
            {
                Tuple<LinkedFace3D, Planar.BoundingBox2D, Planar.Face2D> tuple = tuples[i];

                List<Tuple<LinkedFace3D, Planar.BoundingBox2D, Planar.Face2D>> tuples_Temp = tuples.FindAll(x => x.Item2.InRange(tuple.Item2, tolerance_Distance));

                List<Planar.Segment2D> segment2Ds = new List<Planar.Segment2D>();
                foreach (Tuple<LinkedFace3D, Planar.BoundingBox2D, Planar.Face2D> tuple_Temp in tuples_Temp)
                {
                    Planar.Face2D face2D = tuple_Temp.Item3;

                    Planar.ISegmentable2D segmentable2D = face2D.ExternalEdge2D as Planar.ISegmentable2D;
                    if (segmentable2D != null)
                    {
                        List<Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                        if (segment2Ds_Temp != null)
                        {
                            segment2Ds.AddRange(segment2Ds_Temp);
                        }
                    }

                    List<Planar.IClosed2D> closed2Ds = face2D.InternalEdge2Ds;
                    if (closed2Ds != null)
                    {
                        foreach (Planar.IClosed2D closed2D in closed2Ds)
                        {
                            List<Planar.Segment2D> segment2Ds_Temp = (closed2D as Planar.ISegmentable2D)?.GetSegments();
                            if (segment2Ds_Temp != null)
                            {
                                segment2Ds.AddRange(segment2Ds_Temp);
                            }
                        }
                    }
                }

                segment2Ds = Planar.Query.Split(segment2Ds, tolerance_Distance);

                segment2Ds = Planar.Query.Snap(segment2Ds, true, tolerance_Snap);

                List<Planar.Face2D> face2Ds = Planar.Create.Face2Ds(segment2Ds, tolerance_Distance);
                if (face2Ds == null)
                {
                    return;
                }

                List<Planar.IClosed2D> closed2Ds_Holes = Planar.Query.Holes(face2Ds);
                if (closed2Ds_Holes != null && closed2Ds_Holes.Count > 0)
                {
                    closed2Ds_Holes.ForEach(x => face2Ds.Add(new Planar.Face2D(x)));
                }

                //List<Planar.Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);
                //if (polygon2Ds == null)
                //{
                //    return;
                //}

                //foreach (Planar.Polygon2D polygon2D in polygon2Ds)
                foreach (Planar.Face2D face2D in face2Ds)
                {
                    //Planar.Point2D point2D = polygon2D?.GetInternalPoint2D(tolerance_Distance);
                    Planar.Point2D point2D = face2D?.GetInternalPoint2D(tolerance_Distance);
                    if (point2D == null)
                    {
                        continue;
                    }

                    if (!tuple.Item2.InRange(point2D, tolerance_Distance) || !tuple.Item3.Inside(point2D, tolerance_Distance))
                    {
                        continue;
                    }

                    Point3D point3D_Start = plane.Convert(point2D);
                    Point3D point3D_End = point3D_Start.GetMoved(vector3D_Ray) as Point3D;

                    Segment3D segment3D = new Segment3D(point3D_Start, point3D_End);
                    BoundingBox3D boundingBox3D_Segment3D = segment3D.GetBoundingBox();

                    Dictionary<LinkedFace3D, Point3D> dictionary_Intersection = IntersectionDictionary(segment3D, tuples_Temp.ConvertAll(x => x.Item1), true, tolerance_Distance);
                    if (dictionary_Intersection == null || dictionary_Intersection.Count == 0)
                    {
                        continue;
                    }

                    List<Tuple<LinkedFace3D, List<Planar.Face2D>>> tuples_Result = null;
                    if (dictionary_Intersection.Keys.FirstOrDefault() == tuple.Item1 && visible)
                    {
                        tuples_Result = tuples_Visible;
                    }
                    else if (dictionary_Intersection.Keys.FirstOrDefault() != tuple.Item1 && hidden)
                    {
                        tuples_Result = tuples_Hidden;
                    }

                    if (tuples_Result == null)
                    {
                        continue;
                    }

                    if (tuples_Result[i] == null)
                    {
                        tuples_Result[i] = new Tuple<LinkedFace3D, List<Planar.Face2D>>(tuple.Item1, new List<Planar.Face2D>());
                    }

                    tuples_Result[i].Item2.Add(face2D);
                }

            });

            tuples_Hidden.RemoveAll(x => x == null);
            tuples_Visible.RemoveAll(x => x == null);

            if (tuples_Hidden != null && tuples_Hidden.Count != 0)
            {
                for (int i = 0; i < tuples_Hidden.Count; i++)
                {
                    List<LinkedFace3D> linkedFace3Ds_Temp = Create.LinkedFace3Ds(tuples_Hidden[i].Item1, vector3D, tuples_Hidden[i].Item2, plane, tolerance_Distance);
                    if (linkedFace3Ds_Temp == null || linkedFace3Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    linkedFace3Ds_Hidden.AddRange(linkedFace3Ds_Temp);
                }
            }

            if (tuples_Visible != null && tuples_Visible.Count != 0)
            {
                for (int i = 0; i < tuples_Visible.Count; i++)
                {

                    List<LinkedFace3D> linkedFace3Ds_Temp = Create.LinkedFace3Ds(tuples_Visible[i].Item1, vector3D, tuples_Visible[i].Item2, plane, tolerance_Distance);
                    if (linkedFace3Ds_Temp == null || linkedFace3Ds_Temp.Count == 0)
                    {
                        //return;
                        continue;
                    }

                    linkedFace3Ds_Visible.AddRange(linkedFace3Ds_Temp);
                }
            }

        }

        //    public static void ViewField_Obsolete1<T>(this IEnumerable<T> face3DObjects, Vector3D viewDirection, out List<LinkedFace3D> linkedFace3Ds_Hidden, out List<LinkedFace3D> linkedFace3Ds_Visible, bool hidden = true, bool visible = true, double tolerance_Area = Tolerance.MacroDistance, double tolerance_Snap = Tolerance.MacroDistance, double tolerance_Distance = Tolerance.Distance) where T : IFace3DObject
        //    {
        //        linkedFace3Ds_Hidden = null;
        //        linkedFace3Ds_Visible = null;

        //        if (face3DObjects == null || viewDirection == null || !viewDirection.IsValid())
        //        {
        //            return;
        //        }

        //        if (visible)
        //        {
        //            linkedFace3Ds_Visible = new List<LinkedFace3D>();
        //        }

        //        if (hidden)
        //        {
        //            linkedFace3Ds_Hidden = new List<LinkedFace3D>();
        //        }

        //        List<LinkedFace3D> linkedFace3Ds = new List<LinkedFace3D>();
        //        foreach (T face3DObject in face3DObjects)
        //        {
        //            if (face3DObject == null)
        //            {
        //                continue;
        //            }

        //            if (face3DObject is LinkedFace3D)
        //            {
        //                linkedFace3Ds.Add((LinkedFace3D)(object)face3DObject);
        //                continue;
        //            }

        //            LinkedFace3D linkedFace3D = Create.LinkedFace3D(face3DObject);
        //            if (linkedFace3D != null)
        //            {
        //                linkedFace3Ds.Add(linkedFace3D);
        //            }
        //        }

        //        if (linkedFace3Ds.Count() < 2)
        //        {
        //            LinkedFace3D linkedFace3D = linkedFace3Ds.ElementAt(0);
        //            if (linkedFace3D != null && linkedFace3Ds_Visible != null)
        //            {
        //                linkedFace3Ds_Visible.Add(new LinkedFace3D(linkedFace3D));
        //            }

        //            return;
        //        }

        //        BoundingBox3D boundingBox3D = Create.BoundingBox3D(linkedFace3Ds);
        //        if (boundingBox3D == null || !boundingBox3D.IsValid())
        //        {
        //            return;
        //        }

        //        Vector3D vector3D = new Vector3D(viewDirection).Unit * boundingBox3D.Min.Distance(boundingBox3D.Max);
        //        Point3D point3D = boundingBox3D.GetCentroid().GetMoved(vector3D.GetNegated()) as Point3D;

        //        Line3D line3D = new Line3D(point3D, vector3D);
        //        Plane plane = new Plane(point3D, vector3D.Unit);


        //        List<Tuple<double, LinkedFace3D>> tuples = new List<Tuple<double, LinkedFace3D>>();
        //        foreach (LinkedFace3D linkedFace3D in linkedFace3Ds)
        //        {
        //            //if(solarFace == null)
        //            //{
        //            //    continue;
        //            //}

        //            //solarFace.DistantPoint3D(line3D, out double distance);

        //            //double distance = MinProjectedDistance(solarFace, line3D);
        //            double distance = MaxProjectedDistance(linkedFace3D, plane);
        //            if (double.IsNaN(distance))
        //            {
        //                continue;
        //            }

        //            tuples.Add(new Tuple<double, LinkedFace3D>(distance, linkedFace3D));
        //        }

        //        tuples.Sort((x, y) => x.Item1.CompareTo(y.Item1));



        //        List<Planar.Face2D> face2Ds_Union = new List<Planar.Face2D>();
        //        double area = double.NaN;

        //        List<LinkedFace3D> linkedFace3Ds_Filtered = new List<LinkedFace3D>();
        //        List<Planar.Segment2D> segment2Ds = new List<Planar.Segment2D>();
        //        foreach (LinkedFace3D linkedFace3D in tuples.ConvertAll(x => x.Item2))
        //        {
        //            Face3D face3D = linkedFace3D?.Face3D;
        //            if (face3D == null)
        //            {
        //                continue;
        //            }

        //            Face3D face3D_Project = plane.Project(face3D, vector3D, tolerance_Distance);
        //            if (face3D_Project == null || !face3D_Project.IsValid())
        //            {
        //                continue;
        //            }

        //            Planar.Face2D face2D = plane.Convert(face3D_Project);
        //            if (face2D == null || face2D.GetArea() < tolerance_Area)
        //            {
        //                continue;
        //            }

        //            face2Ds_Union.Add(face2D);

        //            if (double.IsNaN(area))
        //            {
        //                area = face2D.GetArea();
        //            }
        //            else
        //            {
        //                face2Ds_Union = Planar.Query.Union(face2Ds_Union);
        //                double area_Temp = face2Ds_Union.ConvertAll(x => x.GetArea()).Sum();
        //                if (System.Math.Abs(area - area_Temp) < tolerance_Distance)
        //                {
        //                    if (linkedFace3Ds_Hidden != null)
        //                    {
        //                        linkedFace3Ds_Hidden.Add(linkedFace3D);
        //                    }

        //                    continue;
        //                }
        //                area = area_Temp;
        //            }

        //            Planar.ISegmentable2D segmentable2D = face2D.ExternalEdge2D as Planar.ISegmentable2D;
        //            if (segmentable2D != null)
        //            {
        //                List<Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
        //                if (segment2Ds_Temp != null)
        //                {
        //                    segment2Ds.AddRange(segment2Ds_Temp);
        //                }
        //            }

        //            List<Planar.IClosed2D> closed2Ds = face2D.InternalEdge2Ds;
        //            if (closed2Ds != null)
        //            {
        //                foreach (Planar.IClosed2D closed2D in closed2Ds)
        //                {
        //                    List<Planar.Segment2D> segment2Ds_Temp = (closed2D as Planar.ISegmentable2D)?.GetSegments();
        //                    if (segment2Ds_Temp != null)
        //                    {
        //                        segment2Ds.AddRange(segment2Ds_Temp);
        //                    }
        //                }
        //            }

        //            linkedFace3Ds_Filtered.Add(linkedFace3D);
        //        }

        //        if (segment2Ds == null || linkedFace3Ds_Filtered == null)
        //        {
        //            return;
        //        }

        //        segment2Ds = Planar.Query.Split(segment2Ds, tolerance_Distance);

        //        segment2Ds = Planar.Query.Snap(segment2Ds, true, tolerance_Snap);

        //        List<Planar.Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);
        //        if (polygon2Ds == null)
        //        {
        //            return;
        //        }

        //        List<Tuple<Planar.Polygon2D, List<LinkedFace3D>>> tuples_Hidden = Enumerable.Repeat<Tuple<Planar.Polygon2D, List<LinkedFace3D>>>(null, polygon2Ds.Count).ToList();
        //        List<Tuple<Planar.Polygon2D, LinkedFace3D>> tuples_Visible = Enumerable.Repeat<Tuple<Planar.Polygon2D, LinkedFace3D>>(null, polygon2Ds.Count).ToList();

        //        Vector3D vector3D_Ray = 2 * vector3D;

        //        System.Threading.Tasks.Parallel.For(0, polygon2Ds.Count, (int i) =>
        //        //for(int i =0; i < polygon2Ds.Count; i++)
        //        {
        //            Planar.Polygon2D polygon2D = polygon2Ds[i];

        //            Planar.Point2D point2D = polygon2D?.GetInternalPoint2D(tolerance_Distance);
        //            if (point2D == null)
        //            {
        //                return;
        //                //continue;
        //            }

        //            Point3D point3D_Start = plane.Convert(point2D);
        //            Point3D point3D_End = point3D_Start.GetMoved(vector3D_Ray) as Point3D;

        //            Segment3D segment3D = new Segment3D(point3D_Start, point3D_End);
        //            BoundingBox3D boundingBox3D_Segment3D = segment3D.GetBoundingBox();

        //            List<LinkedFace3D> linkedFace3Ds_Polygon2D = linkedFace3Ds_Filtered.FindAll(x => x.GetBoundingBox().InRange(boundingBox3D_Segment3D, tolerance_Distance) && x.GetBoundingBox().InRange(segment3D, tolerance_Distance));

        //            Dictionary<LinkedFace3D, Point3D> dictionary_Intersection = IntersectionDictionary(segment3D, linkedFace3Ds_Polygon2D, true, tolerance_Distance);
        //            if (dictionary_Intersection == null || dictionary_Intersection.Count == 0)
        //            {
        //                return;
        //                //continue;
        //            }

        //            List<LinkedFace3D> linkedFace3Ds_Temp = new List<LinkedFace3D>(dictionary_Intersection.Keys);

        //            if (visible)
        //            {
        //                tuples_Visible[i] = new Tuple<Planar.Polygon2D, LinkedFace3D>(polygon2D, linkedFace3Ds_Temp[0]);
        //            }

        //            if (!hidden || dictionary_Intersection.Count < 2)
        //            {
        //                return;
        //                //continue;
        //            }

        //            linkedFace3Ds_Temp.RemoveAt(0);

        //            tuples_Hidden[i] = new Tuple<Planar.Polygon2D, List<LinkedFace3D>>(polygon2D, linkedFace3Ds_Temp);
        //        });

        //        tuples_Visible.RemoveAll(x => x == null);
        //        tuples_Hidden.RemoveAll(x => x == null);

        //        if (tuples_Hidden != null || tuples_Hidden.Count != 0)
        //        {
        //            List<List<LinkedFace3D>> linkedFace3DsList = Enumerable.Repeat<List<LinkedFace3D>>(null, linkedFace3Ds_Filtered.Count).ToList();

        //            System.Threading.Tasks.Parallel.For(0, linkedFace3DsList.Count, (int i) =>
        //            //for (int i = 0; i < solarFaces_Filtered.Count; i++)
        //            {
        //                LinkedFace3D linkedFace3D = linkedFace3Ds_Filtered[i];

        //                List<Tuple<Planar.Polygon2D, List<LinkedFace3D>>> tuples_SolarFace = tuples_Hidden.FindAll(x => x.Item2.Contains(linkedFace3D));
        //                if (tuples_SolarFace == null || tuples_SolarFace.Count == 0)
        //                {
        //                    return;
        //                    //continue;
        //                }

        //                List<LinkedFace3D> solarFaces_Temp = Create.LinkedFace3Ds(linkedFace3D, vector3D, tuples_SolarFace.ConvertAll(x => x.Item1), plane, tolerance_Distance);
        //                if (solarFaces_Temp == null || solarFaces_Temp.Count == 0)
        //                {
        //                    return;
        //                    //continue;
        //                }

        //                linkedFace3DsList.Add(solarFaces_Temp);
        //            });

        //            foreach (List<LinkedFace3D> solarFaces_Temp in linkedFace3DsList)
        //            {
        //                if (solarFaces_Temp == null || solarFaces_Temp.Count == 0)
        //                {
        //                    continue;
        //                }

        //                linkedFace3Ds_Hidden.AddRange(solarFaces_Temp);
        //            }
        //        }

        //        if (tuples_Visible != null || tuples_Visible.Count != 0)
        //        {
        //            List<List<LinkedFace3D>> linkedFace3DsList = Enumerable.Repeat<List<LinkedFace3D>>(null, linkedFace3Ds_Filtered.Count).ToList();

        //            System.Threading.Tasks.Parallel.For(0, linkedFace3DsList.Count, (int i) =>
        //            //for (int i = 0; i < solarFaces_Filtered.Count; i++)
        //            {
        //                LinkedFace3D linkedFace3D = linkedFace3Ds_Filtered[i];

        //                List<Tuple<Planar.Polygon2D, LinkedFace3D>> tuples_SolarFace = tuples_Visible.FindAll(x => x.Item2 == linkedFace3D);
        //                if (tuples_SolarFace == null || tuples_SolarFace.Count == 0)
        //                {
        //                    return;
        //                    //continue;
        //                }

        //                List<LinkedFace3D> solarFaces_Temp = Create.LinkedFace3Ds(linkedFace3D, vector3D, tuples_SolarFace.ConvertAll(x => x.Item1), plane, tolerance_Distance);
        //                if (solarFaces_Temp == null || solarFaces_Temp.Count == 0)
        //                {
        //                    return;
        //                    //continue;
        //                }

        //                linkedFace3DsList.Add(solarFaces_Temp);
        //            });

        //            foreach (List<LinkedFace3D> linkedFace3Ds_Temp in linkedFace3DsList)
        //            {
        //                if (linkedFace3Ds_Temp == null || linkedFace3Ds_Temp.Count == 0)
        //                {
        //                    continue;
        //                }

        //                linkedFace3Ds_Visible.AddRange(linkedFace3Ds_Temp);
        //            }
        //        }
        //    }

        //    public static void ViewField_Obselete2<T>(this IEnumerable<T> face3DObjects, Vector3D viewDirection, out List<LinkedFace3D> linkedFace3Ds_Hidden, out List<LinkedFace3D> linkedFace3Ds_Visible, bool hidden = true, bool visible = true, double tolerance_Area = Tolerance.MacroDistance, double tolerance_Snap = Tolerance.MacroDistance, double tolerance_Distance = Tolerance.Distance) where T : IFace3DObject
        //    {
        //        linkedFace3Ds_Hidden = null;
        //        linkedFace3Ds_Visible = null;

        //        if (face3DObjects == null || viewDirection == null || !viewDirection.IsValid())
        //        {
        //            return;
        //        }

        //        if (visible)
        //        {
        //            linkedFace3Ds_Visible = new List<LinkedFace3D>();
        //        }

        //        if (hidden)
        //        {
        //            linkedFace3Ds_Hidden = new List<LinkedFace3D>();
        //        }

        //        List<LinkedFace3D> linkedFace3Ds = new List<LinkedFace3D>();
        //        foreach (T face3DObject in face3DObjects)
        //        {
        //            if (face3DObject == null)
        //            {
        //                continue;
        //            }

        //            if (face3DObject is LinkedFace3D)
        //            {
        //                linkedFace3Ds.Add((LinkedFace3D)(object)face3DObject);
        //                continue;
        //            }

        //            LinkedFace3D linkedFace3D = Create.LinkedFace3D(face3DObject);
        //            if (linkedFace3D != null)
        //            {
        //                linkedFace3Ds.Add(linkedFace3D);
        //            }
        //        }

        //        if (linkedFace3Ds.Count() < 2)
        //        {
        //            return;
        //        }

        //        BoundingBox3D boundingBox3D = Create.BoundingBox3D(linkedFace3Ds);
        //        if (boundingBox3D == null || !boundingBox3D.IsValid())
        //        {
        //            return;
        //        }

        //        Vector3D vector3D = new Vector3D(viewDirection).Unit * boundingBox3D.Min.Distance(boundingBox3D.Max);
        //        Point3D point3D = boundingBox3D.GetCentroid().GetMoved(vector3D.GetNegated()) as Point3D;

        //        Plane plane = new Plane(point3D, vector3D.Unit);

        //        List<LinkedFace3D> linkedFace3Ds_Filtered = new List<LinkedFace3D>();
        //        List<Planar.Segment2D> segment2Ds = new List<Planar.Segment2D>();
        //        foreach (LinkedFace3D linkedFace3D in linkedFace3Ds)
        //        {
        //            Face3D face3D = linkedFace3D?.Face3D;
        //            if (face3D == null)
        //            {
        //                continue;
        //            }

        //            Face3D face3D_Project = plane.Project(face3D, vector3D, tolerance_Distance);
        //            if (face3D_Project == null || !face3D_Project.IsValid())
        //            {
        //                continue;
        //            }

        //            Planar.Face2D face2D = plane.Convert(face3D_Project);
        //            if (face2D == null || face2D.GetArea() < tolerance_Area)
        //            {
        //                continue;
        //            }

        //            Planar.ISegmentable2D segmentable2D = face2D.ExternalEdge2D as Planar.ISegmentable2D;
        //            if (segmentable2D != null)
        //            {
        //                List<Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
        //                if (segment2Ds_Temp != null)
        //                {
        //                    segment2Ds.AddRange(segment2Ds_Temp);
        //                }
        //            }

        //            List<Planar.IClosed2D> closed2Ds = face2D.InternalEdge2Ds;
        //            if (closed2Ds != null)
        //            {
        //                foreach (Planar.IClosed2D closed2D in closed2Ds)
        //                {
        //                    List<Planar.Segment2D> segment2Ds_Temp = (closed2D as Planar.ISegmentable2D)?.GetSegments();
        //                    if (segment2Ds_Temp != null)
        //                    {
        //                        segment2Ds.AddRange(segment2Ds_Temp);
        //                    }
        //                }
        //            }

        //            linkedFace3Ds_Filtered.Add(linkedFace3D);
        //        }

        //        if (segment2Ds == null || linkedFace3Ds_Filtered == null)
        //        {
        //            return;
        //        }

        //        segment2Ds = Planar.Query.Split(segment2Ds, tolerance_Distance);

        //        segment2Ds = Planar.Query.Snap(segment2Ds, true, tolerance_Snap);

        //        List<Planar.Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);
        //        if (polygon2Ds == null)
        //        {
        //            return;
        //        }

        //        Vector3D vector3D_Ray = 2 * vector3D;

        //        List<Tuple<Planar.Polygon2D, List<LinkedFace3D>>> tuples_Hidden = Enumerable.Repeat<Tuple<Planar.Polygon2D, List<LinkedFace3D>>>(null, polygon2Ds.Count).ToList();
        //        List<Tuple<Planar.Polygon2D, LinkedFace3D>> tuples_Visible = Enumerable.Repeat<Tuple<Planar.Polygon2D, LinkedFace3D>>(null, polygon2Ds.Count).ToList();

        //        System.Threading.Tasks.Parallel.For(0, polygon2Ds.Count, (int i) =>
        //        //for(int i =0; i < polygon2Ds.Count; i++)
        //        {
        //            Planar.Polygon2D polygon2D = polygon2Ds[i];

        //            Planar.Point2D point2D = polygon2D?.GetInternalPoint2D(tolerance_Distance);
        //            if (point2D == null)
        //            {
        //                return;
        //                //continue;
        //            }

        //            Point3D point3D_Start = plane.Convert(point2D);
        //            Point3D point3D_End = point3D_Start.GetMoved(vector3D_Ray) as Point3D;

        //            Segment3D segment3D = new Segment3D(point3D_Start, point3D_End);
        //            BoundingBox3D boundingBox3D_Segment3D = segment3D.GetBoundingBox();

        //            List<LinkedFace3D> linkedFace3Ds_Polygon2D = linkedFace3Ds_Filtered.FindAll(x => x.GetBoundingBox().InRange(boundingBox3D_Segment3D, tolerance_Distance) && x.GetBoundingBox().InRange(segment3D, tolerance_Distance));

        //            Dictionary<LinkedFace3D, Point3D> dictionary_Intersection = IntersectionDictionary(segment3D, linkedFace3Ds_Polygon2D, true, tolerance_Distance);
        //            if (dictionary_Intersection == null || dictionary_Intersection.Count == 0)
        //            {
        //                return;
        //                //continue;
        //            }

        //            List<LinkedFace3D> linkedFace3Ds_Temp = new List<LinkedFace3D>(dictionary_Intersection.Keys);

        //            if (visible)
        //            {
        //                tuples_Visible[i] = new Tuple<Planar.Polygon2D, LinkedFace3D>(polygon2D, linkedFace3Ds_Temp[0]);
        //            }

        //            if (!hidden || dictionary_Intersection.Count < 2)
        //            {
        //                return;
        //                //continue;
        //            }

        //            linkedFace3Ds_Temp.RemoveAt(0);

        //            tuples_Hidden[i] = new Tuple<Planar.Polygon2D, List<LinkedFace3D>>(polygon2D, linkedFace3Ds_Temp);
        //        });

        //        tuples_Visible.RemoveAll(x => x == null);
        //        tuples_Hidden.RemoveAll(x => x == null);


        //        if (tuples_Hidden != null || tuples_Hidden.Count != 0)
        //        {
        //            List<List<LinkedFace3D>> linkedFace3DsList = Enumerable.Repeat<List<LinkedFace3D>>(null, linkedFace3Ds_Filtered.Count).ToList();

        //            System.Threading.Tasks.Parallel.For(0, linkedFace3DsList.Count, (int i) =>
        //            //for (int i = 0; i < solarFaces_Filtered.Count; i++)
        //            {
        //                LinkedFace3D solarFace = linkedFace3Ds_Filtered[i];

        //                List<Tuple<Planar.Polygon2D, List<LinkedFace3D>>> tuples_linkedFace3D = tuples_Hidden.FindAll(x => x.Item2.Contains(solarFace));
        //                if (tuples_linkedFace3D == null || tuples_linkedFace3D.Count == 0)
        //                {
        //                    return;
        //                    //continue;
        //                }

        //                List<LinkedFace3D> linkedFace3Ds_Temp = Create.LinkedFace3Ds(solarFace, vector3D, tuples_linkedFace3D.ConvertAll(x => x.Item1), plane, tolerance_Distance);
        //                if (linkedFace3Ds_Temp == null || linkedFace3Ds_Temp.Count == 0)
        //                {
        //                    return;
        //                    //continue;
        //                }

        //                linkedFace3DsList.Add(linkedFace3Ds_Temp);
        //            });

        //            foreach (List<LinkedFace3D> linkedFace3Ds_Temp in linkedFace3DsList)
        //            {
        //                if (linkedFace3Ds_Temp == null || linkedFace3Ds_Temp.Count == 0)
        //                {
        //                    continue;
        //                }

        //                linkedFace3Ds_Hidden.AddRange(linkedFace3Ds_Temp);
        //            }
        //        }

        //        if (tuples_Visible != null || tuples_Visible.Count != 0)
        //        {
        //            List<List<LinkedFace3D>> linkedFace3DsList = Enumerable.Repeat<List<LinkedFace3D>>(null, linkedFace3Ds_Filtered.Count).ToList();

        //            System.Threading.Tasks.Parallel.For(0, linkedFace3DsList.Count, (int i) =>
        //            //for (int i = 0; i < solarFaces_Filtered.Count; i++)
        //            {
        //                LinkedFace3D solarFace = linkedFace3Ds_Filtered[i];

        //                List<Tuple<Planar.Polygon2D, LinkedFace3D>> tuples_linkedFace3D = tuples_Visible.FindAll(x => x.Item2 == solarFace);
        //                if (tuples_linkedFace3D == null || tuples_linkedFace3D.Count == 0)
        //                {
        //                    return;
        //                    //continue;
        //                }

        //                List<LinkedFace3D> linkedFace3Ds_Temp = Create.LinkedFace3Ds(solarFace, vector3D, tuples_linkedFace3D.ConvertAll(x => x.Item1), plane, tolerance_Distance);
        //                if (linkedFace3Ds_Temp == null || linkedFace3Ds_Temp.Count == 0)
        //                {
        //                    return;
        //                    //continue;
        //                }

        //                linkedFace3DsList.Add(linkedFace3Ds_Temp);
        //            });

        //            foreach (List<LinkedFace3D> linkedFace3Ds_Temp in linkedFace3DsList)
        //            {
        //                if (linkedFace3Ds_Temp == null || linkedFace3Ds_Temp.Count == 0)
        //                {
        //                    continue;
        //                }

        //                linkedFace3Ds_Visible.AddRange(linkedFace3Ds_Temp);
        //            }
        //        }
        //    }
        //}
    }
}