// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool TryGetInternalCondition(this Space space, InternalConditionLibrary internalConditionLibrary, TextMap textMap, out InternalCondition internalCondition)
        {
            internalCondition = null;

            if (!TryGetInternalConditions(space, internalConditionLibrary, textMap, out List<InternalCondition> internalConditions) || internalConditions == null || internalConditions.Count == 0)
            {
                return false;
            }

            internalCondition = internalConditions.Find(x => x != null);
            return internalCondition != null;
        }
    }
}
