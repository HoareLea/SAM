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
    }
}