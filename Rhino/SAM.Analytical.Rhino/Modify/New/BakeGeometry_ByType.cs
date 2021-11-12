using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Rhino
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByType(this RhinoDoc rhinoDoc, IEnumerable<IPartition> partitions, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
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
            layer_PartitionType.Name = "PartitionType";
            layer_PartitionType.ParentLayerId = layer_SAM.Id;

            index = layerTable.Add();
            Layer layer_OpeningType = layerTable[index];
            layer_OpeningType.Name = "OpeningType";
            layer_OpeningType.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            Random random = new Random();

            List<Guid> guids = new List<Guid>();
            foreach (IPartition partition in partitions)
            {
                if (partition == null)
                    continue;

                System.Drawing.Color color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));
                
                string layerName = partition.Name;
                if (string.IsNullOrWhiteSpace(layerName))
                {
                    layerName = "???";
                }

                Layer layer = Core.Rhino.Modify.GetLayer(layerTable, layer_PartitionType.Id, layerName, color);

                //layerTable.SetCurrentLayerIndex(layer.Index, true);
                objectAttributes.LayerIndex = layer.Index;

                List<IPartition> partitions_FixEdges = partition.FixEdges();
                if(partitions_FixEdges == null)
                {
                    continue;
                }

                foreach(IPartition partition_FixEdges in partitions_FixEdges)
                {
                    if (BakeGeometry(partition, rhinoDoc, objectAttributes, out Guid guid, cutOpenings, tolerance))
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

                            string openingTypeName = opening.Type()?.Name;
                            if (string.IsNullOrWhiteSpace(openingTypeName))
                                openingTypeName = opening.Name;

                            if (string.IsNullOrWhiteSpace(openingTypeName))
                                continue;

                            color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                            layer = Core.Rhino.Modify.GetLayer(layerTable, layer_OpeningType.Id, openingTypeName, color);

                            //layerTable.SetCurrentLayerIndex(layer.Index, true);
                            objectAttributes.LayerIndex = layer.Index;

                            List<IOpening> openings_FixEdges = opening.FixEdges();
                            if(openings_FixEdges == null)
                            {
                                continue;
                            }

                            foreach(IOpening opening_FixEdges in openings_FixEdges)
                            {
                                guid = default;
                                if (BakeGeometry(opening, rhinoDoc, objectAttributes, out guid))
                                    guids.Add(guid);
                            }
                        }
                    }
                }
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}