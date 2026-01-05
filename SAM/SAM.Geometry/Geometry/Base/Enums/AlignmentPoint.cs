// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Geometry
{
    [Description("Alignment Point")]
    public enum AlignmentPoint
    {
        [Description("Undefined")] Undefined,
        [Description("Start")] Start,
        [Description("Mid")] Mid,
        [Description("End")] End,
    }
}
