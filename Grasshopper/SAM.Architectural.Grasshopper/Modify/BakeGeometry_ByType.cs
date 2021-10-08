using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByType(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
        {
            if (rhinoDoc == null)
                return;

            List<HostPartition> hostPartitions = new List<HostPartition>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GoohostPartition)
                {
                    hostPartitions.Add(((GoohostPartition)variable).Value);
                }
                else if(variable is GooArchitecturalModel)
                {
                    ArchitecturalModel architecturalModel = ((GooArchitecturalModel)variable).Value;
                    if(architecturalModel != null)
                    {
                        List<HostPartition> hostPartitions_Temp = architecturalModel.GetObjects<HostPartition>();
                        if(hostPartitions_Temp != null && hostPartitions_Temp.Count > 0)
                        {
                            hostPartitions.AddRange(hostPartitions_Temp);
                        }
                    }
                }
            }

            BakeGeometry_ByType(rhinoDoc, hostPartitions, cutOpenings, tolerance);
        }

        public static void BakeGeometry_ByType(this RhinoDoc rhinoDoc, IEnumerable<HostPartition> hostPartitions, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
        {
            Rhino.DocObjects.Tables.LayerTable layerTable = rhinoDoc?.Layers;
            if (layerTable == null)
                return;

            Layer layer_SAM = Core.Grasshopper.Modify.AddSAMLayer(layerTable);
            if (layer_SAM == null)
                return;

            int index = -1;

            index = layerTable.Add();
            Layer layer_Construction = layerTable[index];
            layer_Construction.Name = "Type";
            layer_Construction.ParentLayerId = layer_SAM.Id;

            index = layerTable.Add();
            Layer layer_ApertureConstruction = layerTable[index];
            layer_ApertureConstruction.Name = "OpeninigType";
            layer_ApertureConstruction.ParentLayerId = layer_SAM.Id;

            //int currentIndex = layerTable.CurrentLayerIndex;

            ObjectAttributes objectAttributes = rhinoDoc.CreateDefaultAttributes();

            Random random = new Random();

            List<Guid> guids = new List<Guid>();
            foreach (HostPartition hostPartition in hostPartitions)
            {
                if (hostPartition == null)
                    continue;

                System.Drawing.Color color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));
                
                string layerName = hostPartition.Name;
                if (string.IsNullOrWhiteSpace(layerName))
                {
                    layerName = "???";
                }

                Layer layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_Construction.Id, layerName, color);

                //layerTable.SetCurrentLayerIndex(layer.Index, true);
                objectAttributes.LayerIndex = layer.Index;

                Guid guid = default;
                if (BakeGeometry(hostPartition, rhinoDoc, objectAttributes, out guid, cutOpenings, tolerance))
                    guids.Add(guid);

                List<IOpening> openings = hostPartition.Openings;
                if (openings == null || openings.Count == 0)
                    continue;

                foreach (IOpening opening in openings)
                {
                    if(opening == null)
                    {
                        continue;
                    }

                    string apertureConstructionName = ((opening as dynamic).SAMType as OpeningType)?.Name;
                    if (string.IsNullOrWhiteSpace(apertureConstructionName))
                        apertureConstructionName = opening.Name;
                    
                    if (string.IsNullOrWhiteSpace(apertureConstructionName))
                        continue;

                    color = System.Drawing.Color.FromArgb(random.Next(0, 254), random.Next(0, 254), random.Next(0, 254));

                    layer = Core.Grasshopper.Modify.GetLayer(layerTable, layer_ApertureConstruction.Id, apertureConstructionName, color);

                    //layerTable.SetCurrentLayerIndex(layer.Index, true);
                    objectAttributes.LayerIndex = layer.Index;

                    guid = default;
                    if (BakeGeometry(opening, rhinoDoc, objectAttributes, out guid))
                        guids.Add(guid);
                }
            }

            //layerTable.SetCurrentLayerIndex(currentIndex, true);
        }
    }
}