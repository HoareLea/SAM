// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("HostPartition Category")]
    public enum HostPartitionCategory
    {
        [Description("Undefined")] Undefined,
        [Description("Wall")] Wall,
        [Description("Roof")] Roof,
        [Description("Floor")] Floor,
    }
}
