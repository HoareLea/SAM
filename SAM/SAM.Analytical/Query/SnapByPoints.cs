using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Panel SnapByPoints(this Panel panel, IEnumerable<Point3D> point3Ds, double maxDixtance = double.NaN)
        {
            Panel panel_New = new Panel(panel);
            panel_New.Snap(point3Ds, maxDixtance);

            return panel_New;
        }

        public static IEnumerable<Panel> SnapByPoints(this IEnumerable<Panel> panels, IEnumerable<Point3D> point3Ds, double maxDixtance = double.NaN)
        {
            return panels.ToList().ConvertAll(x => SnapByPoints(x, point3Ds, maxDixtance));
        }
    }
}