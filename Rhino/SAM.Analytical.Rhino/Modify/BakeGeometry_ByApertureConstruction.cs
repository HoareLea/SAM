using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByApertureConstruction(this RhinoDoc rhinoDoc, IEnumerable<Aperture> apertures)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Rhino.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_ApertureConstruction = layerTable[index];
            layer_ApertureConstruction.Name = "ApertureConstruction";
            layer_ApertureConstruction.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            Random random = new Random();

            List<Guid> guids = new List<Guid>();
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

                Color color = Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                Layer layer = Core.Rhino.Modify.GetLayer(layerTable, layer_ApertureConstruction.Id, apertureConstructionName, color);

                //layerTable.SetCurrentLayerIndex(layer.Index, true);
                objectAttributes.LayerIndex = layer.Index;

                Guid guid = default;
                if (BakeGeometry(aperture, rhinoDoc, objectAttributes, out guid))
                    guids.Add(guid);
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}