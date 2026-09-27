// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Which design airflow an MVHR dwelling's selected strategy is materialised at - the <b>intent</b>, never
    /// the airflow itself.
    /// <para>
    /// <b>One airflow authority.</b> The design airflow lives on <c>VentilationTerminal.DesignFlowRate_Lps</c>
    /// and nowhere else. A strategy states only which design it expects there - the Approved Document F
    /// requirement as realised, or a design the engineer accepted (an Iteration 2B outcome) - plus, for the
    /// latter, a fingerprint of the terminal set it accepted (<c>Query.PartODwellingDesignFingerprint</c>).
    /// It never holds a flow. See <c>documentation/PartO-MixedDwellingStrategies-PR0.md</c> §D3.
    /// </para>
    /// </summary>
    [Description("Part O Design Air Flow Basis.")]
    public enum PartODesignAirFlowBasis
    {
        /// <summary>Not stated, or unreadable. A strategy carrying it is invalid.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// The Approved Document F continuous requirement, realised as design terminals. A dwelling whose
        /// baseline terminals differ from the requirement is refused rather than silently reset.
        /// </summary>
        [Description("Part F Requirement")] PartFRequirement,

        /// <summary>
        /// A design the engineer accepted and wrote onto the baseline's own terminals, guarded by the
        /// strategy's design fingerprint. Terminals that no longer match the fingerprint are stale and refused.
        /// Valid for MVHR only.
        /// </summary>
        [Description("Retained Design")] RetainedDesign,
    }
}
