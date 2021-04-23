using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Panels(this AdjacencyCluster adjacencyCluster, Construction construction)
        {
            if (adjacencyCluster == null || construction == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null)
                return null;

            Guid guid = construction.Guid;

            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
                if (panel.SAMTypeGuid.Equals(guid))
                    result.Add(panel);

            return result;
        }

        public static List<Panel> Panels(this IEnumerable<Panel> panels, double elevation, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach(Panel panel in panels)
            {
                BoundingBox3D boundingBox3D = panel?.GetBoundingBox(tolerance);
                if(boundingBox3D == null)
                {
                    continue;
                }

                if(boundingBox3D.Max.Z >= elevation && boundingBox3D.Min.Z <= elevation)
                {
                    result.Add(panel);
                }
            }

            return result;
        }
    }
}