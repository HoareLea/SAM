using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> TriangulateConcavePanels(this AdjacencyCluster adjacencyCluster, out List<Panel> triangulatedPanels, double tolerance = Tolerance.Distance)
        {
            triangulatedPanels = null;

            List<Panel> panels = adjacencyCluster?.GetPanels();
            if(panels == null)
            {
                return null;
            }

            triangulatedPanels = new List<Panel>();
            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
            {
                if(!Geometry.Spatial.Query.Concave(panel))
                {
                    continue;
                }

                List<Panel> panels_Triangulate = panel.Triangulate(tolerance);

                if (panels_Triangulate == null)
                {
                    continue;
                }

                triangulatedPanels.Add(panel);

                List<object> relatedObjects = adjacencyCluster.GetRelatedObjects(panel);

                foreach (Panel panel_Temp in panels_Triangulate)
                {
                    if (panel_Temp == null)
                    {
                        continue;
                    }

                    result.Add(panel_Temp);

                    adjacencyCluster.AddObject(panel_Temp);

                    if (relatedObjects != null && relatedObjects.Count > 0)
                        foreach (object relatedObject in relatedObjects)
                            adjacencyCluster.AddRelation(panel_Temp, relatedObject);
                }
            }

            return result;
        }
    }
}