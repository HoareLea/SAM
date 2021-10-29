using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void SimplifyByAngle(this AdjacencyCluster adjacencyCluster, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            List<Panel> panels = adjacencyCluster?.GetPanels();
            if(panels == null)
            {
                return;
            }


            foreach(Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if(face3D == null)
                {
                    continue;
                }

                face3D = face3D.SimplifyByAngle(tolerance_Angle, tolerance_Distance);
                Panel panel_New = Create.Panel(panel.Guid, panel, face3D);
                List<Aperture> apertures = panel_New.Apertures;
                if(apertures != null)
                {
                    foreach(Aperture aperture in apertures)
                    {
                        Face3D face3D_Aperture = aperture?.Face3D;
                        if(face3D_Aperture == null)
                        {
                            continue;
                        }

                        face3D_Aperture = face3D_Aperture.SimplifyByAngle(tolerance_Angle, tolerance_Distance);
                        Aperture aperture_New = Create.Aperture(aperture, face3D_Aperture, aperture.Guid);
                        if(aperture_New != null)
                        {
                            panel_New.RemoveAperture(aperture.Guid);
                            panel_New.AddAperture(aperture_New);
                        }
                    }
                }

                adjacencyCluster.AddObject(panel_New);
            }
        }
    }
}