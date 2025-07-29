using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool Merge(this AdjacencyCluster adjacencyCluster, Type type, MergeSettings mergeSettings)
        {
            if (adjacencyCluster == null || type == null || mergeSettings == null)
            {
                return false;
            }

            string fullTypeName = Core.Query.FullTypeName(type);
            if (string.IsNullOrWhiteSpace(fullTypeName))
            {
                return false;
            }

            TypeMergeSettings typeMergeSettings = mergeSettings[fullTypeName];

            if(type.IsAssignableFrom(typeof(ApertureConstruction)))
            {
                return Merge_ApertureConstruction(adjacencyCluster, typeMergeSettings?.ExcludedParameterNames);
            }

            throw new NotImplementedException();
        }

        public static bool Merge_ApertureConstruction(this AdjacencyCluster adjacencyCluster, IEnumerable<string> excludedParameterNames = null)
        {
            List<ApertureConstruction> apertureConstructions =  adjacencyCluster?.GetApertureConstructions();
            if(apertureConstructions == null || apertureConstructions.Count == 0)
            {
                return false;
            }
            List<List<ApertureConstruction>> apertureConstructionsList_Sorted = new List<List<ApertureConstruction>>();
            while (apertureConstructions != null && apertureConstructions.Count > 0)
            {
                ApertureConstruction apertureConstruction = apertureConstructions[0];
                List<ApertureConstruction> ApertureConstructions_Similar = Query.FindSimilar(apertureConstruction, apertureConstructions, excludedParameterNames);
                apertureConstructions.RemoveAll(x => ApertureConstructions_Similar.Contains(x));

                apertureConstructionsList_Sorted.Add(ApertureConstructions_Similar);
            }
            List<Aperture> apertures = new List<Aperture>();
            foreach(List<ApertureConstruction> apertureConstructions_Sorted in apertureConstructionsList_Sorted)
            {
                if(apertureConstructions_Sorted == null || apertureConstructions_Sorted.Count <= 1)
                {
                    continue;
                }

                ApertureConstruction apertureConstruction = apertureConstructions_Sorted[0];

                for (int i = 1; i < apertureConstructions_Sorted.Count; i++)
                {
                    List<Aperture> apertures_ApertureConstruction = adjacencyCluster.GetApertures(apertureConstructions_Sorted[i]);
                    if(apertures_ApertureConstruction == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < apertures_ApertureConstruction.Count; j++)
                    {
                        apertures.Add(new Aperture(apertures_ApertureConstruction[j], apertureConstruction));
                    }
                }
            }

            apertures = adjacencyCluster.UpdateApertures(apertures);

            return apertures != null && apertures.Count != 0;

        }
    }
}