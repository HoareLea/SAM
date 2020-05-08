using SAM.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool TryGetConstruction(IEnumerable<Panel> panels, out Panel panel, out Construction construction)
        {
            construction = null;
            panel = null;
            
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
                        construction = Construction(Analytical.PanelType.SlabOnGrade);
                    }
                    else
                    {
                        //Exposed
                        construction = Construction(Analytical.PanelType.FloorExposed);
                    }
                }
                else
                {
                    construction = Query.Construction(panel.PanelType);
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
                        construction = Construction(Analytical.PanelType.WallExternal);
                    }
                    else
                    {
                        //WallInternal
                        panel = panels_Temp.Find(x => x.PanelType == Analytical.PanelType.WallInternal);
                        if (panel == null)
                            panel = panels_Temp.First();

                        construction = Construction(Analytical.PanelType.WallInternal);
                    }
                    return true;
                }

                panels_Temp = panels.ToList().FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Floor);
                if (panels_Temp != null && panels_Temp.Count > 0)
                {
                    if (panels_Temp.Count == 1)
                    {
                        Panel panel_Roof = panels.ToList().Find(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Roof);
                        if(panel_Roof != null)
                        {
                            //FloorInternal
                            panel = panels_Temp.Find(x => x.PanelType == Analytical.PanelType.FloorInternal);
                            if (panel == null)
                                panel = panels_Temp.First();

                            construction = Query.Construction(Analytical.PanelType.FloorInternal);
                        }
                        else
                        {
                            //Floor
                            panel = panels_Temp[0];
                            construction = Construction(Analytical.PanelType.Floor);
                        }
                    }
                    else
                    {
                        //FloorInternal
                        panel = panels_Temp.Find(x => x.PanelType == Analytical.PanelType.FloorInternal);
                        if (panel == null)
                            panel = panels_Temp.First();

                        construction = Query.Construction(Analytical.PanelType.FloorInternal);
                    }
                    return true;
                }

                panel = panels.First();
                construction = Construction(panel.PanelType);
            }

            return true;
        }
    }
}