using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByPanelType(this RhinoDoc rhinoDoc, IEnumerable<Panel> panels, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Rhino.Modify.AddSAMLayer(layerTable);
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

                List<Panel> panels_FixEdges = panel.FixEdges(cutApertures, tolerance);
                if(panels_FixEdges == null || panels_FixEdges.Count == 0)
                {
                    panels_FixEdges = new List<Panel>() { panel };
                }

                PanelType panelType = panel.PanelType;

                Layer layer = Core.Rhino.Modify.GetLayer(layerTable, layer_PanelType.Id, panelType.ToString(), Query.Color(panelType));

                //layerTable.SetCurrentLayerIndex(layer.Index, true);
                objectAttributes.LayerIndex = layer.Index;

                foreach (Panel panel_FixEdges in panels_FixEdges)
                {
                    Guid guid = default;
                    if (BakeGeometry(panel_FixEdges, rhinoDoc, objectAttributes, out guid, cutApertures, tolerance))
                        guids.Add(guid);
                }

                List<Aperture> apertures = panel.Apertures;
                if (apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                foreach (Aperture aperture in apertures)
                {
                    if (aperture == null)
                        continue;

                    ApertureType apertureType = aperture.ApertureType;

                    layer = Core.Rhino.Modify.GetLayer(layerTable, layer_ApertureType.Id, apertureType.ToString(), Query.Color(apertureType));

                    //layerTable.SetCurrentLayerIndex(layer.Index, true);
                    objectAttributes.LayerIndex = layer.Index;

                    Guid guid = default;
                    if (BakeGeometry(aperture, rhinoDoc, objectAttributes, out guid))
                        guids.Add(guid);
                }
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}