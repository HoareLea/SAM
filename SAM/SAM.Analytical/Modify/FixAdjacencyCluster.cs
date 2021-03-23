using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> FixAdjacencyCluster(this AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null)
                return null;

            List<Construction> constructions = adjacencyCluster.GetConstructions();
            if (constructions == null || constructions.Count == 0)
                return null;

            Dictionary<string, Construction> dictionary = new Dictionary<string, Construction>();
            List<Panel> result = new List<Panel>();
            foreach(Construction construction in constructions)
            {
                string name = construction.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                List<Panel> panels = adjacencyCluster.GetPanels(construction);
                panels?.RemoveAll(x => x.PanelGroup != PanelGroup.Wall);
                if (panels == null || panels.Count == 0)
                    continue;

                List<bool> externals = panels.ConvertAll(x => adjacencyCluster.External(x));

                IEnumerable<bool> externals_Unique = externals.Distinct();
                if (externals_Unique == null || externals_Unique.Count() <= 1)
                    continue;

                for (int i = 0; i < panels.Count; i++)
                {
                    Panel panel = panels[i];
                    bool external = externals[i];

                    PanelType panelType = panel.PanelType;
                    if (external == panelType.External())
                        continue;

                    string name_New = null;
                    if(external)
                    {
                        if (name.StartsWith("SIM_INT_GLZ"))
                            name_New = "SIM_EXT_GLZ" + name.Substring(("SIM_INT_GLZ").Length);
                        else if (name.StartsWith("SIM_INT_SLD_Core"))
                            name_New = "SIM_EXT_GRD" + name.Substring(("SIM_INT_SLD_Core").Length);
                        else if (name.StartsWith("SIM_INT_SLD_Core"))
                            name_New = "SIM_EXT_SLD" + name.Substring(("SIM_INT_SLD_Core").Length);

                    }
                    else
                    {
                        if (name.StartsWith("SIM_EXT_GLZ"))
                            name_New = "SIM_INT_GLZ" + name.Substring(("SIM_EXT_GLZ").Length);
                        else if (name.StartsWith("SIM_EXT_GRD"))
                            name_New = "SIM_INT_SLD_Core" + name.Substring(("SIM_EXT_GRD").Length);
                        else if (name.StartsWith("SIM_EXT_SLD"))
                            name_New = "SIM_INT_SLD_Core" + name.Substring(("SIM_EXT_SLD").Length);
                    }

                    if (string.IsNullOrWhiteSpace(name_New))
                        continue;

                    if (!dictionary.TryGetValue(name_New, out Construction construction_New) || construction_New == null)
                        construction_New = new Construction(construction, name_New);

                    Panel panel_New = new Panel(panel, construction_New);
                    if (external)
                        panel_New = new Panel(panel_New, PanelType.WallExternal);
                    else
                        panel_New = new Panel(panel_New, PanelType.WallInternal);

                    result.Add(panel_New);
                }
            }

            if (result != null)
                result.ForEach(x => adjacencyCluster.AddObject(x));

            return result;
        }
    }
}