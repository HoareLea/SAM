using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> ReplaceTransparentPanels(this AdjacencyCluster adjacencyCluster, MaterialLibrary materialLibrary = null, double offset = 0)
        {
            if (adjacencyCluster == null)
                return null;

            List<Panel> result = new List<Panel>();

            List<Panel> panels = adjacencyCluster.TransparentPanels(materialLibrary);
            if (panels == null || panels.Count == 0)
                return result;

            Dictionary<Guid, ApertureConstruction> dictionary = new Dictionary<Guid, ApertureConstruction>();
            foreach(Panel panel in panels)
            {
                PanelType panelType = panel.PanelType;

                if(panelType.PanelGroup() == PanelGroup.Wall)
                {
                    List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                    if (spaces == null || spaces.Count == 0)
                    {
                        if (panelType == PanelType.CurtainWall)
                            panelType = PanelType.Wall;
                    }
                    else if (spaces.Count == 1)
                    {
                        panelType = PanelType.WallExternal;
                    }
                    else
                    {
                        panelType = PanelType.WallInternal;
                    }
                }

                Construction construction = Query.DefaultConstruction(panelType);

                Construction construction_Panel = panel.Construction;
                if (construction_Panel == null)
                    construction_Panel = construction;

                ApertureConstruction apertureConstruction = null; 
                if(!dictionary.TryGetValue(construction_Panel.Guid, out apertureConstruction))
                {
                    apertureConstruction = new ApertureConstruction(Guid.NewGuid(), construction_Panel.Name, ApertureType.Window, construction_Panel.ConstructionLayers);
                    dictionary[construction_Panel.Guid] = apertureConstruction;
                }

                Panel panel_New = new Panel(panel, construction);
                if (panel_New.PanelType != panelType)
                    panel_New = new Panel(panel_New, panelType);

                panel_New.AddAperture(apertureConstruction, panel.GetFace3D(), false);
                if (offset > 0)
                    panel_New.OffsetAperturesOnEdge(offset);

                if (adjacencyCluster.AddObject(panel_New))
                    result.Add(panel_New);
            }

            return result;

        }
    }
}