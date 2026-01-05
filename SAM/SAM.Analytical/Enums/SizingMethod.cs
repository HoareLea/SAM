// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Sizing Method.")]
    public enum SizingMethod
    {
        [Description("Undefined")] Undefined,
        [Description("Cooling Design Day")] CDD,
        [Description("Heating Design Day")] HDD,
        [Description("Simulation")] Simulation,
    }
}
