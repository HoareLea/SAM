// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Which fans, and which airflow, a published specific fan power divides by.
    /// <para>
    /// <b>Stated, never assumed</b>, for the same reason as <see cref="HeatRecoveryEfficiencyBasis"/>: a
    /// per-fan figure read as a whole-unit one halves the unit's fan power, and the reverse doubles it.
    /// <see cref="FanPerformance"/> refuses data whose basis is <see cref="Undefined"/>.
    /// </para>
    /// </summary>
    [Description("Specific Fan Power Basis")]
    public enum SpecificFanPowerBasis
    {
        /// <summary>Nothing was stated. Never usable - the data carrying it is refused.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// The electrical input of <b>both</b> fans together, divided by the airflow on one side at
        /// balanced airflow - the whole-unit figure a domestic heat recovery unit's SFP is certified as
        /// (the SAP Product Characteristics Database convention).
        /// <para>
        /// A total, not a per-fan split. How it is shared between the supply and the extract fan is not
        /// something this basis states, and deciding that is the consumer's declared simplification, not
        /// manufacturer data.
        /// </para>
        /// </summary>
        [Description("Total Both Fans")] TotalBothFans,
    }
}
