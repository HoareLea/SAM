// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// Turns a sized dwelling into its clause-level Approved Document F checks.
    /// <para>
    /// Three kinds of requirement are represented, and the difference between them is the point of the
    /// class. Some are <b>calculated</b> - the whole dwelling rate, the Table 1.2 minimums, the balance of
    /// supply and extract - and this decides them outright. Some are <b>verified from geometry</b> - that
    /// a habitable room can purge directly outside, that a supply space can reach an extract location -
    /// and this decides those too. The rest are <b>facts about the built work</b> that no analytical model
    /// contains: that the system is designed to minimise noise, that filters are reachable, that the
    /// occupier was given operating instructions. Those are recorded as
    /// <see cref="PartFComplianceStatus.CannotBeDetermined"/> and wait for a person to confirm them.
    /// </para>
    /// <para>
    /// They are never quietly passed and never quietly dropped. A requirement nobody has answered has to
    /// keep the dwelling off an overall pass, or the assessment would be reporting silence as compliance.
    /// </para>
    /// <para>
    /// A person's confirmation is read back from
    /// <see cref="PartFCommissioningData.InstallationChecks"/>, matched by check name. It can resolve a
    /// check this class could not decide; it cannot overturn one this class calculated as failing. A
    /// calculated failure is arithmetic against the Approved Document, and a checkbox does not change
    /// arithmetic.
    /// </para>
    /// </summary>
    public static class PartFCheckBuilder
    {
        private const string category_Extract = "Extract ventilation";
        private const string category_WholeDwelling = "Whole dwelling ventilation";
        private const string category_Supply = "Supply ventilation";
        private const string category_Transfer = "Transfer air";
        private const string category_Purge = "Purge ventilation";
        private const string category_System = "System design and installation";
        private const string category_Pollutants = "External pollutants";
        private const string category_Commissioning = "Commissioning and information";

        /// <summary>
        /// Tolerance [l/s] on a rate comparison. Rates are distributed by proportion, so an exact equality
        /// test would fail on the last bit of a division.
        /// </summary>
        private const double tolerance_Lps = 1e-6;

        /// <summary>Builds every check for one dwelling and adds it to the dwelling's compliance result.</summary>
        public static void Build(PartFDwellingResult partFDwellingResult, PartFData partFData)
        {
            PartFComplianceResult partFComplianceResult = partFDwellingResult?.ComplianceResult;
            if (partFComplianceResult is null)
            {
                return;
            }

            ExtractChecks(partFDwellingResult, partFComplianceResult, partFData);
            WholeDwellingChecks(partFDwellingResult, partFComplianceResult);
            SupplyChecks(partFDwellingResult, partFComplianceResult);
            TransferChecks(partFComplianceResult);
            PurgeChecks(partFComplianceResult);
            SystemChecks(partFComplianceResult);
            PollutantChecks(partFComplianceResult);
            CommissioningChecks(partFComplianceResult);

            //Everything above this line is what SAM calculated. Sealing it in one pass, rather than at
            //each of the several dozen places a status is assigned, means no check can reach a person's
            //hands without its calculated result recorded first - and the guard in
            //PartFComplianceCheck.ApplyUserResolution has something to hold a failure against.
            foreach (PartFComplianceCheck check in partFComplianceResult.Checks ?? [])
            {
                check.CalculatedStatus = check.Status;
            }

            ApplyUserConfirmations(partFComplianceResult);
        }

        // ------------------------------------------------------------------
        // Extract ventilation
        // ------------------------------------------------------------------

        private static void ExtractChecks(PartFDwellingResult partFDwellingResult, PartFComplianceResult partFComplianceResult, PartFData partFData)
        {
            List<PartFVentilationTerminalRequirement> terminals_Extract = [.. partFComplianceResult.Terminals.Where(x => x.IsExtract)];

            //Paragraph 1.17 (page 8): "Extract ventilation to the outside should be provided in all of the
            //following spaces. a. Kitchens. b. Utility rooms. c. Bathrooms. d. Sanitary accommodation."
            PartFComplianceCheck check = New("Extract ventilation provided in every required space", "paragraph 1.17 (page 8)", category_Extract,
                "Extract ventilation to the outside is provided in every kitchen, utility room, bathroom and sanitary accommodation.");

            if (terminals_Extract.Count == 0)
            {
                check.Status = PartFComplianceStatus.Fail;
                check.Evidence = "The dwelling has no extract terminal of any kind.";
            }
            else
            {
                //Three populations, and the difference between the last two is the point. A terminal whose
                //provision FAILED has a recorded arrangement that does not extract to the outside. A
                //terminal that is merely PROPOSED has no recorded arrangement at all: SAM generated it
                //because the room needs one, which sizes the schedule but proves nothing about the built
                //work. Treating a proposal as an installation would report SAM's own suggestion back as
                //evidence of compliance.
                List<PartFVentilationTerminalRequirement> terminals_Failed = [.. terminals_Extract.Where(x => x.ProvisionStatus == PartFComplianceStatus.Fail)];
                List<PartFVentilationTerminalRequirement> terminals_Proposed = [.. terminals_Extract.Where(x => !x.IsProvisionRecorded)];

                if (terminals_Failed.Count != 0)
                {
                    check.Status = PartFComplianceStatus.Fail;
                    check.Evidence = string.Format("{0} required extract location(s) have a recorded provision that does not extract to the outside: {1}.", terminals_Failed.Count, string.Join(", ", terminals_Failed.ConvertAll(x => string.Format("{0} ({1})", x.SpaceName, Core.Query.Description(x.ProvidedExtractMethod)))));
                }
                else if (terminals_Proposed.Count != 0)
                {
                    check.Status = PartFComplianceStatus.CannotBeDetermined;
                    check.Evidence = string.Format("{0} of {1} required extract location(s) have no recorded extract method, so the terminal shown for them is SAM's proposal rather than a provision established by the model: {2}. Record the actual arrangement for each. Paragraph 1.17 requires extract ventilation TO THE OUTSIDE, and a recirculating cooker hood on its own does not provide it (Diagram 1.2 note 1, page 9).", terminals_Proposed.Count, terminals_Extract.Count, string.Join(", ", terminals_Proposed.ConvertAll(x => x.SpaceName)));
                }
                else
                {
                    //Every required location has a recorded, nominally external provision. Whether the duct
                    //actually reaches outside air is still a construction fact, so this is reported as
                    //needing confirmation rather than passed.
                    check.Status = PartFComplianceStatus.CannotBeDetermined;
                    check.Evidence = string.Format("An extract method is recorded in all {0} required location(s): {1}. Paragraph 1.17 requires extract ventilation TO THE OUTSIDE, and whether each terminal discharges outside is a construction fact the analytical model does not contain. Confirm it at commissioning.", terminals_Extract.Count, string.Join(", ", terminals_Extract.ConvertAll(x => string.Format("{0} ({1})", x.SpaceName, Core.Query.Description(x.ProvidedExtractMethod)))));
                }
            }

            partFComplianceResult.AddCheck(check);

            //Paragraph 1.17a and Table 1.2: the room containing the cooking function.
            List<PartFVentilationTerminalRequirement> terminals_Kitchen = partFComplianceResult.LocalKitchenExtractTerminals;

            PartFComplianceCheck check_Kitchen = New("Local kitchen extract from the room containing the cooking function", "paragraph 1.17a (page 8), Table 1.1 (page 8) and Table 1.2 (page 10)", category_Extract,
                string.Format("The room containing the cooking function has its own extract ventilation to the outside, of at least {0:0.##} l/s on a continuous system (Table 1.2 kitchen high rate) or the applicable Table 1.1 rate on an intermittent one. Extract from a bathroom, ensuite or other wet room does not satisfy this.", partFData?.GetKitchenExtractHighRate_Lps() ?? 13));

            if (terminals_Kitchen.Count == 0)
            {
                check_Kitchen.Status = PartFComplianceStatus.Fail;
                check_Kitchen.Evidence = "The dwelling contains no kitchen, open plan living kitchen or studio, so no cooking space was found to extract from. Check that the cooking space is named in a way the space use text map recognises.";
            }
            else
            {
                //The worst individual outcome governs, over BOTH the rate assessment and the provision:
                //a compliant kitchen elsewhere in the dwelling cannot make up for a cooking space with a
                //recirculating hood, and a correctly sized terminal nobody has confirmed as installed
                //cannot pass on its arithmetic alone.
                check_Kitchen.Status = Worst([.. terminals_Kitchen.ConvertAll(x => x.ComplianceStatus), .. terminals_Kitchen.ConvertAll(x => x.ProvisionStatus)]);
                check_Kitchen.Evidence = string.Join(" ", terminals_Kitchen.ConvertAll(x => string.Format("{0}: required {1}, proposed {2}, provided {3} - {4}",
                    x.SpaceName,
                    x.RequiredHighFlowRate_Lps is null ? "no rate established" : string.Format("{0:0.##} l/s", x.RequiredHighFlowRate_Lps.Value),
                    Core.Query.Description(x.ProposedExtractMethod),
                    Core.Query.Description(x.ProvidedExtractMethod),
                    x.Diagnostic)));
            }

            partFComplianceResult.AddCheck(check_Kitchen);

            //Paragraph 1.70 and Table 1.2: each extract room reaches its own minimum HIGH rate. This is the
            //per-room half of Table 1.2 and is assessed entirely separately from the whole-dwelling
            //continuous total in WholeDwellingChecks. Neither implies the other, and in particular the
            //continuous dwelling rate is never raised to the sum of these per-room figures.
            List<PartFVentilationTerminalRequirement> terminals_Balanced = [.. terminals_Extract.Where(x => x.IsInBalancedFlow)];

            PartFComplianceCheck check_High = New("Each extract room reaches its Table 1.2 minimum high rate", "paragraph 1.70 (page 17), Table 1.2 and Table 1.2 note 1 (page 10)", category_Extract,
                "Every kitchen, bathroom, utility room and sanitary accommodation served by the continuous system can reach at least its own Table 1.2 minimum high rate. This is a per-room requirement on the HIGH rate, separate from the requirement that the total of continuous extract reaches the whole dwelling ventilation rate. Table 1.2 note 1: where the continuous rate provided in a room is equal to or higher than that minimum, no extra ventilation is needed for that room.");

            List<PartFVentilationTerminalRequirement> terminals_Below = [.. terminals_Balanced.Where(x => (x.HighFlowRate_Lps ?? 0) + tolerance_Lps < (x.MinimumRequiredFlowRate_Lps ?? 0))];

            if (terminals_Balanced.Count == 0)
            {
                check_High.Status = PartFComplianceStatus.NotApplicable;
                check_High.Evidence = "No extract terminal forms part of the continuous system, so Table 1.2 does not apply to any of them.";
            }
            else if (terminals_Below.Count != 0)
            {
                check_High.Status = PartFComplianceStatus.Fail;
                check_High.Evidence = string.Join("; ", terminals_Below.ConvertAll(x => string.Format("{0}: high rate {1:0.##} l/s against a Table 1.2 minimum of {2:0.##} l/s", x.SpaceName, x.HighFlowRate_Lps, x.MinimumRequiredFlowRate_Lps)));
            }
            else
            {
                check_High.Status = PartFComplianceStatus.Pass;
                check_High.Evidence = string.Join("; ", terminals_Balanced.ConvertAll(x => string.Format("{0}: high rate {1:0.##} l/s against a Table 1.2 minimum of {2:0.##} l/s{3}", x.SpaceName, x.HighFlowRate_Lps, x.MinimumRequiredFlowRate_Lps, x.HighRateIncreaseRequired ? ", so the terminal has to boost above its continuous rate" : ", already met at the continuous rate so no boost is needed (Table 1.2 note 1)")));
            }

            partFComplianceResult.AddCheck(check_High);
        }

        // ------------------------------------------------------------------
        // Whole dwelling ventilation
        // ------------------------------------------------------------------

        private static void WholeDwellingChecks(PartFDwellingResult partFDwellingResult, PartFComplianceResult partFComplianceResult)
        {
            //Paragraph 1.24 (page 10) and paragraph 1.69 (page 16), with the continuous rate column of
            //Table 1.2: the total of all extract on its continuous rate is at least the whole dwelling
            //ventilation rate.
            PartFComplianceCheck check = New("Total continuous extract reaches the whole dwelling ventilation rate", "paragraph 1.24 (page 10), Table 1.2 continuous rate column (page 10) and paragraph 1.69 (page 16)", category_WholeDwelling,
                "The sum of all extract ventilation in the dwelling on its continuous rate is at least the whole dwelling ventilation rate, which is the greater of the Table 1.3 rate set by the rooms and 0.3 l/s per m2 of internal floor area.");

            check.Evidence = string.Format(
                "Continuous design rate {0:0.##} l/s, the greater of the bedroom or one-habitable-room rate {1:0.##} l/s and the floor area rate {2:0.##} l/s over {3:0.##} m2. Total continuous extract {4:0.##} l/s. The sum of the Table 1.2 per-room minimum high rates is {5:0.##} l/s; those are high-rate figures assessed room by room at the high condition and do not raise this continuous rate.",
                partFDwellingResult.ContinuousDesignSystemRate_Lps,
                partFDwellingResult.BedroomOrHabitableRate_Lps,
                partFDwellingResult.AreaBasedRate_Lps,
                partFDwellingResult.InternalFloorArea_M2,
                partFDwellingResult.TotalExtract_Lps,
                partFDwellingResult.WetRoomMinimumTotal_Lps);

            check.Status = partFDwellingResult.TotalExtract_Lps + tolerance_Lps >= partFDwellingResult.ContinuousDesignSystemRate_Lps
                ? PartFComplianceStatus.Pass
                : PartFComplianceStatus.Fail;

            partFComplianceResult.AddCheck(check);

            //Balanced mechanical ventilation with heat recovery supplies as much as it extracts.
            PartFComplianceCheck check_Balance = New("Continuous supply and extract are balanced", "paragraphs 1.67 and 1.69 (page 16)", category_WholeDwelling,
                "For a balanced mechanical ventilation with heat recovery system, total continuous supply equals total continuous extract, and both reach the whole dwelling ventilation rate.");

            check_Balance.Evidence = string.Format("Total continuous supply {0:0.##} l/s, total continuous extract {1:0.##} l/s, whole dwelling continuous design rate {2:0.##} l/s.", partFDwellingResult.TotalSupply_Lps, partFDwellingResult.TotalExtract_Lps, partFDwellingResult.ContinuousDesignSystemRate_Lps);

            check_Balance.Status = System.Math.Abs(partFDwellingResult.TotalSupply_Lps - partFDwellingResult.TotalExtract_Lps) <= tolerance_Lps
                && partFDwellingResult.TotalSupply_Lps + tolerance_Lps >= partFDwellingResult.ContinuousDesignSystemRate_Lps
                ? PartFComplianceStatus.Pass
                : PartFComplianceStatus.Fail;

            partFComplianceResult.AddCheck(check_Balance);

            PartFComplianceCheck check_High = New("High rate supply and extract are balanced", "paragraphs 1.67 and 1.70 (pages 16 and 17), Table 1.2 (page 10)", category_WholeDwelling,
                "With the system at its high rate, total supply still equals total extract across the balanced system. Extract from an intermittent device such as a cooker hood is deliberately outside this balance, because it does not run as part of the balanced system.");

            check_High.Evidence = string.Format("Total high rate supply {0:0.##} l/s, total high rate extract {1:0.##} l/s. Intermittent extract outside the balance: {2:0.##} l/s.", partFDwellingResult.TotalHighSupply_Lps, partFDwellingResult.TotalHighExtract_Lps, partFDwellingResult.TotalIntermittentExtract_Lps);

            check_High.Status = System.Math.Abs(partFDwellingResult.TotalHighSupply_Lps - partFDwellingResult.TotalHighExtract_Lps) <= tolerance_Lps
                ? PartFComplianceStatus.Pass
                : PartFComplianceStatus.Fail;

            partFComplianceResult.AddCheck(check_High);
        }

        // ------------------------------------------------------------------
        // Supply ventilation
        // ------------------------------------------------------------------

        private static void SupplyChecks(PartFDwellingResult partFDwellingResult, PartFComplianceResult partFComplianceResult)
        {
            List<PartFVentilationTerminalRequirement> terminals_Supply = partFComplianceResult.SupplyTerminals;

            //Paragraph 1.67 (page 16): "each habitable room should have mechanical supply ventilation."
            PartFComplianceCheck check = New("Mechanical supply to every habitable room", "paragraph 1.67 (page 16)", category_Supply,
                "Every habitable room has mechanical supply ventilation.");

            if (partFDwellingResult.HabitableRoomCount == 0)
            {
                check.Status = PartFComplianceStatus.Fail;
                check.Evidence = "No space in this dwelling was classified as a habitable room, so no room has supply provision.";
            }
            else if (terminals_Supply.Count < partFDwellingResult.HabitableRoomCount)
            {
                check.Status = PartFComplianceStatus.Fail;
                check.Evidence = string.Format("{0} habitable room(s) ({1}) against {2} supply terminal(s) ({3}).", partFDwellingResult.HabitableRoomCount, string.Join(", ", partFDwellingResult.HabitableRoomNames), terminals_Supply.Count, string.Join(", ", terminals_Supply.ConvertAll(x => x.SpaceName)));
            }
            else
            {
                check.Status = PartFComplianceStatus.Pass;
                check.Evidence = string.Format("{0} habitable room(s), each with a supply terminal: {1}.", partFDwellingResult.HabitableRoomCount, string.Join(", ", terminals_Supply.ConvertAll(x => string.Format("{0} {1:0.##} l/s", x.SpaceName, x.ContinuousDesignFlowRate_Lps))));
            }

            partFComplianceResult.AddCheck(check);

            //Paragraph 1.67: "The total supply air flow should be distributed proportionately to the volume
            //of each habitable room."
            PartFComplianceCheck check_Distribution = New("Supply distributed in proportion to habitable room volume", "paragraph 1.67 (page 16)", category_Supply,
                "The total supply air flow is distributed proportionately to the volume of each habitable room.");

            if (terminals_Supply.Count == 0)
            {
                check_Distribution.Status = PartFComplianceStatus.NotApplicable;
                check_Distribution.Evidence = "No supply terminal was established, so there was nothing to distribute.";
            }
            else if (terminals_Supply.Exists(x => x.ContinuousDesignFlowRate_Lps is null))
            {
                check_Distribution.Status = PartFComplianceStatus.Fail;
                check_Distribution.Evidence = "One or more supply terminals received no rate, normally because their space has no volume.";
            }
            else
            {
                check_Distribution.Status = PartFComplianceStatus.Pass;
                check_Distribution.Evidence = string.Join("; ", terminals_Supply.ConvertAll(x => string.Format("{0} {1:0.##} l/s", x.SpaceName, x.ContinuousDesignFlowRate_Lps)));
            }

            partFComplianceResult.AddCheck(check_Distribution);

            //Paragraph 1.68 (page 16): mechanical supply terminals should be located and directed to avoid
            //draughts. Nothing in an analytical model says which way a diffuser points.
            partFComplianceResult.AddCheck(Manual("Supply terminals located and directed to avoid draughts", "paragraph 1.68 (page 16)", category_Supply,
                "Mechanical supply terminals are located and directed to avoid draughts.",
                "The direction a supply terminal is throwing air is a product and installation property that an analytical model does not contain. Confirm from the ventilation layout."));
        }

        // ------------------------------------------------------------------
        // Transfer air
        // ------------------------------------------------------------------

        private static void TransferChecks(PartFComplianceResult partFComplianceResult)
        {
            List<PartFDoorTransferData> transferPaths = [.. (partFComplianceResult.TransferPaths ?? []).Where(x => x.IsInternalDwellingDoor)];

            //Paragraph 1.25 (page 10): internal doors should allow air to flow through the dwelling by
            //providing a minimum free area equivalent to a 10mm undercut in a 760mm wide door.
            PartFComplianceCheck check = New("Internal doors allow air to flow through the dwelling", "paragraph 1.25 (page 10)", category_Transfer,
                string.Format("Every internal door within the dwelling provides a minimum free area of {0:0.##} mm2, equivalent to a {1:0.##} mm undercut in a {2:0.##} mm wide door, achieved as {1:0.##} mm above a fitted floor finish or {3:0.##} mm above an unfinished floor surface.", PartFDoorTransferData.NominalEquivalentFreeArea_mm2, PartFDoorTransferData.ReferenceUndercutHeight_mm, PartFDoorTransferData.ReferenceDoorWidth_mm, PartFDoorTransferData.UndercutHeightBeforeFloorFinish_mm));

            if (partFComplianceResult.HasNoInternalAdjacency)
            {
                //A gap in the model rather than an absent requirement, and never treated as a pass: a
                //dwelling of several rooms certainly has doors between them, they are just not modelled.
                check.Status = PartFComplianceStatus.CannotBeDetermined;
                check.Evidence = string.Format("This dwelling has {0} spaces but no internal separating element between any two of them in the model, so there is nothing to assess paragraph 1.25 against. Model the internal partitions and the doors in them, or record the transfer provision explicitly.", partFComplianceResult.DwellingSpaceCount);
            }
            else if (transferPaths.Count == 0)
            {
                //A one-room dwelling with its own extract has nowhere for air to transfer to, and that is
                //not a failure.
                check.Status = PartFComplianceStatus.NotApplicable;
                check.Evidence = "The dwelling has no internal route between two of its own spaces, so there is no internal door for paragraph 1.25 to apply to.";
            }
            else
            {
                check.Status = Worst(transferPaths.ConvertAll(x => x.ComplianceStatus));

                int count_Pass = transferPaths.Count(x => x.IsCompliant);
                int count_Fail = transferPaths.Count(x => x.ComplianceStatus == PartFComplianceStatus.Fail);
                int count_Unknown = transferPaths.Count(x => x.ComplianceStatus == PartFComplianceStatus.CannotBeDetermined);
                int count_NotModelled = transferPaths.Count(x => !x.IsDoorRepresented);

                check.Evidence = string.Format("{0} internal route(s): {1} meeting the free area, {2} below it, {3} with no recorded undercut or free area. {4} of them have no door or other transfer opening modelled on the separating element at all.", transferPaths.Count, count_Pass, count_Fail, count_Unknown, count_NotModelled);
            }

            partFComplianceResult.AddCheck(check);

            PartFComplianceCheck check_Route = New("Transfer air routes connect the supply spaces to the extract locations", "paragraph 1.25 (page 10)", category_Transfer,
                "Air can flow from every space with net supply to an extract location within the same dwelling, and every extract location is reachable from the supply spaces. The l/s values reported against each route are SAM's calculated airflow-network routing: paragraph 1.25 requires a free area through an internal door and prescribes no flow rate for any individual door, so no route is assessed on its flow.");

            List<PartFDoorTransferData> transferPaths_NotCalculable = [.. transferPaths.Where(x => x.RouteStatus == PartFTransferRouteStatus.NotCalculable)];
            List<PartFDoorTransferData> transferPaths_Ambiguous = [.. transferPaths.Where(x => x.RouteStatus == PartFTransferRouteStatus.Ambiguous || x.RouteStatus == PartFTransferRouteStatus.AllocationStrategy)];

            if (partFComplianceResult.HasNoInternalAdjacency)
            {
                check_Route.Status = PartFComplianceStatus.CannotBeDetermined;
                check_Route.Evidence = string.Format("This dwelling has {0} spaces but no internal separating element between any two of them in the model, so no transfer air route could be established and it could not be shown that supply air can reach an extract location.", partFComplianceResult.DwellingSpaceCount);
            }
            else if (transferPaths.Count == 0)
            {
                check_Route.Status = PartFComplianceStatus.NotApplicable;
                check_Route.Evidence = "The dwelling has no internal route between two of its own spaces.";
            }
            else if (transferPaths_NotCalculable.Count != 0)
            {
                check_Route.Status = PartFComplianceStatus.Fail;
                check_Route.Evidence = string.Format("{0} route(s) could not be resolved: {1}.", transferPaths_NotCalculable.Count, string.Join(", ", transferPaths_NotCalculable.ConvertAll(x => x.Name)));
            }
            else if (transferPaths_Ambiguous.Count != 0)
            {
                check_Route.Status = PartFComplianceStatus.EngineeringReviewRequired;
                check_Route.Evidence = string.Format("{0} of {1} route(s) are not fixed by the dwelling's topology alone, so the split between parallel airflow paths was set by the documented allocation strategy. The totals are correct and every route reaches an extract location; the split is a design decision to confirm or override: {2}.", transferPaths_Ambiguous.Count, transferPaths.Count, string.Join(", ", transferPaths_Ambiguous.ConvertAll(x => x.Name).Distinct()));
            }
            else
            {
                check_Route.Status = PartFComplianceStatus.Pass;
                check_Route.Evidence = string.Format("All {0} route(s) are fixed by conservation of air flow. Calculated routing: {1}.", transferPaths.Count, string.Join("; ", transferPaths.ConvertAll(x => string.Format("{0} to {1} {2:0.##} l/s", x.UpstreamSpaceName, x.DownstreamSpaceName, x.ContinuousDesignTransferFlowRate_Lps))));
            }

            partFComplianceResult.AddCheck(check_Route);
        }

        // ------------------------------------------------------------------
        // Purge ventilation
        // ------------------------------------------------------------------

        private static void PurgeChecks(PartFComplianceResult partFComplianceResult)
        {
            List<PartFPurgeVentilationData> purge = [.. (partFComplianceResult.PurgeVentilation ?? []).Where(x => x.IsRequired)];

            PartFComplianceCheck check = New("Purge ventilation in every habitable room", "paragraphs 1.26 to 1.31 and Table 1.4 (page 11)", category_Purge,
                "Every habitable room has a purge ventilation system capable of extracting at least four air changes per hour directly to the outside, through openings meeting the Table 1.4 minimum areas or through a mechanical extract system.");

            if (purge.Count == 0)
            {
                check.Status = PartFComplianceStatus.NotApplicable;
                check.Evidence = "The dwelling contains no habitable room, so paragraph 1.26 applies to nothing here.";
            }
            else
            {
                check.Status = Worst(purge.ConvertAll(x => x.ComplianceStatus));
                check.Evidence = string.Join(" ", purge.ConvertAll(x => string.Format("{0}: {1}", x.SpaceName, x.Diagnostic)));
            }

            check.Notes = PartFPurgeAssessor.PartOInteractionNote;

            partFComplianceResult.AddCheck(check);
        }

        // ------------------------------------------------------------------
        // System design and installation
        // ------------------------------------------------------------------

        private static void SystemChecks(PartFComplianceResult partFComplianceResult)
        {
            //Paragraph 1.20 (page 8): extract terminals and fans, not including cooker hoods, as high as
            //practicable and a maximum of 400mm below the ceiling.
            partFComplianceResult.AddCheck(Manual("Extract terminals installed high in the room", "paragraph 1.20 (page 8)", category_System,
                "Extract ventilation terminals and fans, not including cooker extract hoods, are installed as high as is practicable in the room and a maximum of 400mm below the ceiling.",
                "SAM models the room, not the position of a grille on its wall, so the height of an extract terminal is not something the analytical model can be read for. Confirm from the ventilation layout."));

            //Paragraph 1.21 (page 8): cooker hood height above the hob. Only relevant where a cooker hood
            //is actually the arrangement.
            bool hasCookerHood = partFComplianceResult.Terminals.Exists(x => x.ExtractMethod == PartFExtractMethod.CookerHoodExtractingOutside);

            PartFComplianceCheck check_Hood = New("Cooker hood height above the hob surface", "paragraph 1.21 (page 8)", category_System,
                "Where a cooker hood extracts to the outside, its height above the hob surface is as specified in the manufacturer's instructions or, where no specification is available, between 650mm and 750mm.");

            if (hasCookerHood)
            {
                check_Hood.Status = PartFComplianceStatus.CannotBeDetermined;
                check_Hood.Evidence = "A cooker hood extracting to the outside is recorded for this dwelling. Its height above the hob is a product and installation property that the analytical model does not contain. Confirm it.";
            }
            else
            {
                check_Hood.Status = PartFComplianceStatus.NotApplicable;
                check_Hood.Evidence = "No cooker hood extracting to the outside is recorded for this dwelling.";
            }

            partFComplianceResult.AddCheck(check_Hood);

            //Paragraph 1.71 (page 17).
            partFComplianceResult.AddCheck(Manual("Moist air from the wet rooms is not recirculated to the habitable rooms", "paragraph 1.71 (page 17)", category_System,
                "The mechanical ventilation with heat recovery system is designed to avoid moist air from the wet rooms recirculating to the habitable rooms.",
                "This is a property of the heat exchanger and the ductwork arrangement, not of the room geometry. BS EN 13141-8 Class U4 for internal and external leakage and for mixing is the relevant performance standard (Table 1.5, page 12). Confirm from the unit specification."));

            //Paragraph 1.72 (page 17).
            partFComplianceResult.AddCheck(Manual("Background ventilators are not installed with mechanical ventilation with heat recovery", "paragraph 1.72 (page 17)", category_System,
                "To avoid unintended air pathways, background ventilators are not installed with mechanical ventilation with heat recovery.",
                "SAM does not model background ventilators or trickle ventilators, so their absence cannot be verified from the model. Confirm that none is specified on the windows or walls of this dwelling."));

            //Paragraphs 1.5 to 1.7 (page 6).
            partFComplianceResult.AddCheck(Manual("System designed and installed to minimise noise", "paragraphs 1.5 to 1.7 (page 6)", category_System,
                "The system is designed and installed to minimise noise, with correctly sized and jointed ducts, securely fixed equipment, and fans sized so that they are not operating near maximum capacity in normal background ventilation mode. The guidance levels are 30dB LAeq,T in noise-sensitive rooms at the minimum low rate and 45dB LAeq,T in less noise-sensitive rooms at the minimum high rate.",
                "Acoustic performance depends on the selected fan unit, the duct sizes and the mountings, none of which is in the analytical model. Confirm from the equipment schedule and any acoustic assessment."));

            //Paragraphs 1.8 (page 7) and 1.75 (page 17).
            partFComplianceResult.AddCheck(Manual("Reasonable access for maintenance", "paragraph 1.8 (page 7) and paragraph 1.75 (page 17)", category_System,
                "Reasonable access is provided for maintaining the system, including access to replace filters, fans and coils, access points for cleaning ductwork, and adequate space for general maintenance of the plant.",
                "Access to plant and filters depends on the installed arrangement and the space around the unit, not on the ventilation calculation. Confirm from the plant layout."));

            //Paragraphs 1.33, 1.35, 1.36 and 1.37 (page 12).
            partFComplianceResult.AddCheck(Manual("Ventilation controls", "paragraphs 1.33, 1.35, 1.36 and 1.37 (page 12)", category_System,
                "Ventilation is controllable. Continuously running fans operate without occupant intervention and may have manual or automatic controls for selecting the high rate; any manual high rate controls are local to the spaces served. Humidity sensors are not used for sanitary accommodation, where odour is the main pollutant. Automatic controls operate according to the need for ventilation, and background ventilators with automatic controls also have manual override.",
                "The control strategy and the position of the boost switches are a product and installation matter. Confirm from the controls specification."));

            //Paragraphs 1.74 to 1.83 (pages 17 and 18).
            partFComplianceResult.AddCheck(Manual("Installation of the ventilation system", "paragraphs 1.74 to 1.83 (pages 17 and 18)", category_System,
                "The system is installed so as not to compromise its performance: rigid ducts wherever possible; flexible ductwork only for final connections, up to 1.5m, pulled taut and to BSRIA BG 43/2013; ducts sized for the air flow rate with the length and number of bends minimised; each air terminal with a free area of at least 90% of that of its duct; and duct connections mechanically secured and sealed.",
                "Duct routing, duct sizes and terminal free areas are not part of the analytical model. Confirm from the ventilation drawings and the installer's declaration in Appendix C Part 2a."));
        }

        // ------------------------------------------------------------------
        // External pollutants
        // ------------------------------------------------------------------

        private static void PollutantChecks(PartFComplianceResult partFComplianceResult)
        {
            //Paragraphs 2.2 to 2.6 (pages 20).
            partFComplianceResult.AddCheck(Manual("Outdoor air intake location", "Section 2, paragraphs 2.1 to 2.6 (pages 19 and 20)", category_Pollutants,
                "Ventilation intakes are located away from the direct impact of local pollution sources. Next to busy urban roads they are as high as possible and on the less polluted side of the building, and they are kept out of courtyards and enclosed urban spaces where pollutants are discharged wherever practicable.",
                "The position of the outdoor air intake on the facade, and the pollution sources around the site, are outside the ventilation calculation. Section 2 applies where the Table 2.1 pollutant limits are exceeded or the dwelling is near a significant local source. Confirm from the site air quality assessment and the intake location."));

            //Paragraphs 2.7 to 2.9 (page 20).
            partFComplianceResult.AddCheck(Manual("Exhaust outlet location", "Section 2, paragraphs 2.7 to 2.9 (page 20)", category_Pollutants,
                "Exhaust outlets are located so that re-entry of exhaust air into this or a nearby building is minimised and there is no harmful effect on the surrounding area; they are downwind of intakes where there is a prevailing wind direction, and do not discharge into courtyards, enclosures or architectural screens.",
                "Whether exhaust air can re-enter through an intake or an opening depends on the facade layout and the prevailing wind, neither of which the ventilation calculation covers. Confirm from the facade and ventilation layouts."));
        }

        // ------------------------------------------------------------------
        // Commissioning and information
        // ------------------------------------------------------------------

        private static void CommissioningChecks(PartFComplianceResult partFComplianceResult)
        {
            PartFCommissioningData partFCommissioningData = partFComplianceResult.Commissioning;

            //Paragraph 4.1 (page 31).
            PartFComplianceCheck check_Commissioned = New("System commissioned and commissioning notice given", "paragraph 4.1 (page 31) and paragraph 4.3 (page 31)", category_Commissioning,
                "The mechanical ventilation system is commissioned to provide adequate ventilation, a commissioning notice is given to the building control body, and the air flow measurement test and commissioning sheets include, as a minimum, everything in Part 3 of the Appendix C example sheet.");

            if (partFCommissioningData is null)
            {
                check_Commissioned.Status = PartFComplianceStatus.CannotBeDetermined;
                check_Commissioned.Evidence = "No commissioning record has been supplied for this dwelling. This is expected at design stage; the check remains open until the system is commissioned.";
            }
            else if (partFCommissioningData.CommissioningNoticeGiven)
            {
                check_Commissioned.Status = PartFComplianceStatus.Pass;
                check_Commissioned.Evidence = string.Format("Commissioned on {0} by {1}. Commissioning notice recorded as given.", Text(partFCommissioningData.CommissioningDate), Text(partFCommissioningData.CommissioningEngineer));
                check_Commissioned.Date = partFCommissioningData.CommissioningDate;
                check_Commissioned.ResponsiblePerson = partFCommissioningData.CommissioningEngineer;
            }
            else
            {
                check_Commissioned.Status = PartFComplianceStatus.CannotBeDetermined;
                check_Commissioned.Evidence = "A commissioning record exists but does not record that the commissioning notice has been given to the building control body.";
            }

            partFComplianceResult.AddCheck(check_Commissioned);

            //Paragraphs 4.2, 4.9 and 4.10 (pages 31 and 33).
            PartFComplianceCheck check_Measured = New("Air flow rates measured and notice given", "paragraphs 4.2 (page 31), 4.9 and 4.10 (page 33)", category_Commissioning,
                "Air flow rates for the mechanical ventilation are measured with a calibrated device with a proprietary hood, of plus or minus 5% accuracy, calibrated within the last 12 months at a UKAS-accredited centre, with all transfer devices open and all internal and external doors and windows closed; and a notice of the measured rates is given to the building control body.");

            if (partFCommissioningData is null || !partFCommissioningData.HasMeasuredValues)
            {
                check_Measured.Status = PartFComplianceStatus.CannotBeDetermined;
                check_Measured.Evidence = "No measured air flow rates have been recorded for this dwelling. This is expected at design stage.";
            }
            else
            {
                //The clause requires the PROCEDURE, not only the notice: a calibrated device, of a known
                //accuracy, itself calibrated within the last 12 months. AirFlowRateNoticeGiven alone proves
                //someone ticked a box, not that the measurement behind it meets paragraph 4.10c - so it is
                //no longer sufficient on its own for a Pass. Equipment and calibration date are free text
                //(so a project's own conventions survive), which is why this can only confirm they were
                //RECORDED, not validate the accuracy or the 12-month window; that remains a person's check.
                bool hasEquipment = !string.IsNullOrWhiteSpace(partFCommissioningData.MeasurementEquipment);
                bool hasCalibrationDate = !string.IsNullOrWhiteSpace(partFCommissioningData.CalibrationDate);

                check_Measured.Status = partFCommissioningData.AirFlowRateNoticeGiven && hasEquipment && hasCalibrationDate
                    ? PartFComplianceStatus.Pass
                    : PartFComplianceStatus.CannotBeDetermined;

                List<string> missing = [];
                if (!partFCommissioningData.AirFlowRateNoticeGiven)
                {
                    missing.Add("the notice of measured rates has not been recorded as given");
                }
                if (!hasEquipment)
                {
                    missing.Add("no measurement equipment is recorded");
                }
                if (!hasCalibrationDate)
                {
                    missing.Add("no calibration date is recorded");
                }

                check_Measured.Evidence = string.Format("Equipment: {0}, last UKAS calibration {1}. Measured totals - continuous supply {2}, continuous extract {3}, high supply {4}, high extract {5}. Notice of measured rates recorded as {6}.{7}",
                    Text(partFCommissioningData.MeasurementEquipment),
                    Text(partFCommissioningData.CalibrationDate),
                    Rate(partFCommissioningData.MeasuredContinuousSupplyTotal_Lps),
                    Rate(partFCommissioningData.MeasuredContinuousExtractTotal_Lps),
                    Rate(partFCommissioningData.MeasuredHighSupplyTotal_Lps),
                    Rate(partFCommissioningData.MeasuredHighExtractTotal_Lps),
                    partFCommissioningData.AirFlowRateNoticeGiven ? "given" : "not yet given",
                    missing.Count == 0 ? string.Empty : string.Format(" Outstanding: {0}.", string.Join("; ", missing)));
            }

            partFComplianceResult.AddCheck(check_Measured);

            //Appendix C paragraph C2 (page 42): the pass condition on the measured rates.
            PartFComplianceCheck check_Compare = New("Measured air flow rates meet the design air flow rates", "Appendix C, paragraph C2 (page 42)", category_Commissioning,
                "The measured rate for each fan is equal to or greater than its design value. Where any measured value is lower, the system is adjusted and all air flows are remeasured.");

            List<PartFVentilationTerminalRequirement> terminals_Measured = [.. partFComplianceResult.Terminals.Where(x => x.MeasuredContinuousFlowRate_Lps is not null || x.MeasuredHighFlowRate_Lps is not null)];

            //Every REQUIRED terminal that carries a design rate is a fan Appendix C paragraph C2 expects to
            //have been measured - not only the ones that happen to have a measurement recorded. A dwelling
            //where one fan out of five was measured, and happened to meet its own design rate, must not
            //report the same clean Pass as one where all five were verified: the other four were simply
            //never checked, and reporting Pass would say they were.
            List<PartFVentilationTerminalRequirement> terminals_Applicable = [.. partFComplianceResult.Terminals.Where(x => x.IsRequired && (x.ContinuousDesignFlowRate_Lps is not null || x.HighFlowRate_Lps is not null))];

            if (terminals_Measured.Count == 0 && (partFCommissioningData is null || !partFCommissioningData.HasMeasuredValues))
            {
                check_Compare.Status = PartFComplianceStatus.CannotBeDetermined;
                check_Compare.Evidence = "No measured air flow rates have been recorded, so there is nothing to compare against the design rates. Design and measured values are held separately and a measured value never overwrites a design value.";
            }
            else
            {
                List<string> shortfalls = [];
                List<string> unmeasured = [];

                foreach (PartFVentilationTerminalRequirement terminal in terminals_Applicable)
                {
                    if (terminal.ContinuousDesignFlowRate_Lps is not null)
                    {
                        if (terminal.MeasuredContinuousFlowRate_Lps is null)
                        {
                            unmeasured.Add(string.Format("{0} continuous", terminal.SpaceName));
                        }
                        else if (terminal.MeasuredContinuousFlowRate_Lps.Value + tolerance_Lps < terminal.ContinuousDesignFlowRate_Lps.Value)
                        {
                            shortfalls.Add(string.Format("{0} continuous: measured {1:0.##} l/s against a design {2:0.##} l/s", terminal.SpaceName, terminal.MeasuredContinuousFlowRate_Lps, terminal.ContinuousDesignFlowRate_Lps));
                        }
                    }

                    if (terminal.HighFlowRate_Lps is not null)
                    {
                        if (terminal.MeasuredHighFlowRate_Lps is null)
                        {
                            unmeasured.Add(string.Format("{0} high", terminal.SpaceName));
                        }
                        else if (terminal.MeasuredHighFlowRate_Lps.Value + tolerance_Lps < terminal.HighFlowRate_Lps.Value)
                        {
                            shortfalls.Add(string.Format("{0} high: measured {1:0.##} l/s against a design {2:0.##} l/s", terminal.SpaceName, terminal.MeasuredHighFlowRate_Lps, terminal.HighFlowRate_Lps));
                        }
                    }
                }

                if (shortfalls.Count != 0)
                {
                    check_Compare.Status = PartFComplianceStatus.Fail;
                    check_Compare.Evidence = string.Format("{0} measured rate(s) are below their design value: {1}. Appendix C paragraph C2 requires the system to be adjusted and all air flows remeasured.", shortfalls.Count, string.Join("; ", shortfalls));
                }
                else if (terminals_Measured.Count == 0)
                {
                    check_Compare.Status = PartFComplianceStatus.CannotBeDetermined;
                    check_Compare.Evidence = "Dwelling totals have been measured but no individual terminal has a measured rate, so the per-fan comparison Appendix C paragraph C2 asks for could not be made.";
                }
                else if (unmeasured.Count != 0)
                {
                    check_Compare.Status = PartFComplianceStatus.CannotBeDetermined;
                    check_Compare.Evidence = string.Format("{0} of {1} applicable design rate(s) have not been measured yet: {2}. Appendix C paragraph C2 requires every fan to be measured, so a partial set cannot pass the comparison.", unmeasured.Count, terminals_Applicable.Count(x => x.ContinuousDesignFlowRate_Lps is not null) + terminals_Applicable.Count(x => x.HighFlowRate_Lps is not null), string.Join("; ", unmeasured));
                }
                else
                {
                    check_Compare.Status = PartFComplianceStatus.Pass;
                    check_Compare.Evidence = string.Format("All {0} measured terminal rate(s) are at or above their design value.", terminals_Measured.Count);
                }
            }

            partFComplianceResult.AddCheck(check_Compare);

            //Paragraphs 4.13 to 4.17 (page 34).
            PartFComplianceCheck check_Information = New("Operating and maintenance information issued to the building owner", "paragraphs 4.13 to 4.17 (page 34)", category_Commissioning,
                "Sufficient information about the system and its maintenance requirements is given to the building owner, in a clear manner for a non-technical audience, including the design flow rates, the location and use of the controls, how and when to clean and maintain the system and its filters, and a copy of the completed Appendix C commissioning sheet.");

            if (partFCommissioningData is null)
            {
                check_Information.Status = PartFComplianceStatus.CannotBeDetermined;
                check_Information.Evidence = "No commissioning record has been supplied for this dwelling, so the issue of the operating and maintenance information is not recorded.";
            }
            else
            {
                check_Information.Status = partFCommissioningData.OperatingAndMaintenanceInformationIssued ? PartFComplianceStatus.Pass : PartFComplianceStatus.CannotBeDetermined;
                check_Information.Evidence = partFCommissioningData.OperatingAndMaintenanceInformationIssued
                    ? "Recorded as issued to the building owner."
                    : "The commissioning record does not yet show the operating and maintenance information as issued.";
            }

            partFComplianceResult.AddCheck(check_Information);

            //Paragraphs 4.18 and 4.19 (page 35).
            PartFComplianceCheck check_Guide = New("Home User Guide provided", "paragraphs 4.18 and 4.19 (page 35)", category_Commissioning,
                "A Home User Guide is provided for the new dwelling, as described in Section 9 of Approved Document L, Volume 1: Dwellings, containing a Ventilation section with non-technical advice on the ventilation systems provided.");

            if (partFCommissioningData is null)
            {
                check_Guide.Status = PartFComplianceStatus.CannotBeDetermined;
                check_Guide.Evidence = "No commissioning record has been supplied for this dwelling, so the issue of the Home User Guide is not recorded.";
            }
            else
            {
                check_Guide.Status = partFCommissioningData.HomeUserGuideIssued ? PartFComplianceStatus.Pass : PartFComplianceStatus.CannotBeDetermined;
                check_Guide.Evidence = partFCommissioningData.HomeUserGuideIssued
                    ? "Recorded as provided."
                    : "The commissioning record does not yet show the Home User Guide as provided.";
            }

            partFComplianceResult.AddCheck(check_Guide);
        }

        // ------------------------------------------------------------------
        // User confirmations
        // ------------------------------------------------------------------

        /// <summary>
        /// Adopts each person's recorded answer, matched to a built check by name.
        /// <para>
        /// Every recorded answer is offered to the check, including one against a calculated failure -
        /// recording the evidence, the alternative compliance method and the reason is exactly what a
        /// person should be doing there. What that record cannot do is turn the failure into a pass;
        /// <see cref="PartFComplianceCheck.ApplyUserResolution"/> holds that line and reports which
        /// records it would not take at face value.
        /// </para>
        /// </summary>
        private static void ApplyUserConfirmations(PartFComplianceResult partFComplianceResult)
        {
            List<PartFComplianceCheck> checks_Recorded = partFComplianceResult.Commissioning?.InstallationChecks;
            if (checks_Recorded is null || checks_Recorded.Count == 0)
            {
                return;
            }

            List<string> refused = [];

            foreach (PartFComplianceCheck check in partFComplianceResult.Checks)
            {
                PartFComplianceCheck check_Recorded = checks_Recorded.Find(x => x is not null && string.Equals(x.Name, check.Name, StringComparison.Ordinal));
                if (check_Recorded is null || check_Recorded.Status == PartFComplianceStatus.NotAssessed)
                {
                    continue;
                }

                if (!check.ApplyUserResolution(check_Recorded) && check.CalculatedStatus == PartFComplianceStatus.Fail)
                {
                    refused.Add(string.Format("'{0}' (recorded as {1}, reported as {2})", check.Name, Core.Query.Description(check_Recorded.Status), Core.Query.Description(check.Status)));
                }
            }

            if (refused.Count != 0)
            {
                partFComplianceResult.Warnings.Add(string.Format("{0} check(s) SAM calculated as FAILED carry a recorded confirmation of compliance, which has not been adopted: {1}. A calculated failure cannot be turned into a pass by changing its status. Correct the input and recalculate, supply the missing measured or geometric evidence, record an alternative compliance method, or refer the item for engineering or building control review. The calculated result is retained on every one of them.", refused.Count, string.Join(", ", refused)));
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static PartFComplianceCheck New(string name, string paragraph, string category, string requirement)
        {
            return new PartFComplianceCheck(name, string.Format("Approved Document F, Volume 1: Dwellings (2021 edition), {0}", paragraph), requirement)
            {
                Category = category,
                IsMandatory = true,
            };
        }

        private static PartFComplianceCheck Manual(string name, string paragraph, string category, string requirement, string evidence)
        {
            PartFComplianceCheck result = New(name, paragraph, category, requirement);

            result.Status = PartFComplianceStatus.CannotBeDetermined;
            result.Evidence = evidence;

            return result;
        }

        /// <summary>
        /// The most severe of a set of statuses, so that one failing room cannot be averaged away by a
        /// dozen compliant ones. Anything unresolved beats anything resolved.
        /// </summary>
        private static PartFComplianceStatus Worst(List<PartFComplianceStatus> statuses)
        {
            if (statuses is null || statuses.Count == 0)
            {
                return PartFComplianceStatus.NotAssessed;
            }

            if (statuses.Contains(PartFComplianceStatus.Fail))
            {
                return PartFComplianceStatus.Fail;
            }

            if (statuses.Contains(PartFComplianceStatus.EngineeringReviewRequired))
            {
                return PartFComplianceStatus.EngineeringReviewRequired;
            }

            if (statuses.Contains(PartFComplianceStatus.CannotBeDetermined))
            {
                return PartFComplianceStatus.CannotBeDetermined;
            }

            if (statuses.Contains(PartFComplianceStatus.NotAssessed))
            {
                return PartFComplianceStatus.NotAssessed;
            }

            if (statuses.Contains(PartFComplianceStatus.Pass))
            {
                return PartFComplianceStatus.Pass;
            }

            if (statuses.Contains(PartFComplianceStatus.UserConfirmed))
            {
                return PartFComplianceStatus.UserConfirmed;
            }

            return PartFComplianceStatus.NotApplicable;
        }

        private static string Text(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "not recorded" : text;
        }

        private static string Rate(double? value_Lps)
        {
            return value_Lps is null ? "not recorded" : string.Format("{0:0.##} l/s", value_Lps.Value);
        }
    }
}
