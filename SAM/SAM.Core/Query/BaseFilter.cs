// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static IFilter BaseFilter(this IRelationFilter relationFilter)
        {
            IRelationFilter relationFilter_Temp = relationFilter.Filter as IRelationFilter;
            if (relationFilter_Temp == null)
            {
                return relationFilter.Filter;
            }

            return BaseFilter(relationFilter_Temp);
        }
    }
}
