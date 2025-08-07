using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static IEnumerable<Panel> SetDefaultConstructionLayerByPanelType(this AdjacencyCluster adjacencyCluster, out List<Panel> panelsWithIssues, IEnumerable<Guid> guids = null)
        {
            panelsWithIssues = null;

            if (adjacencyCluster == null)
                return null;

            string wallInternalConstructionPrefix = "SIM_INT_";
            string wallExternalConstructionPrefix = "SIM_EXT_";
            string floorInternalConstructionPrefix = "SIM_INT_";
            string roofExternalConstructionPrefix = "SIM_EXT_";
            string floorExposedConstructionPrefix = "SIM_EXT_";
            string slabOnGradeConstructionPrefix = "SIM_EXT_";
            string floorConstructionName = "SIM_EXT_GRD_FLR";

            List<Panel> panels = adjacencyCluster.GetObjects<Panel>();
            if (panels == null || panels.Count == 0)
                return null;

            if (guids != null && guids.Count() > 0)
                panels = panels.FindAll(x => guids.Contains(x.Guid));

            List<Tuple<Panel, PanelType>> tuples = new List<Tuple<Panel, PanelType>>();

            foreach (Panel panel in panels)
            {
                PanelType panelType = panel.PanelType;
                if(panelType == PanelType.Undefined)
                {
                    continue;
                }

                bool update = false;

                Construction construction = panel.Construction;
                if (string.IsNullOrWhiteSpace(construction?.Name))
                {
                    if(panelType != PanelType.Air)
                    {
                        update = true;
                    }
                }
                else
                {
                    double elevation = double.NaN;

                    switch (panelType)
                    {
                        case PanelType.Air:
                            update = construction != null;
                            break;
                        case PanelType.Ceiling:
                            panelType = Query.PanelType(panel.Normal);
                            List<Space> spaces = adjacencyCluster?.GetRelatedObjects<Space>(panel);
                            switch (panelType.PanelGroup())
                            {
                                case PanelGroup.Wall:
                                    update = !construction.Name.StartsWith(wallExternalConstructionPrefix);
                                    panelType = spaces != null && spaces.Count > 1 ? PanelType.WallInternal : PanelType.WallExternal;
                                    break;
                                case PanelGroup.Floor:
                                    update = !construction.Name.StartsWith(floorExposedConstructionPrefix);
                                    panelType = spaces != null && spaces.Count > 1 ? PanelType.FloorInternal : PanelType.FloorExposed;
                                    break;
                                case PanelGroup.Roof:
                                    update = !construction.Name.StartsWith(roofExternalConstructionPrefix);
                                    panelType = spaces != null && spaces.Count > 1 ? PanelType.FloorInternal : PanelType.Roof;
                                    break;
                            }
                            break;
                        case PanelType.CurtainWall:
                            update = !construction.Name.StartsWith(wallExternalConstructionPrefix);
                            panelType = PanelType.Roof;
                            break;
                        case PanelType.Wall:
                            elevation = Geometry.Object.Spatial.Query.MaxElevation(panel);

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
                                elevation = Geometry.Object.Spatial.Query.MaxElevation(panel);

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
                            update = construction.Name.Equals(floorConstructionName) || !construction.Name.StartsWith(slabOnGradeConstructionPrefix) || !construction.Name.Contains("_GRD_") || construction.Name.Contains("Roof");
                            break;
                        case PanelType.UndergroundSlab:
                            update = !construction.Name.StartsWith(slabOnGradeConstructionPrefix) || !construction.Name.Contains("_GRD_") || construction.Name.Contains("Roof");
                            break;
                        case PanelType.FloorInternal:
                            update = !construction.Name.StartsWith(floorInternalConstructionPrefix);
                            break;
                        case PanelType.UndergroundCeiling:
                            update = !construction.Name.StartsWith(floorInternalConstructionPrefix) || !construction.Name.Contains("_GRD_");
                            break;
                        case PanelType.FloorExposed:
                            update = !construction.Name.StartsWith(floorExposedConstructionPrefix) || construction.Name.Contains("_GRD_") || construction.Name.Contains("Roof");
                            break;
                        case PanelType.FloorRaised:
                            update = !construction.Name.StartsWith(floorInternalConstructionPrefix);
                            break;
                        case PanelType.Shade:
                            update = !construction.Name.Contains("_SHD_");
                            break;
                        case PanelType.UndergroundWall:
                            update = !construction.Name.StartsWith(wallExternalConstructionPrefix) || !construction.Name.Contains("_GRD_");
                            break;
                    }
                }

                if (!update)
                {
                    continue;
                }

                if(tuples.Find(x => x.Item1.Guid == panel.Guid) != null)
                {
                    continue;
                }

                tuples.Add(new Tuple<Panel, PanelType>(panel, panelType));

                //result[panel.Guid] = new Panel(panel, panel.Construction == null ? new Construction(construction) : new Construction(panel.Construction, construction.ConstructionLayers));
            }

            panelsWithIssues = new List<Panel>();

            Dictionary<Guid, Panel> dictionary = new Dictionary<Guid, Panel>();
            while (tuples.Count > 0)
            {
                Tuple<Panel, PanelType> tuple = tuples[0];

                Panel panel = tuple.Item1;
                Construction construction = panel.Construction;

                List<Tuple<Panel, PanelType>> tuples_Construction = tuples.FindAll(x => x.Item1.Construction?.Guid == construction?.Guid);
                tuples_Construction.ForEach(x => tuples.Remove(x));

                IEnumerable<PanelType> panelTypes = tuples_Construction.ConvertAll(x => x.Item2).Distinct();
                foreach(PanelType panelType in panelTypes)
                {
                    List<Tuple<Panel, PanelType>> tuples_PanelType = tuples_Construction.FindAll(x => x.Item2 == panelType);
                    foreach(Tuple<Panel, PanelType> tuple_PanelType in tuples_PanelType)
                    {
                        Construction construction_Default = Query.DefaultConstruction(panelType);
                        if (construction_Default == null)
                        {
                            continue;
                        }

                        Construction construction_PanelType = construction == null ? null : new Construction(Guid.NewGuid(), construction, construction.Name);

                        Panel panel_New = new Panel(panel, construction_PanelType == null ? new Construction(construction_PanelType) : new Construction(construction_PanelType, construction_Default.ConstructionLayers));
                        adjacencyCluster.AddObject(panel_New);
                        dictionary[panel_New.Guid] = panel_New;

                        if(panelTypes.Count() > 1)
                        {
                            panelsWithIssues.Add(panel_New);
                        }
                    }
                }
            }

            return dictionary.Values;
        }
    }
}