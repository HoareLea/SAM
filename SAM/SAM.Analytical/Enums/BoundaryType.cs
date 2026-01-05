// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Analytical Boundary Type.")]
    public enum BoundaryType
    {
        [Description("Undefined")] Undefined,
        [Description("Ground")] Ground,
        [Description("Exposed")] Exposed,
        [Description("Adiabatic")] Adiabatic,
        [Description("Linked")] Linked,
        [Description("Shade")] Shade
    }
}
