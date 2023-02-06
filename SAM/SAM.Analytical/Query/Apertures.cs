using SAM.Geometry.Spatial;
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

        public static List<Aperture> Apertures(this AdjacencyCluster adjacencyCluster, Point3D point3D, int maxCount = 1, double tolerance = Core.Tolerance.Distance)
        {
            if(adjacencyCluster == null || point3D == null)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
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

                if(apertures.Count > 1)
                {
                    if (!panel.GetBoundingBox().InRange(point3D, tolerance))
                    {
                        continue;
                    }
                }

                foreach(Aperture aperture in apertures)
                {
                    BoundingBox3D boundingBox3D = aperture?.GetBoundingBox();
                    if(boundingBox3D == null)
                    {
                        continue;
                    }

                    if(aperture.Face3D.InRange(point3D, tolerance))
                    {
                        result.Add(aperture);
                        if(result.Count == maxCount)
                        {
                            return result;
                        }
                    }
                }
            }

            return result;
        }
    }
}