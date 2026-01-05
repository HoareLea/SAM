// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Ventilation Type.")]
    public enum VentilationType
    {
        [Description("Undefined")] Undefined,
        [Description("Outside Supply Air")] OSA,
        [Description("Total Supply Air")] TSA,
    }
}
