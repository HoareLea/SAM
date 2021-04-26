using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void Align(this List<Panel> panels, double elevation, double referenceElevation, double maxDistance = 0.2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null || double.IsNaN(elevation) || double.IsNaN(referenceElevation))
            {
                return;
            }

            List<Panel> panels_Temp = panels.FilterByElevation(elevation, out List<Panel> panels_Lower, out List<Panel> panels_Upper, tolerance_Distance);
            if(panels_Temp == null || panels_Temp.Count == 0)
            {
                return;
            }
            
            List<Panel> panels_Reference = panels.FilterByElevation(referenceElevation, out panels_Lower, out panels_Upper);
            if(panels_Reference == null || panels_Reference.Count == 0)
            {
                return;
            }

            Plane plane = new Plane(new Point3D(0, 0, referenceElevation), Vector3D.WorldZ);

            Dictionary<Segment2D, Panel> dictionary_Reference = new Dictionary<Segment2D, Panel>();
            foreach (Panel panel_Reference in panels_Reference)
            {
                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, panel_Reference.GetFace3D());
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                List<Segment2D> segment2Ds = planarIntersectionResult.GetGeometry2Ds<Segment2D>();
                if (segment2Ds != null && segment2Ds.Count != 0)
                {
                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        dictionary_Reference[segment2D] = panel_Reference;
                    }
                }

            }

            if (panels_Temp.Count == 0 || dictionary_Reference.Count == 0)
                return;

            List<int> indexes = panels_Temp.ConvertAll(x => panels.IndexOf(x));

            Align(panels_Temp, dictionary_Reference, maxDistance, tolerance_Angle, tolerance_Distance);

            for(int i=0; i < indexes.Count; i++)
            {
                int index = indexes[i];
                if(index == -1)
                {
                    continue;
                }

                panels[index] = panels_Temp[i];
            }
        }

        public static void Align(this List<Panel> panels, Dictionary<Segment2D, Panel> dictionary_Reference, double maxDistance = 0.2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null || dictionary_Reference == null)
                return;

            Plane plane = Plane.WorldXY;

            bool updated = false;
            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                if (panel == null)
                    continue;

                double max = panel.MaxElevation();
                double min = panel.MinElevation();

                Plane plane_Temp = Plane.WorldXY.GetMoved(Vector3D.WorldZ * ((max + min) / 2)) as Plane;

                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane_Temp, panel.GetFace3D());
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<Segment2D> segment2Ds_Intersection = planarIntersectionResult.GetGeometry2Ds<Segment2D>();
                if (segment2Ds_Intersection == null || segment2Ds_Intersection.Count == 0)
                    continue;

                Geometry.Planar.Query.ExtremePoints(segment2Ds_Intersection.UniquePoint2Ds(tolerance_Distance), out Point2D point2D_1, out Point2D point2D_2);
                if (point2D_1.Distance(point2D_2) <= tolerance_Distance)
                    continue;

                Segment2D segment2D = new Segment2D(point2D_1, point2D_2);

                List<Segment2D> segment2Ds_Temp = dictionary_Reference.Keys.ToList().FindAll(x => x.Collinear(segment2D));
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    continue;

                List<Segment2D> segment2Ds_Result = new List<Segment2D>();
                foreach (Segment2D segment2D_Temp in segment2Ds_Temp)
                {
                    double distance = segment2D_Temp.Distance(segment2D, Core.Tolerance.MacroDistance);
                    if(distance < tolerance_Distance)
                    {
                        segment2Ds_Result = null;
                        break;
                    }

                    if (distance > maxDistance + Core.Tolerance.MacroDistance)
                        continue;

                    segment2Ds_Result.Add(segment2D_Temp);
                }

                if (segment2Ds_Result == null || segment2Ds_Result.Count == 0)
                    continue;

                segment2Ds_Temp = segment2Ds_Result;

                segment2Ds_Temp.Sort((x, y) => segment2D.Distance(x).CompareTo(segment2D.Distance(y)));

                Segment2D segment2D_Reference = null;

                foreach (Segment2D segment2D_Temp in segment2Ds_Temp)
                {
                    Panel panel_Reference = dictionary_Reference[segment2D_Temp];
                    if (panel.Construction == null)
                    {
                        if (panel_Reference.Construction == null)
                        {
                            segment2D_Reference = segment2D_Temp;
                            break;
                        }
                        continue;
                    }

                    if (panel.Construction.Name.Equals(panel_Reference.Construction.Name))
                    {
                        segment2D_Reference = segment2D_Temp;
                        break;
                    }
                }

                if (segment2D_Reference == null)
                {
                    HashSet<PanelType> panelTypes = new HashSet<PanelType>();
                    panelTypes.Add(panel.PanelType);
                    switch (panelTypes.First())
                    {
                        case PanelType.CurtainWall:
                            panelTypes.Add(PanelType.WallExternal);
                            break;

                        case PanelType.UndergroundWall:
                            panelTypes.Add(PanelType.WallExternal);
                            break;

                        case PanelType.Undefined:
                            panelTypes.Add(PanelType.WallInternal);
                            break;
                    }

                    foreach (Segment2D segment2D_Temp in segment2Ds_Temp)
                    {
                        PanelType panelType_Temp = dictionary_Reference[segment2D_Temp].PanelType;
                        if (panelTypes.Contains(panelType_Temp))
                        {
                            segment2D_Reference = segment2D_Temp;
                            break;
                        }
                    }
                }

                if (segment2D_Reference == null)
                    segment2D_Reference = segment2Ds_Temp.First();

                if (segment2D_Reference == null)
                    continue;

                point2D_1 = segment2D.Mid();
                point2D_2 = segment2D_Reference.Project(point2D_1);
                if (point2D_1.Distance(point2D_2) <= tolerance_Distance)
                    continue;

                Vector3D vector3D = Plane.WorldXY.Convert(new Vector2D(point2D_1, point2D_2));

                 List<Panel> panels_Connected = panel.ConnectedPanels(panels, true, tolerance_Angle, tolerance_Distance);

                panel = new Panel(panel); 
                panel.Move(vector3D);
                panels[i] = panel;

                if (panels_Connected != null)
                {
                    Face3D face3D_Panel = panel.GetFace3D();
                    Plane plane_Panel = face3D_Panel.GetPlane();

                    foreach (Panel panel_Connected in panels_Connected)
                    {
                        Face3D face3D_Connected = panel_Connected?.GetFace3D();
                        if (face3D_Connected == null)
                            continue;

                        Face3D face3D_Connected_New = null;

                        PlanarIntersectionResult planarIntersectionResult_Temp = Geometry.Spatial.Create.PlanarIntersectionResult(plane_Panel, face3D_Connected, tolerance_Angle, tolerance_Distance);
                        if(planarIntersectionResult_Temp != null && planarIntersectionResult_Temp.Intersecting)
                        {
                            List<ISegmentable3D> segmentable3Ds = planarIntersectionResult_Temp.GetGeometry3Ds<ISegmentable3D>();
                            if(segmentable3Ds != null && segmentable3Ds.Count > 0)
                            {
                                List<Face3D> face3Ds = Geometry.Spatial.Query.Cut(face3D_Connected, plane_Panel, tolerance_Distance);
                                if (face3Ds != null && face3Ds.Count != 0)
                                {
                                    face3Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
                                    face3D_Connected_New = face3Ds[0];
                                }
                            }
                        }
                        else
                        {
                            face3D_Connected_New = Geometry.Spatial.Query.Extend(face3D_Connected, plane_Panel, tolerance_Angle, tolerance_Distance);
                        }
                        
                        if (face3D_Connected_New == null)
                            continue;

                        int index = panels.IndexOf(panel_Connected);
                        if (index != -1)
                            panels[index] = new Panel(panel_Connected.Guid, panel_Connected, face3D_Connected_New);
                    }
                }

                updated = true;
                break;
            }

            if (updated)
                Align(panels, dictionary_Reference, maxDistance, tolerance_Angle, tolerance_Distance);

        }
    }
}