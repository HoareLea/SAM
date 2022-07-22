using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<IMaterial> Materials(this HostPartitionType hostPartitionType, MaterialLibrary materialLibrary)
        {
            if (hostPartitionType == null || materialLibrary == null)
            {
                return null;
            }

            return Architectural.Query.Materials(hostPartitionType.MaterialLayers, materialLibrary);
        }

        public static List<IMaterial> Materials(this OpeningType openingType, MaterialLibrary materialLibrary)
        {
            if (openingType == null || materialLibrary == null)
            {
                return null;
            }

            List<IMaterial> paneMaterials = Architectural.Query.Materials(openingType.PaneMaterialLayers, materialLibrary);
            List<IMaterial> frameMaterials = Architectural.Query.Materials(openingType.FrameMaterialLayers, materialLibrary);

            if(paneMaterials == null && frameMaterials == null)
            {
                return null;
            }

            List<IMaterial> result = new List<IMaterial>();
            if(paneMaterials != null)
            {
                result.AddRange(paneMaterials);
            }

            if(frameMaterials != null)
            {
                foreach(IMaterial material in frameMaterials)
                {
                    if(result.Find(x => x.Name == material.Name) == null)
                    {
                        result.Add(material);
                    }
                }
            }

            return result;
        }

        public static List<IMaterial> Materials(this IHostPartition hostPartition, MaterialLibrary materialLibrary)
        {
            if (hostPartition == null || materialLibrary == null)
            {
                return null;
            }

            return Materials(hostPartition.Type(), materialLibrary);
        }

        public static List<IMaterial> Materials(this IOpening opening, MaterialLibrary materialLibrary)
        {
            if (opening == null || materialLibrary == null)
            {
                return null;
            }

            return Materials(opening.Type(), materialLibrary);
        }

        public static List<IMaterial> Materials(this IEnumerable<IHostPartition> hostPartitions, MaterialLibrary materialLibrary)
        {
            if(hostPartitions == null)
            {
                return null;
            }

            List<IMaterial> result = new List<IMaterial>();
            foreach(IHostPartition hostPartition in hostPartitions)
            {
                List<IMaterial> materials = hostPartition?.Materials(materialLibrary);
                if(materials == null || materials.Count == 0)
                {
                    continue;
                }

                foreach(IMaterial material in materials)
                {
                    if(material == null || result.Find(x => x.Name == material.Name) != null)
                    {
                        continue;
                    }

                    result.Add(material);
                }
            }

            return result;
        }
    }
}