using Eto.Forms;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByBoundaryType(this RhinoDoc rhinoDoc, AdjacencyCluster adjacencyCluster, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Rhino.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = layerTable.Add();
            Layer layer = layerTable[index];
            layer.Name = "BoundaryType";
            layer.ParentLayerId = layer_SAM.Id;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            List<Panel> panels = adjacencyCluster.GetPanels();

            List<Guid> guids = new List<Guid>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                BoundaryType boundaryType = adjacencyCluster.BoundaryType(panel);

                Layer layer_Temp = Core.Rhino.Modify.GetLayer(layerTable, layer.Id, boundaryType.ToString(), Query.Color(boundaryType));

                objectAttributes.LayerIndex = layer_Temp.Index;

                if (BakeGeometry(panel, rhinoDoc, objectAttributes, out List<Guid> guids_Panel, cutApertures, tolerance) && guids_Panel != null)
                {
                    guids.AddRange(guids_Panel);
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

                    if (BakeGeometry(aperture, rhinoDoc, objectAttributes, out Guid guid))
                        guids.Add(guid);
                }
            }
        }
    }
}