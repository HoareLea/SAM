using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<string> MissingMaterialsNames(this AnalyticalModel analyticalModel)
        {
            return MissingMaterialsNames(analyticalModel?.AdjacencyCluster, analyticalModel?.MaterialLibrary);
        }

        public static List<string> MissingMaterialsNames(this AdjacencyCluster adjacencyCluster, MaterialLibrary materialLibrary)
        {
            if(adjacencyCluster == null)
            {
                return null;
            }

            HashSet<string> names = new HashSet<string>();

            MissingMaterialsNames(materialLibrary, adjacencyCluster.GetApertureConstructions())?.ForEach(x => names.Add(x));
            MissingMaterialsNames(materialLibrary, adjacencyCluster.GetConstructions())?.ForEach(x => names.Add(x));

            return names.ToList();
        }

        public static List<string> MissingMaterialsNames(this MaterialLibrary materialLibrary, IEnumerable<Construction> constructions)
        {
            if(constructions == null)
            {
                return null;
            }

            HashSet<string> names = new HashSet<string>();

            foreach (Construction construction in constructions)
            {
                MissingMaterialsNames(materialLibrary, construction)?.ForEach(x => names.Add(x));
            }

            return names.ToList();
        }

        public static List<string> MissingMaterialsNames(this MaterialLibrary materialLibrary, IEnumerable<ApertureConstruction> apertureConstructions)
        {
            if (apertureConstructions == null)
            {
                return null;
            }

            HashSet<string> names = new HashSet<string>();

            foreach (ApertureConstruction apertureConstruction in apertureConstructions)
            {
                MissingMaterialsNames(materialLibrary, apertureConstruction)?.ForEach(x => names.Add(x));
            }

            return names.ToList();
        }

        public static List<string> MissingMaterialsNames(this MaterialLibrary materialLibrary, Construction construction)
        {
            return MissingMaterialsNames(materialLibrary, construction?.ConstructionLayers);
        }

        public static List<string> MissingMaterialsNames(this MaterialLibrary materialLibrary, ApertureConstruction apertureConstruction)
        {
            if(apertureConstruction == null)
            {
                return null;
            }

            List<ConstructionLayer> constructionLayers_1 = apertureConstruction?.FrameConstructionLayers;
            List<ConstructionLayer> constructionLayers_2 = apertureConstruction?.PaneConstructionLayers;
            if(constructionLayers_1 == null && constructionLayers_2 == null)
            {
                return null;
            }

            HashSet<string> names = new HashSet<string>();

            MissingMaterialsNames(materialLibrary, constructionLayers_1)?.ForEach(x => names.Add(x));
            MissingMaterialsNames(materialLibrary, constructionLayers_2)?.ForEach(x => names.Add(x));

            return names.ToList();
        }

        public static List<string> MissingMaterialsNames(this MaterialLibrary materialLibrary, IEnumerable<ConstructionLayer> constructionLayers)
        {
            if(constructionLayers == null)
            {
                return null;
            }

            HashSet<string> names = new HashSet<string>();

            foreach(ConstructionLayer constructionLayer in constructionLayers)
            {
                string name = constructionLayer?.Name;
                if(string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if(materialLibrary?.GetMaterial(name) != null)
                {
                    continue;
                }

                names.Add(name);
            }

            return names.ToList();
        }

        public static List<string> MissingMaterialsNames(this ConstructionManager constructionManager)
        {
            if(constructionManager == null)
            {
                return null;
            }

            HashSet<string> names = new HashSet<string>();

            MaterialLibrary materialLibrary = constructionManager.MaterialLibrary;

            MissingMaterialsNames(materialLibrary, constructionManager.Constructions)?.ForEach(x => names.Add(x));
            MissingMaterialsNames(materialLibrary, constructionManager.ApertureConstructions)?.ForEach(x => names.Add(x));

            return names.ToList();
        }

        public static List<string> MissingMaterialsNames(this ConstructionManager constructionManager, IEnumerable<Guid> guids)
        {
            if (constructionManager == null || guids == null)
            {
                return null;
            }

            HashSet<string> names = new HashSet<string>();

            MaterialLibrary materialLibrary = constructionManager.MaterialLibrary;

            List<Construction> constructions = new List<Construction>();
            List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();

            foreach(Guid guid in guids)
            {
                Construction construction = constructionManager.GetConstruction(guid);
                if(construction != null)
                {
                    constructions.Add(construction);
                    continue;
                }

                ApertureConstruction apertureConstruction = constructionManager.GetApertureConstruction(guid);
                if (apertureConstruction != null)
                {
                    apertureConstructions.Add(apertureConstruction);
                    continue;
                }
            }

            MissingMaterialsNames(materialLibrary, constructions)?.ForEach(x => names.Add(x));
            MissingMaterialsNames(materialLibrary, apertureConstructions)?.ForEach(x => names.Add(x));

            return names.ToList();
        }
    }
}