// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void AssignAdjacencyCluster(this IFilter filter, AdjacencyCluster adjacencyCluster)
        {
            if (filter == null)
            {
                return;
            }

            if (filter is LogicalFilter)
            {
                ((LogicalFilter)filter).Filters?.ForEach(x => AssignAdjacencyCluster(x, adjacencyCluster));
            }
            else if (filter is IRelationFilter)
            {
                AssignAdjacencyCluster(((IRelationFilter)filter).Filter, adjacencyCluster);
            }
            else if (filter is IAdjacencyClusterFilter)
            {
                ((IAdjacencyClusterFilter)filter).AdjacencyCluster = adjacencyCluster;
            }
            else if (filter is ISAMObjectRelationClusterFilter)
            {
                ((ISAMObjectRelationClusterFilter)filter).SAMObjectRelationCluster = adjacencyCluster;
            }
        }
    }
}
