using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> ApertureHosts(this AdjacencyCluster adjacencyCluster, IClosedPlanar3D closedPlanar3D)
        {
            if (adjacencyCluster == null || closedPlanar3D == null)
                return null;

            return ApertureHosts(adjacencyCluster.GetPanels(), closedPlanar3D);
        }

        public static List<Panel> ApertureHosts(this IEnumerable<Panel> panels, IClosedPlanar3D closedPlanar3D, double minArea = Core.Tolerance.MacroDistance, double maxDistance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null || closedPlanar3D == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach(Panel panel in panels)
            {
                if (panel.ApertureHost(closedPlanar3D, minArea, maxDistance, tolerance))
                    result.Add(panel);
            }

            return result;
        }
    }
}