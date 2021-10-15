using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByPanelType(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            if (rhinoDoc == null)
                return;

            List<Panel> panels = new List<Panel>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooPanel)
                {
                    panels.Add(((GooPanel)variable).Value);
                }
                else if (variable is GooAdjacencyCluster)
                {
                    List<Panel> panels_Temp = ((GooAdjacencyCluster)variable).Value?.GetPanels();
                    if (panels_Temp != null && panels_Temp.Count != 0)
                        panels.AddRange(panels_Temp);
                }
                else if (variable is GooAnalyticalModel)
                {
                    List<Panel> panels_Temp = ((GooAnalyticalModel)variable).Value?.AdjacencyCluster.GetPanels();
                    if (panels_Temp != null && panels_Temp.Count != 0)
                        panels.AddRange(panels_Temp);
                }
            }

            BakeGeometry_ByPanelType(rhinoDoc, panels, cutApertures, tolerance);
        }

        public static void BakeGeometry_ByPanelType(this RhinoDoc rhinoDoc, IEnumerable<Panel> panels, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Grasshopper.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_PanelType = layerTable[index];
            layer_PanelType.Name = "PanelType";
            layer_PanelType.ParentLayerId = layer_SAM.Id;

            index = layerTable.Add();
            Layer layer_ApertureType = layerTable[index];
            layer_ApertureType.Name = "ApertureType";
            layer_ApertureType.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            List<Guid> guids = new List<Guid>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                List<Panel> panels_FixEdges = panel.FixEdges();
                if(panels_FixEdges == null || panels_FixEdges.Count == 0)
                {
                    panels_FixEdges = new List<Panel>() { panel };
                }

                PanelType panelType = panel.PanelType;

                Layer layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_PanelType.Id, panelType.ToString(), Analytical.Query.Color(panelType));

                //layerTable.SetCurrentLayerIndex(layer.Index, true);
                objectAttributes.LayerIndex = layer.Index;

                foreach (Panel panel_FixEdges in panels_FixEdges)
                {
                    Guid guid = default;
                    if (BakeGeometry(panel_FixEdges, rhinoDoc, objectAttributes, out guid, cutApertures, tolerance))
                        guids.Add(guid);

                    List<Aperture> apertures = panel_FixEdges.Apertures;
                    if (apertures == null || apertures.Count == 0)
                        continue;

                    foreach (Aperture aperture in apertures)
                    {
                        if (aperture == null)
                            continue;

                        ApertureType apertureType = aperture.ApertureType;

                        layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_ApertureType.Id, apertureType.ToString(), Analytical.Query.Color(apertureType));

                        //layerTable.SetCurrentLayerIndex(layer.Index, true);
                        objectAttributes.LayerIndex = layer.Index;

                        guid = default;
                        if (BakeGeometry(aperture, rhinoDoc, objectAttributes, out guid))
                            guids.Add(guid);
                    }
                }
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}