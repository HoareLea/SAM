using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static HashSet<string> RemoveUnusedMaterials(this ConstructionManager constructionManager)
        {
            if(constructionManager == null)
            {
                return null;
            }

            List<Core.IMaterial> materials = constructionManager.Materials;
            if (materials == null || materials.Count == 0)
            {
                return null;
            }

            HashSet<string> names = new HashSet<string>();
            
            List<Construction> constructions = constructionManager.Constructions;
            if(constructions != null)
            {
                foreach(Construction construction in constructions)
                {
                    List<ConstructionLayer> constructionLayers = construction?.ConstructionLayers;
                    if(constructionLayers == null || constructionLayers.Count == 0)
                    {
                        continue;
                    }

                    constructionLayers.ForEach(x => names.Add(x?.Name));
                }
            }

            List<ApertureConstruction> apertureConstructions = constructionManager.ApertureConstructions;
            if (apertureConstructions != null)
            {
                foreach (ApertureConstruction apertureConstruction in apertureConstructions)
                {
                    List<ConstructionLayer> constructionLayers = null;

                    constructionLayers = apertureConstruction?.FrameConstructionLayers;
                    if (constructionLayers != null && constructionLayers.Count != 0)
                    {
                        constructionLayers.ForEach(x => names.Add(x?.Name));
                    }

                    constructionLayers = apertureConstruction?.PaneConstructionLayers;
                    if (constructionLayers != null && constructionLayers.Count != 0)
                    {
                        constructionLayers.ForEach(x => names.Add(x?.Name));
                    }
                }
            }

            HashSet<string> result = new HashSet<string>();
            
            foreach (Core.IMaterial material in materials)
            {
                if(!names.Contains(material.Name))
                {
                    constructionManager.Remove(material);
                    result.Add(material.Name);
                }
            }

            return result;
        }
    }
}