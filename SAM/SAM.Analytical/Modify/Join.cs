using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using SAM.Geometry.Planar;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Guid> Join(this List<Panel> panels, double distance, double tolerance = Core.Tolerance.Distance, double angleTolerance = Core.Tolerance.Angle)
        {
            if (panels == null)
                return null;

            Plane plane = Plane.WorldXY;

            //Collecting data -> Item1 - Panel, Item2 -> Min Elevation, Item3 - Max Elevation, Item4 - Location of panel as Segment2D on plane
            List<Tuple<Panel, double, double, List<Segment2D>>> tuples = new List<Tuple<Panel, double, double, List<Segment2D>>>();
            Dictionary<Panel, Segment2D> dictionary = new Dictionary<Panel, Segment2D>();
            foreach(Panel panel in panels)
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
                    if(tuples.Find(x => x.Item4 != null && x.Item4.Count != 0 && x.Item4[0].On(segment2D_Temp[0]) && x.Item4[0].On(segment2D_Temp[1])) == null)
                        tuple.Item4.Add(segment2D_Temp);

                    segment2D_Temp = new Segment2D(segment2D[0], segment2D[0].GetMoved(vector2D.GetNegated()));
                    if (tuples.Find(x => x.Item4 != null && x.Item4.Count != 0 && x.Item4[0].On(segment2D_Temp[0]) && x.Item4[0].On(segment2D_Temp[1])) == null)
                        tuple.Item4.Add(segment2D_Temp);

                    segment2Ds.AddRange(tuple.Item4);
                }

                //Spliting Segments and removing short ends
                segment2Ds = segment2Ds.Split(tolerance);
                if(segment2Ds != null && segment2Ds.Count != 0)
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
                        foreach(Segment2D segment2D in tuple.Item4)
                            segment2Ds_Temp.AddRange(segment2Ds.FindAll(x => segment2D.On(x.Mid())));

                        if(segment2Ds_Temp.Count == 0)
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

            List<Tuple<Point2D, List<Segment2D>>> tuples_Connections = new List<Tuple<Point2D, List<Segment2D>>>();
            List<Segment2D> segment2Ds_Old = dictionary.Values.ToList();
            for(int i=0; i < segment2Ds_Old.Count; i++)
            {
                Segment2D segment2D_1 = segment2Ds_Old[i];
                List<Point2D> point2Ds_1 = segment2D_1.GetPoints();

                for (int j= i + 1; j < segment2Ds_Old.Count - 1; j++)
                {
                    Segment2D segment2D_2 = segment2Ds_Old[j];
                    List<Point2D> point2Ds_2 = segment2D_2.GetPoints();

                    foreach(Point2D point2D_1 in point2Ds_1)
                    {
                        if (point2Ds_2.Find(x => x.AlmostEquals(point2D_1)) == null)
                            continue;

                        Tuple<Point2D, List<Segment2D>> tuple = tuples_Connections.Find(x => x.Item1.AlmostEquals(point2D_1, tolerance));
                        if (tuple == null)
                        {
                            tuple = new Tuple<Point2D, List<Segment2D>>(point2D_1, new List<Segment2D>());
                            tuples_Connections.Add(tuple);
                        }

                        if (tuple.Item2.Find(x => x.AlmostSimilar(segment2D_2)) == null)
                            tuple.Item2.Add(segment2D_2);

                        if (tuple.Item2.Find(x => x.AlmostSimilar(segment2D_1)) == null)
                            tuple.Item2.Add(segment2D_1);
                    }
                }
            }

            
            foreach (Tuple<Panel, double, double, List<Segment2D>> tuple in tuples)
            {
                List<Segment2D> segment2Ds = tuple.Item4;
                if(segment2Ds == null || segment2Ds.Count == 0)
                {
                    panels.Remove(tuple.Item1);
                    result.Add(tuple.Item1.Guid);
                    continue;
                }

                Segment2D segment2D_Old = dictionary[tuple.Item1];
                Segment2D segment2D_New = tuple.Item4[0];
                if (!segment2D_Old.Direction.SameHalf(segment2D_New.Direction))
                    segment2D_New.Reverse();

                Tuple<Point2D, List<Segment2D>> tuple_1 = tuples_Connections.Find(x => x.Item1.AlmostEquals(segment2D_Old[0], tolerance));
                if(tuple_1 != null && tuple_1.Item2 != null && tuple_1.Item2.Count > 1)
                    segment2D_New[0] = segment2D_Old[0];

                Tuple<Point2D, List<Segment2D>> tuple_2 = tuples_Connections.Find(x => x.Item1.AlmostEquals(segment2D_Old[1], tolerance));
                if (tuple_2 != null && tuple_2.Item2 != null && tuple_2.Item2.Count > 1)
                    segment2D_New[1] = segment2D_Old[1];

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
    }
}