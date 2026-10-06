// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// How a space stands against TM59's full-year &gt;28 °C reference threshold - the check applied to
    /// communal corridors and, by the same route, to any space with no assessed occupied-space use.
    /// <para>
    /// <b>Why this is not a <see cref="TM59ComplianceStatus"/>.</b> Exceeding the threshold is something to
    /// highlight for design review, not a failure of the occupied-space assessment - the dwellings are judged
    /// against their own criteria and a corridor cannot fail them on their behalf. Reporting it as "Fail"
    /// would state a regulatory outcome TM59 does not reach.
    /// </para>
    /// <para>
    /// <b>And it is not an identification.</b> A <c>SignificantRisk</c> row says the threshold was exceeded;
    /// it does not say the space is a communal corridor, because the assessment's own bucketing cannot tell a
    /// corridor from an unassessed bathroom or hall.
    /// </para>
    /// </summary>
    public enum TM59RiskStatus
    {
        /// <summary>Not a risk-assessed check - the space is judged by a compliance criterion instead.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>Within the reference threshold.</summary>
        [Description("Acceptable")] Acceptable,

        /// <summary>
        /// The reference threshold was exceeded and the space should be highlighted for design review. This is
        /// deliberately not an occupied-space compliance failure.
        /// </summary>
        [Description("Significant Risk")] SignificantRisk,
    }
}
