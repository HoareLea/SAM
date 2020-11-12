using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Close(this IEnumerable<Panel> panels, Plane plane, Construction construction = null, double tolerance = Tolerance.Distance)
        {
            if (panels == null || plane == null)
                return null;

            List<IClosedPlanar3D> closedPlanar3Ds = new List<IClosedPlanar3D>();
            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                Face3D face3D = panel.GetFace3D();
                if (face3D == null)
                    continue;

                closedPlanar3Ds.Add(face3D);
            }

            List<Polygon3D> polygon3Ds = Geometry.Spatial.Query.Close(closedPlanar3Ds, plane, tolerance);
            if (polygon3Ds == null || polygon3Ds.Count == 0)
                return null;

            PanelType panelType = Analytical.PanelType.Undefined;
            if (construction != null)
            {
                string value;
                if (construction.TryGetValue(ConstructionParameter.DefaultPanelType, out value))
                    panelType = Query.PanelType(value, false);
            }

            if (panelType == Analytical.PanelType.Undefined)
                panelType = PanelType(polygon3Ds.Find(x => x!= null)?.GetPlane().Normal);

            if (construction == null)
                construction = DefaultConstruction(panelType);

            if (construction == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach(Polygon3D polygon3D in polygon3Ds)
            {
                if (polygon3D == null)
                    continue;

                new Panel(construction, panelType, new Face3D(polygon3D));
            }

            return result;
        }
    }
}