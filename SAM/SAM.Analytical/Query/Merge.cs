// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel Merge(this AnalyticalModel analyticalModel, Type type, MergeSettings mergeSettings)
        {
            if (analyticalModel == null || type == null || mergeSettings == null)
            {
                return null;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if (adjacencyCluster == null)
            {
                return null;
            }

            if (Modify.Merge(adjacencyCluster, type, mergeSettings))
            {
                return new AnalyticalModel(analyticalModel, adjacencyCluster);
            }

            return new AnalyticalModel(analyticalModel);
        }
    }
}
