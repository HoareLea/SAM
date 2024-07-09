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

            List<List<IPanel>> panelsList = Enumerable.Repeat<List<IPanel>>(null, panels.Count).ToList();

            Parallel.For(0, panels.Count, (int i) =>
            {
                panelsList[i] = panels[i].FixEdges(cutApertures, tolerance);
            });

            for(int i=0; i < panels.Count; i++)
            {
                List<IPanel> panels_Temp = panelsList[i];
                if(panels_Temp == null || panels_Temp.Count == 0)
                {
                    continue;
                }

                if (panels_Temp.Count == 1)
                {
                    result.AddObject(panels_Temp[0]);
                    continue;
                }

                List<IAnalyticalObject> relatedObjects = result.GetRelatedObjects<IAnalyticalObject>(panels[i]);

                foreach (IPanel panel_FixEdge in panels_Temp)
                {
                    result.AddObject(panel_FixEdge);
                    if (relatedObjects != null)
                    {
                        foreach (IAnalyticalObject relatedObject in relatedObjects)
                        {
                            result.AddRelation(panel_FixEdge, relatedObject);
                        }
                    }
                }
            }

            return result;
        }
        
        public static List<IPanel> FixEdges(this IPanel panel, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            if(panel == null)
            {
                return null;
            }

            List<Face3D> face3Ds = panel is Panel ? ((Panel)panel).GetFace3Ds(cutApertures) : new List<Face3D>() { panel.Face3D };
            if(face3Ds == null)
            {
                return null;
            }

            List<Face3D> face3Ds_Temp = new List<Face3D>();
            foreach(Face3D face3D in face3Ds)
            {
                List<Face3D> face3Ds_face3D = face3D.FixEdges(tolerance);
                if (face3Ds_face3D == null)
                {
                    continue;
                }

                face3Ds_Temp.AddRange(face3Ds_face3D);
            }

            face3Ds = face3Ds_Temp;

            List<IPanel> result = new List<IPanel>();
            foreach(Face3D face3D_Temp in face3Ds)
            {
                System.Guid guid = panel.Guid;
                while(result.Find(x => x.Guid == guid) != null)
                {
                    guid = System.Guid.NewGuid();
                }

                IPanel panel_New = null;
                if(panel is Panel)
                {
                    panel_New = Create.Panel(guid, (Panel)panel, face3D_Temp, null, true, tolerance, tolerance);
                }
                else if(panel is ExternalPanel)
                {
                    panel_New = new ExternalPanel(guid, panel as ExternalPanel, face3D_Temp);
                }

                if(panel_New != null)
                {
                    result.Add(panel_New);
                }
            }

            return result;
        }
    }
}