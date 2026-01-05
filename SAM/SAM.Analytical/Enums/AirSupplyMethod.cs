// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Air Supply Method
    /// </summary>
    [Description("Air Supply Method")]
    public enum AirSupplyMethod
    {
        /// <summary>
        /// Undefined Air Supply Method
        /// </summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Outside Air Supply Method
        /// </summary>
        [Description("Outside")] Outside,

        /// <summary>
        /// Total Air Supply Method
        /// </summary>
        [Description("Total")] Total
    }
}
