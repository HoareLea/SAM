// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// The three states of one reported peak. They are never collapsed: a missing peak is not a zero, and a zero is
    /// not missing.
    /// </summary>
    public enum LoadPeakState
    {
        /// <summary>No usable peak: none was recorded, or the recorded one is invalid. Never shown as 0.</summary>
        [Description("Unavailable")] Unavailable,

        /// <summary>The simulation ran and there was no demand: load 0, and no peak timestep.</summary>
        [Description("Zero")] Zero,

        /// <summary>A peak load above zero, with whatever time, state and terms the engine reported with it.</summary>
        [Description("Value")] Value,
    }
}
