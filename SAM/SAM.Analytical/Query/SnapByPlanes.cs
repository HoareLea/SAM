using System.Collections.Generic;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Panel SnapByPlanes(this Panel panel, IEnumerable<Plane> planes, double maxDistance)
        {
            Panel panel_New = new Panel(panel);
            panel_New.Snap(planes, maxDistance);

            return panel_New;
        }
    }
}