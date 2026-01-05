// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Panel Group")]
    public enum PanelGroup
    {
        [Description("Undefined")] Undefined,
        [Description("Floor")] Floor,
        [Description("Roof")] Roof,
        [Description("Wall")] Wall,
        [Description("Other")] Other
    }
}
