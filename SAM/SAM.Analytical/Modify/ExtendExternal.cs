using System;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using SAM.Geometry.Planar;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void ExtendExternal(this List<Panel> panels, double elevation, double maxDistance, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            ExtendExternal(panels, elevation, maxDistance, out List<Panel> externalPanels, out List<Panel> externalPanels_Extended, out List<Polygon3D> externalPolygon3Ds, snapTolerance, tolerance_Angle, tolerance_Distance);
        }

        public static void ExtendExternal(this List<Panel> panels, double elevation, double maxDistance, out List<Panel> externalPanels, out List<Panel> externalPanels_Extended, out List<Polygon3D> externalPolygon3Ds, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            externalPanels = null;
            externalPanels_Extended = null;
            externalPolygon3Ds = null;

            if (panels == null)
                return;

            Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

            Dictionary<Panel, List<ISegmentable2D>> dictionary = panels.SectionDictionary<ISegmentable2D>(plane, tolerance_Distance);

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    segment2Ds.AddRange(segmentable2D.GetSegments());
                }
            }

            segment2Ds = segment2Ds.Split(tolerance_Distance);

            List<Polygon2D> externalPolygon2Ds = Geometry.Planar.Query.ExternalPolygon2Ds(segment2Ds, maxDistance, tolerance_Distance);
            if (externalPolygon2Ds == null || externalPolygon2Ds.Count == 0)
                return;

            externalPolygon3Ds = externalPolygon2Ds.ConvertAll(x => plane.Convert(x));

            List<Segment2D> segment2Ds_Polygon2Ds = new List<Segment2D>();
            foreach (Polygon2D polygon2D in externalPolygon2Ds)
            {
                List<Segment2D> segment2Ds_Temp = polygon2D?.GetSegments();
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                {
                    continue;
                }

                segment2Ds_Polygon2Ds.AddRange(segment2Ds_Temp);
            }

            segment2Ds = new List<Segment2D>(segment2Ds_Polygon2Ds);

            externalPanels = new List<Panel>();
            foreach (KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                List<ISegmentable2D> segmentable2Ds_Temp = keyValuePair.Value;
                if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                {
                    continue;
                }

                bool external = false;
                foreach (ISegmentable2D segmentable2D in segmentable2Ds_Temp)
                {
                    List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                    if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach (Segment2D segment2D in segment2Ds_Temp)
                    {
                        BoundingBox2D boundingBox2D_Segment2D = segment2D.GetBoundingBox();
                        List<Polygon2D> polygon2Ds = externalPolygon2Ds.FindAll(x => x.GetBoundingBox().InRange(boundingBox2D_Segment2D, tolerance_Distance));

                        Polygon2D polygon2D = polygon2Ds.Find(x => x.On(segment2D[0], snapTolerance) || x.On(segment2D[0], snapTolerance) || x.On(segment2D.Mid(), snapTolerance));
                        if (polygon2D != null)
                        {
                            segment2Ds.Add(segment2D);
                        }
                        else
                        {
                            foreach(Polygon2D polygon2D_Temp in polygon2Ds)
                            {
                                Point2D point2D = polygon2D_Temp.Points.Find(x => boundingBox2D_Segment2D.InRange(x, snapTolerance) && segment2D.On(x, snapTolerance));
                                if(point2D != null)
                                {
                                    segment2Ds.Add(segment2D);
                                    break;
                                }
                            }
                        }

                        if(!external)
                        {
                            BoundingBox2D boundingBox2D = segment2D.GetBoundingBox();
                            foreach(Segment2D segment2D_Polygon2D in segment2Ds_Polygon2Ds)
                            {
                                if(boundingBox2D.InRange(segment2D_Polygon2D.GetBoundingBox(), snapTolerance) && segment2D.Collinear(segment2D_Polygon2D, tolerance_Angle))
                                {
                                    if(segment2D.On(segment2D_Polygon2D[0], snapTolerance) || segment2D.On(segment2D_Polygon2D[1], snapTolerance) || segment2D_Polygon2D.On(segment2D[0], snapTolerance) || segment2D_Polygon2D.On(segment2D[1], snapTolerance))
                                    {
                                        external = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if(external)
                {
                    externalPanels.Add(keyValuePair.Key);
                }
            }

            externalPanels_Extended = new List<Panel>(externalPanels);

            segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance_Distance);

            foreach (KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                if (keyValuePair.Value == null)
                {
                    continue;
                }

                foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    if (segment2Ds == null || segment2Ds.Count == 0)
                    {
                        break;
                    }

                    List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                    if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach (Segment2D segment2D in segment2Ds_Temp)
                    {
                        if (segment2D == null || segment2Ds == null || segment2Ds.Count == 0)
                        {
                            break;
                        }

                        Point2D point2D_Mid = segment2D.Mid();

                        segment2Ds.RemoveAll(x => x.On(point2D_Mid, snapTolerance));
                        segment2Ds.RemoveAll(x => segment2D.On(x.Mid(), snapTolerance));
                    }
                }
            }

            foreach (Segment2D segment2D in segment2Ds)
            {
                Point2D point2D_1 = segment2D[0];
                Point2D point2D_2 = segment2D[1];

                Tuple<Panel, Segment2D> tuple_Temp = null;
                foreach (KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
                {
                    foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                    {
                        List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                        Point2D point2D_Temp = null;
                        Segment2D segment2D_Temp = null;

                        segment2D_Temp = segment2Ds_Temp[0];
                        point2D_Temp = segment2D_Temp[0];

                        if (point2D_1.AlmostEquals(point2D_Temp, snapTolerance) || point2D_2.AlmostEquals(point2D_Temp, snapTolerance))
                        {
                            if (segment2D.Direction.SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle || segment2D.Direction.GetNegated().SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle)
                            {
                                tuple_Temp = new Tuple<Panel, Segment2D>(keyValuePair.Key, segment2D_Temp);
                                break;
                            }
                        }

                        segment2D_Temp = segment2Ds_Temp[segment2Ds_Temp.Count - 1];
                        point2D_Temp = segment2D_Temp[1];

                        if (point2D_1.AlmostEquals(point2D_Temp, snapTolerance) || point2D_2.AlmostEquals(point2D_Temp, snapTolerance))
                        {
                            if (segment2D.Direction.SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle || segment2D.Direction.GetNegated().SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle)
                            {
                                tuple_Temp = new Tuple<Panel, Segment2D>(keyValuePair.Key, segment2D_Temp);
                                break;
                            }
                        }
                    }

                    if (tuple_Temp != null)
                    {
                        //TODO: Make sure to split separate parts of section
                        List<Point2D> point2Ds = new List<Point2D>();
                        foreach (ISegmentable2D segmentable2D in keyValuePair.Value)
                        {
                            point2Ds.AddRange(segmentable2D.GetPoints());
                        }

                        Geometry.Planar.Query.ExtremePoints(point2Ds, out Point2D point2D_1_Temp, out Point2D point2D_2_Temp);
                        tuple_Temp = new Tuple<Panel, Segment2D>(tuple_Temp.Item1, new Segment2D(point2D_1_Temp, point2D_2_Temp));

                        break;
                    }
                }

                if (tuple_Temp == null)
                {
                    continue;
                }

                Panel panel_Old = tuple_Temp.Item1;

                BoundingBox3D boundingBox3D = panel_Old.GetBoundingBox();

                Geometry.Planar.Query.ExtremePoints(new Point2D[] { point2D_1, point2D_2, tuple_Temp.Item2[0], tuple_Temp.Item2[1] }, out point2D_1, out point2D_2);

                //Geometry.Planar.Point2D point2D_1_Snap = Geometry.Planar.Query.Snap(segmentable2Ds, point2D_1, snapTolerance);
                //point2D_1 = point2D_1_Snap == null ? point2D_1 : point2D_1_Snap;

                //Geometry.Planar.Point2D point2D_2_Snap = Geometry.Planar.Query.Snap(segmentable2Ds, point2D_2, snapTolerance);
                //point2D_2 = point2D_2_Snap == null ? point2D_2 : point2D_2_Snap;

                Segment2D segment2D_New = new Segment2D(point2D_1, point2D_2);

                Plane plane_Bottom = Plane.WorldXY.GetMoved(new Vector3D(0, 0, boundingBox3D.Min.Z)) as Plane;

                Face3D face3D = Geometry.Spatial.Create.Face3D(plane_Bottom.Convert(segment2D_New), boundingBox3D.Max.Z - boundingBox3D.Min.Z);

                Panel panel_New = new Panel(panel_Old.Guid, panel_Old, face3D);

                int index = panels.IndexOf(panel_Old);
                if(index != -1)
                {
                    if (dictionary.ContainsKey(panel_Old))
                    {
                        dictionary.Remove(panel_Old);
                        dictionary[panel_New] = new List<ISegmentable2D>() { segment2D_New };

                        //dictionary[panel_Old] = new List<ISegmentable2D>() { segment2D_New };
                        panels[index] = panel_New;

                        index = externalPanels_Extended.IndexOf(panel_Old);
                        if(index != -1)
                        {
                            externalPanels_Extended[index] = panel_New;
                        }
                    }
                }
            }
        }
    
        public static void ExtendExternal(this List<Panel> panels, IEnumerable<double> elevations, double maxDistance, out List<Panel> externalPanels, out List<Panel> externalPanels_Extended, out List<Polygon3D> externalPolygon3Ds, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            externalPanels = null;
            externalPanels_Extended = null;
            externalPolygon3Ds = null;

            if(panels == null || elevations == null)
            {
                return;
            }

            Dictionary<Guid, Panel> dictionary = new Dictionary<Guid, Panel>();
            Dictionary<Guid, Panel> dictionary_Extended = new Dictionary<Guid, Panel>();
            externalPolygon3Ds = new List<Polygon3D>();

            foreach (double elevation in elevations)
            {
                ExtendExternal(panels, elevation, maxDistance, out List<Panel> externalPanels_Temp, out List<Panel> externalPanels_Extended_Temp, out List<Polygon3D> externalPolygon3Ds_Temp, snapTolerance, tolerance_Angle, tolerance_Distance);

                externalPanels_Temp?.ForEach(x => dictionary[x.Guid] = x);
                externalPanels_Extended_Temp?.ForEach(x => dictionary_Extended[x.Guid] = x);

                if(externalPolygon3Ds_Temp != null)
                {
                    externalPolygon3Ds.AddRange(externalPolygon3Ds_Temp);
                }
            }

            externalPanels = dictionary.Values.ToList();
            externalPanels_Extended = dictionary_Extended.Values.ToList();
        }
    }
}