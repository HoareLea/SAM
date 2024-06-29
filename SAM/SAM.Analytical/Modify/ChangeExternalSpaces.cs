using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<ExternalSpace> ChangeExternalSpaces(this AdjacencyCluster adjacencyCluster, IEnumerable<Space> spaces, Construction construction_Wall, Construction construction_Floor, Construction construction_Roof, double silverSpace = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || spaces == null)
            {
                return null;
            }


            List<ExternalSpace> result = new List<ExternalSpace>();
            foreach(Space space in spaces)
            {
                List<Panel> panels = adjacencyCluster.GetPanels(space);
                if(panels == null || panels.Count == 0)
                {
                    continue; 
                }

                ExternalSpace externalSpace = new ExternalSpace(space.Name, space.Location);
                adjacencyCluster.AddObject(externalSpace);
                result.Add(externalSpace);

                foreach (Panel panel in panels)
                {
                    List<Space> spaces_Panel =  adjacencyCluster.GetRelatedObjects<Space>(panel);
                    if(spaces_Panel != null && spaces_Panel.Count > 1)
                    {
                        Panel panel_New = new Panel(panel);

                        PanelType panelType = panel.PanelType;

                        switch(panel.PanelGroup)
                        {
                            case PanelGroup.Wall:
                                panel_New = new Panel(panel_New, PanelType.WallExternal);
                                break;

                            case PanelGroup.Floor:
                                panel_New = new Panel(panel_New, PanelType.FloorExposed);
                                break;

                            case PanelGroup.Roof:
                                panel_New = new Panel(panel_New, PanelType.Roof);
                                break;
                        }

                        if(panel_New.PanelType != panelType)
                        {
                            Construction construction_Default = Query.DefaultConstruction(panel_New.PanelType);
                            if (construction_Default != null)
                            {
                                panel_New = new Panel(panel_New, construction_Default);
                            }
                        }

                        adjacencyCluster.AddObject(panel_New);
                        adjacencyCluster.AddRelation(panel_New, externalSpace);
                        continue;
                    }

                    Construction construction = null;

                    switch(panel.PanelGroup)
                    {
                        case PanelGroup.Roof:
                            construction = construction_Roof;
                            break;

                        case PanelGroup.Floor:
                            construction = construction_Floor;
                            break;

                        case PanelGroup.Wall:
                            construction = construction_Wall;
                            break;
                    }

                    ExternalPanel externalPanel = new ExternalPanel(panel.Face3D, construction);

                    adjacencyCluster.RemoveObject<Panel>(panel.Guid);
                    adjacencyCluster.AddObject(externalPanel);
                    adjacencyCluster.AddRelation(externalPanel, externalSpace);
                }

                adjacencyCluster.RemoveObject<Space>(space.Guid);
            }

            return result;
        }

        public static List<ExternalSpace> ChangeExternalSpaces(this AdjacencyCluster adjacencyCluster, IEnumerable<Point3D> point3Ds, Construction construction_Wall, Construction construction_Floor, Construction construction_Roof, double silverSpace = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(adjacencyCluster == null || point3Ds == null)
            {
                return null;
            }

            List<List<Space>> spacesList = adjacencyCluster.GetSpaces(point3Ds, false, silverSpace, tolerance);
            if(spacesList == null)
            {
                return null;
            }

            Dictionary<Guid, Space> dictioray = new Dictionary<Guid, Space>();
            foreach(List<Space> spaces in spacesList)
            {
                spaces?.ForEach(x => dictioray[x.Guid] = x);
            }

            return ChangeExternalSpaces(adjacencyCluster, dictioray.Values, construction_Wall, construction_Floor, construction_Roof, silverSpace, tolerance);
        }
    }
}