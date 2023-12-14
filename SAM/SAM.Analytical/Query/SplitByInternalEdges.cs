using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> SplitByInternalEdges(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null)
                return null;

            Face3D face3D = panel.GetFace3D();
            if (face3D == null || !face3D.IsValid())
                return null;

            List<Face3D> face3Ds = face3D.SplitByInternalEdges(tolerance);
            if (face3Ds == null)
                return null;

            List<Panel> result = new List<Panel>();
            for(int i=0; i < face3Ds.Count; i++)
            {
                Face3D face3D_Temp = face3Ds[i];

                System.Guid guid = System.Guid.NewGuid();
                if (i == 0)
                    guid = panel.Guid;


                Panel panel_Temp = new Panel(guid, panel, face3D_Temp, null, true);
                result.Add(panel_Temp);
            }

            return result;
        }

        public static AdjacencyCluster SplitByInternalEdges(this AdjacencyCluster adjacencyCluster, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return null;

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);
            List<Panel> panels = result.GetPanels();
            if(panels != null && panels.Count > 0)
            {
                foreach(Panel panel in panels)
                {
                    List<Panel> panels_Split = panel.SplitByInternalEdges(tolerance);
                    if (panels_Split == null || panels_Split.Count < 2)
                        continue;

                    List<IJSAMObject> relatedObjects = result.GetRelatedObjects(panel); 

                    foreach(Panel panel_Split in panels_Split)
                    {
                        result.AddObject(panel_Split);

                        if(relatedObjects != null && relatedObjects.Count > 0)
                        {
                            foreach (IJSAMObject relatedObject in relatedObjects)
                            {
                                result.AddRelation(panel_Split, relatedObject);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}