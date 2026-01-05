// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Material Type")]
    public enum MaterialType
    {
        [Description("Undefined Material")] Undefined,
        [Description("Gas Material")] Gas,
        [Description("Opaque Material")] Opaque,
        [Description("Transparent Material")] Transparent,
    }
}
