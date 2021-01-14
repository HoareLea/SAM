using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double ExternalPanelsArea(this AdjacencyCluster adjacencyCluster, Space space, bool exposedToSunOnly = false)
        {
            if (adjacencyCluster == null || space == null)
                return double.NaN;

            List<Panel> panels = adjacencyCluster.GetExternalPanels(space);
            if (panels == null || panels.Count == 0)
                return double.NaN;

            double result = 0;
            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                if (exposedToSunOnly && !adjacencyCluster.ExposedToSun(panel))
                    continue;
                
                double area = panel.GetArea();
                if (double.IsNaN(area) || area == 0)
                    continue;

                result += area;
            }

            return result;
        }
    }
}