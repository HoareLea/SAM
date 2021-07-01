using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static ConstructionLibrary UpdateConstructionsByPanelType(this List<Panel> panels, ConstructionLibrary constructionLibrary)
        {
            if (panels == null || constructionLibrary == null)
                return null;

            ConstructionLibrary result = new ConstructionLibrary(constructionLibrary.Name);
            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                if(panel == null)
                {
                    continue;
                }

                PanelType panelType = panel.PanelType;

                Construction construction = result.GetConstructions(panelType)?.FirstOrDefault();
                if(construction == null)
                {
                    construction = constructionLibrary.GetConstructions(panelType)?.FirstOrDefault();
                    if (construction != null)
                    {
                        result.Add(construction);
                    }
                }

                if (construction == null)
                {
                    continue;
                }

                panels[i] = new Panel(panels[i], construction);
            }
            return result;
        }
    }
}