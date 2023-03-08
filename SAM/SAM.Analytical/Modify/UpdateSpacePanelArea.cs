using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Dictionary<string, double> UpdateSpacePanelArea(this AdjacencyCluster adjacencyCluster, Space space)
        {
            if(adjacencyCluster == null || space == null)
            {
                return null;
            }


            Space space_Temp = adjacencyCluster.GetObject<Space>(space.Guid);
            if(space_Temp == null)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels(space_Temp);
            if(panels == null || panels.Count == 0)
            {
                return null;
            }

            Dictionary<string, double> result = new Dictionary<string, double>();
            result["SAM_Air"] = 0;
            result["SAM_Ceiling"] = 0;
            result["SAM_CurtainWall_External"] = 0;
            result["SAM_CurtainWall_Internal"] = 0;
            result["SAM_Floor"] = 0;
            result["SAM_FloorExposed"] = 0;
            result["SAM_FloorInternal"] = 0;
            result["SAM_FloorRaised"] = 0;
            result["SAM_Roof"] = 0;
            result["SAM_Shade"] = 0;
            result["SAM_SlabOnGrade"] = 0;
            result["SAM_SolarPanel"] = 0;
            result["SAM_UndergroundCeiling"] = 0;
            result["SAM_UndergroundSlab"] = 0;
            result["SAM_UndergroundWall"] = 0;
            result["SAM_Wall"] = 0;
            result["SAM_WallExternal"] = 0;
            result["SAM_WallInternal"] = 0;
            result["SAM_WindowFrame_External"] = 0;
            result["SAM_WindowFrame_Internal"] = 0;
            result["SAM_DoorFrame_External"] = 0;
            result["SAM_DoorFrame_Internal"] = 0;
            result["SAM_WindowPane_External"] = 0;
            result["SAM_WindowPane_Internal"] = 0;
            result["SAM_DoorPane_External"] = 0;
            result["SAM_DoorPane_Internal"] = 0;

            foreach (Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }

                double area = panel.GetAreaNet();
                if(double.IsNaN(area) || area <= 0)
                {
                    continue;
                }

                bool external = adjacencyCluster.External(panel);

                switch (panel.PanelType)
                {
                    case PanelType.Air:
                        result["SAM_Air"] += area;
                        break;

                    case PanelType.Ceiling:
                        result["SAM_Ceiling"] += area;
                        break;

                    case PanelType.CurtainWall:
                        if(external)
                        {
                            result["SAM_CurtainWall_External"] += area;
                        }
                        else
                        {
                            result["SAM_CurtainWall_Internal"] += area;
                        }
                        break;

                    case PanelType.Floor:
                        result["SAM_Floor"] += area;
                        break;

                    case PanelType.FloorExposed:
                        result["SAM_FloorExposed"] += area;
                        break;

                    case PanelType.FloorInternal:
                        result["SAM_FloorInternal"] += area;
                        break;

                    case PanelType.FloorRaised:
                        result["SAM_FloorRaised"] += area;
                        break;

                    case PanelType.Roof:
                        result["SAM_Roof"] += area;
                        break;

                    case PanelType.Shade:
                        result["SAM_Shade"] += area;
                        break;

                    case PanelType.SlabOnGrade:
                        result["SAM_SlabOnGrade"] += area;
                        break;

                    case PanelType.SolarPanel:
                        result["SAM_SolarPanel"] += area;
                        break;

                    case PanelType.UndergroundCeiling:
                        result["SAM_UndergroundCeiling"] += area;
                        break;

                    case PanelType.UndergroundSlab:
                        result["SAM_UndergroundSlab"] += area;
                        break;

                    case PanelType.UndergroundWall:
                        result["SAM_UndergroundWall"] += area;
                        break;

                    case PanelType.Wall:
                        result["SAM_Wall"] += area;
                        break;

                    case PanelType.WallExternal:
                        result["SAM_WallExternal"] += area;
                        break;

                    case PanelType.WallInternal:
                        result["SAM_WallInternal"] += area;
                        break;
                }

                List<Aperture> apertures = panel.Apertures;
                if(apertures != null)
                {
                    foreach(Aperture aperture in apertures)
                    {
                        if(aperture == null)
                        {
                            continue;
                        }

                        switch(aperture.ApertureType)
                        {
                            case ApertureType.Door:
                                 if(external)
                                {
                                    area = aperture.GetPaneArea();
                                    if(!double.IsNaN(area) && area > 0)
                                    {
                                        result["SAM_DoorPane_External"] += area;
                                    }

                                    area = aperture.GetFrameArea();
                                    if (!double.IsNaN(area) && area > 0)
                                    {
                                        result["SAM_DoorFrame_External"] += area;
                                    }
                                }
                                else
                                {
                                    area = aperture.GetPaneArea();
                                    if (!double.IsNaN(area) && area > 0)
                                    {
                                        result["SAM_DoorPane_Internal"] += area;
                                    }

                                    area = aperture.GetFrameArea();
                                    if (!double.IsNaN(area) && area > 0)
                                    {
                                        result["SAM_DoorFrame_Internal"] += area;
                                    }
                                }
                                
                                break;

                            case ApertureType.Window:
                                if (external)
                                {
                                    area = aperture.GetPaneArea();
                                    if (!double.IsNaN(area) && area > 0)
                                    {
                                        result["SAM_WindowPane_External"] += area;
                                    }

                                    area = aperture.GetFrameArea();
                                    if (!double.IsNaN(area) && area > 0)
                                    {
                                        result["SAM_WindowFrame_External"] += area;
                                    }
                                }
                                else
                                {
                                    area = aperture.GetPaneArea();
                                    if (!double.IsNaN(area) && area > 0)
                                    {
                                        result["SAM_WindowPane_Internal"] += area;
                                    }

                                    area = aperture.GetFrameArea();
                                    if (!double.IsNaN(area) && area > 0)
                                    {
                                        result["SAM_WindowFrame_Internal"] += area;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            foreach(KeyValuePair<string, double> keyValuePair in result)
            {
                space_Temp.SetValue(keyValuePair.Key, keyValuePair.Value);
            }

            adjacencyCluster.AddObject(space_Temp);

            return result;
        }
    }
}