// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Geometry
{
    [Description("Edge Orientation Method")]
    public enum EdgeOrientationMethod
    {
        [Description("Undefined")] Undefined,
        [Description("Similar")] Similar,
        [Description("Opposite")] Opposite,
    }
}
