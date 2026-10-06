// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// The outcome of one Approved Document F check, distinguishing what SAM calculated from what it
    /// could not establish and from what a person confirmed.
    /// <para>
    /// A software result is never a legal certification of compliance with the Building Regulations.
    /// These values describe a Part F conformance <i>assessment</i> only.
    /// </para>
    /// </summary>
    public enum PartFComplianceStatus
    {
        /// <summary>The check was not run.</summary>
        [Description("Not Assessed")] NotAssessed,

        /// <summary>The check was run and the requirement is met by calculation or by geometry.</summary>
        [Description("Pass")] Pass,

        /// <summary>The check was run and the requirement is not met.</summary>
        [Description("Fail")] Fail,

        /// <summary>The requirement does not apply to this dwelling, room, terminal or opening.</summary>
        [Description("Not Applicable")] NotApplicable,

        /// <summary>
        /// The requirement applies, but the model does not carry enough information to decide it -
        /// typically a product property or a construction detail that is not geometry. Never treated as
        /// a pass.
        /// </summary>
        [Description("Cannot Be Determined")] CannotBeDetermined,

        /// <summary>
        /// A person has confirmed the requirement is met, with the evidence, date and responsible person
        /// recorded on the check.
        /// </summary>
        [Description("User Confirmed")] UserConfirmed,

        /// <summary>
        /// The requirement applies and something about the design needs an engineer's decision before it
        /// can be resolved. Never treated as a pass.
        /// </summary>
        [Description("Engineering Review Required")] EngineeringReviewRequired,

        /// <summary>
        /// The requirement was calculated as failed and an alternative compliance method has been
        /// recorded against it, which the building control body has not yet accepted.
        /// <para>
        /// This is the one route out of a calculated failure that does not require the failure itself to
        /// be corrected, and it deliberately is not a pass. The original calculated result stays on
        /// <see cref="PartFComplianceCheck.CalculatedStatus"/> and is never erased: an alternative
        /// solution is a case to be made to a person, not an answer software can accept on its own.
        /// </para>
        /// </summary>
        [Description("Alternative Solution Pending Approval")] AlternativeSolutionPendingApproval,
    }
}
