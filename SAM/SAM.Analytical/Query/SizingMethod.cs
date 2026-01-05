// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static SizingMethod SizingMethod(this SpaceSimulationResult spaceSimulationResult)
        {
            if (spaceSimulationResult == null)
                return Analytical.SizingMethod.Undefined;

            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.SizingMethod, out string text) || string.IsNullOrWhiteSpace(text))
                return Analytical.SizingMethod.Undefined;

            return Core.Query.Enum<SizingMethod>(text);
        }
    }
}
