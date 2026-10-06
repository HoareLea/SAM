// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// The Part F conformance assessment of one dwelling: every schedule the assessment produced, every
    /// clause-level check it ran, and the overall outcome those checks add up to.
    /// <para>
    /// This is an <b>assessment</b>, not a certificate. Software cannot certify compliance with the
    /// Building Regulations: compliance is demonstrated to a building control body, on the complete
    /// design and the built work, by a suitably qualified person. What this class records is which
    /// requirements were calculated, which were verified from geometry, which a person confirmed, and
    /// which remain open.
    /// </para>
    /// <para>
    /// <see cref="OverallStatus"/> is deliberately not a boolean. A dwelling cannot reach
    /// <see cref="PartFOverallStatus.Pass"/> while any mandatory check is failed or unresolved, and
    /// "could not be determined" is reported as itself rather than collapsed into either answer.
    /// </para>
    /// </summary>
    public class PartFComplianceResult
    {
        /// <summary>The edition this assessment was made against.</summary>
        public const string SourceDocumentValue = "Approved Document F - Ventilation, Volume 1: Dwellings";

        /// <summary>The edition and territory this assessment was made against.</summary>
        public const string SourceEditionValue = "2021 edition, for use in England, in effect from 15 June 2022";

        public PartFComplianceResult(string dwellingName)
        {
            DwellingName = dwellingName;
        }

        /// <summary>Name of the dwelling zone, or null where the whole model is one dwelling.</summary>
        public string DwellingName { get; private set; }

        /// <summary>The approved document this assessment was made against.</summary>
        public string SourceDocument { get; set; } = SourceDocumentValue;

        /// <summary>The edition and territory of that document.</summary>
        public string SourceEdition { get; set; } = SourceEditionValue;

        /// <summary>
        /// The ventilation system type assessed. Only mechanical ventilation with heat recovery
        /// (paragraphs 1.67 to 1.73) is implemented.
        /// </summary>
        public string SystemType { get; set; } = "Mechanical ventilation with heat recovery (Approved Document F, Volume 1, paragraphs 1.67 to 1.73)";

        /// <summary>
        /// The strategy used to share continuous extract above the Table 1.2 minimums, recorded so a
        /// result carries its own basis.
        /// </summary>
        public PartFExtractAllocationStrategy ExtractAllocationStrategy { get; set; } = PartFExtractAllocationStrategy.MinimumFirstCookingPriority;

        // ------------------------------------------------------------------
        // Schedules
        // ------------------------------------------------------------------

        /// <summary>Every terminal the assessment established for this dwelling, supply and extract.</summary>
        public List<PartFVentilationTerminalRequirement> Terminals { get; set; } = [];

        /// <summary>
        /// Every internal transfer route within this dwelling, whether or not the solver put any air
        /// through it. Paragraph 1.25 applies to internal doors generally, not only to the routes the
        /// network happens to load.
        /// </summary>
        public List<PartFDoorTransferData> TransferPaths { get; set; } = [];

        /// <summary>The purge ventilation assessment of each habitable room, per paragraphs 1.26 to 1.31.</summary>
        public List<PartFPurgeVentilationData> PurgeVentilation { get; set; } = [];

        /// <summary>Commissioning evidence for this dwelling, or null where none has been supplied.</summary>
        public PartFCommissioningData Commissioning { get; set; }

        /// <summary>Every clause-level check run for this dwelling.</summary>
        public List<PartFComplianceCheck> Checks { get; set; } = [];

        /// <summary>Conditions that need the engineer's attention.</summary>
        public List<string> Warnings { get; set; } = [];

        /// <summary>Informational notes about expected but noteworthy conditions.</summary>
        public List<string> Notes { get; set; } = [];

        // ------------------------------------------------------------------
        // Rates
        // ------------------------------------------------------------------

        /// <summary>Whole dwelling ventilation rate [l/s] at the continuous design condition.</summary>
        public double ContinuousDesignSystemRate_Lps { get; set; }

        /// <summary>Sum of the continuous design supply terminal rates [l/s].</summary>
        public double TotalContinuousSupply_Lps { get; set; }

        /// <summary>
        /// Sum of the continuous design extract terminal rates [l/s] that form part of the balanced
        /// mechanical ventilation with heat recovery flow.
        /// </summary>
        public double TotalContinuousExtract_Lps { get; set; }

        /// <summary>Sum of the high rate supply terminal rates [l/s] in the balanced flow.</summary>
        public double TotalHighSupply_Lps { get; set; }

        /// <summary>Sum of the high rate extract terminal rates [l/s] in the balanced flow.</summary>
        public double TotalHighExtract_Lps { get; set; }

        /// <summary>Sum of the setback supply terminal rates [l/s].</summary>
        public double TotalSetbackSupply_Lps { get; set; }

        /// <summary>Sum of the setback extract terminal rates [l/s].</summary>
        public double TotalSetbackExtract_Lps { get; set; }

        /// <summary>Number of classified spaces in this dwelling.</summary>
        public int DwellingSpaceCount { get; set; }

        /// <summary>
        /// Number of internal connections between two spaces of this dwelling, i.e. edges in the transfer
        /// air network.
        /// <para>
        /// Zero in a dwelling of several spaces means the model carries no internal separating elements
        /// between them at all. That is a gap in the model rather than a dwelling whose rooms do not
        /// adjoin, and the two have to be reported differently: a genuinely single-space dwelling has
        /// nothing for paragraph 1.25 to apply to, whereas an unmodelled one has a requirement that simply
        /// cannot be assessed.
        /// </para>
        /// </summary>
        public int InternalConnectionCount { get; set; }

        /// <summary>
        /// True where the dwelling has more than one space but no internal separating element between any
        /// two of them, so its transfer air could not be assessed at all.
        /// </summary>
        public bool HasNoInternalAdjacency
        {
            get { return DwellingSpaceCount > 1 && InternalConnectionCount == 0; }
        }

        /// <summary>
        /// The overall outcome. Derived from <see cref="Checks"/> by <see cref="Resolve"/>; never set by
        /// counting anything else.
        /// </summary>
        public PartFOverallStatus OverallStatus { get; set; } = PartFOverallStatus.NotAssessed;

        /// <summary>Checks that failed.</summary>
        public List<PartFComplianceCheck> FailedChecks
        {
            get { return [.. (Checks ?? []).Where(x => x.Status == PartFComplianceStatus.Fail)]; }
        }

        /// <summary>Checks that could not be decided from the information available.</summary>
        public List<PartFComplianceCheck> UnresolvedChecks
        {
            get { return [.. (Checks ?? []).Where(x => x.Status == PartFComplianceStatus.CannotBeDetermined)]; }
        }

        /// <summary>Checks that need an engineer's decision.</summary>
        public List<PartFComplianceCheck> EngineeringReviewChecks
        {
            get { return [.. (Checks ?? []).Where(x => x.Status == PartFComplianceStatus.EngineeringReviewRequired)]; }
        }

        /// <summary>
        /// Checks that were calculated as failed and now rest on a recorded alternative compliance method
        /// awaiting a building control body's acceptance.
        /// </summary>
        public List<PartFComplianceCheck> AlternativeSolutionChecks
        {
            get { return [.. (Checks ?? []).Where(x => x.Status == PartFComplianceStatus.AlternativeSolutionPendingApproval)]; }
        }

        /// <summary>
        /// Checks a person moved off the status SAM calculated, whichever direction they moved it. Listed
        /// so every departure from the calculation is visible in one place rather than having to be found
        /// by comparing two columns.
        /// </summary>
        public List<PartFComplianceCheck> UserResolvedChecks
        {
            get { return [.. (Checks ?? []).Where(x => x.IsUserResolved)]; }
        }

        /// <summary>Terminals that supply air to a habitable room.</summary>
        public List<PartFVentilationTerminalRequirement> SupplyTerminals
        {
            get { return [.. (Terminals ?? []).Where(x => x.TerminalRole == PartFTerminalRole.Supply)]; }
        }

        /// <summary>Terminals that provide general wet room extract.</summary>
        public List<PartFVentilationTerminalRequirement> GeneralExtractTerminals
        {
            get { return [.. (Terminals ?? []).Where(x => x.TerminalRole == PartFTerminalRole.GeneralExtract)]; }
        }

        /// <summary>Terminals that provide extract local to the cooking function.</summary>
        public List<PartFVentilationTerminalRequirement> LocalKitchenExtractTerminals
        {
            get { return [.. (Terminals ?? []).Where(x => x.TerminalRole == PartFTerminalRole.LocalKitchenExtract)]; }
        }

        /// <summary>
        /// Adds a check and returns it, so a caller can build one and record it in a single statement.
        /// </summary>
        public PartFComplianceCheck AddCheck(PartFComplianceCheck partFComplianceCheck)
        {
            if (partFComplianceCheck is not null)
            {
                Checks.Add(partFComplianceCheck);
            }

            return partFComplianceCheck;
        }

        /// <summary>
        /// Works out the overall status from the checks, in strict severity order so that no unresolved
        /// mandatory requirement can be hidden behind a majority of passes.
        /// <list type="number">
        /// <item>no mandatory check at all: <see cref="PartFOverallStatus.NotAssessed"/>;</item>
        /// <item>any mandatory check failed: <see cref="PartFOverallStatus.Fail"/>;</item>
        /// <item>any mandatory check rests on an unapproved alternative solution:
        /// <see cref="PartFOverallStatus.AlternativeSolutionPendingApproval"/>;</item>
        /// <item>any mandatory check needs an engineering decision:
        /// <see cref="PartFOverallStatus.EngineeringReviewRequired"/>;</item>
        /// <item>any mandatory check could not be determined:
        /// <see cref="PartFOverallStatus.CannotBeDetermined"/>;</item>
        /// <item>any mandatory check was never run while others were resolved:
        /// <see cref="PartFOverallStatus.Partial"/>;</item>
        /// <item>otherwise <see cref="PartFOverallStatus.Pass"/>.</item>
        /// </list>
        /// <para>
        /// Every branch below the first reads <see cref="PartFComplianceCheck.Status"/>, the reported
        /// status, except the failure test, which also reads
        /// <see cref="PartFComplianceCheck.CalculatedStatus"/>. A dwelling with a calculated failure on a
        /// mandatory check cannot reach a pass by any route through this method.
        /// </para>
        /// </summary>
        public PartFOverallStatus Resolve()
        {
            List<PartFComplianceCheck> checks_Mandatory = [.. (Checks ?? []).Where(x => x is not null && x.IsMandatory)];

            if (checks_Mandatory.Count == 0)
            {
                OverallStatus = PartFOverallStatus.NotAssessed;
                return OverallStatus;
            }

            //A mandatory check SAM calculated as FAILED keeps the whole dwelling at Fail, whatever status
            //it now reports. Sending it for engineering review, or relabelling it outright, changes what
            //the check says about itself; neither changes what the design does, and a dwelling that
            //dropped from Fail to "engineering review required" the moment somebody ticked a box would be
            //reporting the tick rather than the arithmetic.
            //
            //The one exception is a recorded alternative compliance method. The Approved Document is
            //guidance rather than the only means of compliance, so that is a real case to be made - and it
            //is reported as its own state, below a pass and distinct from an unaddressed failure, until a
            //building control body accepts it.
            List<PartFComplianceCheck> checks_CalculatedFailure = [.. checks_Mandatory.Where(x => x.CalculatedStatus == PartFComplianceStatus.Fail)];

            if (checks_Mandatory.Any(x => x.Status == PartFComplianceStatus.Fail)
                || checks_CalculatedFailure.Any(x => x.Status != PartFComplianceStatus.AlternativeSolutionPendingApproval))
            {
                OverallStatus = PartFOverallStatus.Fail;
            }
            else if (checks_Mandatory.Any(x => x.Status == PartFComplianceStatus.AlternativeSolutionPendingApproval))
            {
                OverallStatus = PartFOverallStatus.AlternativeSolutionPendingApproval;
            }
            else if (checks_Mandatory.Any(x => x.Status == PartFComplianceStatus.EngineeringReviewRequired))
            {
                OverallStatus = PartFOverallStatus.EngineeringReviewRequired;
            }
            else if (checks_Mandatory.Any(x => x.Status == PartFComplianceStatus.CannotBeDetermined))
            {
                OverallStatus = PartFOverallStatus.CannotBeDetermined;
            }
            else if (checks_Mandatory.Any(x => x.Status == PartFComplianceStatus.NotAssessed))
            {
                OverallStatus = PartFOverallStatus.Partial;
            }
            else
            {
                OverallStatus = PartFOverallStatus.Pass;
            }

            return OverallStatus;
        }
    }
}
