using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Area(this IEnumerable<Panel> panels, params PanelGroup[] panelGroups)
        {
            if (panels == null)
                return double.NaN;

            bool filter = panelGroups != null && panelGroups.Length > 0;

            double result = 0;
            foreach(Panel panel in panels)
            {
                if (filter && !panelGroups.Contains(panel.PanelType.PanelGroup()))
                    continue;

                result += panel.GetArea();
            }

            return result;
        }

        public static double Area(this Space space)
        {
            if (space == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(space, ParameterName_Area(), out result))
                return double.NaN;

            return result;
        }
    }
}