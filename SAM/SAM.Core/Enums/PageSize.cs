// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Page Size")]
    public enum PageSize
    {
        [Description("Undefined")] Undefined,
        [Description("A4")] A4,
        [Description("A3")] A3,
    }
}
