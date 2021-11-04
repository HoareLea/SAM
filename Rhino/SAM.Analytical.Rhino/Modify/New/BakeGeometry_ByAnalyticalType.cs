using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByAnalyticalType(this RhinoDoc rhinoDoc, ArchitecturalModel architecturalModel, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
        {
            global::Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Rhino.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_PartitionType = layerTable[index];
            layer_PartitionType.Name = "Partition Analytical Type";
            layer_PartitionType.ParentLayerId = layer_SAM.Id;

            index = layerTable.Add();
            Layer layer_OpeningType = layerTable[index];
            layer_OpeningType.Name = "Opening Analytical Type";
            layer_OpeningType.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            List<IPartition> partitions = architecturalModel.GetPartitions();

            List<Guid> guids = new List<Guid>();
            foreach (IPartition partition in partitions)
            {
                if (partition == null)
                {
                    continue;
                }

                PartitionAnalyticalType analyticalType = architecturalModel.PartitionAnalyticalType(partition);
                System.Drawing.Color color = analyticalType.Color();

                string layerName = Core.Query.Description(analyticalType);
                if (string.IsNullOrWhiteSpace(layerName))
                {
                    layerName = "???";
                }

                Layer layer = Core.Rhino.Modify.GetLayer(layerTable, layer_PartitionType.Id, layerName, color);

                //layerTable.SetCurrentLayerIndex(layer.Index, true);
                objectAttributes.LayerIndex = layer.Index;

                Guid guid = default;
                if (BakeGeometry(partition, rhinoDoc, objectAttributes, out guid, cutOpenings, tolerance))
                    guids.Add(guid);

                if (partition is IHostPartition)
                {
                    List<IOpening> openings = ((IHostPartition)partition).GetOpenings();
                    if (openings == null || openings.Count == 0)
                        continue;

                    foreach (IOpening opening in openings)
                    {
                        if (opening == null)
                        {
                            continue;
                        }

                        OpeningAnalyticalType openingAnalyticalType = opening.OpeningAnalyticalType();
                        System.Drawing.Color color_Opening = openingAnalyticalType.Color();

                        string layerName_Opening = Core.Query.Description(openingAnalyticalType);
                        if (string.IsNullOrWhiteSpace(layerName_Opening))
                        {
                            layerName_Opening = "???";
                        }

                        layer = Core.Rhino.Modify.GetLayer(layerTable, layer_OpeningType.Id, layerName_Opening, color);

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