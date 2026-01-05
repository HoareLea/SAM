// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Units
{
    public enum UnitStyle
    {
        [Description("Undefined")] Undefined,
        [Description("Imperial")] Imperial,
        [Description("SI")] SI
    }
}
