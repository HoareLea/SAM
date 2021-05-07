using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Panel Extend(this Panel panel, Plane plane, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panel == null || plane == null)
                return null;

            Face3D face3D = panel.GetFace3D()?.Extend(plane, tolerance_Angle, tolerance_Distance);
            if (face3D == null)
                return null;

            return new Panel(panel.Guid, panel, face3D);
        }

        public static void Extend(this List<Panel> panels, double elevation, double maxDistance, out List<Panel> panels_Extended, out List<Segment3D> segment3Ds, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            panels_Extended = null;
            segment3Ds = null;

            if (panels == null)
                return;

            Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

            Dictionary<Panel, List<Geometry.Planar.ISegmentable2D>> dictionary = panels.SectionDictionary<Geometry.Planar.ISegmentable2D>(plane, tolerance_Distance);

            List<Geometry.Planar.ISegmentable2D> segmentable2Ds = new List<Geometry.Planar.ISegmentable2D>();
            foreach (KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
            {
                if (keyValuePair.Value != null)
                {
                    segmentable2Ds.AddRange(keyValuePair.Value);
                }
            }

            List<Geometry.Planar.Segment2D> segment2Ds = Geometry.Planar.Create.Segment2Ds(segmentable2Ds, maxDistance, snapTolerance, tolerance_Distance);
            if (segment2Ds == null || segment2Ds.Count == 0)
                return;

            segment3Ds = segment2Ds.ConvertAll(x => plane.Convert(x));

            foreach (KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
            {
                if (keyValuePair.Value == null)
                {
                    continue;
                }

                foreach (Geometry.Planar.ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    if (segment2Ds == null || segment2Ds.Count == 0)
                    {
                        break;
                    }

                    List<Geometry.Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                    if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach (Geometry.Planar.Segment2D segment2D in segment2Ds_Temp)
                    {
                        if (segment2D == null || segment2Ds == null || segment2Ds.Count == 0)
                        {
                            break;
                        }

                        Geometry.Planar.Point2D point2D_Mid = segment2D.Mid();

                        segment2Ds.RemoveAll(x => x.On(point2D_Mid, snapTolerance));
                        segment2Ds.RemoveAll(x => segment2D.On(x.Mid(), snapTolerance));
                    }
                }
            }

            panels_Extended = new List<Panel>();

            foreach(Geometry.Planar.Segment2D segment2D in segment2Ds)
            {
                Geometry.Planar.Point2D point2D_1 = segment2D[0];
                Geometry.Planar.Point2D point2D_2 = segment2D[1];
                Tuple<Panel, Geometry.Planar.Segment2D> tuple_Temp = null;
                foreach (KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
                {
                    foreach (Geometry.Planar.ISegmentable2D segmentable2D in keyValuePair.Value)
                    {
                        List<Geometry.Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                        Geometry.Planar.Point2D point2D_Temp = null;
                        Geometry.Planar.Segment2D segment2D_Temp = null;

                        segment2D_Temp = segment2Ds_Temp[0];
                        point2D_Temp = segment2D_Temp[0];

                        if (point2D_1.AlmostEquals(point2D_Temp, snapTolerance) || point2D_2.AlmostEquals(point2D_Temp, snapTolerance))
                        {
                            if (segment2D.Direction.SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle || segment2D.Direction.GetNegated().SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle)
                            {
                                tuple_Temp = new Tuple<Panel, Geometry.Planar.Segment2D>(keyValuePair.Key, segment2D_Temp);
                                break;
                            }
                        }

                        segment2D_Temp = segment2Ds_Temp[segment2Ds_Temp.Count - 1];
                        point2D_Temp = segment2D_Temp[1];

                        if (point2D_1.AlmostEquals(point2D_Temp, snapTolerance) || point2D_2.AlmostEquals(point2D_Temp, snapTolerance))
                        {
                            if (segment2D.Direction.SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle || segment2D.Direction.GetNegated().SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle)
                            {
                                tuple_Temp = new Tuple<Panel, Geometry.Planar.Segment2D>(keyValuePair.Key, segment2D_Temp);
                                break;
                            }
                        }
                    }

                    if (tuple_Temp != null)
                    {
                        //TODO: Make sure to split separate parts of section
                        List<Geometry.Planar.Point2D> point2Ds = new List<Geometry.Planar.Point2D>();
                        foreach (Geometry.Planar.ISegmentable2D segmentable2D in keyValuePair.Value)
                        {
                            point2Ds.AddRange(segmentable2D.GetPoints());
                        }

                        Geometry.Planar.Query.ExtremePoints(point2Ds, out Geometry.Planar.Point2D point2D_1_Temp, out Geometry.Planar.Point2D point2D_2_Temp);
                        tuple_Temp = new Tuple<Panel, Geometry.Planar.Segment2D>(tuple_Temp.Item1, new Geometry.Planar.Segment2D(point2D_1_Temp, point2D_2_Temp));

                        break;
                    }
                }

                if (tuple_Temp == null)
                {
                    continue;
                }

                Panel panel_Old = tuple_Temp.Item1;

                BoundingBox3D boundingBox3D = panel_Old.GetBoundingBox();

                Geometry.Planar.Query.ExtremePoints(new Geometry.Planar.Point2D[] { point2D_1, point2D_2, tuple_Temp.Item2[0], tuple_Temp.Item2[1] }, out point2D_1, out point2D_2);

                Geometry.Planar.Point2D point2D_1_Snap = Geometry.Planar.Query.Snap(segmentable2Ds, point2D_1, snapTolerance);
                point2D_1 = point2D_1_Snap == null ? point2D_1 : point2D_1_Snap;

                Geometry.Planar.Point2D point2D_2_Snap = Geometry.Planar.Query.Snap(segmentable2Ds, point2D_2, snapTolerance);
                point2D_2 = point2D_2_Snap == null ? point2D_2 : point2D_2_Snap;

                Geometry.Planar.Segment2D segment2D_New = new Geometry.Planar.Segment2D(point2D_1, point2D_2);

                Plane plane_Bottom = Plane.WorldXY.GetMoved(new Vector3D(0, 0, boundingBox3D.Min.Z)) as Plane;
                Face3D face3D = Geometry.Spatial.Create.Face3D(plane_Bottom.Convert(segment2D_New), boundingBox3D.Max.Z - boundingBox3D.Min.Z);

                Panel panel_New = new Panel(panel_Old.Guid, panel_Old, face3D);

                int index = panels.IndexOf(panel_Old);
                if (index != -1)
                {
                    if (dictionary.ContainsKey(panel_Old))
                    {
                        dictionary[panel_Old] = new List<Geometry.Planar.ISegmentable2D>() { segment2D_New };
                        panels[index] = panel_New;

                        index = panels_Extended.IndexOf(panel_Old);
                        if (index != -1)
                        {
                            panels_Extended[index] = panel_New;
                        }
                        else
                        {
                            panels_Extended.Add(panel_New);
                        }
                    }
                }
            }
        }
    }
}