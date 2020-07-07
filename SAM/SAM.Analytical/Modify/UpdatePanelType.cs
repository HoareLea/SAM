using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static IEnumerable<Panel> UpdatePanelType(this AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null)
                return null;

            string wallInternalConstructionPrefix = "SIM_INT_";
            string wallInternalConstructionSufix = "SLD_Partition";
            string wallExternalConstructionPrefix = "SIM_EXT_";
            string wallExternalConstructionSufix = "SLD";
            string wallShadingConstructionPrefix = "SIM_EXT_";
            string wallShadingConstructionSufix = "SLD";
            string floorInternalConstructionPrefix = "SIM_INT_";
            string floorInternalConstructionSufix = "SLD_FLR FLR02";
            string floorExternalConstructionPrefix = "SIM_EXT_GRD_";
            string floorExternalConstructionSufix = "FLR FLR01";
            string roofExternalConstructionPrefix = "SIM_EXT_";
            string roofExternalConstructionSufix = "SLD_Roof DA01";
            string floorExposedConstructionPrefix = "SIM_EXT_";
            string floorExposedConstructionSufix = "SLD_FLR Exposed";
            string slabOnGradeConstructionPrefix = "SIM_EXT_";
            string slabOnGradeConstructionSufix = "GRD_FLR FLR01";

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
                return null;

            Dictionary<System.Guid, Panel> result = new Dictionary<System.Guid, Panel>();

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
                    panelType = Query.PanelType(panel.Normal);

                    switch(panelType)
                    {
                        case PanelType.Shade:
                        case PanelType.Air:
                        case PanelType.Ceiling:
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
                            panelType = PanelType.WallExternal;
                            break;
                        
                        case PanelType.Floor:
                        case PanelType.FloorExposed:
                        case PanelType.FloorInternal:
                        case PanelType.UndergroundCeiling:
                            panelType = PanelType.FloorExposed;
                            break;
                    }                 

                }
                else if(spaces.Count > 1)
                {
                    panelType = Query.PanelType(panel.Normal);

                    switch (panelType)
                    {
                        case PanelType.Shade:
                        case PanelType.SolarPanel:
                        case PanelType.Air:
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

            


            //Internal
            panels = adjacencyCluster.GetInternalPanels();
            if (panels != null && panels.Count != 0)
            {
                foreach (Panel panel in panels)
                {
                    PanelType panelType = panel.PanelType;

                    Panel panel_New = panel;

                    if (panelType == PanelType.Wall || panelType == PanelType.WallExternal)
                    {
                        panelType = PanelType.WallInternal;

                        if (!panel_New.Construction.Name.StartsWith(wallInternalConstructionPrefix))
                            panel_New = new Panel(panel, new Construction(wallInternalConstructionPrefix + wallInternalConstructionSufix));
                    }
                    else if (panelType == PanelType.Roof || panelType == PanelType.Floor || panelType == PanelType.FloorExposed || panelType == PanelType.SlabOnGrade)
                    {
                        panelType = PanelType.FloorInternal;

                        if (!panel_New.Construction.Name.StartsWith(floorInternalConstructionPrefix))
                            panel_New = new Panel(panel, new Construction(floorInternalConstructionPrefix + floorInternalConstructionSufix));
                    }

                    if (panel_New.PanelType != panelType)
                        panel_New = new Panel(panel_New, panelType);

                    adjacencyCluster.AddObject(panel_New);
                    result[panel_New.Guid] = (panel_New);
                }
            }

            //External
            panels = adjacencyCluster.GetExternalPanels();
            if (panels != null && panels.Count != 0)
            {
                foreach (Panel panel in panels)
                {
                    PanelType panelType = panel.PanelType;

                    Panel panel_New = panel;

                    if (panelType == PanelType.Wall)
                    {
                        panelType = PanelType.WallExternal;

                        if (!panel_New.Construction.Name.StartsWith(wallExternalConstructionPrefix))
                            panel_New = new Panel(panel, new Construction(wallExternalConstructionPrefix + wallExternalConstructionSufix));
                    }
                    else if (panelType == PanelType.Floor || panelType == PanelType.Roof)
                    {
                        Geometry.Spatial.Vector3D vector3D_Normal = panel.PlanarBoundary3D?.Normal;

                        PanelType panelType_Normal = Analytical.Query.PanelType(vector3D_Normal);
                        if (panelType_Normal == PanelType.Floor)
                        {
                            double elevation = Analytical.Query.MaxElevation(panel_New);

                            if (elevation == 0)
                            {
                                panelType = PanelType.SlabOnGrade;

                                if (!panel_New.Construction.Name.StartsWith(floorExternalConstructionPrefix))
                                    panel_New = new Panel(panel, new Construction(floorExternalConstructionPrefix + floorExternalConstructionSufix));
                            }
                            else if (elevation < 0)
                            {
                                panelType = PanelType.SlabOnGrade;

                                if (!panel_New.Construction.Name.StartsWith(slabOnGradeConstructionPrefix))
                                    panel_New = new Panel(panel, new Construction(slabOnGradeConstructionPrefix + slabOnGradeConstructionSufix));
                            }
                            else
                            {
                                panelType = PanelType.FloorExposed;

                                if (!panel_New.Construction.Name.StartsWith(floorExposedConstructionPrefix))
                                    panel_New = new Panel(panel, new Construction(floorExposedConstructionPrefix + floorExposedConstructionSufix));
                            }
                        }
                        else if (panelType_Normal == PanelType.Roof)
                        {
                            panelType = PanelType.Roof;

                            if (!panel_New.Construction.Name.StartsWith(roofExternalConstructionPrefix))
                                panel_New = new Panel(panel, new Construction(roofExternalConstructionPrefix + roofExternalConstructionSufix));
                        }
                    }

                    if (panel_New.PanelType != panelType)
                        panel_New = new Panel(panel_New, panelType);

                    adjacencyCluster.AddObject(panel_New);
                    result[panel_New.Guid] = (panel_New);
                }
            }

            //Shading
            panels = adjacencyCluster.GetShadingPanels();
            if (panels != null && panels.Count != 0)
            {
                foreach (Panel panel in panels)
                {
                    PanelType panelType = panel.PanelType;

                    Panel panel_New = panel;

                    if (panelType == PanelType.Wall)
                    {
                        panelType = PanelType.WallExternal;

                        if (!panel_New.Construction.Name.StartsWith(wallShadingConstructionPrefix))
                            panel_New = new Panel(panel, new Construction(wallShadingConstructionPrefix + wallShadingConstructionSufix));
                    }

                    if (panel_New.PanelType != panelType)
                        panel_New = new Panel(panel_New, panelType);

                    adjacencyCluster.AddObject(panel_New);
                    result[panel_New.Guid] = (panel_New);
                }
            }

            return result.Values;
        }
    }
}