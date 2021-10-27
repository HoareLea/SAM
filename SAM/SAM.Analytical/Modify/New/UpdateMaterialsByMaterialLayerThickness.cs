using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void UpdateMaterialsByMaterialLayerThickness(this ArchitecturalModel architecturalModel, out List<HostPartitionType> hostPartitionTypes, out List<OpeningType> openingTypes, double tolerance = Tolerance.MacroDistance)
        {
            hostPartitionTypes = null;
            openingTypes = null;

            List<Architectural.MaterialLayer> materialLayers = new List<Architectural.MaterialLayer>();

            List<HostPartitionType> hostPartitionTypes_All = architecturalModel?.GetHostPartitionTypes();
            if (hostPartitionTypes_All != null)
            {
                foreach (HostPartitionType hostPartitionType in hostPartitionTypes_All)
                {
                    List<Architectural.MaterialLayer> materialLayers_HostPartitionType = hostPartitionType?.MaterialLayers;

                    if (materialLayers_HostPartitionType == null || materialLayers_HostPartitionType.Count == 0)
                    {
                        continue;
                    }

                    materialLayers.AddRange(materialLayers_HostPartitionType);
                }
            }

            List<OpeningType> openingTypes_All = architecturalModel?.GetOpeningTypes();
            if (openingTypes_All != null)
            {
                foreach (OpeningType openingType in openingTypes_All)
                {
                    List<Architectural.MaterialLayer> materialLayers_OpeningType = null;

                    materialLayers_OpeningType = openingType?.PaneMaterialLayers;
                    if (materialLayers_OpeningType != null && materialLayers_OpeningType.Count != 0)
                    {
                        materialLayers.AddRange(materialLayers_OpeningType);
                    }

                    materialLayers_OpeningType = openingType?.FrameMaterialLayers;
                    if (materialLayers_OpeningType != null && materialLayers_OpeningType.Count != 0)
                    {
                        materialLayers.AddRange(materialLayers_OpeningType);
                    }
                }
            }

            List<Architectural.MaterialLayer> materialLayers_Unique = new List<Architectural.MaterialLayer>();
            foreach (Architectural.MaterialLayer materialLayer in materialLayers)
            {
                double thickness = Core.Query.Round(materialLayer.Thickness, tolerance);

                Architectural.MaterialLayer materialLayer_Old = materialLayers.Find(x => x.Name == materialLayer.Name && x.Thickness == thickness);
                if (materialLayer_Old != null)
                {
                    continue;
                }

                materialLayers_Unique.Add(new Architectural.MaterialLayer(materialLayer.Name, thickness));
            }

            List<Tuple<Architectural.MaterialLayer, Material>> tuples = new List<Tuple<Architectural.MaterialLayer, Material>>();
            while (materialLayers_Unique.Count > 0)
            {
                Architectural.MaterialLayer materialLayer = materialLayers_Unique[0];

                List<Architectural.MaterialLayer> materialLayers_Material = materialLayers.FindAll(x => x.Name == materialLayer.Name);
                materialLayers_Material?.ForEach(x => materialLayers_Unique.Remove(x));

                if (materialLayers_Material.Count < 2)
                {
                    continue;
                }

                Material material = architecturalModel.GetMaterial(materialLayer.Name) as Material;
                if (material == null)
                {
                    continue;
                }

                foreach(Architectural.MaterialLayer materialLayer_Temp in materialLayers_Material)
                {
                    Material material_Thickness = Core.Create.Material(material, GetMaterialName(material.Name, materialLayer_Temp.Thickness, tolerance));
                    if(material_Thickness == null)
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<Architectural.MaterialLayer, Material>(materialLayer_Temp, material_Thickness));
                }
            }

            if(tuples == null || tuples.Count == 0)
            {
                return;
            }

            hostPartitionTypes = new List<HostPartitionType>();
            openingTypes = new List<OpeningType>();

            if (hostPartitionTypes_All != null)
            {
                foreach (HostPartitionType hostPartitionType in hostPartitionTypes_All)
                {
                    List<Architectural.MaterialLayer> materialLayers_HostPartitionType = hostPartitionType?.MaterialLayers;
                    if (materialLayers_HostPartitionType == null)
                    {
                        continue;
                    }

                    List<Material> materials = new List<Material>();
                    for(int i =0; i < materialLayers_HostPartitionType.Count; i++)
                    {
                        Architectural.MaterialLayer materialLayer = materialLayers_HostPartitionType[i];

                        double thickness = Core.Query.Round(materialLayer.Thickness, tolerance);
                        Tuple<Architectural.MaterialLayer, Material> tuple = tuples.Find(x => x.Item1.Name == materialLayer.Name && x.Item1.Thickness == thickness);
                        if(tuple == null)
                        {
                            continue;
                        }

                        materials.Add(tuple.Item2);
                        materialLayers_HostPartitionType[i] = new Architectural.MaterialLayer(tuple.Item2.Name, thickness);
                    }

                    if(materials == null || materials.Count == 0)
                    {
                        continue;
                    }

                    int index = 1;
                    string name = string.Format("{0} {1}", hostPartitionType.Name, index);
                    List<HostPartitionType> hostPartitionTypes_Temp = architecturalModel.GetHostPartitionTypes<HostPartitionType>(name, TextComparisonType.Equals);
                    while (hostPartitionTypes_Temp != null && hostPartitionTypes_Temp.Count > 0)
                    {
                        index++;
                        name = string.Format("{0} {1}", hostPartitionType.Name, index);
                        hostPartitionTypes_Temp = architecturalModel.GetHostPartitionTypes<HostPartitionType>(name, TextComparisonType.Equals);
                    }

                    HostPartitionType hostPartitionType_New = Create.HostPartitionType(hostPartitionType, name);
                    hostPartitionType_New.MaterialLayers = materialLayers_HostPartitionType;

                    List<IHostPartition> hostPartitions = architecturalModel.GetHostPartitions(hostPartitionType);
                    if(hostPartitions == null || hostPartitions.Count == 0)
                    {
                        continue;
                    }

                    foreach (IHostPartition hostPartition in hostPartitions)
                    {
                        hostPartition.Type(hostPartitionType_New);
                        architecturalModel.Add(hostPartition);
                    }

                    foreach (Material material in materials)
                    {
                        architecturalModel.Add(material);
                    }
                }
            }

            if(openingTypes_All != null)
            {
                foreach (OpeningType openingType in openingTypes_All)
                {
                    List<Material> materials = new List<Material>();

                    List<Architectural.MaterialLayer> materialLayers_Pane = openingType?.PaneMaterialLayers;
                    if(materialLayers_Pane != null)
                    {
                        for (int i = 0; i < materialLayers_Pane.Count; i++)
                        {
                            Architectural.MaterialLayer materialLayer = materialLayers_Pane[i];

                            double thickness = Core.Query.Round(materialLayer.Thickness, tolerance);
                            Tuple<Architectural.MaterialLayer, Material> tuple = tuples.Find(x => x.Item1.Name == materialLayer.Name && x.Item1.Thickness == thickness);
                            if (tuple == null)
                            {
                                continue;
                            }

                            materials.Add(tuple.Item2);
                            materialLayers_Pane[i] = new Architectural.MaterialLayer(tuple.Item2.Name, thickness);
                        }
                    }

                    List<Architectural.MaterialLayer> materialLayers_Frame = openingType?.FrameMaterialLayers;
                    if (materialLayers_Frame != null)
                    {
                        for (int i = 0; i < materialLayers_Frame.Count; i++)
                        {
                            Architectural.MaterialLayer materialLayer = materialLayers_Frame[i];

                            double thickness = Core.Query.Round(materialLayer.Thickness, tolerance);
                            Tuple<Architectural.MaterialLayer, Material> tuple = tuples.Find(x => x.Item1.Name == materialLayer.Name && x.Item1.Thickness == thickness);
                            if (tuple == null)
                            {
                                continue;
                            }

                            materials.Add(tuple.Item2);
                            materialLayers_Frame[i] = new Architectural.MaterialLayer(tuple.Item2.Name, thickness);
                        }
                    }

                    if (materials == null || materials.Count == 0)
                    {
                        continue;
                    }

                    int index = 1;
                    string name = string.Format("{0} {1}", openingType.Name, index);
                    List<OpeningType> openingTypes_Temp = architecturalModel.GetOpeningTypes<OpeningType>(name, TextComparisonType.Equals);
                    while (openingTypes_Temp != null && openingTypes_Temp.Count > 0)
                    {
                        index++;
                        name = string.Format("{0} {1}", openingType.Name, index);
                        openingTypes_Temp = architecturalModel.GetOpeningTypes<OpeningType>(name, TextComparisonType.Equals);
                    }

                    OpeningType openingType_New = Create.OpeningType(openingType, name);
                    openingType_New.PaneMaterialLayers = materialLayers_Pane;
                    openingType_New.FrameMaterialLayers = materialLayers_Frame;

                    List<IOpening> openings = architecturalModel.GetOpenings(openingType);
                    if (openings == null || openings.Count == 0)
                    {
                        continue;
                    }

                    foreach(IOpening opening in openings)
                    {
                        IHostPartition hostPartition = architecturalModel.GetHostPartition(opening);
                        if(hostPartition == null)
                        {
                            continue;
                        }

                        hostPartition.AddOpening(opening);
                        architecturalModel.Add(hostPartition);
                    }

                }
            }
        }

        public static void UpdateMaterialsByMaterialLayerThickness(this ArchitecturalModel architecturalModel, double tolerance = Tolerance.MacroDistance)
        {
            if(architecturalModel == null)
            {
                return;
            }

            UpdateMaterialsByMaterialLayerThickness(architecturalModel, out List<HostPartitionType> hostPartitionTypes, out List<OpeningType> openingTypes, tolerance);
        }

        private static string GetMaterialName(string name, double thickness, double tolerance = Tolerance.MacroDistance)
        {
            if (double.IsNaN(thickness) || string.IsNullOrEmpty(name))
                return null;

            return string.Format("{0}_{1}mm", name, System.Math.Round(thickness / tolerance, 0));
        }
    }
}