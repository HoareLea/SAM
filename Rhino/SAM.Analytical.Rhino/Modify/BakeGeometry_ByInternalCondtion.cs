using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByInternalCondition(this RhinoDoc rhinoDoc, IEnumerable<Space> spaces)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Rhino.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_Spaces = layerTable[index];
            layer_Spaces.Name = "Spaces";
            layer_Spaces.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            Random random = new Random();

            List<Guid> guids = new List<Guid>();
            foreach (Space space in spaces)
            {
                Geometry.Spatial.Point3D location = space?.Location;
                if (location == null)
                    continue;

                string internalConditionName = space.InternalCondition?.Name;
                if (string.IsNullOrWhiteSpace(internalConditionName))
                    internalConditionName = "???";

                System.Drawing.Color color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                Layer layer_Level = Core.Rhino.Modify.GetLayer(layerTable, layer_Spaces.Id, internalConditionName, color);

                string layerName = space.Name;
                if (string.IsNullOrWhiteSpace(layerName))
                    layerName = "???";

                color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                Layer layer_Space = Core.Rhino.Modify.GetLayer(layerTable, layer_Level.Id, layerName, color);

                //layerTable.SetCurrentLayerIndex(layer_Space.Index, true);
                objectAttributes.LayerIndex = layer_Space.Index;

                Guid guid = default;
                if (BakeGeometry(space, rhinoDoc, objectAttributes, out guid))
                    guids.Add(guid);
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}