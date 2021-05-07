using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void Trim(this List<Panel> panels, double elevation, double minLength, out List<Panel> panels_Trimmed, out List<Segment3D> segment3Ds, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            panels_Trimmed = null;
            segment3Ds = null;

            if (panels == null)
                return;

            Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

            Dictionary<Panel, List<Geometry.Planar.ISegmentable2D>> dictionary = panels.SectionDictionary<Geometry.Planar.ISegmentable2D>(plane, tolerance);

            List<Geometry.Planar.ISegmentable2D> segmentable2Ds = new List<Geometry.Planar.ISegmentable2D>();
            foreach (KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
            {
                if (keyValuePair.Value != null)
                {
                    segmentable2Ds.AddRange(keyValuePair.Value);
                }
            }

            List<Geometry.Planar.Segment2D> segment2Ds = Geometry.Planar.Query.Split(segmentable2Ds, tolerance);

            segment2Ds = Geometry.Planar.Query.TrimUnconnected(segment2Ds, minLength, snapTolerance, tolerance);

            segment3Ds = segment2Ds.ConvertAll(x => plane.Convert(x));

            panels_Trimmed = new List<Panel>();

            foreach (KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
            {
                int index = panels.IndexOf(keyValuePair.Key);

                bool updated = false;
                foreach(Geometry.Planar.ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    foreach(Geometry.Planar.Segment2D segment2D in segmentable2D.GetSegments())
                    {
                        List<Geometry.Planar.Segment2D> segment2Ds_Temp = segment2Ds.FindAll(x => segment2D.On(x.Mid(), snapTolerance));
                        if(segment2Ds_Temp.Count == 0)
                        {
                            panels_Trimmed.Add(panels[index]);
                            panels.RemoveAt(index);
                            updated = true;
                            break;
                        }
                        else if(segment2Ds_Temp.Count != 0)
                        {
                            Geometry.Planar.Query.ExtremePoints(Geometry.Planar.Query.UniquePoint2Ds(segment2Ds_Temp, tolerance), out Geometry.Planar.Point2D point2D_1, out Geometry.Planar.Point2D point2D_2);
                            if(point2D_1 != null && point2D_2 != null)
                            {
                                double distance = point2D_1.Distance(point2D_2);
                                if (distance < tolerance || System.Math.Abs(segment2D.GetLength() - distance) < tolerance)
                                {
                                    continue;
                                }

                                Panel panel_Old = keyValuePair.Key;

                                BoundingBox3D boundingBox3D = panel_Old.GetBoundingBox();

                                Geometry.Planar.Point2D point2D_1_Snap = Geometry.Planar.Query.Snap(segmentable2Ds, point2D_1, snapTolerance);
                                point2D_1 = point2D_1_Snap == null ? point2D_1 : point2D_1_Snap;

                                Geometry.Planar.Point2D point2D_2_Snap = Geometry.Planar.Query.Snap(segmentable2Ds, point2D_2, snapTolerance);
                                point2D_2 = point2D_2_Snap == null ? point2D_2 : point2D_2_Snap;

                                Geometry.Planar.Segment2D segment2D_New = new Geometry.Planar.Segment2D(point2D_1, point2D_2);

                                Plane plane_Bottom = Plane.WorldXY.GetMoved(new Vector3D(0, 0, boundingBox3D.Min.Z)) as Plane;
                                Face3D face3D = Geometry.Spatial.Create.Face3D(plane_Bottom.Convert(segment2D_New), boundingBox3D.Max.Z - boundingBox3D.Min.Z);

                                Panel panel_New = new Panel(panel_Old.Guid, panel_Old, face3D);

                                panels[index] = panel_New;
                                panels_Trimmed.Add(panel_New);
                                updated = true;
                                break;
                            }
                        }
                    }

                    if(updated)
                    {
                        break;
                    }
                }
            }
        }
    }
}