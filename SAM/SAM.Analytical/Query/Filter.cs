using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<T> Filter<T>(this AdjacencyCluster adjacencyCluster, IFilter filter, IEnumerable<IJSAMObject> jSAMObjects = null) where T: IJSAMObject
        {
            if(adjacencyCluster == null || filter == null)
            {
                return null;
            }

            List<IJSAMObject> jSAMObjects_Filtering = adjacencyCluster.FilteringSAMObjects();

            Modify.AssignAdjacencyCluster(filter, adjacencyCluster);

            if(jSAMObjects != null)
            {
                List<Tuple<Type, Guid>> tuples = jSAMObjects.ToList().ConvertAll(x => x as SAMObject).FindAll(x => x != null).ConvertAll(x => new Tuple<Type, Guid>(x.GetType(), x.Guid));
                if(tuples != null)
                {
                    for (int i = jSAMObjects_Filtering.Count - 1; i >= 0; i--)
                    {
                        SAMObject sAMObject = jSAMObjects_Filtering[i] as SAMObject;
                        if (sAMObject == null)
                        {
                            continue;
                        }

                        if(tuples.Find(x => x.Item1 == sAMObject.GetType() && x.Item2 == sAMObject.Guid) == null)
                        {
                            if(!jSAMObjects.Contains(jSAMObjects_Filtering[i]))
                            {
                                jSAMObjects_Filtering.RemoveAt(i);
                            }
                        }
                    }
                }

                jSAMObjects_Filtering.RemoveAll(x => !(x is SAMObject));
            }

            if(jSAMObjects_Filtering.Count == 0)
            {
                return new List<T>();
            }

            return Core.Query.Filter<T>(jSAMObjects_Filtering, filter);
        }

        public static ConstructionManager Filter(this ConstructionManager constructionManager, IEnumerable<Construction> constructions = null, IEnumerable<ApertureConstruction> apertureConstructions = null, bool removeUnusedMaterials = false)
        {
            if(constructionManager == null)
            {
                return null;
            }

            ConstructionManager result = new ConstructionManager(constructionManager);

            if ((constructions == null || constructions.Count() == 0) && (apertureConstructions == null || apertureConstructions.Count() == 0))
            {
                return result;
            }

            List<Construction> constrcutions_Temp = constructions == null ? new List<Construction>() : new List<Construction>(constructions);
            List<Construction> constructions_All = result.Constructions;
            if (constructions_All != null)
            {
                foreach(Construction construction in constructions_All)
                {
                    if(construction == null)
                    {
                        continue;
                    }

                    if(constrcutions_Temp.Find(x => x.Guid == construction.Guid) == null)
                    {
                        result.Remove(construction);
                    }
                }
            }

            List<ApertureConstruction> apertureConstrcutions_Temp = apertureConstructions == null ? new List<ApertureConstruction>() : new List<ApertureConstruction>(apertureConstructions);
            List<ApertureConstruction> apertureConstructions_All = result.ApertureConstructions;
            if (apertureConstructions_All != null)
            {
                foreach (ApertureConstruction apertureConstruction in apertureConstructions_All)
                {
                    if (apertureConstruction == null)
                    {
                        continue;
                    }

                    if (apertureConstrcutions_Temp.Find(x => x.Guid == apertureConstruction.Guid) == null)
                    {
                        result.Remove(apertureConstruction);
                    }
                }
            }

            if(removeUnusedMaterials)
            {
                result.RemoveUnusedMaterials();
            }

            return result;

        }
    }
}