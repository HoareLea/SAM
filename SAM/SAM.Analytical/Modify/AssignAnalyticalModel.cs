// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void AssignAnalyticalModel(this IFilter filter, AnalyticalModel analyticalModel)
        {
            if (filter == null)
            {
                return;
            }

            if (filter is LogicalFilter)
            {
                ((LogicalFilter)filter).Filters?.ForEach(x => AssignAnalyticalModel(x, analyticalModel));
            }
            else if (filter is IRelationFilter)
            {
                AssignAnalyticalModel(((IRelationFilter)filter).Filter, analyticalModel);
            }
            else if (filter is IAnalyticalModelFilter)
            {
                ((IAnalyticalModelFilter)filter).AnalyticalModel = analyticalModel;
            }

            AssignAdjacencyCluster(filter, analyticalModel.AdjacencyCluster);
        }
    }
}
