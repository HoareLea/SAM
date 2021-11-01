using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Core.IMaterial> AddMissingMaterials(this ArchitecturalModel architecturalModel, Core.MaterialLibrary materialLibrary)
        {
            return AddMissingMaterials(architecturalModel, materialLibrary, out List<string> missiingMaterialNames);
        }


        public static List<Core.IMaterial> AddMissingMaterials(this ArchitecturalModel architecturalModel, Core.MaterialLibrary materialLibrary, out List<string> missingMaterialNames)
        {
            missingMaterialNames = null;
            
            if(architecturalModel == null || materialLibrary == null)
            {
                return null;
            }

            List<Core.IMaterial> result = new List<Core.IMaterial>();

            List<string> materialNames = architecturalModel.GetMissingMaterialNames();
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
                    architecturalModel.Add(material);
                }
            }

            return result;
        }
    }
}