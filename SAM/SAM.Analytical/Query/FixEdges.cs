using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AdjacencyCluster FixEdges(this AdjacencyCluster adjacencyCluster, double tolerance = Core.Tolerance.Distance)
        {
            if(adjacencyCluster == null)
            {
                return null;
            }

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);

            List<Panel> panels = result.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return result;
            }

            foreach(Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }

                List<Panel> panels_FixEdges = panel.FixEdges(tolerance);
                if(panels_FixEdges == null || panels_FixEdges.Count == 0)
                {
                    continue;
                }

                if(panels_FixEdges.Count == 1)
                {
                    result.AddObject(panels_FixEdges[0]);
                    continue;
                }

                List<object> relatedObjects = result.GetRelatedObjects<object>(panel);

                foreach(Panel panel_FixEdge in panels_FixEdges)
                {
                    result.AddObject(panel_FixEdge);
                    if(relatedObjects != null)
                    {
                        foreach(object relatedObject in relatedObjects)
                        {
                            result.AddRelation(panel_FixEdge, relatedObject);
                        }
                    }
                }
            }

            return result;
        }
        
        public static List<Panel> FixEdges(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            if(panel == null)
            {
                return null;
            }

            Face3D face3D = panel.GetFace3D();
            if(face3D == null)
            {
                return null;
            }

            List<Face3D> face3Ds = face3D.FixEdges(tolerance);

            List<Panel> result = new List<Panel>();
            foreach(Face3D face3D_Temp in face3Ds)
            {
                System.Guid guid = panel.Guid;
                while(result.Find(x => x.Guid == guid) != null)
                {
                    guid = System.Guid.NewGuid();
                }

                Panel panel_New = Create.Panel(guid, panel, face3D_Temp, null, true, tolerance, tolerance);
                if(panel_New != null)
                {
                    result.Add(panel_New);
                }
            }

            return result;
        }
    }
}