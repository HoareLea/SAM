using System.Collections.Generic;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Panel> Panels(this List<ISAMGeometry3D> geometry3Ds, PanelType panelType, Construction construction)
        {
            List<Face3D> faces = Geometry.Spatial.Query.Face3Ds(geometry3Ds);
            if (faces == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (Face3D face in faces)
            {
                Panel panel = new Panel(construction, panelType, face);

                if(panelType == PanelType.Undefined)
                {
                    Vector3D normal = panel.PlanarBoundary3D?.GetNormal();
                    if (normal == null)
                        continue;

                    PanelType panelType_New = Query.PanelType(normal);
                    if (panelType_New != panelType)
                        panel = new Panel(panel, panelType_New);
                }

                result.Add(panel);
            }
                
            return result;
        }
    }
}
