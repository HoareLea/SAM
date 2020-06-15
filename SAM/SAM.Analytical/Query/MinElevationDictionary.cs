using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<double, List<Panel>> MinElevationDictionary(this IEnumerable<Panel> panels, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (panels == null)
                return null;

            Dictionary<double, List<Panel>> result = new Dictionary<double, List<Panel>>();
            foreach (Panel panel in panels)
            {               
                double minElevation = Core.Query.Round(panel.MinElevation(), tolerance);

                List<Panel> panels_Elevation = null;
                if (!result.TryGetValue(minElevation, out panels_Elevation))
                {
                    panels_Elevation = new List<Panel>();
                    result[minElevation] = panels_Elevation;
                }

                panels_Elevation.Add(panel);
            }

            return result;
        }
    }
}