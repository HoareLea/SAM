using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool TryGetConstruction(IEnumerable<Panel> panels_All, IEnumerable<Panel> panels, out Panel panel, out Construction construction, out PanelType panelType)
        {
            construction = null;
            panel = null;
            panelType = Analytical.PanelType.Undefined;

            if (panels_All == null || panels_All.Count() == 0)
                return false;

            if (panels == null || panels.Count() == 0)
                panel = panels_All.First();
            else
                panel = panels.First();

            if (panels_All.Count() == 1)
            {
                if (PanelGroup(panel.PanelType) == Analytical.PanelGroup.Floor)
                {
                    if (panel.MinElevation() < Tolerance.MacroDistance)
                    {
                        //SlabOnGrad
                        panelType = Analytical.PanelType.SlabOnGrade;
                        construction = DefaultConstruction(panelType);
                    }
                    else
                    {
                        //Exposed
                        panelType = Analytical.PanelType.FloorExposed;
                        construction = DefaultConstruction(panelType);
                    }
                }
                else
                {
                    panelType = panel.PanelType;
                    construction = DefaultConstruction(panelType);
                }
            }
            else
            {
                List<Panel> panels_Temp = null;

                panels_Temp = panels?.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Wall);
                if(panels_Temp == null || panels_Temp.Count == 0)
                    panels_Temp = panels_All?.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Wall);

                if (panels_Temp != null && panels_Temp.Count > 0)
                {
                    if (panels_Temp.Count == 1)
                    {
                        //WallExternal
                        panel = panels_Temp[0];
                        panelType = Analytical.PanelType.WallExternal;
                        construction = DefaultConstruction(panelType);
                    }
                    else
                    {
                        //WallInternal
                        panel = panels_Temp.Find(x => x.PanelType == Analytical.PanelType.WallInternal);
                        if (panel == null)
                            panel = panels_Temp.First();

                        panelType = Analytical.PanelType.WallInternal;
                        construction = DefaultConstruction(panelType);
                    }
                    return true;
                }

                panels_Temp = panels?.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Floor);
                if (panels_Temp == null || panels_Temp.Count == 0)
                    panels_Temp = panels_All?.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Floor);

                if (panels_Temp != null && panels_Temp.Count > 0)
                {
                    if (panels_Temp.Count == 1)
                    {
                        Panel panel_Roof = panels_All.ToList().Find(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Roof);
                        if (panel_Roof != null)
                        {
                            //FloorInternal
                            panel = panels_Temp.Find(x => x.PanelType == Analytical.PanelType.FloorInternal);
                            if (panel == null)
                                panel = panels_Temp.First();

                            panelType = Analytical.PanelType.FloorInternal;
                            construction = DefaultConstruction(panelType);
                        }
                        else
                        {
                            //Floor
                            panel = panels_Temp[0];
                            panelType = Analytical.PanelType.Floor;
                            construction = DefaultConstruction(panelType);
                        }
                    }
                    else
                    {
                        //FloorInternal
                        panel = panels_Temp.Find(x => x.PanelType == Analytical.PanelType.FloorInternal);
                        if (panel == null)
                            panel = panels_Temp.First();

                        panelType = Analytical.PanelType.FloorInternal;
                        construction = DefaultConstruction(panelType);
                    }
                    return true;
                }

                panels_Temp = panels?.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Roof);
                if (panels_Temp == null || panels_Temp.Count == 0)
                    panels_Temp = panels_All?.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Roof);

                if (panels_Temp != null && panels_Temp.Count > 1)
                {
                    //FloorInternal
                    panel = panels_All.ToList().Find(x => x.PanelType == Analytical.PanelType.FloorInternal);
                    if (panel == null)
                        panel = panels_Temp.First();

                    panelType = Analytical.PanelType.FloorInternal;
                    construction = DefaultConstruction(panelType);
                    return true;
                }

                if (panels == null || panels.Count() == 0)
                    panel = panels_All.First();
                else
                    panel = panels.First();

                panelType = panel.PanelType;
                construction = DefaultConstruction(panelType);
            }

            return true;
        }
    }
}