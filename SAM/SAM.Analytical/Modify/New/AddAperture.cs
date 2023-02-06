using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Aperture AddAperture(this AdjacencyCluster adjacencyCluster, Aperture aperture, double maxDistance = Core.Tolerance.MacroDistance)
        {
            if(adjacencyCluster == null || aperture == null)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if(panels == null || panels.Count == 0)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_Aperture = aperture.GetBoundingBox();
            if(boundingBox3D_Aperture == null)
            {
                return null;
            }


            foreach (Panel panel in panels)
            {
                BoundingBox3D boundingBox3D_Panel = panel?.GetBoundingBox();
                if(boundingBox3D_Panel == null)
                {
                    continue;
                }

                if (!boundingBox3D_Aperture.InRange(boundingBox3D_Panel, maxDistance))
                {
                    continue;
                }

                if (!panel.AddAperture(aperture))
                {
                    continue;
                }

                adjacencyCluster.AddObject(panel);
                return panel.GetAperture(aperture.Guid);
            }

            return null;
        }
    }
}