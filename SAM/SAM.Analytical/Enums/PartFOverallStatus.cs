// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// The overall outcome of a dwelling's Part F conformance assessment. Deliberately not a boolean:
    /// "not a pass" and "a fail" are different answers, and so are "unresolved" and "not applicable".
    /// <para>
    /// A dwelling can never reach <see cref="Pass"/> while any mandatory check is failed or unresolved.
    /// </para>
    /// </summary>
    public enum PartFOverallStatus
    {
        /// <summary>No check was run for this dwelling.</summary>
        [Description("Not Assessed")] NotAssessed,

        /// <summary>Every mandatory check passed, was confirmed by a person, or did not apply.</summary>
        [Description("Pass")] Pass,

        /// <summary>At least one mandatory check failed.</summary>
        [Description("Fail")] Fail,

        /// <summary>
        /// Nothing failed, but at least one mandatory check was never run while others were resolved.
        /// </summary>
        [Description("Partial")] Partial,

        /// <summary>
        /// Nothing failed and nothing needs an engineering decision, but at least one mandatory check
        /// could not be decided from the information available.
        /// </summary>
        [Description("Cannot Be Determined")] CannotBeDetermined,

        /// <summary>
        /// Nothing failed, but at least one mandatory check needs an engineer's decision.
        /// </summary>
        [Description("Engineering Review Required")] EngineeringReviewRequired,

        /// <summary>
        /// At least one mandatory check was calculated as failed and rests on an alternative compliance
        /// method that a building control body has not yet accepted. Reported ahead of
        /// <see cref="Fail"/> only because the case has been made; it is not a pass and the underlying
        /// calculated failure is still on the check.
        /// </summary>
        [Description("Alternative Solution Pending Approval")] AlternativeSolutionPendingApproval,
    }
}
