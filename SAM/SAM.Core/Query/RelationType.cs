// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static RelationType RelationType(int count_1, int count_2)
        {
            if (count_1 <= 0 || count_2 <= 0)
            {
                return Core.RelationType.Undefined;
            }

            if (count_1 == 1 && count_2 == 1)
            {
                return Core.RelationType.OneToOne;
            }

            if (count_1 == 1)
            {
                return Core.RelationType.OneToMany;
            }

            if (count_2 == 1)
            {
                return Core.RelationType.ManyToOne;
            }

            return Core.RelationType.ManyToMany;
        }
    }
}
