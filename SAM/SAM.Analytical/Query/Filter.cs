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

                jSAMObjects_Filtering.RemoveAll(x => x is SAMObject);
            }

            if(jSAMObjects_Filtering.Count == 0)
            {
                return new List<T>();
            }

            return Core.Query.Filter<T>(jSAMObjects_Filtering, filter);
        }
    }
}