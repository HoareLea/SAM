using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void FixAdjacencyCluster(this AdjacencyCluster adjacencyCluster, out List<string> prefixes_Panel, out List<Panel> panels, out List<string> prefixes_Aperture, out List<Aperture> apertures)
        {
            prefixes_Panel = null;
            panels = null;
            prefixes_Aperture = null;
            apertures = null;

            if (adjacencyCluster == null)
                return;

            List<Panel> panels_AdjacencyCluster = adjacencyCluster.GetPanels();
            if (panels_AdjacencyCluster == null || panels_AdjacencyCluster.Count == 0)
                return;

            Dictionary<PanelGroup, Dictionary<string, Tuple<Construction, List<Panel>>>> dictionary = new Dictionary<PanelGroup, Dictionary<string, Tuple<Construction, List<Panel>>>>();
            foreach(Panel panel in panels_AdjacencyCluster)
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

            Dictionary<string, Construction> dictionary_Construction = new Dictionary<string, Construction>();
            Dictionary<string, ApertureConstruction> dictionary_ApertureConstruction = new Dictionary<string, ApertureConstruction>();

            panels = new List<Panel>();
            prefixes_Panel = new List<string>();
            
            apertures = new List<Aperture>();
            prefixes_Aperture = new List<string>();

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

                    string name_Panel = keyValuePair_Name.Key;

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
                        if(panelType == PanelType.Wall)
                        {
                            panelType = external ? PanelType.WallExternal : PanelType.WallInternal;
                            panel = new Panel(panel, panelType);
                            adjacencyCluster.AddObject(panel);
                        }
                        else
                        {
                            if (external == panelType.External() && panelType.External() != panelType.Internal())
                                continue;
                        }

                        string name_New = null;
                        string prefix = null;
                        if (external)
                        {
                            if (name_Panel.StartsWith("SIM_INT_GLZ"))
                            {
                                name_New = "SIM_EXT_GLZ" + name_Panel.Substring(("SIM_INT_GLZ").Length);
                                prefix = "SIM_EXT_GLZ";
                            }
                            //else if (name.StartsWith("SIM_INT_SLD_Core"))
                            //{
                            //    name_New = "SIM_EXT_GRD" + name.Substring(("SIM_INT_SLD_Core").Length);
                            //    prefix = "SIM_INT_SLD_Core";
                            //}
                            else if (name_Panel.StartsWith("SIM_INT_SLD_Core"))
                            {
                                name_New = "SIM_EXT_SLD" + name_Panel.Substring(("SIM_INT_SLD_Core").Length);
                                prefix = "SIM_EXT_SLD";
                            }
                            else if (name_Panel.StartsWith("SIM_INT_SLD_Partition"))
                            {
                                name_New = "SIM_EXT_SLD" + name_Panel.Substring(("SIM_INT_SLD_Partition").Length);
                                prefix = "SIM_EXT_SLD";
                            }
                        }
                        else
                        {
                            if (name_Panel.StartsWith("SIM_EXT_GLZ"))
                            {
                                name_New = "SIM_INT_GLZ" + name_Panel.Substring(("SIM_EXT_GLZ").Length);
                                prefix = "SIM_INT_GLZ";
                            }
                            else if (name_Panel.StartsWith("SIM_EXT_GRD"))
                            {
                                name_New = "SIM_INT_SLD_Core" + name_Panel.Substring(("SIM_EXT_GRD").Length);
                                prefix = "SIM_INT_SLD_Core";
                            }
                            else if (name_Panel.StartsWith("SIM_EXT_SLD"))
                            {
                                name_New = "SIM_INT_SLD_Core" + name_Panel.Substring(("SIM_EXT_SLD").Length);
                                prefix = "SIM_INT_SLD_Core";
                            }
                                
                        }

                        if (string.IsNullOrWhiteSpace(name_New))
                            continue;

                        if (!dictionary_Construction.TryGetValue(name_New, out Construction construction_New) || construction_New == null)
                        {
                            construction_New = new Construction(construction, name_New);
                        }  

                        panel_New = new Panel(panel, construction_New);
                        if (external)
                            panel_New = new Panel(panel_New, PanelType.WallExternal);
                        else
                            panel_New = new Panel(panel_New, PanelType.WallInternal);

                        System.Drawing.Color color = Query.Color(panel_New.PanelType);
                        if(color != System.Drawing.Color.Empty)
                        {
                            panel_New.SetValue(PanelParameter.Color, color);
                            construction_New.SetValue(ConstructionParameter.Color, color);
                        }

                        List<Aperture> apertures_Panel = panel_New.Apertures;
                        if(apertures_Panel != null && apertures_Panel.Count != 0)
                        {
                            foreach(Aperture aperture in apertures_Panel)
                            {
                                ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
                                if(apertureConstruction != null)
                                {
                                    string name_Aperture = apertureConstruction.Name;
                                    string prefix_Aperture = null;
                                    name_New = null;
                                    if (external)
                                    {
                                        if (name_Aperture.StartsWith("SIM_INT_GLZ"))
                                        {
                                            name_New = "SIM_EXT_GLZ" + name_Aperture.Substring(("SIM_INT_GLZ").Length);
                                            prefix_Aperture = "SIM_EXT_GLZ";
                                        }
                                        else if (name_Aperture.StartsWith("SIM_INT_SLD"))
                                        {
                                            name_New = "SIM_EXT_SLD" + name_Aperture.Substring(("SIM_INT_SLD").Length);
                                            prefix_Aperture = "SIM_EXT_SLD";
                                        }
                                    }
                                    else
                                    {
                                        if (name_Aperture.StartsWith("SIM_EXT_GLZ"))
                                        {
                                            name_New = "SIM_INT_GLZ" + name_Aperture.Substring(("SIM_EXT_GLZ").Length);
                                            prefix_Aperture = "SIM_INT_GLZ";
                                        }
                                        else if (name_Aperture.StartsWith("SIM_EXT_SLD"))
                                        {
                                            name_New = "SIM_INT_SLD" + name_Aperture.Substring(("SIM_EXT_SLD").Length);
                                            prefix_Aperture = "SIM_INT_SLD";
                                        }
                                    }

                                    if (string.IsNullOrWhiteSpace(name_New))
                                        continue;

                                    if (!dictionary_ApertureConstruction.TryGetValue(name_New, out ApertureConstruction apertureConstruction_New) || apertureConstruction_New == null)
                                    {
                                        apertureConstruction_New = new ApertureConstruction(apertureConstruction, name_New);
                                    }

                                    if(apertureConstruction_New.ApertureType == ApertureType.Door)
                                    {
                                        color = Query.Color(apertureConstruction_New.ApertureType, AperturePart.Frame);
                                    }
                                    else if(apertureConstruction_New.ApertureType == ApertureType.Window)
                                    {
                                        color = Query.Color(apertureConstruction_New.ApertureType, AperturePart.Pane);
                                    }

                                    if (color != System.Drawing.Color.Empty)
                                    {
                                        apertureConstruction_New.SetValue(ApertureConstructionParameter.Color, color);
                                    }

                                    Aperture aperture_New = new Aperture(aperture, apertureConstruction_New);

                                    panel_New.RemoveAperture(aperture.Guid);
                                    panel_New.AddAperture(aperture_New);
                                }

                            }
                        }

                        panels.Add(panel_New);
                        prefixes_Panel.Add(prefix);
                    }
                }
            }

            if (panels != null)
                panels.ForEach(x => adjacencyCluster.AddObject(x));
        }
    }
}