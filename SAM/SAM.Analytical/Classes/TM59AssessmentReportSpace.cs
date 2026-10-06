// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// One occupied space of a <see cref="TM59AssessmentReport"/>, with its overall TM59 outcome: every
    /// occupied-space check the report holds for that space, and those checks combined.
    /// <para>
    /// <b>The report's per-space verdict, stated once.</b> <see cref="ComplianceStatus"/> is the value the text
    /// report prints in its natural-ventilation "Overall" column, and the formatter reads it from here rather
    /// than working it out itself - so a caller counting passing and failing spaces reads the same verdict an
    /// engineer reads, and cannot restate the combining rule.
    /// </para>
    /// <para>
    /// <b>A view, never a second assessment.</b> Nothing here counts an hour or compares against a limit. Each
    /// check carries its result's own verdict (see <see cref="TM59AssessmentReportCheck.ComplianceStatus"/>),
    /// and they are combined exactly as the report combines a whole section: any failure fails the space, and a
    /// space with nothing applicable to it is <c>NotApplicable</c> rather than a vacuous pass.
    /// </para>
    /// </summary>
    public class TM59AssessmentReportSpace
    {
        internal TM59AssessmentReportSpace(string reference, string spaceName, List<TM59AssessmentReportCheck> checks, TM59ComplianceStatus tM59ComplianceStatus)
        {
            Reference = reference;
            SpaceName = spaceName;
            Checks = checks ?? [];
            ComplianceStatus = tM59ComplianceStatus;
        }

        /// <summary>
        /// The stable identity the space's checks share - the simulated space's Guid, as text. The grouping key,
        /// so two dwellings' "Bedroom 2" are two spaces. See <see cref="TM59AssessmentReportCheck.Reference"/>.
        /// </summary>
        public string Reference { get; }

        /// <summary>The simulated space's name, for display only.</summary>
        public string SpaceName { get; }

        /// <summary>This space's occupied-space checks, in report order - Criterion 1 and 2, or the &gt;26 °C check.</summary>
        public List<TM59AssessmentReportCheck> Checks { get; }

        /// <summary>
        /// The space's overall TM59 outcome: <c>Fail</c> where any of its checks failed, <c>Pass</c> where none
        /// failed and at least one passed, <c>NotApplicable</c> where none applied.
        /// </summary>
        public TM59ComplianceStatus ComplianceStatus { get; }
    }
}
