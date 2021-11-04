using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByCategory(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutOpening = false, double tolerance = Core.Tolerance.Distance)
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
                else if (variable is GooArchitecturalModel)
                {
                    ArchitecturalModel architecturalModel = ((GooArchitecturalModel)variable).Value;
                    if (architecturalModel != null)
                    {
                        List<IPartition> partitions_Temp = architecturalModel.GetObjects<IPartition>();
                        if (partitions_Temp != null && partitions_Temp.Count > 0)
                        {
                            partitions.AddRange(partitions_Temp);
                        }
                    }
                }
            }

            BakeGeometry_ByCategory(rhinoDoc, partitions, cutOpening, tolerance);
        }

        public static void BakeGeometry_ByCategory(this RhinoDoc rhinoDoc, IEnumerable<IPartition> partitions, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Grasshopper.Modify.AddSAMLayer(layerTable);
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

                Layer layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_Host.Id, partition.GetType().ToString(), Analytical.Query.Color(partition));

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

                        layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_Opening.Id, opening.GetType().ToString(), Analytical.Query.Color(opening));

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