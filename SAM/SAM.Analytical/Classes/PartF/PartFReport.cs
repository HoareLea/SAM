// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SAM.Analytical
{
    /// <summary>
    /// Writes the Part F conformance assessment of one or more dwellings as plain text.
    /// <para>
    /// Deliberately free of any user interface dependency. The same report goes to the SAM_UI report
    /// window, to the clipboard, to an exported file, to a Grasshopper output and to the regression
    /// tests, so all of them say the same thing and a change to the wording is caught by a test rather
    /// than noticed on screen.
    /// </para>
    /// <para>
    /// It is a <b>conformance assessment</b> and says so throughout. Software cannot certify compliance
    /// with the Building Regulations: compliance is demonstrated to a building control body, on the
    /// complete design and the built work, by a suitably qualified person. What this reports is which
    /// requirements were calculated, which were verified from the model geometry, which a person
    /// confirmed, and which remain open.
    /// </para>
    /// </summary>
    public static class PartFReport
    {
        /// <summary>
        /// The exact opening of every report. Fixed text, because a reader has to be able to see the basis
        /// of the assessment before any number, and a regression test asserts it verbatim.
        /// </summary>
        public const string Assumptions =
            "ASSUMPTIONS\r\n" +
            "\r\n" +
            "New dwelling in England.\r\n" +
            "Approved Document F, Volume 1, 2021 edition.\r\n";

        /// <summary>The closing statement on the limits of a software assessment.</summary>
        public const string Disclaimer =
            "This is a Part F conformance assessment, not a certificate and not a legal certification of compliance with the Building Regulations. " +
            "It records which requirements were calculated, which were verified from the model geometry, which a person confirmed, and which remain open. " +
            "Compliance is demonstrated to a building control body, on the complete design and the built work, by a suitably qualified person.";

        /// <summary>Renders the assessment of every dwelling a calculation produced.</summary>
        public static string Build(PartFCalculator partFCalculator, PartFOperatingMode partFOperatingMode = PartFOperatingMode.ContinuousDesign)
        {
            return Build(partFCalculator?.DwellingResults, partFOperatingMode);
        }

        /// <summary>Renders the assessment of every supplied dwelling.</summary>
        public static string Build(IEnumerable<PartFDwellingResult> partFDwellingResults, PartFOperatingMode partFOperatingMode = PartFOperatingMode.ContinuousDesign)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.Append(Assumptions);

            List<PartFDwellingResult> dwellingResults = [.. partFDwellingResults ?? []];

            if (dwellingResults.Count == 0)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("No dwelling was assessed.");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(Disclaimer);

                return stringBuilder.ToString();
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format("{0} dwelling(s) assessed. Report mode: {1}.", dwellingResults.Count, ModeText(partFOperatingMode)));

            foreach (PartFDwellingResult dwellingResult in dwellingResults)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append(BuildDwelling(dwellingResult, partFOperatingMode));
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Disclaimer);

            return stringBuilder.ToString();
        }

        /// <summary>Renders the assessment of one dwelling.</summary>
        public static string BuildDwelling(PartFDwellingResult partFDwellingResult, PartFOperatingMode partFOperatingMode = PartFOperatingMode.ContinuousDesign)
        {
            StringBuilder stringBuilder = new();

            if (partFDwellingResult is null)
            {
                return string.Empty;
            }

            PartFComplianceResult complianceResult = partFDwellingResult.ComplianceResult;

            string name = string.IsNullOrWhiteSpace(partFDwellingResult.Name) ? "Dwelling (whole model)" : partFDwellingResult.Name;

            Rule(stringBuilder);
            stringBuilder.AppendLine(string.Format("DWELLING: {0}", name));
            Rule(stringBuilder);
            stringBuilder.AppendLine();

            //The schematic sits at the top, before the schedules: an engineer reading a ventilation
            //assessment wants to see where the air goes before reading how much of it there is.
            if (complianceResult is not null)
            {
                stringBuilder.Append(PartFSchematic.Build(complianceResult, partFOperatingMode));
                stringBuilder.AppendLine();
            }

            Summary(stringBuilder, partFDwellingResult, complianceResult);

            if (complianceResult is null)
            {
                stringBuilder.AppendLine("No conformance assessment was produced for this dwelling.");
                return stringBuilder.ToString();
            }

            SpaceAirflow(stringBuilder, complianceResult, partFOperatingMode);
            TerminalSchedules(stringBuilder, complianceResult);
            TransferSchedule(stringBuilder, complianceResult);
            UndercutSchedule(stringBuilder, complianceResult);
            PurgeSchedule(stringBuilder, complianceResult);
            CommissioningStatus(stringBuilder, complianceResult);
            Messages(stringBuilder, partFDwellingResult, complianceResult);
            Checks(stringBuilder, complianceResult);
            Overall(stringBuilder, complianceResult);

            return stringBuilder.ToString();
        }

        // ------------------------------------------------------------------
        // Sections
        // ------------------------------------------------------------------

        private static void Summary(StringBuilder stringBuilder, PartFDwellingResult partFDwellingResult, PartFComplianceResult partFComplianceResult)
        {
            Section(stringBuilder, "DWELLING SUMMARY");

            stringBuilder.AppendLine(string.Format("Dwelling identifier:            {0}", string.IsNullOrWhiteSpace(partFDwellingResult.Name) ? "whole model, sized as one dwelling" : partFDwellingResult.Name));
            stringBuilder.AppendLine(string.Format("Ventilation system type:        {0}", partFComplianceResult?.SystemType ?? "not recorded"));
            stringBuilder.AppendLine(string.Format("Source document:                {0}", partFComplianceResult?.SourceDocument ?? PartFComplianceResult.SourceDocumentValue));
            stringBuilder.AppendLine(string.Format("Source edition:                 {0}", partFComplianceResult?.SourceEdition ?? PartFComplianceResult.SourceEditionValue));
            stringBuilder.AppendLine(string.Format("Internal floor area:            {0} m2", Number(partFDwellingResult.InternalFloorArea_M2)));
            stringBuilder.AppendLine(string.Format("Habitable rooms:                {0}{1}", partFDwellingResult.HabitableRoomCount, partFDwellingResult.HabitableRoomNames.Count == 0 ? string.Empty : string.Format(" ({0})", string.Join(", ", partFDwellingResult.HabitableRoomNames))));
            stringBuilder.AppendLine(string.Format("Bedrooms:                       {0}", partFDwellingResult.BedroomCount));
            stringBuilder.AppendLine();

            stringBuilder.AppendLine(string.Format("Continuous design airflow:      {0} l/s   (supply {1} l/s, extract {2} l/s)", Number(partFDwellingResult.ContinuousDesignSystemRate_Lps), Number(partFDwellingResult.TotalSupply_Lps), Number(partFDwellingResult.TotalExtract_Lps)));
            stringBuilder.AppendLine(string.Format("High/boost airflow:             supply {0} l/s, extract {1} l/s", Number(partFDwellingResult.TotalHighSupply_Lps), Number(partFDwellingResult.TotalHighExtract_Lps)));
            stringBuilder.AppendLine(string.Format("Setback airflow:                {0} l/s   (supply {1} l/s, extract {2} l/s) at {3}% of continuous design", Number(partFDwellingResult.SetbackSystemRate_Lps), Number(partFDwellingResult.TotalSetbackSupply_Lps), Number(partFDwellingResult.TotalSetbackExtract_Lps), Number(partFDwellingResult.SetbackFlowRateFactor * 100)));

            if (partFDwellingResult.TotalIntermittentExtract_Lps > 0)
            {
                stringBuilder.AppendLine(string.Format("Intermittent extract:           {0} l/s, outside the balanced flow (Table 1.1)", Number(partFDwellingResult.TotalIntermittentExtract_Lps)));
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Governing calculation:");
            stringBuilder.AppendLine(string.Format("  Bedroom/one-habitable-room rate  {0} l/s{1}", Number(partFDwellingResult.BedroomOrHabitableRate_Lps), partFDwellingResult.OneHabitableRoomRuleApplied ? "   (Table 1.3 note 1: exactly one habitable room)" : string.Format("   (Table 1.3, {0} bedroom(s))", partFDwellingResult.BedroomCount)));
            stringBuilder.AppendLine(string.Format("  Floor area rate                  {0} l/s   (paragraph 1.24a, 0.3 l/s per m2 of {1} m2)", Number(partFDwellingResult.AreaBasedRate_Lps), Number(partFDwellingResult.InternalFloorArea_M2)));
            stringBuilder.AppendLine(string.Format("  Governing                        {0} l/s   ({1})", Number(partFDwellingResult.ContinuousDesignSystemRate_Lps), Governing(partFDwellingResult)));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format("  Table 1.2 high-rate minimums     {0} l/s   (sum over the continuous extract terminals)", Number(partFDwellingResult.WetRoomMinimumTotal_Lps)));
            stringBuilder.AppendLine("  Table 1.2 sets two separate requirements: the TOTAL of continuous extract reaches the whole");
            stringBuilder.AppendLine("  dwelling rate above, and EACH room reaches its own minimum high rate at the high condition.");
            stringBuilder.AppendLine("  The sum of the per-room high-rate minimums does not raise the continuous dwelling rate.");

            if (partFComplianceResult is not null)
            {
                stringBuilder.AppendLine(string.Format("  Extract allocation strategy      {0}", Core.Query.Description(partFComplianceResult.ExtractAllocationStrategy)));
            }

            stringBuilder.AppendLine();
        }

        private static void SpaceAirflow(StringBuilder stringBuilder, PartFComplianceResult partFComplianceResult, PartFOperatingMode partFOperatingMode)
        {
            Section(stringBuilder, string.Format("SPACE AIRFLOW {0} {1}", PartFSchematic.EmDash, ModeText(partFOperatingMode).ToUpperInvariant()));

            stringBuilder.Append(PartFSchematic.BuildSpaceAirflow(partFComplianceResult, partFOperatingMode));
        }

        private static void TerminalSchedules(StringBuilder stringBuilder, PartFComplianceResult partFComplianceResult)
        {
            TerminalSchedule(stringBuilder, "SUPPLY TERMINAL SCHEDULE", partFComplianceResult.SupplyTerminals, false);
            TerminalSchedule(stringBuilder, "GENERAL EXTRACT SCHEDULE", partFComplianceResult.GeneralExtractTerminals, true);
            TerminalSchedule(stringBuilder, "LOCAL KITCHEN EXTRACT SCHEDULE", partFComplianceResult.LocalKitchenExtractTerminals, true);
        }

        private static void TerminalSchedule(StringBuilder stringBuilder, string title, List<PartFVentilationTerminalRequirement> terminals, bool showMethod)
        {
            Section(stringBuilder, title);

            if (terminals is null || terminals.Count == 0)
            {
                stringBuilder.AppendLine("None.");
                stringBuilder.AppendLine();
                return;
            }

            foreach (PartFVentilationTerminalRequirement terminal in terminals)
            {
                stringBuilder.AppendLine(terminal.SpaceName);
                stringBuilder.AppendLine(string.Format("  Continuous design:      {0}", Rate(terminal.ContinuousDesignFlowRate_Lps)));
                stringBuilder.AppendLine(string.Format("  High/boost:             {0}{1}", Rate(terminal.HighFlowRate_Lps), HighRateNote(terminal)));
                stringBuilder.AppendLine(string.Format("  Setback:                {0}", Rate(terminal.SetbackFlowRate_Lps)));

                if (terminal.MinimumRequiredFlowRate_Lps is not null)
                {
                    stringBuilder.AppendLine(string.Format("  Required high rate:     {0}", Rate(terminal.RequiredHighFlowRate_Lps)));
                }

                if (showMethod)
                {
                    //Required, proposed and provided are printed as three separate lines on purpose. A
                    //reader has to be able to see at a glance whether the method behind a rate was stated
                    //by the design or supplied by SAM.
                    stringBuilder.AppendLine(string.Format("  Sizing method:          {0}{1}", Core.Query.Description(terminal.ExtractMethod), terminal.IsInBalancedFlow ? "   (in the balanced continuous flow)" : "   (outside the balanced continuous flow)"));
                    stringBuilder.AppendLine(string.Format("  Proposed by SAM:        {0}", Core.Query.Description(terminal.ProposedExtractMethod)));
                    stringBuilder.AppendLine(string.Format("  Provided by design:     {0}{1}", Core.Query.Description(terminal.ProvidedExtractMethod), terminal.IsProvisionRecorded ? string.Empty : "   (nothing recorded - the method above is SAM's proposal, not a provision)"));
                    stringBuilder.AppendLine(string.Format("  Provision status:       {0}", Core.Query.Description(terminal.ProvisionStatus)));
                }

                if (terminal.MeasuredContinuousFlowRate_Lps is not null || terminal.MeasuredHighFlowRate_Lps is not null)
                {
                    stringBuilder.AppendLine(string.Format("  Measured:               continuous {0}, high {1}", Rate(terminal.MeasuredContinuousFlowRate_Lps), Rate(terminal.MeasuredHighFlowRate_Lps)));
                }

                stringBuilder.AppendLine(string.Format("  Source:                 {0}", terminal.SourceReference));
                stringBuilder.AppendLine(string.Format("  Status:                 {0}", Core.Query.Description(terminal.ComplianceStatus)));

                if (!string.IsNullOrWhiteSpace(terminal.Diagnostic))
                {
                    stringBuilder.AppendLine(string.Format("  Note:                   {0}", terminal.Diagnostic));
                }

                stringBuilder.AppendLine();
            }
        }

        private static void TransferSchedule(StringBuilder stringBuilder, PartFComplianceResult partFComplianceResult)
        {
            Section(stringBuilder, "INTERNAL TRANSFER AIR ROUTING (CALCULATED)");

            List<PartFDoorTransferData> transferPaths = partFComplianceResult.TransferPaths;

            if (transferPaths is null || transferPaths.Count == 0)
            {
                stringBuilder.AppendLine("No internal route between two spaces of this dwelling was found.");
                stringBuilder.AppendLine();
                return;
            }

            //Said before the numbers, not after them: a reader who takes an l/s figure here for a Part F
            //door requirement has misread the assessment, and the only reliable place to prevent that is
            //above the table.
            stringBuilder.AppendLine("The l/s figures below are SAM's calculated airflow-network routing, obtained by conserving air across");
            stringBuilder.AppendLine("the dwelling. Approved Document F paragraph 1.25 requires a free AREA through an internal door and");
            stringBuilder.AppendLine("prescribes no flow rate for any individual door, so nothing here is a Part F door-flow requirement and");
            stringBuilder.AppendLine("no door passes or fails on these values. The paragraph 1.25 assessment is the free area schedule below.");
            stringBuilder.AppendLine();

            foreach (PartFDoorTransferData partFDoorTransferData in transferPaths)
            {
                stringBuilder.AppendLine(partFDoorTransferData.Name);
                stringBuilder.AppendLine(string.Format("  {0} {1} {2}", partFDoorTransferData.UpstreamSpaceName, PartFSchematic.ArrowRight, partFDoorTransferData.DownstreamSpaceName));
                stringBuilder.AppendLine(string.Format("  Calculated continuous:  {0}", Rate(partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps)));
                stringBuilder.AppendLine(string.Format("  Calculated high/boost:  {0}", Rate(partFDoorTransferData.HighTransferFlowRate_Lps)));
                stringBuilder.AppendLine(string.Format("  Calculated setback:     {0}", Rate(partFDoorTransferData.SetbackTransferFlowRate_Lps)));
                stringBuilder.AppendLine(string.Format("  Route status:           {0}", Core.Query.Description(partFDoorTransferData.RouteStatus)));

                if (!string.IsNullOrWhiteSpace(partFDoorTransferData.CalculationSource))
                {
                    stringBuilder.AppendLine(string.Format("  Calculation source:     {0}", partFDoorTransferData.CalculationSource));
                }

                stringBuilder.AppendLine();
            }
        }

        private static void UndercutSchedule(StringBuilder stringBuilder, PartFComplianceResult partFComplianceResult)
        {
            Section(stringBuilder, "DOOR UNDERCUT AND FREE AREA SCHEDULE (PARAGRAPH 1.25 ASSESSMENT)");

            List<PartFDoorTransferData> transferPaths = partFComplianceResult.TransferPaths;

            if (transferPaths is null || transferPaths.Count == 0)
            {
                stringBuilder.AppendLine("No internal door within this dwelling.");
                stringBuilder.AppendLine();
                return;
            }

            stringBuilder.AppendLine("This is where paragraph 1.25 is assessed: on free area, not on the calculated flows above.");
            stringBuilder.AppendLine();

            foreach (PartFDoorTransferData partFDoorTransferData in transferPaths)
            {
                stringBuilder.AppendLine(partFDoorTransferData.Name);
                stringBuilder.AppendLine(string.Format("  {0} {1} {2}", partFDoorTransferData.UpstreamSpaceName, PartFSchematic.ArrowRight, partFDoorTransferData.DownstreamSpaceName));
                stringBuilder.AppendLine(string.Format("  Door modelled:          {0}", partFDoorTransferData.IsDoorRepresented ? "yes" : "no - the two spaces are adjacent but no door aperture is in the model"));
                stringBuilder.AppendLine(string.Format("  Required free area:     {0}", Area(partFDoorTransferData.MinimumRequiredFreeArea_mm2)));
                stringBuilder.AppendLine(string.Format("  Required undercut:      {0} above a fitted floor finish, {1} above an unfinished floor surface", Length(partFDoorTransferData.RequiredUndercutHeightFinished_mm), Length(partFDoorTransferData.RequiredUndercutHeightBeforeFloorFinish_mm)));
                stringBuilder.AppendLine(string.Format("  Provided undercut:      {0}", partFDoorTransferData.ProvidedUndercutHeight_mm is null ? "not recorded" : Length(partFDoorTransferData.ProvidedUndercutHeight_mm)));
                stringBuilder.AppendLine(string.Format("  Provided free area:     {0}", partFDoorTransferData.EffectiveProvidedFreeArea_mm2() is null ? "not recorded" : Area(partFDoorTransferData.EffectiveProvidedFreeArea_mm2())));
                stringBuilder.AppendLine(string.Format("  Clear door width:       {0}", partFDoorTransferData.ClearDoorWidth_mm is null ? "not recorded" : Length(partFDoorTransferData.ClearDoorWidth_mm)));
                stringBuilder.AppendLine(string.Format("  Transfer device:        {0}", Core.Query.Description(partFDoorTransferData.TransferDeviceType)));
                stringBuilder.AppendLine(string.Format("  Status:                 {0}", Core.Query.Description(partFDoorTransferData.ComplianceStatus)));

                if (!string.IsNullOrWhiteSpace(partFDoorTransferData.Diagnostic))
                {
                    stringBuilder.AppendLine(string.Format("  Note:                   {0}", partFDoorTransferData.Diagnostic));
                }

                stringBuilder.AppendLine();
            }
        }

        private static void PurgeSchedule(StringBuilder stringBuilder, PartFComplianceResult partFComplianceResult)
        {
            Section(stringBuilder, "PURGE VENTILATION ASSESSMENT");

            List<PartFPurgeVentilationData> purge = partFComplianceResult.PurgeVentilation;

            if (purge is null || purge.Count == 0)
            {
                stringBuilder.AppendLine("No habitable room, so paragraph 1.26 applies to nothing in this dwelling.");
                stringBuilder.AppendLine();
                return;
            }

            foreach (PartFPurgeVentilationData partFPurgeVentilationData in purge)
            {
                stringBuilder.AppendLine(partFPurgeVentilationData.SpaceName);
                stringBuilder.AppendLine(string.Format("  Room volume:            {0} m3", Number(partFPurgeVentilationData.RoomVolume_M3 ?? 0)));
                stringBuilder.AppendLine(string.Format("  Room floor area:        {0} m2", Number(partFPurgeVentilationData.RoomFloorArea_M2 ?? 0)));
                stringBuilder.AppendLine(string.Format("  Required air changes:   {0} per hour", Number(partFPurgeVentilationData.RequiredAirChangesPerHour_Value)));
                stringBuilder.AppendLine(string.Format("  Required purge rate:    {0}", Rate(partFPurgeVentilationData.RequiredPurgeRate_Lps)));
                stringBuilder.AppendLine(string.Format("  Purge method:           {0}", Core.Query.Description(partFPurgeVentilationData.PurgeMethod)));
                stringBuilder.AppendLine(string.Format("  Opening type:           {0}{1}", Core.Query.Description(partFPurgeVentilationData.OpeningType), partFPurgeVentilationData.OpeningAngle_Degrees is null ? string.Empty : string.Format("   ({0} degrees)", Number(partFPurgeVentilationData.OpeningAngle_Degrees.Value))));
                stringBuilder.AppendLine(string.Format("  Required opening area:  {0}", partFPurgeVentilationData.RequiredOpeningArea_M2 is null ? "cannot be determined - the Table 1.4 row depends on the opening type and angle" : string.Format("{0} m2", Number(partFPurgeVentilationData.RequiredOpeningArea_M2.Value))));
                stringBuilder.AppendLine(string.Format("  Openable window area:   {0}", partFPurgeVentilationData.OpenableWindowArea_M2 is null ? "not recorded" : string.Format("{0} m2", Number(partFPurgeVentilationData.OpenableWindowArea_M2.Value))));
                stringBuilder.AppendLine(string.Format("  External door area:     {0}", partFPurgeVentilationData.ExternalDoorOpeningArea_M2 is null ? "not recorded" : string.Format("{0} m2", Number(partFPurgeVentilationData.ExternalDoorOpeningArea_M2.Value))));
                stringBuilder.AppendLine(string.Format("  Mechanical purge:       {0}", partFPurgeVentilationData.MechanicalPurgeCapacity_Lps is null ? "not recorded" : Rate(partFPurgeVentilationData.MechanicalPurgeCapacity_Lps)));
                stringBuilder.AppendLine(string.Format("  Window area in model:   {0}   (the area of the windows, NOT the area they open to)", partFPurgeVentilationData.ExternalApertureArea_M2 is null ? "none" : string.Format("{0} m2", Number(partFPurgeVentilationData.ExternalApertureArea_M2.Value))));
                stringBuilder.AppendLine(string.Format("  Purge route outside:    {0}", partFPurgeVentilationData.IsPurgeRouteDirectlyOutside ? "yes" : "no"));
                stringBuilder.AppendLine(string.Format("  Status:                 {0}", Core.Query.Description(partFPurgeVentilationData.ComplianceStatus)));

                if (!string.IsNullOrWhiteSpace(partFPurgeVentilationData.Diagnostic))
                {
                    stringBuilder.AppendLine(string.Format("  Note:                   {0}", partFPurgeVentilationData.Diagnostic));
                }

                stringBuilder.AppendLine();
            }

            stringBuilder.AppendLine(PartFPurgeAssessor.PartOInteractionNote);
            stringBuilder.AppendLine();
        }

        private static void CommissioningStatus(StringBuilder stringBuilder, PartFComplianceResult partFComplianceResult)
        {
            Section(stringBuilder, "COMMISSIONING STATUS");

            PartFCommissioningData partFCommissioningData = partFComplianceResult.Commissioning;

            if (partFCommissioningData is null)
            {
                stringBuilder.AppendLine("No commissioning record has been supplied for this dwelling. This is expected at design stage.");
                stringBuilder.AppendLine("Approved Document F paragraph 4.1 requires the system to be commissioned and a commissioning notice given to the building control body, and paragraph 4.2 requires the air flow rates of a new dwelling to be measured and a notice of the measured rates given.");
                stringBuilder.AppendLine();
                return;
            }

            stringBuilder.AppendLine(string.Format("System classification:          {0}", Text(partFCommissioningData.SystemClassification)));
            stringBuilder.AppendLine(string.Format("Installation engineer:          {0}", Text(partFCommissioningData.InstallationEngineer)));
            stringBuilder.AppendLine(string.Format("Commissioning engineer:         {0}", Text(partFCommissioningData.CommissioningEngineer)));
            stringBuilder.AppendLine(string.Format("Commissioning date:             {0}", Text(partFCommissioningData.CommissioningDate)));
            stringBuilder.AppendLine(string.Format("Measurement equipment:          {0}", Text(partFCommissioningData.MeasurementEquipment)));
            stringBuilder.AppendLine(string.Format("Last UKAS calibration:          {0}", Text(partFCommissioningData.CalibrationDate)));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format("Design continuous supply:       {0}", Rate(partFComplianceResult.TotalContinuousSupply_Lps)));
            stringBuilder.AppendLine(string.Format("Measured continuous supply:     {0}", Rate(partFCommissioningData.MeasuredContinuousSupplyTotal_Lps)));
            stringBuilder.AppendLine(string.Format("Design continuous extract:      {0}", Rate(partFComplianceResult.TotalContinuousExtract_Lps)));
            stringBuilder.AppendLine(string.Format("Measured continuous extract:    {0}", Rate(partFCommissioningData.MeasuredContinuousExtractTotal_Lps)));
            stringBuilder.AppendLine(string.Format("Design high supply:             {0}", Rate(partFComplianceResult.TotalHighSupply_Lps)));
            stringBuilder.AppendLine(string.Format("Measured high supply:           {0}", Rate(partFCommissioningData.MeasuredHighSupplyTotal_Lps)));
            stringBuilder.AppendLine(string.Format("Design high extract:            {0}", Rate(partFComplianceResult.TotalHighExtract_Lps)));
            stringBuilder.AppendLine(string.Format("Measured high extract:          {0}", Rate(partFCommissioningData.MeasuredHighExtractTotal_Lps)));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format("Commissioning notice given:     {0}", YesNo(partFCommissioningData.CommissioningNoticeGiven)));
            stringBuilder.AppendLine(string.Format("Air flow rate notice given:     {0}", YesNo(partFCommissioningData.AirFlowRateNoticeGiven)));
            stringBuilder.AppendLine(string.Format("O&M information issued:         {0}", YesNo(partFCommissioningData.OperatingAndMaintenanceInformationIssued)));
            stringBuilder.AppendLine(string.Format("Home User Guide issued:         {0}", YesNo(partFCommissioningData.HomeUserGuideIssued)));

            if (!string.IsNullOrWhiteSpace(partFCommissioningData.Notes))
            {
                stringBuilder.AppendLine(string.Format("Notes:                          {0}", partFCommissioningData.Notes));
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Design and measured values are held separately: a measured value never overwrites a design value.");
            stringBuilder.AppendLine();
        }

        private static void Messages(StringBuilder stringBuilder, PartFDwellingResult partFDwellingResult, PartFComplianceResult partFComplianceResult)
        {
            List<string> warnings = [.. partFDwellingResult.Warnings, .. partFComplianceResult.Warnings];
            if (warnings.Count != 0)
            {
                Section(stringBuilder, "WARNINGS");
                foreach (string warning in warnings)
                {
                    stringBuilder.AppendLine("- " + warning);
                }

                stringBuilder.AppendLine();
            }

            List<string> notes = [.. partFDwellingResult.Remarks, .. partFComplianceResult.Notes];
            if (notes.Count != 0)
            {
                Section(stringBuilder, "NOTES");
                foreach (string note in notes)
                {
                    stringBuilder.AppendLine("- " + note);
                }

                stringBuilder.AppendLine();
            }
        }

        private static void Checks(StringBuilder stringBuilder, PartFComplianceResult partFComplianceResult)
        {
            CheckList(stringBuilder, "FAILED CHECKS", partFComplianceResult.FailedChecks, "None.");
            CheckList(stringBuilder, "UNRESOLVED CHECKS", partFComplianceResult.UnresolvedChecks, "None.");
            CheckList(stringBuilder, "ENGINEERING REVIEW REQUIRED", partFComplianceResult.EngineeringReviewChecks, "None.");
            CheckList(stringBuilder, "ALTERNATIVE SOLUTIONS PENDING APPROVAL", partFComplianceResult.AlternativeSolutionChecks, "None.");
            CheckList(stringBuilder, "CHECKS RESOLVED BY A PERSON", partFComplianceResult.UserResolvedChecks, "None. Every reported status is the one SAM calculated.");

            Section(stringBuilder, "REGULATORY REFERENCES");

            foreach (PartFComplianceCheck check in partFComplianceResult.Checks ?? [])
            {
                stringBuilder.AppendLine(string.Format("{0,-32} {1,-30} {2}", Truncate(check.Category, 31), Core.Query.Description(check.Status), check.SourceReference));
            }

            stringBuilder.AppendLine();
        }

        private static void CheckList(StringBuilder stringBuilder, string title, List<PartFComplianceCheck> checks, string empty)
        {
            Section(stringBuilder, title);

            if (checks is null || checks.Count == 0)
            {
                stringBuilder.AppendLine(empty);
                stringBuilder.AppendLine();
                return;
            }

            foreach (PartFComplianceCheck check in checks)
            {
                stringBuilder.AppendLine(check.Name);
                stringBuilder.AppendLine(string.Format("  Source:                 {0}", check.SourceReference));
                stringBuilder.AppendLine(string.Format("  Requirement:            {0}", check.Requirement));
                stringBuilder.AppendLine(string.Format("  SAM calculated:         {0}", Core.Query.Description(check.CalculatedStatus)));
                stringBuilder.AppendLine(string.Format("  Reported:               {0}", Core.Query.Description(check.FinalAssessmentStatus)));

                if (!string.IsNullOrWhiteSpace(check.Evidence))
                {
                    stringBuilder.AppendLine(string.Format("  Calculated evidence:    {0}", check.Evidence));
                }

                if (!string.IsNullOrWhiteSpace(check.UserEvidence))
                {
                    stringBuilder.AppendLine(string.Format("  Recorded evidence:      {0}", check.UserEvidence));
                }

                if (!string.IsNullOrWhiteSpace(check.AlternativeComplianceMethod))
                {
                    stringBuilder.AppendLine(string.Format("  Alternative method:     {0}", check.AlternativeComplianceMethod));
                }

                if (!string.IsNullOrWhiteSpace(check.OverrideReason))
                {
                    stringBuilder.AppendLine(string.Format("  Reason recorded:        {0}", check.OverrideReason));
                }

                if (check.CalculatedStatus == PartFComplianceStatus.Fail && check.IsUserResolved)
                {
                    stringBuilder.AppendLine("  Note:                   SAM calculated this check as failed. That result is retained and is not overturned by the record above.");
                }

                if (!string.IsNullOrWhiteSpace(check.Notes))
                {
                    stringBuilder.AppendLine(string.Format("  Notes:                  {0}", check.Notes));
                }

                if (!string.IsNullOrWhiteSpace(check.ConfirmedBy) || !string.IsNullOrWhiteSpace(check.ConfirmationDate))
                {
                    stringBuilder.AppendLine(string.Format("  Confirmed by:           {0} on {1}", Text(check.ConfirmedBy), Text(check.ConfirmationDate)));
                }

                stringBuilder.AppendLine();
            }
        }

        private static void Overall(StringBuilder stringBuilder, PartFComplianceResult partFComplianceResult)
        {
            Section(stringBuilder, "OVERALL PART F CONFORMANCE ASSESSMENT");

            stringBuilder.AppendLine(string.Format("Status: {0}", Core.Query.Description(partFComplianceResult.OverallStatus)));
            stringBuilder.AppendLine();

            int count_Total = partFComplianceResult.Checks?.Count ?? 0;
            int count_Resolved = partFComplianceResult.Checks?.Count(x => x.IsResolved) ?? 0;

            stringBuilder.AppendLine(string.Format("{0} of {1} check(s) resolved. {2} failed, {3} could not be determined, {4} need engineering review, {5} rest on an alternative solution pending approval.",
                count_Resolved,
                count_Total,
                partFComplianceResult.FailedChecks.Count,
                partFComplianceResult.UnresolvedChecks.Count,
                partFComplianceResult.EngineeringReviewChecks.Count,
                partFComplianceResult.AlternativeSolutionChecks.Count));

            int count_UserResolved = partFComplianceResult.UserResolvedChecks.Count;
            if (count_UserResolved != 0)
            {
                stringBuilder.AppendLine(string.Format("{0} check(s) are reported at a status other than the one SAM calculated. Each one's calculated result is retained alongside it.", count_UserResolved));
            }

            if (partFComplianceResult.OverallStatus != PartFOverallStatus.Pass)
            {
                stringBuilder.AppendLine("A dwelling cannot reach an overall pass while any mandatory check is failed or unresolved.");
                stringBuilder.AppendLine("A check SAM calculated as failed cannot be turned into a pass by changing its status. It leaves the assessment by the input being corrected and the dwelling recalculated, by missing measured or geometric evidence being supplied, or by an alternative compliance method being recorded and accepted by a building control body.");
            }

            stringBuilder.AppendLine();
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Why a terminal's high rate is what it is.
        /// <para>
        /// Table 1.2 and its note 1 are about EXTRACT ventilation in a room. Quoting them against a
        /// supply terminal, as this once did, cites the wrong clause: a balanced system's supply is
        /// governed by paragraphs 1.67 to 1.69, and its high rate is whatever balances the dwelling's
        /// high extract total. Paragraph 1.70 is what applies the Table 1.2 high rates, and it applies
        /// them to wet rooms.
        /// </para>
        /// </summary>
        private static string HighRateNote(PartFVentilationTerminalRequirement partFVentilationTerminalRequirement)
        {
            if (partFVentilationTerminalRequirement.TerminalRole == PartFTerminalRole.Supply)
            {
                return "   (balanced to the dwelling high/boost extract total, paragraphs 1.67 to 1.69)";
            }

            if (partFVentilationTerminalRequirement.HighRateIncreaseRequired)
            {
                return "   (a boost above the continuous rate is required)";
            }

            return partFVentilationTerminalRequirement.ContinuousDesignFlowRate_Lps is null
                ? string.Empty
                : "   (already met at the continuous rate, Table 1.2 note 1)";
        }

        private static string Governing(PartFDwellingResult partFDwellingResult)
        {
            double rate = partFDwellingResult.ContinuousDesignSystemRate_Lps;

            List<string> governing = [];

            if (System.Math.Abs(rate - partFDwellingResult.BedroomOrHabitableRate_Lps) < 1e-9)
            {
                governing.Add(partFDwellingResult.OneHabitableRoomRuleApplied ? "Table 1.3 note 1" : "Table 1.3 bedroom rate");
            }

            if (System.Math.Abs(rate - partFDwellingResult.AreaBasedRate_Lps) < 1e-9)
            {
                governing.Add("paragraph 1.24a floor area rate");
            }

            return governing.Count == 0 ? "no minimum reached this rate" : string.Join(" and ", governing);
        }

        private static void Section(StringBuilder stringBuilder, string title)
        {
            stringBuilder.AppendLine(title);
            stringBuilder.AppendLine(new string('-', title.Length));
        }

        private static void Rule(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(new string('=', 78));
        }

        private static string ModeText(PartFOperatingMode partFOperatingMode)
        {
            return Core.Query.Description(partFOperatingMode);
        }

        private static string Rate(double? value_Lps)
        {
            return value_Lps is null ? "not applicable" : string.Format("{0} l/s", Number(value_Lps.Value));
        }

        private static string Area(double? value_mm2)
        {
            return value_mm2 is null ? "not recorded" : string.Format("{0} mm2", value_mm2.Value.ToString("#,##0.##", CultureInfo.InvariantCulture));
        }

        private static string Length(double? value_mm)
        {
            return value_mm is null ? "not recorded" : string.Format("{0} mm", Number(value_mm.Value));
        }

        private static string Number(double value)
        {
            return PartFSchematic.Number(value);
        }

        private static string Text(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "not recorded" : text;
        }

        private static string YesNo(bool value)
        {
            return value ? "yes" : "no";
        }

        private static string Truncate(string text, int length)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Length <= length ? text : text.Substring(0, length);
        }
    }
}
