// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static IFilter FindByType(this IRelationFilter relationFilter, System.Type type)
        {
            if (relationFilter == null || type == null)
            {
                return null;
            }

            if (type.IsAssignableFrom(relationFilter.GetType()))
            {
                return relationFilter;
            }

            IFilter result = relationFilter.Filter;
            if (result == null)
            {
                return null;
            }

            if (type.IsAssignableFrom(result.GetType()))
            {
                return result;
            }

            return FindByType(result as IRelationFilter, type);
        }
    }
}
