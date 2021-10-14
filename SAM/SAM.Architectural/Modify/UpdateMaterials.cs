using SAM.Core;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Modify
    {
        public static List<IMaterial> UpdateMaterials(this ArchitecturalModel architecturalModel, MaterialLibrary materialLibrary, out HashSet<string> missingMaterialsNames)
        {
            missingMaterialsNames = null;

            if (architecturalModel == null || materialLibrary == null)
            {
                return null;
            }

            HashSet<string> materialNames = new HashSet<string>();

            List<HostPartitionType> hostPartitionTypes = architecturalModel.GetHostPartitionTypes<HostPartitionType>();
            if(hostPartitionTypes != null || hostPartitionTypes.Count != 0)
            {
                foreach (HostPartitionType hostPartitionType in hostPartitionTypes)
                {
                    List<MaterialLayer> materialLayers = hostPartitionType?.MaterialLayers;
                    if (materialLayers == null || materialLayers.Count == 0)
                    {
                        continue;
                    }

                    foreach (MaterialLayer materialLayer in materialLayers)
                    {
                        materialNames.Add(materialLayer?.Name);
                    }
                }
            }

            List<OpeningType> openingTypes = architecturalModel.GetOpeningTypes<OpeningType>();
            if (openingTypes != null || openingTypes.Count != 0)
            {
                foreach (OpeningType openingType in openingTypes)
                {
                    List<MaterialLayer> materialLayers = new List<MaterialLayer>();

                    List<MaterialLayer> materialLayers_Temp = null;

                    materialLayers_Temp = openingType.FrameMaterialLayers;
                    if (materialLayers_Temp != null && materialLayers.Count != 0)
                    {
                        materialLayers.AddRange(materialLayers_Temp);
                    }

                    materialLayers_Temp = openingType.PaneMaterialLayers;
                    if (materialLayers_Temp != null && materialLayers.Count != 0)
                    {
                        materialLayers.AddRange(materialLayers_Temp);
                    }

                    foreach (MaterialLayer materialLayer in materialLayers)
                    {
                        materialNames.Add(materialLayer?.Name);
                    }
                }
            }

            missingMaterialsNames = new HashSet<string>();
            List<IMaterial> result = new List<IMaterial>();
            foreach (string materialName in materialNames)
            {
                if (string.IsNullOrWhiteSpace(materialName))
                {
                    continue;
                }

                IMaterial material = architecturalModel.GetMaterial(materialName);
                if (material != null)
                {
                    continue;
                }

                material = materialLibrary.GetMaterial(materialName);
                if (material == null)
                {
                    missingMaterialsNames.Add(materialName);
                    continue;
                }

                if (architecturalModel.AddMaterial(material))
                {
                    result.Add(material);
                }
            }

            return result;
        }
    }
}