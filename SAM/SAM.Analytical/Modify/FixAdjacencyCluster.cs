using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> FixAdjacencyCluster(this AdjacencyCluster adjacencyCluster, out List<string> prefixes)
        {
            prefixes = null;

            if (adjacencyCluster == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
                return null;

            Dictionary<PanelGroup, Dictionary<string, Tuple<Construction, List<Panel>>>> dictionary = new Dictionary<PanelGroup, Dictionary<string, Tuple<Construction, List<Panel>>>>();
            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                Construction construction = panel.Construction;
                if (construction == null)
                    continue;

                string name = construction.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                PanelGroup panelGroup = panel.PanelGroup;
                if(!dictionary.TryGetValue(panelGroup, out Dictionary<string, Tuple<Construction, List<Panel>>> dictionary_Temp))
                {
                    dictionary_Temp = new Dictionary<string, Tuple<Construction, List<Panel>>>();
                    dictionary[panelGroup] = dictionary_Temp;
                }

                if(!dictionary_Temp.TryGetValue(name, out Tuple<Construction, List<Panel>> tuple))
                {
                    tuple = new Tuple<Construction, List<Panel>>(construction, new List<Panel>());
                    dictionary_Temp[name] = tuple;

                }

                tuple.Item2.Add(panel);
            }

            Dictionary<string, Construction> dictionary_Result = new Dictionary<string, Construction>();
            List<Panel> result = new List<Panel>();
            prefixes = new List<string>();
            foreach (KeyValuePair<PanelGroup, Dictionary<string, Tuple<Construction, List<Panel>>>> keyValuePair_PanelGroup in dictionary)
            {
                foreach(KeyValuePair<string, Tuple<Construction, List<Panel>>> keyValuePair_Name in keyValuePair_PanelGroup.Value)
                {
                    Construction construction = keyValuePair_Name.Value.Item1;

                    List<Panel> panels_Temp = keyValuePair_Name.Value.Item2;
                    for(int i =0; i < panels_Temp.Count; i++)
                    {
                        panels_Temp[i] = new Panel(panels_Temp[i], construction);
                        adjacencyCluster.AddObject(panels_Temp[i]);
                    }

                    List<Panel> panels_Temp_External = panels_Temp?.FindAll(x => x.PanelGroup == PanelGroup.Floor || x.PanelGroup == PanelGroup.Roof);
                    if (panels_Temp_External != null && panels_Temp_External.Count != 0)
                    {
                        foreach(Panel panel in panels_Temp_External)
                        {
                            List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                            if(spaces== null || spaces.Count == 0)
                            {
                                adjacencyCluster.AddObject(new Panel(panel, PanelType.Shade));
                            }
                        }

                    }

                    panels_Temp?.RemoveAll(x => x.PanelGroup != PanelGroup.Wall);
                    if (panels_Temp == null || panels_Temp.Count == 0)
                        continue;

                    string name = keyValuePair_Name.Key;

                    for (int i = 0; i < panels_Temp.Count; i++)
                    {
                        Panel panel = panels_Temp[i];
                        Panel panel_New = null;

                        List <Space> spaces = adjacencyCluster.GetSpaces(panel);
                        if (spaces == null || spaces.Count == 0)
                        {
                            panel_New = new Panel(panel, PanelType.Shade);
                            adjacencyCluster.AddObject(panel_New);
                            continue;
                        }

                        bool external = spaces.Count == 1;

                        PanelType panelType = panel.PanelType;
                        if (external == panelType.External())
                            continue;

                        string name_New = null;
                        string prefix = null;
                        if (external)
                        {
                            if (name.StartsWith("SIM_INT_GLZ"))
                            {
                                name_New = "SIM_EXT_GLZ" + name.Substring(("SIM_INT_GLZ").Length);
                                prefix = "SIM_EXT_GLZ";
                            }
                            //else if (name.StartsWith("SIM_INT_SLD_Core"))
                            //{
                            //    name_New = "SIM_EXT_GRD" + name.Substring(("SIM_INT_SLD_Core").Length);
                            //    prefix = "SIM_INT_SLD_Core";
                            //}
                            else if (name.StartsWith("SIM_INT_SLD_Core") || name.StartsWith("SIM_INT_SLD_Partition"))
                            {
                                name_New = "SIM_EXT_SLD" + name.Substring(("SIM_INT_SLD_Core").Length);
                                prefix = "SIM_EXT_SLD";
                            }
                        }
                        else
                        {
                            if (name.StartsWith("SIM_EXT_GLZ"))
                            {
                                name_New = "SIM_INT_GLZ" + name.Substring(("SIM_EXT_GLZ").Length);
                                prefix = "SIM_INT_GLZ";
                            }
                            else if (name.StartsWith("SIM_EXT_GRD"))
                            {
                                name_New = "SIM_INT_SLD_Core" + name.Substring(("SIM_EXT_GRD").Length);
                                prefix = "SIM_INT_SLD_Core";
                            }
                            else if (name.StartsWith("SIM_EXT_SLD"))
                            {
                                name_New = "SIM_INT_SLD_Core" + name.Substring(("SIM_EXT_SLD").Length);
                                prefix = "SIM_INT_SLD_Core";
                            }
                                
                        }

                        if (string.IsNullOrWhiteSpace(name_New))
                            continue;

                        if (!dictionary_Result.TryGetValue(name_New, out Construction construction_New) || construction_New == null)
                            construction_New = new Construction(construction, name_New);

                        panel_New = new Panel(panel, construction_New);
                        if (external)
                            panel_New = new Panel(panel_New, PanelType.WallExternal);
                        else
                            panel_New = new Panel(panel_New, PanelType.WallInternal);

                        result.Add(panel_New);
                        prefixes.Add(prefix);
                    }
                }
            }

            if (result != null)
                result.ForEach(x => adjacencyCluster.AddObject(x));

            return result;
        }
    }
}