using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Aperture> Apertures(this AdjacencyCluster adjacencyCluster, ApertureConstruction apertureConstruction)
        {
            if (adjacencyCluster == null || apertureConstruction == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null)
                return null;

            Guid guid = apertureConstruction.Guid;

            List<Aperture> result = new List<Aperture>();
            foreach (Panel panel in panels)
            {
                List<Aperture> apertures = panel.Apertures;
                if (apertures == null || apertures.Count == 0)
                    continue;

                foreach (Aperture aperture in apertures)
                    if (aperture.TypeGuid.Equals(guid))
                        result.Add(aperture);
            }

            return result;
        }
    }
}