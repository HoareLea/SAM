using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool TryGetConstruction(IEnumerable<Panel> panels, out Panel panel, out Construction construction, out PanelType panelType)
        {
            construction = null;
            panel = null;
            panelType = Analytical.PanelType.Undefined;

            if (panels == null || panels.Count() == 0)
                return false;

            panel = panels.First();

            if (panels.Count() == 1)
            {
                if (PanelGroup(panel.PanelType) == Analytical.PanelGroup.Floor)
                {
                    if (panel.MinElevation() < Tolerance.MacroDistance)
                    {
                        //SlabOnGrad
                        panelType = Analytical.PanelType.SlabOnGrade;
                        construction = Construction(panelType);
                    }
                    else
                    {
                        //Exposed
                        panelType = Analytical.PanelType.FloorExposed;
                        construction = Construction(panelType);
                    }
                }
                else
                {
                    panelType = panel.PanelType;
                    construction = Query.Construction(panelType);
                }
            }
            else
            {
                List<Panel> panels_Temp = null;

                panels_Temp = panels.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Wall);
                if (panels_Temp != null && panels_Temp.Count > 0)
                {
                    if (panels_Temp.Count == 1)
                    {
                        //WallExternal
                        panel = panels_Temp[0];
                        panelType = Analytical.PanelType.WallExternal;
                        construction = Construction(panelType);
                    }
                    else
                    {
                        //WallInternal
                        panel = panels_Temp.Find(x => x.PanelType == Analytical.PanelType.WallInternal);
                        if (panel == null)
                            panel = panels_Temp.First();

                        panelType = Analytical.PanelType.WallInternal;
                        construction = Construction(panelType);
                    }
                    return true;
                }

                panels_Temp = panels.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Floor);
                if (panels_Temp != null && panels_Temp.Count > 0)
                {
                    if (panels_Temp.Count == 1)
                    {
                        Panel panel_Roof = panels.ToList().Find(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Roof);
                        if (panel_Roof != null)
                        {
                            //FloorInternal
                            panel = panels_Temp.Find(x => x.PanelType == Analytical.PanelType.FloorInternal);
                            if (panel == null)
                                panel = panels_Temp.First();

                            panelType = Analytical.PanelType.FloorInternal;
                            construction = Query.Construction(panelType);
                        }
                        else
                        {
                            //Floor
                            panel = panels_Temp[0];
                            panelType = Analytical.PanelType.Floor;
                            construction = Construction(panelType);
                        }
                    }
                    else
                    {
                        //FloorInternal
                        panel = panels_Temp.Find(x => x.PanelType == Analytical.PanelType.FloorInternal);
                        if (panel == null)
                            panel = panels_Temp.First();

                        panelType = Analytical.PanelType.FloorInternal;
                        construction = Query.Construction(panelType);
                    }
                    return true;
                }

                panels_Temp = panels.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Roof);
                if (panels_Temp != null && panels_Temp.Count > 1)
                {
                    //FloorInternal
                    panel = panels.ToList().Find(x => x.PanelType == Analytical.PanelType.FloorInternal);
                    if (panel == null)
                        panel = panels_Temp.First();

                    panelType = Analytical.PanelType.FloorInternal;
                    construction = Query.Construction(panelType);
                    return true;
                }

                panel = panels.First();
                panelType = panel.PanelType;
                construction = Construction(panelType);
            }

            return true;
        }
    }
}