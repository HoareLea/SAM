// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Classes;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Cases Cases<T>(IEnumerable<T> cases) where T : Case
        {
            if (cases == null)
            {
                return null;
            }

            List<Case> cases_Temp = [];

            foreach (T @case in cases)
            {
                if (@case is null)
                {
                    continue;
                }

                cases_Temp.Add(@case);
            }

            return [.. cases_Temp];
        }
    }
}
