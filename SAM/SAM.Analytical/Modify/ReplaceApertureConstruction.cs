using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Aperture> ReplaceApertureConstruction(this AdjacencyCluster adjacencyCluster, IEnumerable<Guid> guids, ApertureConstruction apertureConstruction)
        {
            if (adjacencyCluster == null || guids == null || apertureConstruction == null)
                return null;
            
            
            List<Aperture> result = new List<Aperture>();

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
            {
                return result;
            }

            foreach(Panel panel in panels)
            {
                List<Aperture> apertures = panel?.Apertures;
                if(apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                Panel panel_New = null;
                foreach(Aperture aperture in apertures)
                {
                    if(aperture == null || !guids.Contains(aperture.Guid))
                    {
                        continue;
                    }

                    if(panel_New == null)
                    {
                        panel_New = new Panel(panel);
                    }

                    Aperture aperture_New = new Aperture(aperture, apertureConstruction);

                    panel_New.RemoveAperture(aperture.Guid);
                    panel_New.AddAperture(aperture_New);
                    result.Add(aperture_New);
                }

                if(panel_New != null)
                {
                    adjacencyCluster.AddObject(panel_New);
                }
            }

            return result;
        }
    }
}