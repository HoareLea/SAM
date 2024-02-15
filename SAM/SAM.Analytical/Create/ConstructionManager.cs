using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static ConstructionManager ConstructionManager(this AnalyticalModel analyticalModel)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if(adjacencyCluster == null)
            {
                return null;
            }

            List<Construction> constructions = adjacencyCluster.GetConstructions();

            List<ApertureConstruction> apertureConstructions = adjacencyCluster.GetApertureConstructions();

            MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;

            return new ConstructionManager(apertureConstructions, constructions, materialLibrary);
        }

        public static ConstructionManager ConstructionManager(ConstructionManager constructionManager, Dictionary<PanelType, Construction> constructionDictionary, Dictionary<PanelType, ApertureConstruction> apertureConctructionDictionary, IEnumerable<IMaterial> materials)
        {
            ConstructionManager result = constructionManager == null ? new ConstructionManager() : new ConstructionManager(constructionManager);

            if(constructionDictionary != null)
            {
                foreach(KeyValuePair<PanelType, Construction> keyValuePair in constructionDictionary)
                {
                    if(keyValuePair.Value == null)
                    {
                        continue;
                    }

                    result.GetConstructions(keyValuePair.Key)?.ForEach(x => result.Remove(x));

                    Construction construction = keyValuePair.Value;
                    if(construction != null)
                    {
                        result.Add(new Construction(construction, System.Guid.NewGuid()), keyValuePair.Key);
                    }
                }
            }

            if (apertureConctructionDictionary != null)
            {
                foreach (KeyValuePair<PanelType, ApertureConstruction> keyValuePair in apertureConctructionDictionary)
                {
                    if (keyValuePair.Value == null)
                    {
                        continue;
                    }

                    result.GetApertureConstructions(keyValuePair.Value.ApertureType, keyValuePair.Key)?.ForEach(x => result.Remove(x));
                    
                    ApertureConstruction apertureConstruction = keyValuePair.Value;
                    if (apertureConstruction != null)
                    {
                        result.Add(new ApertureConstruction(System.Guid.NewGuid(), apertureConstruction, apertureConstruction.Name), keyValuePair.Key);
                        
                    }
                }
            }

            if(materials != null)
            {
                foreach(IMaterial material in materials)
                {
                    if(material == null)
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