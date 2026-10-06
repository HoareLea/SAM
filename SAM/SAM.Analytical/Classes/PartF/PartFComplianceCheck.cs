// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One clause-level Approved Document F check, with where it came from, what it concluded and what
    /// that conclusion rests on.
    /// <para>
    /// Many Part F requirements are not calculable from an analytical model at all - that a system is
    /// designed to minimise noise (paragraph 1.5), that filters are accessible (paragraph 1.8), that the
    /// occupier was given operating instructions (paragraph 4.13). Those are represented as structured
    /// checks a person resolves and signs, rather than being silently omitted from the assessment or,
    /// worse, silently passed.
    /// </para>
    /// <para>
    /// <see cref="IsMandatory"/> decides whether an unresolved check can block an overall pass. A check
    /// that is <see cref="PartFComplianceStatus.NotApplicable"/> never blocks anything.
    /// </para>
    /// </summary>
    public class PartFComplianceCheck : SAMObject
    {
        public PartFComplianceCheck()
        {
        }

        public PartFComplianceCheck(string name, string sourceReference, string requirement)
            : base(name)
        {
            SourceReference = sourceReference;
            Requirement = requirement;
        }

        public PartFComplianceCheck(PartFComplianceCheck partFComplianceCheck)
            : base(partFComplianceCheck)
        {
            if (partFComplianceCheck is not null)
            {
                SourceReference = partFComplianceCheck.SourceReference;
                Requirement = partFComplianceCheck.Requirement;
                Category = partFComplianceCheck.Category;
                Status = partFComplianceCheck.Status;
                CalculatedStatus = partFComplianceCheck.CalculatedStatus;
                IsMandatory = partFComplianceCheck.IsMandatory;
                Evidence = partFComplianceCheck.Evidence;
                UserEvidence = partFComplianceCheck.UserEvidence;
                AlternativeComplianceMethod = partFComplianceCheck.AlternativeComplianceMethod;
                OverrideReason = partFComplianceCheck.OverrideReason;
                Notes = partFComplianceCheck.Notes;
                Date = partFComplianceCheck.Date;
                ResponsiblePerson = partFComplianceCheck.ResponsiblePerson;
                SpaceName = partFComplianceCheck.SpaceName;
            }
        }

        public PartFComplianceCheck(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>The Approved Document paragraph, table or appendix this check implements.</summary>
        public string SourceReference { get; set; }

        /// <summary>What the Approved Document requires, in the engineer's language.</summary>
        public string Requirement { get; set; }

        /// <summary>
        /// A grouping label for reporting, e.g. "Extract ventilation", "Transfer air", "Commissioning".
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The outcome carried into the assessment, i.e. the final assessment status. See
        /// <see cref="PartFComplianceStatus"/> and <see cref="FinalAssessmentStatus"/>.
        /// <para>
        /// Set it directly only while a check is being built. Once <see cref="CalculatedStatus"/> is
        /// recorded, apply a person's answer through <see cref="ApplyUserResolution"/>, which will not let
        /// a calculated failure be turned into a pass.
        /// </para>
        /// </summary>
        public PartFComplianceStatus Status { get; set; } = PartFComplianceStatus.NotAssessed;

        /// <summary>
        /// What SAM calculated, before any person touched the check. Never changed by a confirmation, an
        /// override or an alternative compliance method.
        /// <para>
        /// It exists so a calculated failure survives everything that happens to the check afterwards. An
        /// assessment that let the original result be erased could not show a reader what the design
        /// actually did, only what someone decided to say about it.
        /// </para>
        /// </summary>
        public PartFComplianceStatus CalculatedStatus { get; set; } = PartFComplianceStatus.NotAssessed;

        /// <summary>
        /// The status the assessment reports for this check: <see cref="CalculatedStatus"/> where nobody
        /// has recorded anything, and otherwise the recorded outcome, subject to the rules in
        /// <see cref="ApplyUserResolution"/>.
        /// <para>An alias of <see cref="Status"/>, named for what it is.</para>
        /// </summary>
        public PartFComplianceStatus FinalAssessmentStatus
        {
            get { return Status; }
            set { Status = value; }
        }

        /// <summary>
        /// What a person supplied that the model did not contain: a measurement, a drawing reference, a
        /// product datasheet, a site observation. Held separately from <see cref="Evidence"/>, which is
        /// what SAM calculated or read from the geometry, so the two are never confused.
        /// </summary>
        public string UserEvidence { get; set; }

        /// <summary>
        /// An alternative way of meeting the requirement, recorded where the design does not meet the
        /// Approved Document's guidance route. The Approved Document is guidance, not the only means of
        /// compliance, but an alternative is a case made to a building control body - so recording one
        /// moves the check to
        /// <see cref="PartFComplianceStatus.AlternativeSolutionPendingApproval"/> and never to a pass.
        /// </summary>
        public string AlternativeComplianceMethod { get; set; }

        /// <summary>
        /// Why the recorded outcome differs from <see cref="CalculatedStatus"/>. Required in practice for
        /// any check whose calculated result was a failure, so that the departure is on the record with
        /// its reasoning rather than as a bare status.
        /// </summary>
        public string OverrideReason { get; set; }

        /// <summary>
        /// True where the reported status is not the one SAM calculated, so a reader can find every
        /// check a person has moved without comparing the two by eye.
        /// </summary>
        public bool IsUserResolved
        {
            get { return CalculatedStatus != PartFComplianceStatus.NotAssessed && Status != CalculatedStatus; }
        }

        /// <summary>
        /// True where leaving this check unresolved prevents the dwelling reaching an overall pass.
        /// </summary>
        public bool IsMandatory { get; set; } = true;

        /// <summary>
        /// What the conclusion rests on: the calculated values, the geometry read, or the document a
        /// person is relying on.
        /// </summary>
        public string Evidence { get; set; }

        /// <summary>Any qualification the engineer wants carried with the result.</summary>
        public string Notes { get; set; }

        /// <summary>
        /// When the check was resolved, as free text so a project's own date convention survives a round
        /// trip unchanged.
        /// </summary>
        public string Date { get; set; }

        /// <summary>Who resolved the check.</summary>
        public string ResponsiblePerson { get; set; }

        /// <summary>Who resolved the check. An alias of <see cref="ResponsiblePerson"/>.</summary>
        public string ConfirmedBy
        {
            get { return ResponsiblePerson; }
            set { ResponsiblePerson = value; }
        }

        /// <summary>When the check was resolved. An alias of <see cref="Date"/>.</summary>
        public string ConfirmationDate
        {
            get { return Date; }
            set { Date = value; }
        }

        /// <summary>The space the check applies to, or null where it applies to the whole dwelling.</summary>
        public string SpaceName { get; set; }

        /// <summary>
        /// True where this check is resolved in a way that permits an overall pass: it passed, a person
        /// confirmed it, or it does not apply.
        /// <para>
        /// A check SAM calculated as failed is never resolved, whatever <see cref="Status"/> now says.
        /// <see cref="ApplyUserResolution"/> will not produce that combination, and this makes the rule
        /// hold even if <see cref="Status"/> is assigned directly from somewhere else. A calculated
        /// failure leaves the assessment by being corrected and recalculated, not by being relabelled.
        /// </para>
        /// </summary>
        public bool IsResolved
        {
            get { return ClaimsRequirementMet && !IsCalculatedFailureOverstated; }
        }

        /// <summary>
        /// True where SAM calculated a failure and the reported status nevertheless claims the
        /// requirement is met.
        /// <para>
        /// <see cref="ApplyUserResolution"/> never produces this combination. It is checked anyway,
        /// because <see cref="Status"/> is a settable property and a deserialised, hand-edited or
        /// third-party-written check must not be able to reach a pass that the calculation refused.
        /// </para>
        /// </summary>
        public bool IsCalculatedFailureOverstated
        {
            get { return CalculatedStatus == PartFComplianceStatus.Fail && ClaimsRequirementMet; }
        }

        /// <summary>True where the reported status asserts the requirement is met or does not apply.</summary>
        private bool ClaimsRequirementMet
        {
            get
            {
                return Status == PartFComplianceStatus.Pass
                    || Status == PartFComplianceStatus.UserConfirmed
                    || Status == PartFComplianceStatus.NotApplicable;
            }
        }

        /// <summary>
        /// Applies a person's recorded answer to this check and reports whether it was adopted as given.
        /// <para>
        /// <b>A calculated failure can never be turned into a pass by changing its status.</b> A failed
        /// check is arithmetic or geometry measured against the Approved Document, and neither moves
        /// because someone ticked a box. Where <see cref="CalculatedStatus"/> is
        /// <see cref="PartFComplianceStatus.Fail"/>, a recorded <see cref="PartFComplianceStatus.Pass"/>
        /// or <see cref="PartFComplianceStatus.UserConfirmed"/> is refused and the check is redirected to
        /// <see cref="PartFComplianceStatus.AlternativeSolutionPendingApproval"/> where an
        /// <see cref="AlternativeComplianceMethod"/> has been recorded, and otherwise to
        /// <see cref="PartFComplianceStatus.EngineeringReviewRequired"/>. The four routes out of a
        /// calculated failure are: correct the input and recalculate; supply the missing measured or
        /// geometric evidence and recalculate; record an alternative compliance method; or send the item
        /// for engineering or building control review. None of them erases
        /// <see cref="CalculatedStatus"/>.
        /// </para>
        /// <para>
        /// Where the check could not be decided, a person's answer is adopted as recorded - that is the
        /// case this mechanism exists for. Where it passed, a person may still record a worse outcome:
        /// evidence that contradicts a calculation is information, and only the direction that turns
        /// absence of evidence into compliance is barred.
        /// </para>
        /// </summary>
        /// <param name="partFComplianceCheck">The recorded answer to adopt.</param>
        /// <returns>True where the recorded status was adopted exactly as given.</returns>
        public bool ApplyUserResolution(PartFComplianceCheck partFComplianceCheck)
        {
            if (partFComplianceCheck is null || partFComplianceCheck.Status == PartFComplianceStatus.NotAssessed)
            {
                return false;
            }

            ConfirmedBy = partFComplianceCheck.ConfirmedBy;
            ConfirmationDate = partFComplianceCheck.ConfirmationDate;

            UserEvidence = string.IsNullOrWhiteSpace(partFComplianceCheck.UserEvidence)
                ? partFComplianceCheck.Evidence
                : partFComplianceCheck.UserEvidence;

            AlternativeComplianceMethod = partFComplianceCheck.AlternativeComplianceMethod;
            OverrideReason = partFComplianceCheck.OverrideReason;

            if (!string.IsNullOrWhiteSpace(partFComplianceCheck.Notes))
            {
                Notes = partFComplianceCheck.Notes;
            }

            bool isFailure = CalculatedStatus == PartFComplianceStatus.Fail;
            bool isPass = partFComplianceCheck.Status == PartFComplianceStatus.Pass
                || partFComplianceCheck.Status == PartFComplianceStatus.UserConfirmed
                || partFComplianceCheck.Status == PartFComplianceStatus.NotApplicable;

            if (isFailure && isPass)
            {
                Status = string.IsNullOrWhiteSpace(AlternativeComplianceMethod)
                    ? PartFComplianceStatus.EngineeringReviewRequired
                    : PartFComplianceStatus.AlternativeSolutionPendingApproval;

                return false;
            }

            Status = partFComplianceCheck.Status;

            return true;
        }

        /// <summary>
        /// The recorded resolution as a sentence, or null where nobody has recorded one. Written here
        /// rather than in the report so that every surface - report, grid, export - says the same thing
        /// about a check a person has moved.
        /// </summary>
        public string ResolutionSummary()
        {
            if (!IsUserResolved && string.IsNullOrWhiteSpace(UserEvidence) && string.IsNullOrWhiteSpace(AlternativeComplianceMethod))
            {
                return null;
            }

            List<string> parts = [];

            if (CalculatedStatus != PartFComplianceStatus.NotAssessed)
            {
                parts.Add(string.Format("SAM calculated this check as {0}; it is reported as {1}.", Core.Query.Description(CalculatedStatus), Core.Query.Description(Status)));
            }

            if (!string.IsNullOrWhiteSpace(UserEvidence))
            {
                parts.Add(string.Format("Recorded evidence: {0}", UserEvidence));
            }

            if (!string.IsNullOrWhiteSpace(AlternativeComplianceMethod))
            {
                parts.Add(string.Format("Alternative compliance method: {0}", AlternativeComplianceMethod));
            }

            if (!string.IsNullOrWhiteSpace(OverrideReason))
            {
                parts.Add(string.Format("Reason: {0}", OverrideReason));
            }

            if (CalculatedStatus == PartFComplianceStatus.Fail)
            {
                parts.Add("The calculated failure is retained and is not overturned by this record.");
            }

            return parts.Count == 0 ? null : string.Join(" ", parts);
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            SourceReference = PartFJson.String(jsonObject, "SourceReference");
            Requirement = PartFJson.String(jsonObject, "Requirement");
            Category = PartFJson.String(jsonObject, "Category");
            Evidence = PartFJson.String(jsonObject, "Evidence");
            UserEvidence = PartFJson.String(jsonObject, "UserEvidence");
            AlternativeComplianceMethod = PartFJson.String(jsonObject, "AlternativeComplianceMethod");
            OverrideReason = PartFJson.String(jsonObject, "OverrideReason");
            Notes = PartFJson.String(jsonObject, "Notes");
            Date = PartFJson.String(jsonObject, "Date");
            ResponsiblePerson = PartFJson.String(jsonObject, "ResponsiblePerson");
            SpaceName = PartFJson.String(jsonObject, "SpaceName");

            IsMandatory = PartFJson.Boolean(jsonObject, "IsMandatory", IsMandatory);

            if (jsonObject.ContainsKey("Status"))
            {
                Status = Core.Query.Enum<PartFComplianceStatus>(PartFJson.String(jsonObject, "Status"));
            }

            if (jsonObject.ContainsKey("CalculatedStatus"))
            {
                CalculatedStatus = Core.Query.Enum<PartFComplianceStatus>(PartFJson.String(jsonObject, "CalculatedStatus"));
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            PartFJson.SetString(result, "SourceReference", SourceReference);
            PartFJson.SetString(result, "Requirement", Requirement);
            PartFJson.SetString(result, "Category", Category);
            PartFJson.SetString(result, "Evidence", Evidence);
            PartFJson.SetString(result, "UserEvidence", UserEvidence);
            PartFJson.SetString(result, "AlternativeComplianceMethod", AlternativeComplianceMethod);
            PartFJson.SetString(result, "OverrideReason", OverrideReason);
            PartFJson.SetString(result, "Notes", Notes);
            PartFJson.SetString(result, "Date", Date);
            PartFJson.SetString(result, "ResponsiblePerson", ResponsiblePerson);
            PartFJson.SetString(result, "SpaceName", SpaceName);

            result["IsMandatory"] = IsMandatory;
            result["Status"] = Status.ToString();
            result["CalculatedStatus"] = CalculatedStatus.ToString();

            return result;
        }
    }
}
