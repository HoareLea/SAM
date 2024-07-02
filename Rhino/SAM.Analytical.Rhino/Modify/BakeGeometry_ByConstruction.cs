using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByConstruction(this RhinoDoc rhinoDoc, IEnumerable<IPanel> panels, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Rhino.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_Construction = layerTable[index];
            layer_Construction.Name = "Construction";
            layer_Construction.ParentLayerId = layer_SAM.Id;

            index = layerTable.Add();
            Layer layer_ApertureConstruction = layerTable[index];
            layer_ApertureConstruction.Name = "ApertureConstruction";
            layer_ApertureConstruction.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            Random random = new Random();

            List<Guid> guids = new List<Guid>();
            foreach (IPanel panel in panels)
            {
                if (panel == null)
                    continue;

                PanelType panelType = panel is Panel ? ((Panel)panel).PanelType : PanelType.Air;

                System.Drawing.Color color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));
                
                string layerName = panel is Panel ? ((Panel)panel).Name : panel.GetType().Name;
                if (string.IsNullOrWhiteSpace(layerName))
                {
                    if (panelType == PanelType.Air)
                    {
                        layerName = "Air";
                        color = Query.Color(PanelType.Air);
                    }
                    else
                    {
                        layerName = "???";
                    }
                }

                Layer layer = Core.Rhino.Modify.GetLayer(layerTable, layer_Construction.Id, layerName, color);

                //layerTable.SetCurrentLayerIndex(layer.Index, true);
                objectAttributes.LayerIndex = layer.Index;

                if (BakeGeometry(panel, rhinoDoc, objectAttributes, out List<Guid> guids_Panel, cutApertures, tolerance) && guids_Panel != null)
                {
                    guids.AddRange(guids_Panel);
                }

                if(panel is Panel)
                {
                    List<Aperture> apertures = ((Panel)panel).Apertures;

                    if (apertures != null && apertures.Count != 0)
                    {
                        foreach (Aperture aperture in apertures)
                        {
                            if (aperture == null)
                            {
                                continue;
                            }

                            string apertureConstructionName = aperture.ApertureConstruction?.Name;
                            if (string.IsNullOrWhiteSpace(apertureConstructionName))
                                apertureConstructionName = aperture.Name;

                            if (string.IsNullOrWhiteSpace(apertureConstructionName))
                                continue;

                            color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                            layer = Core.Rhino.Modify.GetLayer(layerTable, layer_ApertureConstruction.Id, apertureConstructionName, color);

                            //layerTable.SetCurrentLayerIndex(layer.Index, true);
                            objectAttributes.LayerIndex = layer.Index;

                            Guid guid = default;
                            if (BakeGeometry(aperture, rhinoDoc, objectAttributes, out guid))
                                guids.Add(guid);
                        }
                    }
                }
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}