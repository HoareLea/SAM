using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static IEnumerable<Panel> UpdateConstructions(this AdjacencyCluster adjacencyCluster, IEnumerable<Guid> guids = null)
        {
            if (adjacencyCluster == null)
                return null;

            string wallInternalConstructionPrefix = "SIM_INT_";
            string wallExternalConstructionPrefix = "SIM_EXT_";
            string wallShadingConstructionPrefix = "SIM_EXT_";
            string floorInternalConstructionPrefix = "SIM_INT_";
            string floorExternalConstructionPrefix = "SIM_EXT_GRD_";
            string roofExternalConstructionPrefix = "SIM_EXT_";
            string floorExposedConstructionPrefix = "SIM_EXT_";
            string slabOnGradeConstructionPrefix = "SIM_EXT_";

            List<Panel> panels = adjacencyCluster.GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            if (guids != null)
                panels = panels.FindAll(x => guids.Contains(x.Guid));

            Dictionary<Guid, Panel> result = new Dictionary<Guid, Panel>();

            foreach (Panel panel in panels)
            {
                PanelType panelType = panel.PanelType;

                bool update = false;

                double elevation = double.NaN;

                switch (panelType)
                {
                    case PanelType.Air:
                        update = panel.Construction != null;
                        break;
                    case PanelType.Ceiling:
                        panelType = Query.PanelType(panel.Normal);
                        switch (panelType.PanelGroup())
                        {
                            case PanelGroup.Wall:
                                update = !panel.Construction.Name.StartsWith(wallExternalConstructionPrefix);
                                panelType = PanelType.WallExternal;
                                break;
                            case PanelGroup.Floor:
                                update = !panel.Construction.Name.StartsWith(floorExposedConstructionPrefix);
                                panelType = PanelType.FloorExposed;
                                break;
                            case PanelGroup.Roof:
                                update = !panel.Construction.Name.StartsWith(roofExternalConstructionPrefix);
                                panelType = PanelType.Roof;
                                break;
                        }
                        break;
                    case PanelType.CurtainWall:
                        update = !panel.Construction.Name.StartsWith(wallExternalConstructionPrefix);
                        panelType = PanelType.Roof;
                        break;
                    case PanelType.Wall:
                        elevation = Query.MaxElevation(panel);

                        if (elevation <= 0)
                        {
                            update = !panel.Construction.Name.StartsWith(wallExternalConstructionPrefix);
                            panelType = PanelType.UndergroundWall;
                        }
                        else
                        {
                            update = !panel.Construction.Name.StartsWith(wallExternalConstructionPrefix);
                            panelType = PanelType.WallExternal;
                        }
                        break;
                    case PanelType.WallExternal:
                        update = !panel.Construction.Name.StartsWith(wallExternalConstructionPrefix);
                        break;
                    case PanelType.WallInternal:
                        update = !panel.Construction.Name.StartsWith(wallInternalConstructionPrefix);
                        break;
                    case PanelType.Roof:
                        update = !panel.Construction.Name.StartsWith(roofExternalConstructionPrefix);
                        break;
                    case PanelType.Floor:
                        PanelType panelType_Normal = Query.PanelType(panel.Normal);
                        if (panelType_Normal == PanelType.Floor)
                        {
                            elevation = Query.MaxElevation(panel);

                            if (elevation == 0)
                            {
                                update = !panel.Construction.Name.StartsWith(slabOnGradeConstructionPrefix);
                                panelType = PanelType.SlabOnGrade;
                            }
                            else if (elevation < 0)
                            {
                                update = !panel.Construction.Name.StartsWith(slabOnGradeConstructionPrefix);
                                panelType = PanelType.SlabOnGrade;
                            }
                            else
                            {
                                panelType = PanelType.FloorExposed;
                            }

                        }
                        else if (panelType_Normal == PanelType.Roof)
                        {
                            panelType = PanelType.Roof;
                        }
                        break;
                    case PanelType.SlabOnGrade:
                        update = !panel.Construction.Name.StartsWith(slabOnGradeConstructionPrefix);
                        break;
                    case PanelType.FloorInternal:
                        update = !panel.Construction.Name.StartsWith(floorInternalConstructionPrefix);
                        break;
                    case PanelType.FloorExposed:
                        update = !panel.Construction.Name.StartsWith(floorExposedConstructionPrefix);
                        break;
                    case PanelType.FloorRaised:
                        update = !panel.Construction.Name.StartsWith(floorInternalConstructionPrefix);
                        break;
                    case PanelType.Shade:
                        update = !panel.Construction.Name.StartsWith(wallShadingConstructionPrefix);
                        break;
                }

                if (!update)
                    continue;

                Construction construction = Query.Construction(panelType);
                Panel panel_New = new Panel(panel, construction);
                adjacencyCluster.AddObject(panel_New);
                result[panel_New.Guid] = panel_New;
            }

            return result.Values;
        }
    }
}