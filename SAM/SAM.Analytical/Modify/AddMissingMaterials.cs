using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Core.IMaterial> AddMissingMaterials(this AnalyticalModel analyticalModel, Core.MaterialLibrary materialLibrary)
        {
            return AddMissingMaterials(analyticalModel, materialLibrary, out List<string> missiingMaterialNames);
        }


        public static List<Core.IMaterial> AddMissingMaterials(this AnalyticalModel analyticalModel, Core.MaterialLibrary materialLibrary, out List<string> missingMaterialNames)
        {
            missingMaterialNames = null;
            
            if(analyticalModel == null || materialLibrary == null)
            {
                return null;
            }

            List<Core.IMaterial> result = new List<Core.IMaterial>();

            List<string> materialNames = analyticalModel.GetMissingMaterialNames();
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
                    analyticalModel.AddMaterial(material);
                }
            }

            return result;
        }
    }
}