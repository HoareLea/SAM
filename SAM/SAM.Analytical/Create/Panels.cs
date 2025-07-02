using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Panel> Panels(this List<ISAMGeometry3D> geometry3Ds, PanelType panelType = PanelType.Undefined, Construction construction = null, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            List<Face3D> face3Ds = Geometry.Spatial.Query.Face3Ds(geometry3Ds, tolerance);
            if (face3Ds == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (Face3D face3D in face3Ds)
            {
                if (face3D == null || !face3D.IsValid())
                    continue;

                if (minArea != 0 && face3D.GetArea() < minArea)
                    continue;

                Panel panel = new Panel(construction, panelType, face3D);

                if (panelType == PanelType.Undefined)
                {
                    Vector3D normal = panel.Normal;
                    if (normal == null)
                        continue;

                    PanelType panelType_New = Query.PanelType(normal);
                    if (panelType_New != panelType)
                        panel = new Panel(panel, panelType_New);
                }

                if (panel.Construction == null)
                {
                    Construction construction_Temp = Query.DefaultConstruction(panel.PanelType);
                    if (construction_Temp != null)
                        panel = new Panel(panel, construction_Temp);
                }

                result.Add(panel);
            }

            return result;
        }

        public static List<Panel> Panels(this IEnumerable<ISegmentable3D> segmentable3Ds, double height, PanelType panelType = PanelType.Undefined, Construction construction = null)
        {
            if (segmentable3Ds == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach(ISegmentable3D segmentable3D in segmentable3Ds)
            {
                List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
                if (segment3Ds == null || segment3Ds.Count == 0)
                {
                    continue;
                }
                
                foreach(Segment3D segment3D in segment3Ds)
                {
                    Polygon3D polygon3D = Geometry.Spatial.Create.Polygon3D(segment3D, height);
                    if (polygon3D == null)
                    {
                        return null;
                    }

                    PanelType panelType_Temp = panelType;
                    if (panelType_Temp == PanelType.Undefined)
                    {
                        Vector3D normal = polygon3D.GetPlane().Normal;
                        panelType_Temp = Query.PanelType(normal);
                    }

                    Construction construction_Temp = construction;
                    if(construction_Temp == null)
                    {
                        construction_Temp = Query.DefaultConstruction(panelType_Temp);
                    }

                    result.Add(new Panel(construction_Temp, panelType_Temp, new Face3D(polygon3D)));
                }
            }

            return result;
        }

        public static List<Panel> Panels(this IEnumerable<Panel> panels, Plane plane, PanelType panelType = PanelType.Undefined, bool checkIntersection = true, bool union = true, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null || plane == null)
                return null;

            List<IClosedPlanar3D> closedPlanar3Ds = new List<IClosedPlanar3D>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                Face3D face3D = panel.GetFace3D();
                if (face3D == null)
                    continue;

                closedPlanar3Ds.Add(face3D);
            }

            List<Polygon3D> polygon3Ds = Geometry.Spatial.Create.Polygon3Ds(closedPlanar3Ds, plane, checkIntersection, union, tolerance);
            if (polygon3Ds == null || polygon3Ds.Count == 0)
                return null;

            if(panelType == PanelType.Undefined)
                panelType = Query.PanelType(polygon3Ds.Find(x => x != null)?.GetPlane().Normal);

            Construction construction = Query.DefaultConstruction(panelType);
            if (construction == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (Polygon3D polygon3D in polygon3Ds)
            {
                if (polygon3D == null)
                    continue;

                result.Add(new Panel(construction, panelType, new Face3D(polygon3D)));
            }

            return result;
        }
    
        public static List<Panel> Panels(this Shell shell, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (shell == null)
                return null;

            Shell shell_Temp = new Shell(shell);
            shell_Temp.OrientNormals(false, silverSpacing, tolerance);

            List<Face3D> face3Ds = shell_Temp.Face3Ds;
            if (face3Ds == null)
                return null;

            ConstructionLibrary constructionLibrary =ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            List<Panel> result = new List<Panel>();
            foreach(Face3D face3D in face3Ds)
            {
                PanelType panelType = Query.PanelType(face3D.GetPlane().Normal);
                Construction construction = null;
                if (panelType != PanelType.Undefined && constructionLibrary != null)
                {
                    construction = constructionLibrary.GetConstructions(panelType).FirstOrDefault();
                    if (construction == null)
                        construction = constructionLibrary.GetConstructions(panelType.PanelGroup()).FirstOrDefault();
                }

                result.Add(new Panel(construction, panelType, face3D));
            }

            return result;
        }

        public static List<Panel> Panels(this AdjacencyCluster adjacencyCluster, Plane plane, IEnumerable<Space> spaces = null, PanelType panelType = PanelType.Air, Construction construction = null, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            return Panels(adjacencyCluster, plane, out List<Panel> existingPanels, spaces, panelType, construction, tolerance_Angle, tolerance_Distance, tolerance_Snap);
        }

        public static List<Panel> Panels(this AdjacencyCluster adjacencyCluster, Plane plane, out List<Panel> existingPanels, IEnumerable<Space> spaces = null, PanelType panelType = PanelType.Air, Construction construction = null, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            existingPanels = null;

            if (adjacencyCluster == null || plane == null)
            {
                return null;
            }

            List<Panel> panels = null;
            if(spaces == null || spaces.Count() == 0)
            {
                panels = adjacencyCluster.GetPanels();
            }
            else
            {
                panels = new List<Panel>();
                foreach (Space space in spaces)
                {
                    List<Panel> panels_Space = adjacencyCluster.GetPanels(space);
                    if (panels_Space == null || panels_Space.Count == 0)
                    {
                        continue;
                    }

                    foreach (Panel panel_Space in panels_Space)
                    {
                        if (panels.Find(x => x.Guid == panel_Space.Guid) == null)
                        {
                            panels.Add(panel_Space);
                        }
                    }
                }
            }

            if(panels == null || panels.Count == 0)
            {
                return null;
            }

            Dictionary<Panel, List<ISegmentable2D>> dictionary = panels.SectionDictionary<ISegmentable2D>(plane, tolerance_Distance);
            if(dictionary == null || dictionary.Count == 0)
            {
                return null;
            }

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach(List<ISegmentable2D> segmentable2Ds in dictionary.Values)
            {
                List<Segment2D> segment2Ds_Temp = Geometry.Planar.Query.Segment2Ds(segmentable2Ds);
                if (segment2Ds_Temp != null)
                {
                    segment2Ds.AddRange(segment2Ds_Temp);
                }
            }
            
            segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance_Distance);
            segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, tolerance_Snap);

            List<Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(segment2Ds);
            if(face2Ds == null || face2Ds.Count == 0)
            {
                return null;
            }

            existingPanels = new List<Panel>();

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(Face2D face2D in face2Ds)
            {
                Face3D face3D = plane.Convert(face2D);
                if(face3D == null)
                {
                    continue;
                }

                List<Face2D> face2Ds_Difference = new List<Face2D>() { face2D };

                List<Panel> panels_Face3D = Query.PanelsByFace3D(panels, face3D, tolerance_Distance, tolerance_Snap, tolerance_Angle, tolerance_Distance);
                if(panels_Face3D != null && panels_Face3D.Count != 0)
                {
                    existingPanels.AddRange(panels_Face3D);
                    
                    foreach(Panel panel_Face3D in panels_Face3D)
                    {
                        if(face2Ds_Difference == null || face2Ds_Difference.Count == 0)
                        {
                            break;
                        }
                        
                        Face2D face2D_Panel = plane.Convert(plane.Project(panel_Face3D.GetFace3D()));
                        if(face2D_Panel == null)
                        {
                            continue;
                        }

                        int count = face2Ds_Difference.Count;
                        for (int i = count - 1; i >= 0; i--)
                        {
                            Face2D face2D_Difference = face2Ds_Difference[i];
                            List<Face2D> face2Ds_Difference_Temp = face2D_Difference.Difference(face2D_Panel, tolerance_Distance);
                            if (face2Ds_Difference_Temp == null || face2Ds_Difference_Temp.Count == 0)
                            {
                                continue;
                            }

                            face2Ds_Difference.RemoveAt(i);
                            face2Ds_Difference.AddRange(face2Ds_Difference_Temp);
                        }
                    }
                }

                if (face2Ds_Difference == null || face2Ds_Difference.Count == 0)
                {
                    continue;
                }

                for(int i = 0; i < face2Ds_Difference.Count; i++)
                {
                    face2Ds_Difference[i] = face2Ds_Difference[i].Snap(face2Ds, tolerance_Snap, tolerance_Distance);
                    face3Ds.Add(plane.Convert(face2Ds_Difference[i]));
                }
            }

            List<Panel> result = new List<Panel>();

            if(face3Ds == null || face3Ds.Count == 0)
            {
                return result;
            }

            return face3Ds.ConvertAll(x => Panel(construction, panelType, x));

        }

        public static List<Panel> Panels(this AnalyticalModel analyticalModel, Plane plane, IEnumerable<Space> spaces = null, PanelType panelType = PanelType.Air, Construction construction = null, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            return Panels(analyticalModel?.AdjacencyCluster, plane, spaces, panelType, construction, tolerance_Angle, tolerance_Distance, tolerance_Snap);
        }
    }
}