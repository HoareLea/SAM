using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Segment3D> Grid(this IEnumerable<Panel> panels, double x, double y, Plane plane = null, Point3D point3D = null, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, bool keepFull = false)
        {
            if (panels == null || double.IsNaN(x) || double.IsNaN(y))
                return null;

            List<IClosedPlanar3D> closedPlanar3Ds = new List<IClosedPlanar3D>();
            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                IClosedPlanar3D closedPlanar3D = panel.GetFace3D();
                if (closedPlanar3D == null)
                    continue;

                closedPlanar3Ds.Add(closedPlanar3D);
            }

            return Geometry.Spatial.Query.Grid(closedPlanar3Ds, x, y, plane, point3D, tolerance_Angle, tolerance_Distance, keepFull);
        }
    }
}