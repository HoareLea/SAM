// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Lighting Photoelectric Controls")]
    public enum LightingPhotoelectricControls
    {
        [Description("Undefined")] Undefined,
        [Description("None")] None,
        [Description("Manual")] Manual,
        [Description("Photocell On Off")] PhotocellOnOff,
        [Description("Photocell Dimming")] PhotocellDimming,
    }
}
