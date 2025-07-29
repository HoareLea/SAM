using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel Merge(this AnalyticalModel analyticalModel, Type type, MergeSettings mergeSettings)
        {
            if(analyticalModel == null || type == null || mergeSettings == null)
            {
                return null;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
            {
                return null;
            }

            if(Modify.Merge(adjacencyCluster, type, mergeSettings))
            {
                return new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            return new AnalyticalModel(analyticalModel);
        }
    }
}