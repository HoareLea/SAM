using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> Cut(this AdjacencyCluster adjacencyCluster, double elevation, IEnumerable<Space> spaces = null, double tolerance = Tolerance.Distance)
        {
            return Cut(adjacencyCluster, Geometry.Spatial.Create.Plane(elevation), spaces, tolerance);
        }

        public static List<Panel> Cut(this AdjacencyCluster adjacencyCluster, Plane plane, IEnumerable<Space> spaces = null, double tolerance = Tolerance.Distance)
        {
            if (adjacencyCluster == null || plane == null)
            {
                return null;
            }

            List<Panel> panels = null;
            if(spaces == null || spaces.Count() == 0)
            {
                panels = adjacencyCluster.GetPanels();
            }
            else
            {
                panels = new List<Panel>();
                foreach(Space space in spaces)
                {
                    List<Panel> panels_Space = adjacencyCluster.GetPanels(space);
                    if(panels_Space == null || panels_Space.Count == 0)
                    {
                        continue;
                    }

                    foreach(Panel panel_Space in panels_Space)
                    {
                        if(panels.Find(x => x.Guid == panel_Space.Guid) == null)
                        {
                            panels.Add(panel_Space);
                        }
                    }
                }
            }

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