using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Panel Closest(this IEnumerable<Panel> panels, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if(panels == null || point3D == null)
            {
                return null;
            }

            double distance = double.MaxValue;
            Panel result = null;
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if(face3D == null)
                {
                    continue;
                }

                double distance_Panel = face3D.Distance(point3D, tolerance);
                if(distance_Panel < distance)
                {
                    result = panel;
                    distance = distance_Panel;
                }
            }

            return result;
        }
    }
}