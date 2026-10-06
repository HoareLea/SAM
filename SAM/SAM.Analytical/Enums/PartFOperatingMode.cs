// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// The operating condition a flow rate belongs to. Rates from different modes are never combined
    /// into one number, because only the continuous design condition is the Approved Document F sizing
    /// case.
    /// </summary>
    public enum PartFOperatingMode
    {
        /// <summary>
        /// The Approved Document F sizing condition: whole dwelling (general) ventilation running
        /// continuously, per paragraph 1.24 (page 10) and paragraph 1.69 (page 16). Equipment is
        /// selected on this rate, and every implemented minimum is checked against it.
        /// </summary>
        [Description("Continuous Design")] ContinuousDesign,

        /// <summary>
        /// The high rate of Table 1.2 (page 10), for when additional extraction is required
        /// (paragraph 1.22, page 10).
        /// </summary>
        [Description("High/Boost")] HighBoost,

        /// <summary>
        /// A SAM reduced-operation convention obtained by scaling the continuous design rates. Not a
        /// regulatory condition: neither the 2021 nor the 2026 edition of Approved Document F, Volume 1
        /// specifies a reduced operating rate for mechanical ventilation with heat recovery.
        /// </summary>
        [Description("Setback")] Setback,

        /// <summary>
        /// Air flow rates measured on site during commissioning, per Section 4 and Appendix C Part 3.
        /// Recorded separately from, and never written over, the design values.
        /// </summary>
        [Description("Measured Commissioning")] MeasuredCommissioning,
    }
}
