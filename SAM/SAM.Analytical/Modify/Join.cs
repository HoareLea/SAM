using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using SAM.Geometry.Planar;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        [Obsolete]
        public static List<Guid> Join_Obsolete(this List<Panel> panels, double distance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            Plane plane = Plane.WorldXY;

            //Collecting data -> Item1 - Panel, Item2 -> Min Elevation, Item3 - Max Elevation, Item4 - Location of panel as Segment2D on plane
            List<Tuple<Panel, double, double, List<Segment2D>>> tuples = new List<Tuple<Panel, double, double, List<Segment2D>>>();
            Dictionary<Panel, Segment2D> dictionary = new Dictionary<Panel, Segment2D>();
            foreach (Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if (face3D == null)
                    continue;

                if (!Geometry.Spatial.Query.Vertical(face3D.GetPlane(), tolerance))
                    continue;

                if (!face3D.Rectangular())
                    continue;

                ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D == null)
                    continue;

                List<Point3D> point3Ds = segmentable3D.GetPoints();
                if (point3Ds == null || point3Ds.Count == 0)
                    continue;

                BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                Plane plane_Temp = new Plane(plane, plane.Origin.GetMoved(new Vector3D(0, 0, boundingBox3D.Min.Z)) as Point3D);

                List<Point2D> point2Ds = point3Ds.ConvertAll(x => plane_Temp.Convert(plane_Temp.Project(x)));

                Point2D point2D_1 = null;
                Point2D point2D_2 = null;
                Geometry.Planar.Query.ExtremePoints(point2Ds, out point2D_1, out point2D_2);
                if (point2D_1 == null || point2D_2 == null || point2D_1.AlmostEquals(point2D_2, tolerance))
                    continue;

                Segment2D segment2D = new Segment2D(point2D_1, point2D_2);

                dictionary[panel] = segment2D;
                tuples.Add(new Tuple<Panel, double, double, List<Segment2D>>(panel, boundingBox3D.Min.Z, boundingBox3D.Max.Z, new List<Segment2D>() { segment2D }));
            }

            bool updated = false;
            do
            {
                updated = false;

                //Extending Segments in both ways
                List<Segment2D> segment2Ds = new List<Segment2D>();
                List<Segment2D> segment2Ds_Main = new List<Segment2D>();
                foreach (Tuple<Panel, double, double, List<Segment2D>> tuple in tuples)
                {
                    if (tuple.Item4 == null || tuple.Item4.Count == 0)
                        continue;

                    Segment2D segment2D = tuple.Item4[0];
                    segment2Ds_Main.Add(segment2D);

                    Vector2D vector2D = segment2D.Direction * distance;

                    Segment2D segment2D_Temp = null;

                    segment2D_Temp = new Segment2D(segment2D[1], segment2D[1].GetMoved(vector2D));
                    if (tuples.Find(x => x.Item4 != null && x.Item4.Count != 0 && x.Item4[0].On(segment2D_Temp[0]) && x.Item4[0].On(segment2D_Temp[1])) == null)
                        tuple.Item4.Add(segment2D_Temp);

                    segment2D_Temp = new Segment2D(segment2D[0], segment2D[0].GetMoved(vector2D.GetNegated()));
                    if (tuples.Find(x => x.Item4 != null && x.Item4.Count != 0 && x.Item4[0].On(segment2D_Temp[0]) && x.Item4[0].On(segment2D_Temp[1])) == null)
                        tuple.Item4.Add(segment2D_Temp);

                    segment2Ds.AddRange(tuple.Item4);
                }

                //Spliting Segments and removing short ends
                segment2Ds = segment2Ds.Split(tolerance);
                if (segment2Ds != null && segment2Ds.Count != 0)
                {
                    List<Segment2D> segment2Ds_ToBeRemoved = new List<Segment2D>();
                    do
                    {
                        if (segment2Ds_ToBeRemoved.Count > 0)
                            segment2Ds_ToBeRemoved.ForEach(x => segment2Ds.Remove(x));

                        segment2Ds_ToBeRemoved = new List<Segment2D>();

                        List<Point2D> point2Ds = segment2Ds.Point2Ds(tolerance);
                        if (point2Ds != null && point2Ds.Count != 0)
                        {
                            foreach (Point2D point2D in point2Ds)
                            {
                                List<Segment2D> segment2Ds_Temp = segment2Ds.FindAll(x => x[0].AlmostEquals(point2D, tolerance) || x[1].AlmostEquals(point2D, tolerance));
                                if (segment2Ds_Temp.Count != 1)
                                    continue;

                                Segment2D segment2D = segment2Ds_Temp[0];
                                if (segment2D == null || segment2D.GetLength() >= distance + tolerance)
                                    continue;

                                if (segment2Ds_Main.Find(x => x.Similar(segment2D, tolerance)) != null)
                                    continue;

                                segment2Ds_ToBeRemoved.Add(segment2Ds_Temp[0]);
                            }
                        }

                    } while (segment2Ds_ToBeRemoved.Count > 0);

                    //Reorganizing input data
                    foreach (Tuple<Panel, double, double, List<Segment2D>> tuple in tuples)
                    {
                        if (tuple.Item4 == null || tuple.Item4.Count == 0)
                            continue;

                        List<Segment2D> segment2Ds_Temp = new List<Segment2D>();
                        foreach (Segment2D segment2D in tuple.Item4)
                            segment2Ds_Temp.AddRange(segment2Ds.FindAll(x => segment2D.On(x.Mid())));

                        if (segment2Ds_Temp.Count == 0)
                        {
                            tuple.Item4.Clear();
                            updated = true;
                            continue;
                        }

                        segment2Ds_Temp.ForEach(x => segment2Ds.Remove(x));

                        List<Point2D> point2Ds = segment2Ds_Temp.Point2Ds(tolerance);

                        Point2D point2D_1 = null;
                        Point2D point2D_2 = null;
                        point2Ds.ExtremePoints(out point2D_1, out point2D_2);
                        if (point2D_1 == null || point2D_2 == null || point2D_1.AlmostEquals(point2D_2, tolerance))
                        {
                            tuple.Item4.Clear();
                            updated = true;
                            continue;
                        }

                        Segment2D segmnet2D_Old = tuple.Item4[0];
                        Segment2D segmnet2D_New = new Segment2D(point2D_1, point2D_2);

                        if (!segmnet2D_Old.Direction.SameHalf(segmnet2D_New.Direction))
                            segmnet2D_New.Reverse();

                        tuple.Item4.Clear();

                        if (segmnet2D_New.AlmostSimilar(segmnet2D_Old, tolerance))
                        {
                            tuple.Item4.Add(segmnet2D_Old);
                            continue;
                        }

                        tuple.Item4.Add(segmnet2D_New);
                        updated = true;
                    }
                }

            } while (updated);

            List<Guid> result = new List<Guid>();
            if (tuples == null || tuples.Count == 0 || dictionary == null || dictionary.Count == 0)
                return result;

            //List<Tuple<Point2D, List<Segment2D>>> tuples_Connections = new List<Tuple<Point2D, List<Segment2D>>>();
            //List<Segment2D> segment2Ds_Old = dictionary.Values.ToList();
            //for(int i=0; i < segment2Ds_Old.Count; i++)
            //{
            //    Segment2D segment2D_1 = segment2Ds_Old[i];
            //    List<Point2D> point2Ds_1 = segment2D_1.GetPoints();

            //    for (int j= i + 1; j < segment2Ds_Old.Count; j++)
            //    {
            //        Segment2D segment2D_2 = segment2Ds_Old[j];
            //        List<Point2D> point2Ds_2 = segment2D_2.GetPoints();

            //        List<Point2D> point2Ds = new List<Point2D>();
            //        point2Ds_1.FindAll(x => segment2D_2.On(x, tolerance)).ForEach(x => Geometry.Planar.Modify.Add(point2Ds, x, tolerance));
            //        point2Ds_2.FindAll(x => segment2D_1.On(x, tolerance)).ForEach(x => Geometry.Planar.Modify.Add(point2Ds, x, tolerance));

            //        foreach (Point2D point2D in point2Ds)
            //        {
            //            Tuple<Point2D, List<Segment2D>> tuple = tuples_Connections.Find(x => x.Item1.AlmostEquals(point2D, tolerance));
            //            if (tuple == null)
            //            {
            //                tuple = new Tuple<Point2D, List<Segment2D>>(point2D, new List<Segment2D>());
            //                tuples_Connections.Add(tuple);
            //            }

            //            if (tuple.Item2.Find(x => x.AlmostSimilar(segment2D_2)) == null)
            //                tuple.Item2.Add(segment2D_2);

            //            if (tuple.Item2.Find(x => x.AlmostSimilar(segment2D_1)) == null)
            //                tuple.Item2.Add(segment2D_1);
            //        }
            //    }
            //}


            foreach (Tuple<Panel, double, double, List<Segment2D>> tuple in tuples)
            {
                List<Segment2D> segment2Ds = tuple.Item4;
                if (segment2Ds == null || segment2Ds.Count == 0)
                {
                    panels.Remove(tuple.Item1);
                    result.Add(tuple.Item1.Guid);
                    continue;
                }

                Segment2D segment2D_Old = dictionary[tuple.Item1];
                Segment2D segment2D_New = tuple.Item4[0];
                //if (!segment2D_Old.Direction.SameHalf(segment2D_New.Direction))
                //    segment2D_New.Reverse();

                //Tuple<Point2D, List<Segment2D>> tuple_1 = tuples_Connections.Find(x => x.Item1.AlmostEquals(segment2D_Old[0], tolerance));
                //if(tuple_1 != null && tuple_1.Item2 != null && tuple_1.Item2.Count > 1)
                //    segment2D_New = new Segment2D(segment2D_Old[0], segment2D_New[1]);

                //Tuple<Point2D, List<Segment2D>> tuple_2 = tuples_Connections.Find(x => x.Item1.AlmostEquals(segment2D_Old[1], tolerance));
                //if (tuple_2 != null && tuple_2.Item2 != null && tuple_2.Item2.Count > 1)
                //    segment2D_New = new Segment2D(segment2D_New[0], segment2D_Old[1]);

                if (segment2D_Old.AlmostSimilar(segment2D_New, tolerance))
                    continue;

                Segment3D segment3D = (Segment3D)plane.Convert(segment2D_New).GetMoved(new Vector3D(0, 0, tuple.Item2));

                Panel panel_Old = tuple.Item1;
                Panel panel_New = Create.Panel(panel_Old, segment3D, tuple.Item3 - tuple.Item2);
                if (panel_New == null)
                    continue;

                int index = panels.IndexOf(panel_Old);
                panels[index] = panel_New;
                result.Add(panel_New.Guid);
            }

            return result;
        }

        public static List<Guid> Join(this List<Panel> panels, double distance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            Plane plane = Plane.WorldXY;

            //Collecting data -> Item1 - Panel, Item2 -> Min Elevation, Item3 - Max Elevation, Item4 - Location of panel as Segment2D on plane
            List<Tuple<Panel, double, double, Segment2D>> tuples_All = new List<Tuple<Panel, double, double, Segment2D>>();
            Dictionary<Panel, Segment2D> dictionary = new Dictionary<Panel, Segment2D>();
            foreach (Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if (face3D == null)
                    continue;

                if (!Geometry.Spatial.Query.Vertical(face3D.GetPlane(), tolerance))
                    continue;

                if (!face3D.Rectangular())
                    continue;

                ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D == null)
                    continue;

                List<Point3D> point3Ds = segmentable3D.GetPoints();
                if (point3Ds == null || point3Ds.Count == 0)
                    continue;

                BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                Plane plane_Temp = new Plane(plane, plane.Origin.GetMoved(new Vector3D(0, 0, boundingBox3D.Min.Z)) as Point3D);

                List<Point2D> point2Ds = point3Ds.ConvertAll(x => plane_Temp.Convert(plane_Temp.Project(x)));

                Point2D point2D_1 = null;
                Point2D point2D_2 = null;
                Geometry.Planar.Query.ExtremePoints(point2Ds, out point2D_1, out point2D_2);
                if (point2D_1 == null || point2D_2 == null || point2D_1.AlmostEquals(point2D_2, tolerance))
                    continue;

                Segment2D segment2D = new Segment2D(point2D_1, point2D_2);
                dictionary[panel] = segment2D;
                tuples_All.Add(new Tuple<Panel, double, double, Segment2D>(panel, boundingBox3D.Min.Z, boundingBox3D.Max.Z, segment2D));
            }

            //Looping through all panels data till panels cannot be join anymore. Every change will stop current iteration and start from beginning
            Join(tuples_All, true, distance, tolerance); //Extend
            Join(tuples_All, false, distance, tolerance); //Trim

            bool updated = false;
            //do
            //{
            //    updated = false;

            //    //Filtering panels have been removed (tuple which segment2D is null)
            //    List<Tuple<Panel, double, double, Segment2D>> tuples = tuples_All.FindAll(x => x.Item4 != null && x.Item4.GetLength() > tolerance);

            //    foreach (Tuple<Panel, double, double, Segment2D> tuple in tuples)
            //    {
            //        Segment2D segment2D = tuple.Item4;

            //        List<Tuple<Panel, double, double, Segment2D>> tuples_Overlap = null;

            //        //Removing overlaps for segment2D
            //        tuples_Overlap = tuples.FindAll(x => x.Item4.On(segment2D[0]) && x.Item4.On(segment2D[1]));
            //        if (tuples_Overlap.Count > 1)
            //        {
            //            Tuple<Panel, double, double, Segment2D> tuple_New = new Tuple<Panel, double, double, Segment2D>(tuple.Item1, tuple.Item2, tuple.Item3, null);
            //            tuples_All[tuples_All.IndexOf(tuple)] = tuple_New;
            //            updated = true;
            //            break;
            //        }

            //        //Removing overlaps for segment2D
            //        tuples_Overlap = tuples.FindAll(x => segment2D.On(x.Item4[0]) && segment2D.On(x.Item4[1]));
            //        if (tuples_Overlap.Count > 1)
            //        {
            //            foreach (Tuple<Panel, double, double, Segment2D> tuple_Overlap in tuples_Overlap)
            //            {
            //                Tuple<Panel, double, double, Segment2D> tuple_New = new Tuple<Panel, double, double, Segment2D>(tuple_Overlap.Item1, tuple_Overlap.Item2, tuple_Overlap.Item3, null);
            //                tuples_All[tuples_All.IndexOf(tuple_Overlap)] = tuple_New;
            //            }

            //            updated = true;
            //            break; ;
            //        }

            //        //Iterating through each end point of segment2D
            //        foreach (Point2D point2D in segment2D.GetPoints())
            //        {
            //            //Checking if point is connected with other segments (panels). If connected then skip
            //            List<Tuple<Panel, double, double, Segment2D>> tuples_Temp = tuples.FindAll(x => x.Item4.On(point2D, tolerance));
            //            if (tuples_Temp == null || tuples_Temp.Count != 1)
            //                continue;

            //            //Looking for all segments which are not further than provided distance
            //            tuples_Temp = tuples.FindAll(x => x.Item4.Distance(point2D) < distance + tolerance);
            //            if (tuples_Temp == null || tuples_Temp.Count < 2)
            //                continue;

            //            tuples_Temp.Remove(tuple);

            //            //Looking for intersection points
            //            List<Tuple<Segment2D, Point2D>> tuples_Segment2D_Intersection = new List<Tuple<Segment2D, Point2D>>();
            //            foreach (Tuple<Panel, double, double, Segment2D> tuple_Temp in tuples_Temp)
            //            {
            //                Segment2D segment2D_Temp = tuple_Temp.Item4;

            //                Point2D point2D_Segment2D_Intersection = segment2D.Intersection(segment2D_Temp, false, tolerance);
            //                if (point2D_Segment2D_Intersection != null)
            //                {
            //                    if (point2D_Segment2D_Intersection.Distance(point2D) <= distance)
            //                        tuples_Segment2D_Intersection.Add(new Tuple<Segment2D, Point2D>(segment2D_Temp, point2D_Segment2D_Intersection));
            //                    continue;
            //                }

            //                point2D_Segment2D_Intersection = segment2D_Temp.ClosestEnd(point2D);
            //                Vector2D vector2D = new Vector2D(point2D, point2D_Segment2D_Intersection).Unit;
            //                if (!(vector2D.AlmostEqual(segment2D.Direction) || vector2D.AlmostEqual(segment2D.Direction.GetNegated())))
            //                    continue;

            //                tuples_Segment2D_Intersection.Add(new Tuple<Segment2D, Point2D>(segment2D_Temp, point2D_Segment2D_Intersection));
            //            }

            //            if (tuples_Segment2D_Intersection.Count == 0)
            //                continue;

            //            tuples_Segment2D_Intersection.Sort((x, y) => x.Item2.Distance(point2D).CompareTo(y.Item2.Distance(point2D)));

            //            Tuple<Segment2D, Point2D> tuple_Segment2D_Intersection = tuples_Segment2D_Intersection.First();

            //            Tuple<Panel, double, double, Segment2D> tuple_Intersection = tuples_Temp.Find(x => x.Item4 == tuple_Segment2D_Intersection.Item1);
            //            if (tuple_Intersection == null)
            //                continue;

            //            //Adjusting segments to intersection point has been found
            //            Segment2D segment2D_Intersection = tuple_Segment2D_Intersection.Item1;
            //            Point2D point2D_Intersection = tuple_Segment2D_Intersection.Item2;

            //            List<Point2D> point2Ds = null;
            //            Point2D point2D_1 = null;
            //            Point2D point2D_2 = null;

            //            point2Ds = segment2D.GetPoints();
            //            point2Ds.Add(point2D_Intersection);

            //            point2Ds.ExtremePoints(out point2D_1, out point2D_2);
            //            if (point2D_1 != null && point2D_2 != null && !point2D_1.AlmostEquals(point2D_2, tolerance))
            //            {
            //                Segment2D segment2D_New = new Segment2D(point2D_1, point2D_2);
            //                if (!segment2D_New.Direction.SameHalf(segment2D.Direction))
            //                    segment2D_New.Reverse();

            //                if (!segment2D_New.AlmostSimilar(segment2D))
            //                {
            //                    Tuple<Panel, double, double, Segment2D> tuple_New = new Tuple<Panel, double, double, Segment2D>(tuple.Item1, tuple.Item2, tuple.Item3, segment2D_New);
            //                    tuples_All[tuples_All.IndexOf(tuple)] = tuple_New;
            //                    updated = true;
            //                }
            //            }

            //            if (!segment2D.On(point2D_Intersection))
            //            {
            //                point2Ds = segment2D_Intersection.GetPoints();
            //                point2Ds.Add(point2D_Intersection);

            //                point2Ds.ExtremePoints(out point2D_1, out point2D_2);
            //                if (point2D_1 != null && point2D_2 != null && !point2D_1.AlmostEquals(point2D_2, tolerance))
            //                {
            //                    Segment2D segment2D_New = new Segment2D(point2D_1, point2D_2);
            //                    if (!segment2D_New.Direction.SameHalf(segment2D_Intersection.Direction))
            //                        segment2D_New.Reverse();

            //                    if (!segment2D_New.AlmostSimilar(segment2D_Intersection))
            //                    {
            //                        Tuple<Panel, double, double, Segment2D> tuple_New = new Tuple<Panel, double, double, Segment2D>(tuple_Intersection.Item1, tuple_Intersection.Item2, tuple_Intersection.Item3, segment2D_New);
            //                        tuples_All[tuples_All.IndexOf(tuple_Intersection)] = tuple_New;
            //                        updated = true;
            //                    }
            //                }
            //            }

            //            if (updated)
            //                break;
            //        }

            //        if (updated)
            //            break;
            //    }

            //} while (updated);

            //Removing short, not connected panels
            updated = false;
            do
            {
                updated = false;
                
                //Filtering panels have been removed (tuple which segment2D is null)
                List<Tuple<Panel, double, double, Segment2D>> tuples = tuples_All.FindAll(x => x.Item4 != null && x.Item4.GetLength() > tolerance);

                List<Segment2D> segment2Ds = tuples.ConvertAll(x => x.Item4);
                segment2Ds = segment2Ds.Split(tolerance);
                
                foreach (Tuple<Panel, double, double, Segment2D> tuple in tuples)
                {
                    Segment2D segment2D = tuple.Item4;

                    List<Segment2D> segment2Ds_Split = segment2Ds.FindAll(x => segment2D.On(x.Mid(), tolerance));
                    if (segment2Ds_Split == null || segment2Ds_Split.Count == 0)
                        continue;

                    List<Point2D> point2Ds = segment2Ds_Split.Point2Ds(tolerance);
                    if (point2Ds.Count < 2)
                        continue;

                    if (point2Ds.Count == 2 && segment2D.GetLength() >= distance)
                        continue;

                    //int i = -1;
                    //Iterating through each end point of segment2D
                    foreach (Point2D point2D in segment2D.GetPoints())
                    {
                        //i++;
                        point2Ds.SortByDistance(point2D);

                        if(point2Ds[1].Distance(point2D) >= distance)
                            continue;

                        //Checking if point is connected with other segments (panels). If connected then skip
                        List<Tuple<Panel, double, double, Segment2D>> tuples_Temp = tuples.FindAll(x => x.Item4.On(point2D, tolerance));
                        if (tuples_Temp == null || tuples_Temp.Count != 1)
                            continue;

                        Segment2D segment2D_New = new Segment2D(point2Ds[1], point2Ds.Last());

                        double length_New = segment2D_New.GetLength();
                        if (length_New <= tolerance)
                            continue;

                        if (!segment2D_New.Direction.SameHalf(segment2D.Direction))
                            segment2D_New.Reverse();

                        if (segment2D_New.AlmostSimilar(segment2D))
                            continue;

                        //Segment2D segment2D_New = null;
                        //if (i == 0)
                        //    segment2D_New = new Segment2D(point2Ds[1], segment2D[1]);
                        //else
                        //    segment2D_New = new Segment2D(segment2D[0], point2Ds[1]);

                        //if (segment2D_New.GetLength() > tolerance)
                        //{
                        //    if (!segment2D_New.Direction.SameHalf(segment2D.Direction))
                        //        segment2D_New.Reverse();
                        //}
                        //else
                        //{
                        //    segment2D_New = null;
                        //}

                        Tuple<Panel, double, double, Segment2D> tuple_New = new Tuple<Panel, double, double, Segment2D>(tuple.Item1, tuple.Item2, tuple.Item3, segment2D_New);
                        tuples_All[tuples_All.IndexOf(tuple)] = tuple_New;

                        updated = true;
                        break;
                    }

                    if (updated)
                        break;
                }

            } while (updated);


            //Preparing output data
            List<Guid> result = new List<Guid>();
            foreach (Tuple<Panel, double, double, Segment2D> tuple in tuples_All)
            {
                Segment2D segment2D_New = tuple.Item4;
                if (segment2D_New == null)
                {
                    panels.Remove(tuple.Item1);
                    result.Add(tuple.Item1.Guid);
                    continue;
                }

                Segment2D segment2D_Old = dictionary[tuple.Item1];
                if (segment2D_Old.AlmostSimilar(segment2D_New, tolerance))
                    continue;

                Segment3D segment3D = (Segment3D)plane.Convert(segment2D_New).GetMoved(new Vector3D(0, 0, tuple.Item2));

                Panel panel_Old = tuple.Item1;
                Panel panel_New = Create.Panel(panel_Old, segment3D, tuple.Item3 - tuple.Item2);
                if (panel_New == null)
                    continue;

                int index = panels.IndexOf(panel_Old);
                panels[index] = panel_New;
                result.Add(panel_New.Guid);
            }

            return result;
        }

        private static void Join(this List<Tuple<Panel, double, double, Segment2D>> tuples_All, bool extend, double distance, double tolerance = Core.Tolerance.Distance)
        {
            bool updated = false;
            do
            {
                updated = false;

                //Filtering panels have been removed (tuple which segment2D is null)
                List<Tuple<Panel, double, double, Segment2D>> tuples = tuples_All.FindAll(x => x.Item4 != null && x.Item4.GetLength() > tolerance);

                foreach (Tuple<Panel, double, double, Segment2D> tuple in tuples)
                {
                    Segment2D segment2D = tuple.Item4;

                    List<Tuple<Panel, double, double, Segment2D>> tuples_Overlap = null;

                    //Removing overlaps for segment2D
                    tuples_Overlap = tuples.FindAll(x => x.Item4.On(segment2D[0]) && x.Item4.On(segment2D[1]));
                    if (tuples_Overlap.Count > 1)
                    {
                        Tuple<Panel, double, double, Segment2D> tuple_New = new Tuple<Panel, double, double, Segment2D>(tuple.Item1, tuple.Item2, tuple.Item3, null);
                        tuples_All[tuples_All.IndexOf(tuple)] = tuple_New;
                        updated = true;
                        break;
                    }

                    //Removing overlaps for segment2D
                    tuples_Overlap = tuples.FindAll(x => segment2D.On(x.Item4[0]) && segment2D.On(x.Item4[1]));
                    if (tuples_Overlap.Count > 1)
                    {
                        foreach (Tuple<Panel, double, double, Segment2D> tuple_Overlap in tuples_Overlap)
                        {
                            Tuple<Panel, double, double, Segment2D> tuple_New = new Tuple<Panel, double, double, Segment2D>(tuple_Overlap.Item1, tuple_Overlap.Item2, tuple_Overlap.Item3, null);
                            tuples_All[tuples_All.IndexOf(tuple_Overlap)] = tuple_New;
                        }

                        updated = true;
                        break; ;
                    }

                    //Iterating through each end point of segment2D
                    foreach (Point2D point2D in segment2D.GetPoints())
                    {
                        //Checking if point is connected with other segments (panels). If connected then skip
                        List<Tuple<Panel, double, double, Segment2D>> tuples_Temp = tuples.FindAll(x => x.Item4.On(point2D, tolerance));
                        if (tuples_Temp == null || tuples_Temp.Count != 1)
                            continue;

                        //Looking for all segments which are not further than provided distance
                        tuples_Temp = tuples.FindAll(x => x.Item4.Distance(point2D) < distance + tolerance);
                        if (tuples_Temp == null || tuples_Temp.Count < 2)
                            continue;

                        tuples_Temp.Remove(tuple);

                        //Looking for intersection points
                        List<Tuple<Segment2D, Point2D>> tuples_Segment2D_Intersection = new List<Tuple<Segment2D, Point2D>>();
                        foreach (Tuple<Panel, double, double, Segment2D> tuple_Temp in tuples_Temp)
                        {
                            Segment2D segment2D_Temp = tuple_Temp.Item4;

                            Point2D point2D_Segment2D_Intersection = segment2D.Intersection(segment2D_Temp, false, tolerance);
                            if (point2D_Segment2D_Intersection != null)
                            {
                                if (point2D_Segment2D_Intersection.Distance(point2D) <= distance)
                                    tuples_Segment2D_Intersection.Add(new Tuple<Segment2D, Point2D>(segment2D_Temp, point2D_Segment2D_Intersection));
                                continue;
                            }

                            point2D_Segment2D_Intersection = segment2D_Temp.ClosestEnd(point2D);
                            Vector2D vector2D = new Vector2D(point2D, point2D_Segment2D_Intersection).Unit;
                            if (!(vector2D.AlmostEqual(segment2D.Direction) || vector2D.AlmostEqual(segment2D.Direction.GetNegated())))
                                continue;

                            tuples_Segment2D_Intersection.Add(new Tuple<Segment2D, Point2D>(segment2D_Temp, point2D_Segment2D_Intersection));
                        }

                        if (tuples_Segment2D_Intersection.Count == 0)
                            continue;

                        tuples_Segment2D_Intersection.Sort((x, y) => x.Item2.Distance(point2D).CompareTo(y.Item2.Distance(point2D)));

                        Tuple<Segment2D, Point2D> tuple_Segment2D_Intersection = tuples_Segment2D_Intersection.First();

                        Tuple<Panel, double, double, Segment2D> tuple_Intersection = tuples_Temp.Find(x => x.Item4 == tuple_Segment2D_Intersection.Item1);
                        if (tuple_Intersection == null)
                            continue;

                        //Adjusting segments to intersection point has been found
                        Point2D point2D_Intersection = tuple_Segment2D_Intersection.Item2;

                        List<Point2D> point2Ds = null;
                        Point2D point2D_1 = null;
                        Point2D point2D_2 = null;

                        point2Ds = segment2D.GetPoints();
                        point2Ds.Add(point2D_Intersection);

                        point2Ds.ExtremePoints(out point2D_1, out point2D_2);
                        if (point2D_1 != null && point2D_2 != null && !point2D_1.AlmostEquals(point2D_2, tolerance))
                        {
                            Segment2D segment2D_New = new Segment2D(point2D_1, point2D_2);
                            if (!segment2D_New.Direction.SameHalf(segment2D.Direction))
                                segment2D_New.Reverse();

                            if (!segment2D_New.AlmostSimilar(segment2D))
                            {
                                bool extended = segment2D_New.GetLength() > segment2D.GetLength();

                                if (extend == extended)
                                {
                                    Tuple<Panel, double, double, Segment2D> tuple_New = new Tuple<Panel, double, double, Segment2D>(tuple.Item1, tuple.Item2, tuple.Item3, segment2D_New);
                                    tuples_All[tuples_All.IndexOf(tuple)] = tuple_New;
                                    updated = true;
                                }
                            }
                        }

                        if (!segment2D.On(point2D_Intersection))
                        {
                            Segment2D segment2D_Intersection = tuple_Segment2D_Intersection.Item1;

                            point2Ds = segment2D_Intersection.GetPoints();
                            point2Ds.Add(point2D_Intersection);

                            point2Ds.ExtremePoints(out point2D_1, out point2D_2);
                            if (point2D_1 != null && point2D_2 != null && !point2D_1.AlmostEquals(point2D_2, tolerance))
                            {
                                Segment2D segment2D_New = new Segment2D(point2D_1, point2D_2);
                                if (!segment2D_New.Direction.SameHalf(segment2D_Intersection.Direction))
                                    segment2D_New.Reverse();

                                if (!segment2D_New.AlmostSimilar(segment2D_Intersection))
                                {
                                    bool extended = segment2D_New.GetLength() > segment2D_Intersection.GetLength();

                                    if (extend == extended)
                                    {
                                        Tuple<Panel, double, double, Segment2D> tuple_New = new Tuple<Panel, double, double, Segment2D>(tuple_Intersection.Item1, tuple_Intersection.Item2, tuple_Intersection.Item3, segment2D_New);
                                        tuples_All[tuples_All.IndexOf(tuple_Intersection)] = tuple_New;
                                        updated = true;
                                    }
                                }
                            }
                        }

                        if (updated)
                            break;
                    }

                    if (updated)
                        break;
                }

            } while (updated);
        }

    }
}