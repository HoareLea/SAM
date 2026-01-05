// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Geometry
{
    [Description("Dimension")]
    public enum Dimension
    {
        [Description("Undefined")] Undefined,
        [Description("X")] X,
        [Description("Y")] Y,
        [Description("Z")] Z,
    }
}
