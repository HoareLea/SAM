using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Panel> Panels(this List<ISAMGeometry3D> geometry3Ds, PanelType panelType = PanelType.Undefined, Construction construction = null, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            List<Face3D> faces = Geometry.Spatial.Query.Face3Ds(geometry3Ds, tolerance);
            if (faces == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (Face3D face in faces)
            {
                if (minArea != 0 && face.GetArea() < minArea)
                    continue;

                Panel panel = new Panel(construction, panelType, face);

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

        public static List<Panel> Panels(this IEnumerable<Panel> panels, Plane plane, PanelType panelType = PanelType.Undefined, bool checkIntersection = true, double tolerance = Core.Tolerance.Distance)
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

            List<Polygon3D> polygon3Ds = Geometry.Spatial.Create.Polygon3Ds(closedPlanar3Ds, plane, checkIntersection, tolerance);
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
    }
}