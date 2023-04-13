using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static IEnumerable<Aperture> SetDefaultApertureConstruction(this AdjacencyCluster adjacencyCluster, IEnumerable<Guid> guids = null)
        {
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return null;
            }


            List<Aperture> result = new List<Aperture>();
            foreach(Panel panel in panels)
            {
                List<Aperture> apertures = panel?.Apertures;
                if(apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                PanelType panelType = panel.PanelType;

                List<Tuple<Aperture, ApertureConstruction>> tuples = new List<Tuple<Aperture, ApertureConstruction>>();
                foreach(Aperture aperture in apertures)
                {
                    if(aperture == null)
                    {
                        continue;
                    }

                    if (guids != null && guids.Count() != 0 && !guids.Contains(aperture.Guid))
                    {
                        continue;
                    }

                    ApertureConstruction apertureConstruction = Query.DefaultApertureConstruction(panel, aperture.ApertureType);
                    if(apertureConstruction == null)
                    {
                        continue;
                    }

                    if(aperture.Name != null && apertureConstruction.Name == aperture.Name)
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<Aperture, ApertureConstruction>(aperture, apertureConstruction));
                }

                if(tuples == null || tuples.Count == 0)
                {
                    continue;
                }

                Panel panel_New = new Panel(panel);

                foreach(Tuple<Aperture, ApertureConstruction> tuple in tuples)
                {
                    Aperture aperture = new Aperture(tuple.Item1, tuple.Item2);
                    if(aperture == null)
                    {
                        continue;
                    }

                    if(panel_New.RemoveAperture(aperture.Guid))
                    {
                        panel_New.AddAperture(aperture);
                        result.Add(aperture);
                    }
                }

                adjacencyCluster.AddObject(panel_New);
            }

            return result;
        }
    }
}