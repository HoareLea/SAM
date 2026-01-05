// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Load Type.")]
    public enum LoadType
    {
        [Description("Undefined")] Undefined,
        [Description("Cooling")] Cooling,
        [Description("Heating")] Heating,
    }
}
