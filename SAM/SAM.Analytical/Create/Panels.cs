using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Panel> Panels(this List<ISAMGeometry3D> geometry3Ds, PanelType panelType, Construction construction = null, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
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
    }
}