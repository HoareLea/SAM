using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Panel SnapByPlanes(this Panel panel, IEnumerable<Plane> planes, double maxDistance, double tolerance = Core.Tolerance.Distance)
        {
            Panel panel_New = new Panel(panel);
            panel_New.Snap(planes, maxDistance);

            return panel_New;
        }

        public static List<Panel> SnapByPlanes(this IEnumerable<Panel> panels, IEnumerable<Plane> planes, double maxDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();
            foreach(Panel panel in panels)
            {
                result.Add(panel?.SnapByPlanes(planes, maxDistance, tolerance));
            }

            return result;
        }

        public static List<Panel> SnapByPlanes(this IEnumerable<Panel> panels, IEnumerable<double> elevations, double maxDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(panels == null)
            {
                return null;
            }

            List<Plane> planes = elevations?.ToList().ConvertAll(x => Plane.WorldXY.GetMoved(new Vector3D(0, 0, x)) as Plane);

            return SnapByPlanes(panels, planes, maxDistance, tolerance);
        }
    }
}