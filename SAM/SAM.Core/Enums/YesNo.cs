// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Yes/No Enum")]
    public enum YesNo
    {
        [Description("Undefined")] Undefined,
        [Description("Yes")] Yes,
        [Description("No")] No
    }
}
