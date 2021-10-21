using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static IEnumerable<Panel> SetDefaultConstructionByPanelType(this AdjacencyCluster adjacencyCluster, IEnumerable<Guid> guids = null)
        {
            if (adjacencyCluster == null)
                return null;

            string wallInternalConstructionPrefix = "SIM_INT_";
            string wallExternalConstructionPrefix = "SIM_EXT_";
            string floorInternalConstructionPrefix = "SIM_INT_";
            string roofExternalConstructionPrefix = "SIM_EXT_";
            string floorExposedConstructionPrefix = "SIM_EXT_";
            string slabOnGradeConstructionPrefix = "SIM_EXT_";

            List<Panel> panels = adjacencyCluster.GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            if (guids != null && guids.Count() > 0)
                panels = panels.FindAll(x => guids.Contains(x.Guid));

            Dictionary<Guid, Panel> result = new Dictionary<Guid, Panel>();

            foreach (Panel panel in panels)
            {
                Construction construction = panel.Construction;
                if (construction == null)
                    continue;
                
                PanelType panelType = panel.PanelType;

                bool update = false;

                double elevation = double.NaN;

                switch (panelType)
                {
                    case PanelType.Air:
                        update = construction != null;
                        break;
                    case PanelType.Ceiling:
                        panelType = Query.PanelType(panel.Normal);
                        switch (panelType.PanelGroup())
                        {
                            case PanelGroup.Wall:
                                update = !construction.Name.StartsWith(wallExternalConstructionPrefix);
                                panelType = PanelType.WallExternal;
                                break;
                            case PanelGroup.Floor:
                                update = !construction.Name.StartsWith(floorExposedConstructionPrefix);
                                panelType = PanelType.FloorExposed;
                                break;
                            case PanelGroup.Roof:
                                update = !construction.Name.StartsWith(roofExternalConstructionPrefix);
                                panelType = PanelType.Roof;
                                break;
                        }
                        break;
                    case PanelType.CurtainWall:
                        update = !construction.Name.StartsWith(wallExternalConstructionPrefix);
                        panelType = PanelType.Roof;
                        break;
                    case PanelType.Wall:
                        elevation = Geometry.Spatial.Query.MaxElevation(panel);

                        if (elevation <= 0)
                        {
                            update = !construction.Name.StartsWith(wallExternalConstructionPrefix);
                            panelType = PanelType.UndergroundWall;
                        }
                        else
                        {
                            update = !construction.Name.StartsWith(wallExternalConstructionPrefix);
                            panelType = PanelType.WallExternal;
                        }
                        break;
                    case PanelType.WallExternal:
                        update = !construction.Name.StartsWith(wallExternalConstructionPrefix);
                        break;
                    case PanelType.WallInternal:
                        update = !panel.Construction.Name.StartsWith(wallInternalConstructionPrefix);
                        break;
                    case PanelType.Roof:
                        update = !construction.Name.StartsWith(roofExternalConstructionPrefix);
                        break;
                    case PanelType.Floor:
                        PanelType panelType_Normal = Query.PanelType(panel.Normal);
                        if (panelType_Normal == PanelType.Floor)
                        {
                            elevation = Geometry.Spatial.Query.MaxElevation(panel);

                            if (elevation == 0)
                            {
                                update = !construction.Name.StartsWith(slabOnGradeConstructionPrefix);
                                panelType = PanelType.SlabOnGrade;
                            }
                            else if (elevation < 0)
                            {
                                update = !construction.Name.StartsWith(slabOnGradeConstructionPrefix);
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
                        update = !construction.Name.StartsWith(slabOnGradeConstructionPrefix);
                        break;
                    case PanelType.UndergroundSlab:
                        update = !construction.Name.StartsWith(slabOnGradeConstructionPrefix);
                        break;
                    case PanelType.FloorInternal:
                        update = !construction.Name.StartsWith(floorInternalConstructionPrefix);
                        break;
                    case PanelType.UndergroundCeiling:
                        update = !construction.Name.StartsWith(floorInternalConstructionPrefix);
                        break;
                    case PanelType.FloorExposed:
                        update = !construction.Name.StartsWith(floorExposedConstructionPrefix) || panel.Construction.Name.Contains("_GRD_");
                        break;
                    case PanelType.FloorRaised:
                        update = !construction.Name.StartsWith(floorInternalConstructionPrefix);
                        break;
                    case PanelType.Shade:
                        update = !construction.Name.Contains("_SHD_");
                        break;
                    case PanelType.UndergroundWall:
                        update = !construction.Name.StartsWith(wallExternalConstructionPrefix) || !panel.Construction.Name.Contains("_GRD_");
                        break;
                }

                if (!update)
                    continue;

                construction = Query.DefaultConstruction(panelType);
                Panel panel_New = new Panel(panel, construction);
                adjacencyCluster.AddObject(panel_New);
                result[panel_New.Guid] = panel_New;
            }

            return result.Values;
        }
    }
}