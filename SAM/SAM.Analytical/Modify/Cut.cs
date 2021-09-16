using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> Cut(this AdjacencyCluster adjacencyCluster, double elevation, double tolerance = Tolerance.Distance)
        {
            return Cut(adjacencyCluster, Geometry.Spatial.Create.Plane(elevation), tolerance);
        }

        public static List<Panel> Cut(this AdjacencyCluster adjacencyCluster, Plane plane, double tolerance = Tolerance.Distance)
        {
            if (adjacencyCluster == null || plane == null)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
            {
                List<Panel> panels_Cut = panel.Cut(plane, tolerance);
                if (panels_Cut != null && panels_Cut.Count > 1)
                {
                    List<object> relatedObjects = adjacencyCluster.GetRelatedObjects(panel);
                    if (adjacencyCluster.RemoveObject<Panel>(panel.Guid))
                    {
                        foreach (Panel panel_Cut in panels_Cut)
                        {
                            adjacencyCluster.AddObject(panel_Cut);
                            relatedObjects?.ForEach(x => adjacencyCluster.AddRelation(panel_Cut, x));
                        }
                    }

                    result.AddRange(panels_Cut);
                }
            }

            return result;
        }
    }
}