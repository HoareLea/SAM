// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Direction Enum")]
    public enum Direction
    {
        [Description("Undefined")] Undefined,
        [Description("In")] In,
        [Description("Out")] Out
    }
}
