using System.Collections.Generic;
using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> SnapByElevations(this IEnumerable<Panel> panels, IEnumerable<double> elevations, int maxIterations = 1, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            if(panels == null)
            {
                return null;
            }

            List<Panel> panels_Rectangular = new List<Panel>();
            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }

                if(panel.Rectangular(maxTolerance))
                {
                    panels_Rectangular.Add(panel);
                }
                else
                {
                    result.Add(new Panel(panel));
                }
            }

            if(panels_Rectangular.Count != 0)
            {
                int count = -1;
                for (int i = 0; i < maxIterations; i++)
                {
                    Dictionary<Point3D, List<Panel>> dictionary = null;

                    if (count == -1 && maxIterations > 1)
                    {
                        dictionary = SpacingDictionary(panels_Rectangular, maxTolerance, minTolerance);
                        if (dictionary == null || dictionary.Count == 0)
                        {
                            break;
                        }

                        count = dictionary.Count;
                    }

                    List<Panel> panels_Rectangular_Temp = panels_Rectangular.ConvertAll(x => new Panel(x));

                    foreach (double elevation in elevations)
                    {
                        double[] elevations_Temp = new double[] { elevation };

                        panels_Rectangular = SnapByElevations_Ends(panels_Rectangular, elevations_Temp, maxTolerance, minTolerance);
                        panels_Rectangular = SnapByElevations_Intersections(panels_Rectangular, elevations_Temp, maxTolerance, minTolerance);
                    }

                    panels_Rectangular = SnapByElevations_Ends(panels_Rectangular, elevations, maxTolerance, minTolerance);
                    panels_Rectangular = SnapByElevations_Intersections(panels_Rectangular, elevations, maxTolerance, minTolerance);

                    if(count != -1)
                    {
                        dictionary = SpacingDictionary(panels_Rectangular, maxTolerance, minTolerance);
                        if (dictionary == null || dictionary.Count == 0)
                        {
                            break;
                        }

                        int count_Temp = dictionary.Count;
                        if(count_Temp > count)
                        {
                            panels_Rectangular = panels_Rectangular_Temp;
                            break;
                        }
                        else if(count_Temp == count)
                        {
                            break;
                        }

                        count = count_Temp;
                    }
                }

                if (panels_Rectangular != null && panels_Rectangular.Count != 0)
                {
                    result.AddRange(panels_Rectangular);
                }
            }

            return result;
        }

        private static List<Panel> SnapByElevations_Ends(this IEnumerable<Panel> panels, IEnumerable<double> elevations, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            if (panels == null || elevations == null)
            {
                return null;
            }

            Dictionary<Panel, List<ISegmentable2D>> dictionary = new Dictionary<Panel, List<ISegmentable2D>>();
            foreach(double elevation in elevations)
            {
                Plane plane = Plane.WorldXY.GetMoved(Vector3D.WorldZ * elevation) as Plane;

                Dictionary<Panel, List<ISegmentable2D>> dictionary_Temp = panels.SectionDictionary<ISegmentable2D>(plane, maxTolerance);
                if (dictionary_Temp == null)
                {
                    continue;
                }

                foreach (KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary_Temp)
                {
                    if(dictionary.ContainsKey(keyValuePair.Key))
                    {
                        continue;
                    }

                    dictionary[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            List<Tuple<Panel, List<Segment2D>>> tuples = new List<Tuple<Panel, List<Segment2D>>>();
            List<Tuple<Point2D, Segment2D, bool, BoundingBox2D>> tuples_Point2D = new List<Tuple<Point2D, Segment2D, bool, BoundingBox2D>>();
            foreach (KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                if (keyValuePair.Key == null || keyValuePair.Value == null || keyValuePair.Value.Count == 0)
                {
                    continue;
                }

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                    if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    segment2Ds.AddRange(segment2Ds_Temp);
                }

                foreach (Segment2D segment2D in segment2Ds)
                {
                    BoundingBox2D boundingBox2D = segment2D.GetBoundingBox();

                    tuples_Point2D.Add(new Tuple<Point2D, Segment2D, bool, BoundingBox2D>(segment2D[0], segment2D, true, boundingBox2D));
                    tuples_Point2D.Add(new Tuple<Point2D, Segment2D, bool, BoundingBox2D>(segment2D[1], segment2D, false, boundingBox2D));
                }

                tuples.Add(new Tuple<Panel, List<Segment2D>>(keyValuePair.Key, segment2Ds));
            }

            List<Tuple<Point2D, Segment2D, bool>> tuples_Point2D_Snap = new List<Tuple<Point2D, Segment2D, bool>>();
            while (tuples_Point2D.Count > 0)
            {
                List<Tuple<Point2D, Segment2D, bool, BoundingBox2D>> tuples_Point2D_Temp = tuples_Point2D.FindAll(x => x.Item4.InRange(tuples_Point2D[0].Item1, maxTolerance));
                tuples_Point2D_Temp = tuples_Point2D_Temp.FindAll(x => x.Item1.Distance(tuples_Point2D[0].Item1) <= maxTolerance);
                tuples_Point2D_Temp.ForEach(x => tuples_Point2D.Remove(x));
                if (tuples_Point2D_Temp.Count < 2)
                {
                    continue;
                }

                bool equals = true;
                for (int i = 1; i < tuples_Point2D_Temp.Count; i++)
                {
                    if (!tuples_Point2D_Temp[0].Item1.AlmostEquals(tuples_Point2D_Temp[i].Item1, minTolerance))
                    {
                        equals = false;
                        break;
                    }
                }

                if (equals)
                {
                    continue;
                }

                List<Point2D> point2Ds = tuples_Point2D_Temp.ConvertAll(x => x.Item1);
                point2Ds.Add(point2Ds.Average());

                List<Tuple<Point2D, double>> tuples_Weight = new List<Tuple<Point2D, double>>();
                foreach (Point2D point2D in point2Ds)
                {
                    List<Tuple<Segment2D, double>> tuples_Segment2D_Weight = new List<Tuple<Segment2D, double>>();

                    foreach (Tuple<Point2D, Segment2D, bool, BoundingBox2D> tuple in tuples_Point2D_Temp)
                    {
                        Segment2D segment2D = null;
                        if (tuple.Item3)
                        {
                            segment2D = new Segment2D(point2D, tuple.Item2[1]);
                        }
                        else
                        {
                            segment2D = new Segment2D(tuple.Item2[0], point2D);
                        }

                        tuples_Segment2D_Weight.Add(new Tuple<Segment2D, double>(segment2D, segment2D.GetLength() * segment2D.Direction.SmallestAngle(tuple.Item2.Direction)));
                    }

                    tuples_Weight.Add(new Tuple<Point2D, double>(point2D, tuples_Segment2D_Weight.ConvertAll(x => x.Item2).Sum()));
                }

                tuples_Weight.Sort((x, y) => x.Item2.CompareTo(y.Item2));

                Point2D point2D_Snap = tuples_Weight[0].Item1;

                foreach (Tuple<Point2D, Segment2D, bool, BoundingBox2D> tuple in tuples_Point2D_Temp)
                {
                    tuples_Point2D_Snap.Add(new Tuple<Point2D, Segment2D, bool>(point2D_Snap, tuple.Item2, tuple.Item3));
                }
            }

            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
            {
                if (!dictionary.ContainsKey(panel))
                {
                    result.Add(panel);
                }
            }

            foreach (Tuple<Panel, List<Segment2D>> tuple in tuples)
            {
                if(tuple.Item2 == null || tuple.Item2.Count == 0)
                {
                    result.Add(tuple.Item1);
                    continue;
                }
                
                List<Tuple<Point2D, Segment2D, bool>> tuples_Point2D_Snap_Panel = new List<Tuple<Point2D, Segment2D, bool>>();
                foreach (Segment2D segment2D in tuple.Item2)
                {
                    tuples_Point2D_Snap_Panel.Add(tuples_Point2D_Snap.Find(x => x.Item2 == segment2D));
                }

                if (tuples_Point2D_Snap_Panel.TrueForAll(x => x == null))
                {
                    result.Add(tuple.Item1);
                    continue;
                }

                for (int i = 0; i < tuple.Item2.Count; i++)
                {
                    Segment2D segment2D = tuple.Item2[i];
                    if (tuples_Point2D_Snap_Panel[i] != null)
                    {
                        if (tuples_Point2D_Snap_Panel[i].Item3)
                        {
                            segment2D = new Segment2D(tuples_Point2D_Snap_Panel[i].Item1, segment2D[1]);
                        }
                        else
                        {
                            segment2D = new Segment2D(segment2D[0], tuples_Point2D_Snap_Panel[i].Item1);
                        }
                    }

                    Guid guid = tuple.Item1.Guid;
                    if (result.Find(x => x.Guid == guid) != null)
                    {
                        guid = Guid.NewGuid();
                    }

                    BoundingBox3D boundingBox3D = tuple.Item1.GetBoundingBox();
                    Vector3D vector3D = Vector3D.WorldZ * boundingBox3D.Height;

                    Plane plane = Plane.WorldXY.GetMoved(Vector3D.WorldZ * boundingBox3D.Min.Z) as Plane;

                    Segment3D segment3D = plane.Convert(segment2D);

                    Face3D face3D = new Face3D(new Polygon3D(new Point3D[] { segment3D[0], segment3D[1], segment3D[1].GetMoved(vector3D) as Point3D, segment3D[0].GetMoved(vector3D) as Point3D }));

                    Panel panel = Create.Panel(guid, tuple.Item1, face3D, null, true, minTolerance, maxTolerance);
                    //Panel panel = Create.Panel(guid, tuple.Item1, new PlanarBoundary3D()));

                    result.Add(panel);
                }

            }

            return result;
        }

        private static List<Panel> SnapByElevations_Intersections(this IEnumerable<Panel> panels, IEnumerable<double> elevations, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            if (panels == null || elevations == null)
            {
                return null;
            }

            Dictionary<Panel, List<ISegmentable2D>> dictionary = new Dictionary<Panel, List<ISegmentable2D>>();
            foreach (double elevation in elevations)
            {
                Plane plane = Plane.WorldXY.GetMoved(Vector3D.WorldZ * elevation) as Plane;

                Dictionary<Panel, List<ISegmentable2D>> dictionary_Temp = panels.SectionDictionary<ISegmentable2D>(plane, maxTolerance);
                if (dictionary_Temp == null)
                {
                    continue;
                }

                foreach (KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary_Temp)
                {
                    if (dictionary.ContainsKey(keyValuePair.Key))
                    {
                        continue;
                    }

                    dictionary[keyValuePair.Key] = keyValuePair.Value;
                }
            }

            List<Tuple<Panel, List<Segment2D>>> tuples = new List<Tuple<Panel, List<Segment2D>>>();
            List<Tuple<Point2D, Segment2D, bool>> tuples_Point2D = new List<Tuple<Point2D, Segment2D, bool>>();
            List<Tuple<Segment2D, BoundingBox2D>> tuples_Segment2D = new List<Tuple<Segment2D, BoundingBox2D>>();
            foreach (KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                if (keyValuePair.Key == null || keyValuePair.Value == null || keyValuePair.Value.Count == 0)
                {
                    continue;
                }

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                    if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    segment2Ds.AddRange(segment2Ds_Temp);
                }

                foreach (Segment2D segment2D in segment2Ds)
                {
                    tuples_Point2D.Add(new Tuple<Point2D, Segment2D, bool>(segment2D[0], segment2D, true));
                    tuples_Point2D.Add(new Tuple<Point2D, Segment2D, bool>(segment2D[1], segment2D, false));
                }

                segment2Ds.ForEach(x => tuples_Segment2D.Add(new Tuple<Segment2D, BoundingBox2D>(x, x.GetBoundingBox())));
                tuples.Add(new Tuple<Panel, List<Segment2D>>(keyValuePair.Key, segment2Ds));
            }

            List<Tuple<Point2D, Segment2D, bool>> tuples_Point2D_Snap = new List<Tuple<Point2D, Segment2D, bool>>();
            foreach(Tuple<Point2D, Segment2D, bool> tuple in tuples_Point2D)
            {
                List<Segment2D> segment2Ds_Temp = new List<Segment2D>();
                foreach(Tuple<Segment2D, BoundingBox2D> tuple_Segment2D in tuples_Segment2D)
                {
                    if(!tuple_Segment2D.Item2.InRange(tuple.Item1, maxTolerance))
                    {
                        continue;
                    }

                    double distance = tuple_Segment2D.Item1.Distance(tuple.Item1);
                    if(distance <= maxTolerance && distance>= minTolerance)
                    {
                        segment2Ds_Temp.Add(tuple_Segment2D.Item1);
                    }
                }

                segment2Ds_Temp.Remove(tuple.Item2);
                if(segment2Ds_Temp.Count < 1)
                {
                    continue;
                }

                Vector2D direction = tuple.Item3 ? direction = tuple.Item2.Direction.GetNegated() : tuple.Item2.Direction;

                List<Point2D> point2D_Intersections = Geometry.Planar.Query.Intersections(tuple.Item1, direction, segment2Ds_Temp, false, true, true, true, minTolerance);
                if(point2D_Intersections == null || point2D_Intersections.Count == 0)
                {
                    continue;
                }

                tuples_Point2D_Snap.Add(new Tuple<Point2D, Segment2D, bool>(point2D_Intersections[0], tuple.Item2, tuple.Item3));
            }

            List<Panel> result = new List<Panel>();
            foreach(Panel panel in panels)
            {
                if(!dictionary.ContainsKey(panel))
                {
                    result.Add(panel);
                }
            }

            foreach (Tuple<Panel, List<Segment2D>> tuple in tuples)
            {
                if (tuple.Item2 == null || tuple.Item2.Count == 0)
                {
                    result.Add(tuple.Item1);
                    continue;
                }

                List<Tuple<Point2D, Segment2D, bool>> tuples_Point2D_Snap_Panel = new List<Tuple<Point2D, Segment2D, bool>>();
                foreach (Segment2D segment2D in tuple.Item2)
                {
                    tuples_Point2D_Snap_Panel.Add(tuples_Point2D_Snap.Find(x => x.Item2 == segment2D));
                }

                if (tuples_Point2D_Snap_Panel.TrueForAll(x => x == null))
                {
                    result.Add(tuple.Item1);
                    continue;
                }

                for (int i = 0; i < tuple.Item2.Count; i++)
                {
                    Segment2D segment2D = tuple.Item2[i];
                    if (tuples_Point2D_Snap_Panel[i] != null)
                    {
                        if (tuples_Point2D_Snap_Panel[i].Item3)
                        {
                            segment2D = new Segment2D(tuples_Point2D_Snap_Panel[i].Item1, segment2D[1]);
                        }
                        else
                        {
                            segment2D = new Segment2D(segment2D[0], tuples_Point2D_Snap_Panel[i].Item1);
                        }
                    }

                    Guid guid = tuple.Item1.Guid;
                    if (result.Find(x => x.Guid == guid) != null)
                    {
                        guid = Guid.NewGuid();
                    }

                    BoundingBox3D boundingBox3D = tuple.Item1.GetBoundingBox();
                    Vector3D vector3D = Vector3D.WorldZ * boundingBox3D.Height;

                    Plane plane = Plane.WorldXY.GetMoved(Vector3D.WorldZ * boundingBox3D.Min.Z) as Plane;

                    Segment3D segment3D = plane.Convert(segment2D);

                    Face3D face3D = new Face3D(new Polygon3D(new Point3D[] { segment3D[0], segment3D[1], segment3D[1].GetMoved(vector3D) as Point3D, segment3D[0].GetMoved(vector3D) as Point3D }));

                    Panel panel = Create.Panel(guid, tuple.Item1, face3D, null, true, minTolerance, maxTolerance);

                    //Panel panel = Create.Panel(guid, tuple.Item1, new PlanarBoundary3D(new Polygon3D(new Point3D[] { segment3D[0], segment3D[1], segment3D[1].GetMoved(vector3D) as Point3D, segment3D[0].GetMoved(vector3D) as Point3D })));
                    result.Add(panel);
                }

            }

            return result;
        }
    }
}