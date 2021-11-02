using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AdjacencyCluster FixEdges(this AdjacencyCluster adjacencyCluster, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
            {
                return null;
            }

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);

            List<Panel> panels = result.GetPanels();
            if (panels == null || panels.Count == 0)
            {
                return result;
            }

            List<List<Panel>> panelsList = Enumerable.Repeat<List<Panel>>(null, panels.Count).ToList();

            Parallel.For(0, panels.Count, (int i) =>
            {
                panelsList[i] = panels[i].FixEdges(cutApertures, tolerance);
            });

            for(int i=0; i < panels.Count; i++)
            {
                List<Panel> panels_Temp = panelsList[i];
                if(panels_Temp == null || panels_Temp.Count == 0)
                {
                    continue;
                }

                if (panels_Temp.Count == 1)
                {
                    result.AddObject(panels_Temp[0]);
                    continue;
                }

                List<object> relatedObjects = result.GetRelatedObjects<object>(panels[i]);

                foreach (Panel panel_FixEdge in panels_Temp)
                {
                    result.AddObject(panel_FixEdge);
                    if (relatedObjects != null)
                    {
                        foreach (object relatedObject in relatedObjects)
                        {
                            result.AddRelation(panel_FixEdge, relatedObject);
                        }
                    }
                }
            }

            return result;
        }
        
        public static List<Panel> FixEdges(this Panel panel, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            if(panel == null)
            {
                return null;
            }

            Face3D face3D = panel.GetFace3D(cutApertures);
            if(face3D == null)
            {
                return null;
            }

            List<Face3D> face3Ds = face3D.FixEdges(tolerance);
            if(face3Ds == null)
            {
                return null;
            }

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