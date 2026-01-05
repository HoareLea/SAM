// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Opening Part")]
    public enum OpeningPart
    {
        [Description("Undefined")] Undefined,
        [Description("Frame")] Frame,
        [Description("Pane")] Pane,
    }
}
