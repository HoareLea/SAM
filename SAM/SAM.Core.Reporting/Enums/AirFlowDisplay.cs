// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Preferred SI display unit for volumetric air flow.
    /// </summary>
    public enum AirFlowDisplay
    {
        [Description("Liters Per Second")] LitersPerSecond,
        [Description("Cubic Meters Per Second")] CubicMetersPerSecond,
    }
}
