using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural
{
    public static partial class Modify
    {
        public static List<IMaterial> UpdateMaterials(this ArchitecturalModel architecturalModel, MaterialLibrary materialLibrary, out HashSet<string> missingMaterialNames)
        {
            missingMaterialNames = null;

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

            return UpdateMaterials(architecturalModel, materialNames, materialLibrary, out missingMaterialNames);
        }

        public static List<IMaterial> UpdateMaterials(this ArchitecturalModel architecturalModel, IEnumerable<string> materialNames, MaterialLibrary materialLibrary, out HashSet<string> missingMaterialsNames)
        {
            missingMaterialsNames = null;
            
            if(architecturalModel == null || materialNames == null || materialLibrary == null)
            {
                return null;
            }

            missingMaterialsNames = new HashSet<string>();

            HashSet<string> materialNames_Unique = new HashSet<string>();
            foreach(string materialName in materialNames)
            {
                materialNames_Unique.Add(materialName);
            }

            List<IMaterial> result = new List<IMaterial>();

            foreach (string materialName in materialNames_Unique)
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

                if (architecturalModel.Add(material))
                {
                    result.Add(material);
                }
            }

            return result;
        }
    
        public static List<IMaterial> UpdateMaterials(this ArchitecturalModel architecturalModel, IEnumerable<MaterialLayer> materialLayers, MaterialLibrary materialLibrary, out HashSet<string> missingMaterialsNames)
        {
            return UpdateMaterials(architecturalModel, materialLayers?.ToList().ConvertAll(x => x?.Name), materialLibrary, out missingMaterialsNames);
        }
    }
}