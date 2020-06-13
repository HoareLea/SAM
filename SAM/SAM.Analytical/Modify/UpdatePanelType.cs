using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> UpdatePanelType(this AdjacencyCluster adjacencyCluster)
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

            List<Panel> panelList = null;

            List<Panel> result = new List<Panel>();

            //Internal
            panelList = adjacencyCluster.GetInternalPanels();
            if (panelList != null && panelList.Count != 0)
            {
                foreach (Panel panel in panelList)
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
                    result.Add(panel_New);
                }
            }

            //External
            panelList = adjacencyCluster.GetExternalPanels();
            if (panelList != null && panelList.Count != 0)
            {
                foreach (Panel panel in panelList)
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
                    result.Add(panel_New);
                }
            }

            //Shading
            panelList = adjacencyCluster.GetShadingPanels();
            if (panelList != null && panelList.Count != 0)
            {
                foreach (Panel panel in panelList)
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
                    result.Add(panel_New);
                }
            }

            return result;
        }
    }
}