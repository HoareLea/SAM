// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Opening Analytical Type")]
    public enum OpeningAnalyticalType
    {
        [Description("Undefined")] Undefined,
        [Description("Window")] Window,
        [Description("Door")] Door,
    }
}
