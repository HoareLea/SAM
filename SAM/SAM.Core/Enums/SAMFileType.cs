// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Base SAM File Types.")]
    public enum SAMFileType
    {
        [Description("Undefined")] Undefined,
        [Description("Json")] Json,
        [Description("SAM")] SAM,
    }
}
