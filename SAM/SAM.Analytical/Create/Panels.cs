using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Panel> Panels(this List<ISAMGeometry3D> geometry3Ds, PanelType panelType, Construction construction = null, double minArea = Core.Tolerance.MacroDistance)
        {
            List<Face3D> faces = Geometry.Spatial.Query.Face3Ds(geometry3Ds);
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
                    Construction construction_Temp = Query.Construction(panel.PanelType);
                    if (construction_Temp != null)
                        panel = new Panel(panel, construction_Temp);
                }

                result.Add(panel);
            }

            return result;
        }
    }
}