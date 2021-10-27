using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByType(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
        {
            if (rhinoDoc == null)
                return;

            List<IPartition> partitions = new List<IPartition>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooPartition)
                {
                    partitions.Add(((GooPartition)variable).Value);
                }
                else if(variable is GooArchitecturalModel)
                {
                    ArchitecturalModel architecturalModel = ((GooArchitecturalModel)variable).Value;
                    if(architecturalModel != null)
                    {
                        List<IPartition> hostPartitions_Temp = architecturalModel.GetObjects<IPartition>();
                        if(hostPartitions_Temp != null && hostPartitions_Temp.Count > 0)
                        {
                            partitions.AddRange(hostPartitions_Temp);
                        }
                    }
                }
            }

            BakeGeometry_ByType(rhinoDoc, partitions, cutOpenings, tolerance);
        }

        public static void BakeGeometry_ByType(this RhinoDoc rhinoDoc, IEnumerable<IPartition> partitions, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Grasshopper.Modify.AddSAMLayer(layerTable);
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

                Layer layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_PartitionType.Id, layerName, color);

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
                        {
                            continue;
                        }

                        string apertureConstructionName = ((opening as dynamic).SAMType as OpeningType)?.Name;
                        if (string.IsNullOrWhiteSpace(apertureConstructionName))
                            apertureConstructionName = opening.Name;

                        if (string.IsNullOrWhiteSpace(apertureConstructionName))
                            continue;

                        color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                        layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_OpeningType.Id, apertureConstructionName, color);

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