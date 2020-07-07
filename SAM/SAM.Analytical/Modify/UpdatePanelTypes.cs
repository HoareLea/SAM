using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static IEnumerable<Panel> UpdatePanelTypes(this AdjacencyCluster adjacencyCluster, IEnumerable<Guid> guids = null)
        {
            if (adjacencyCluster == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            if (guids != null && guids.Count() > 0)
                panels = panels.FindAll(x => guids.Contains(x.Guid));

            Dictionary<Guid, Panel> result = new Dictionary<Guid, Panel>();

            foreach (Panel panel in panels)
            {
                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(panel);
                PanelType panelType = panel.PanelType;

                if(spaces == null || spaces.Count == 0)
                {
                    if (panelType != PanelType.SolarPanel)
                        panelType = PanelType.Shade;
                }
                else if(spaces.Count == 1)
                {
                    double elevation = double.NaN;

                    switch (panelType)
                    {
                        case PanelType.Shade:
                        case PanelType.Air:
                        case PanelType.Ceiling:
                            panelType = Query.PanelType(panel.Normal);
                            switch (panelType.PanelGroup())
                            {
                                case PanelGroup.Wall:
                                    panelType = PanelType.WallExternal;
                                    break;
                                case PanelGroup.Floor:
                                    panelType = PanelType.FloorExposed;
                                    break;
                                case PanelGroup.Roof:
                                    panelType = PanelType.Roof;
                                    break;
                            }
                            break;
                        
                        case PanelType.Wall:
                        case PanelType.WallInternal:
                            
                            elevation = Query.MaxElevation(panel);

                            if (elevation <= 0)
                                panelType = PanelType.UndergroundWall;
                            else
                                panelType = PanelType.WallExternal;

                            break;
                        
                        case PanelType.FloorInternal:
                        case PanelType.UndergroundCeiling:
                            panelType = PanelType.FloorExposed;
                            break;

                        case PanelType.Floor:
                            PanelType panelType_Normal = Query.PanelType(panel.Normal);
                            if (panelType_Normal == PanelType.Floor)
                            {
                                elevation = Query.MaxElevation(panel);

                                if (elevation == 0)
                                    panelType = PanelType.SlabOnGrade;
                                else if (elevation < 0)
                                    panelType = PanelType.SlabOnGrade;
                                else
                                    panelType = PanelType.FloorExposed;
                            }
                            else if (panelType_Normal == PanelType.Roof)
                            {
                                panelType = PanelType.Roof;
                            }
                            break;
                    }                 

                }
                else if(spaces.Count > 1)
                {
                    switch (panelType)
                    {
                        case PanelType.Shade:
                        case PanelType.SolarPanel:
                        case PanelType.Air:
                            panelType = Query.PanelType(panel.Normal);
                            switch (panelType.PanelGroup())
                            {
                                case PanelGroup.Wall:
                                    panelType = PanelType.WallInternal;
                                    break;
                                case PanelGroup.Floor:
                                    panelType = PanelType.FloorInternal;
                                    break;
                            }
                            break;

                        case PanelType.Wall:
                        case PanelType.WallExternal:
                        case PanelType.CurtainWall:
                        case PanelType.UndergroundWall:
                            panelType = PanelType.WallInternal;
                            break;

                        case PanelType.Floor:
                        case PanelType.FloorExposed:
                        case PanelType.FloorRaised:
                        case PanelType.Roof:
                        case PanelType.SlabOnGrade:
                        case PanelType.UndergroundSlab:
                            panelType = PanelType.FloorInternal;
                            break;
                    }
                }

                if(panelType != panel.PanelType)
                {
                    Panel panel_New = new Panel(panel, panelType);
                    adjacencyCluster.AddObject(panel_New);
                    result[panel_New.Guid] = panel_New;
                }

            }

            return result.Values;
        }
    }
}