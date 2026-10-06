// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// The outcome of one TM59 criterion applied to one space, distinguishing a criterion that was met from
    /// one that was exceeded and from one that was never required of that space.
    /// <para>
    /// A software result is never a legal certification of compliance with the Building Regulations. These
    /// values describe a CIBSE TM59 <i>assessment</i> of simulated temperatures only, and in particular do
    /// not establish that the Approved Document O modelling assumptions behind the simulation were applied.
    /// </para>
    /// <para>
    /// Deliberately separate from <see cref="TM59RiskStatus"/>: the communal-corridor threshold is a
    /// reporting reference, not a mandatory occupied-space test, and folding the two into one Pass/Fail
    /// would state a regulatory failure TM59 does not make.
    /// </para>
    /// </summary>
    public enum TM59ComplianceStatus
    {
        /// <summary>No criterion outcome is known - nothing was assessed.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>The criterion applies to this space and was satisfied.</summary>
        [Description("Pass")] Pass,

        /// <summary>The criterion applies to this space and was exceeded.</summary>
        [Description("Fail")] Fail,

        /// <summary>
        /// The criterion is not required of this space - a bedroom night-time check on a room that is not a
        /// bedroom, or the full-year &gt;28 °C threshold, which is reported as a <see cref="TM59RiskStatus"/>
        /// instead. Never treated as a pass.
        /// </summary>
        [Description("Not Applicable")] NotApplicable,
    }
}
