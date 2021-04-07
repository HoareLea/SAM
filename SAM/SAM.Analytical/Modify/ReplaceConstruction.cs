using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> ReplaceConstruction(this AdjacencyCluster adjacencyCluster, IEnumerable<Guid> guids, Construction construction, ApertureConstruction apertureConstruction = null, double offset = 0)
        {
            if (adjacencyCluster == null || guids == null || construction == null)
                return null;
            
            
            List<Panel> result = new List<Panel>();

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
                return result;

            foreach(Guid guid in guids)
            {
                Panel panel = adjacencyCluster.GetObject<Panel>(guid);
                if (panel == null)
                    continue;

                Panel panel_New = new Panel(panel, construction);

                if(apertureConstruction != null)
                {
                    panel_New.AddApertures(apertureConstruction, panel.GetFace3D(), false);

                    if (offset > 0)
                        panel.OffsetAperturesOnEdge(offset);
                }


                if (adjacencyCluster.AddObject(panel_New))
                    result.Add(panel_New);

            }

            return result;
        }
    }
}