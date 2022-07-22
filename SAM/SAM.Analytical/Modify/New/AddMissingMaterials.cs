using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Core.IMaterial> AddMissingMaterials(this BuildingModel buildingModel, Core.MaterialLibrary materialLibrary)
        {
            return AddMissingMaterials(buildingModel, materialLibrary, out List<string> missiingMaterialNames);
        }


        public static List<Core.IMaterial> AddMissingMaterials(this BuildingModel buildingModel, Core.MaterialLibrary materialLibrary, out List<string> missingMaterialNames)
        {
            missingMaterialNames = null;
            
            if(buildingModel == null || materialLibrary == null)
            {
                return null;
            }

            List<Core.IMaterial> result = new List<Core.IMaterial>();

            List<string> materialNames = buildingModel.GetMissingMaterialNames();
            if(materialNames == null || materialNames.Count == 0)
            {
                return result;
            }

            missingMaterialNames = new List<string>();
            foreach(string materialName in materialNames)
            {
                if(string.IsNullOrWhiteSpace(materialName))
                {
                    continue;
                }

                Core.IMaterial material = materialLibrary.GetMaterial(materialName);
                if(material == null)
                {
                    missingMaterialNames.Add(materialName);
                }
                else
                {
                    result.Add(material);
                    buildingModel.Add(material);
                }
            }

            return result;
        }
    }
}