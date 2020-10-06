using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Aperture> UpdateApertures(this AdjacencyCluster adjacencyCluster, IEnumerable<Aperture> apertures)
        {
            if (adjacencyCluster == null || apertures == null)
                return null;

            if (apertures.Count() == 0)
                return null;

            List<Aperture> result = new List<Aperture>();

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null)
                return result;

            foreach(Panel panel in panels)
            {
                if (!panel.HasApertures)
                    continue;

                foreach(Aperture aperture in apertures)
                {
                    if (aperture == null)
                        continue;

                    Guid guid = aperture.Guid;

                    if (panel.RemoveAperture(guid))
                        if(panel.AddAperture(aperture))
                            if (adjacencyCluster.AddObject(panel))
                                result.Add(aperture);
                }
            }

            return result;
        }
    }
}