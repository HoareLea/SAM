using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByCategory(this RhinoDoc rhinoDoc, IEnumerable<IPartition> partitions, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Rhino.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_Host = layerTable[index];
            layer_Host.Name = "Partition";
            layer_Host.ParentLayerId = layer_SAM.Id;

            index = layerTable.Add();
            Layer layer_Opening = layerTable[index];
            layer_Opening.Name = "Opening";
            layer_Opening.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            List<Guid> guids = new List<Guid>();
            foreach (IPartition partition in partitions)
            {
                if (partition == null)
                    continue;

                Layer layer = Core.Rhino.Modify.GetLayer(layerTable, layer_Host.Id, partition.GetType().ToString(), Query.Color(partition));

                //layerTable.SetCurrentLayerIndex(layer.Index, true);
                objectAttributes.LayerIndex = layer.Index;

                Guid guid = default;
                if (BakeGeometry(partition, rhinoDoc, objectAttributes, out guid, cutOpenings, tolerance))
                    guids.Add(guid);

                if(partition is IHostPartition)
                {
                    List<IOpening> openings = ((IHostPartition)partition).GetOpenings();
                    if (openings == null || openings.Count == 0)
                        continue;

                    foreach (IOpening opening in openings)
                    {
                        if (opening == null)
                            continue;

                        layer = Core.Rhino.Modify.GetLayer(layerTable, layer_Opening.Id, opening.GetType().ToString(), Query.Color(opening));

                        //layerTable.SetCurrentLayerIndex(layer.Index, true);
                        objectAttributes.LayerIndex = layer.Index;

                        guid = default;
                        if (BakeGeometry(opening, rhinoDoc, objectAttributes, out guid))
                            guids.Add(guid);
                    }
                }
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}