// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string Id(this FlowClassification flowClassification, Direction direction)
        {
            if (flowClassification == FlowClassification.Undefined || direction == Direction.Undefined)
            {
                return null;
            }

            return string.Format("{0} {1}", flowClassification.Description(), direction.Description());
        }
    }
}
