// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Page Orientation")]
    public enum PageOrientation
    {
        [Description("Undefined")] Undefined,
        [Description("Portrait")] Portrait,
        [Description("Landscape")] Landscape,
    }
}
