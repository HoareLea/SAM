// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// The studio reference calculation, locked end to end.
    /// </summary>
    /// <remarks>
    /// One studio of 75 m2 and 300 m3 containing the cooking function, one bathroom of 25 m2 and 100 m3,
    /// one internal door between them, balanced mechanical ventilation with heat recovery. This is Flat 1
    /// of the SAM_zoningAM_v1 example model, reproduced here as a synthetic model so the test owns its own
    /// inputs and never touches the example file.
    /// <para>
    /// Whole dwelling rate: one habitable room, so Approved Document F, Volume 1 (2021 edition) Table 1.3
    /// note 1 gives 13 l/s and paragraph 1.24a gives 0.3 x 100 = 30 l/s, so the whole dwelling rate is
    /// 30 l/s. The Table 1.2 per-room minimum HIGH rates total kitchen 13 + bathroom 8 = 21 l/s; that
    /// total is reported and does not raise the continuous rate, and here it is below it anyway.
    /// </para>
    /// <para>
    /// Terminal allocation by the minimum-first, cooking-priority strategy: every terminal takes its
    /// Table 1.2 minimum (13 + 8 = 21 l/s), and the remaining 9 l/s goes to the local kitchen extract, so
    /// the studio extracts 22 l/s and the bathroom 8 l/s. Supply is 30 l/s to the studio, the dwelling's
    /// only habitable room. The studio is then +30 - 22 = +8 l/s net and the bathroom -8 l/s, so 8 l/s of
    /// transfer air crosses the internal door between them.
    /// </para>
    /// <para>
    /// This allocation is NOT the only Part F compliant split. What Approved Document F fixes is that the
    /// total continuous extract reaches the dwelling design rate and that each room reaches its Table 1.2
    /// minimum high rate; the share of the surplus is an engineering strategy, which is why it is named on
    /// the result and can be changed.
    /// </para>
    /// </remarks>
    public class PartFStudioReferenceTests
    {
        private const string dataFileName = "SAM_PartFSpaceRulesUKDwellingsMVHR.json";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // Whole dwelling rates
        // ------------------------------------------------------------------

        [Fact]
        public void Studio_OneHabitableRoomRuleAppliesAndTheFloorAreaRateGoverns()
        {
            PartFDwellingResult dwellingResult = Reference();

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.Equal(1, dwellingResult.BedroomCount);
            Assert.True(dwellingResult.OneHabitableRoomRuleApplied);

            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
            Assert.Equal(100, dwellingResult.InternalFloorArea_M2, tolerance);
            Assert.Equal(30, dwellingResult.AreaBasedRate_Lps, tolerance);

            //Kitchen 13 (the studio's own local kitchen extract) + bathroom 8.
            Assert.Equal(21, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);

            Assert.Equal(30, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
        }

        // ------------------------------------------------------------------
        // Terminals
        // ------------------------------------------------------------------

        /// <summary>
        /// The studio holds BOTH a supply terminal, because Appendix A makes it a habitable room, and a
        /// local kitchen extract terminal, because it contains the cooking function. Neither displaces the
        /// other.
        /// </summary>
        [Fact]
        public void Studio_HoldsBothASupplyTerminalAndALocalKitchenExtractTerminal()
        {
            PartFComplianceResult complianceResult = Reference().ComplianceResult;

            List<PartFVentilationTerminalRequirement> terminals_Studio = [.. complianceResult.Terminals.Where(x => x.SpaceName == "Studio")];

            Assert.Equal(2, terminals_Studio.Count);
            Assert.Contains(terminals_Studio, x => x.TerminalRole == PartFTerminalRole.Supply);
            Assert.Contains(terminals_Studio, x => x.TerminalRole == PartFTerminalRole.LocalKitchenExtract);
        }

        [Fact]
        public void Studio_ReferenceTerminalRates()
        {
            PartFComplianceResult complianceResult = Reference().ComplianceResult;

            Assert.Equal(30, Terminal(complianceResult, "Studio", PartFTerminalRole.Supply).ContinuousDesignFlowRate_Lps.Value, tolerance);
            Assert.Equal(22, Terminal(complianceResult, "Studio", PartFTerminalRole.LocalKitchenExtract).ContinuousDesignFlowRate_Lps.Value, tolerance);
            Assert.Equal(8, Terminal(complianceResult, "Bathroom", PartFTerminalRole.GeneralExtract).ContinuousDesignFlowRate_Lps.Value, tolerance);
        }

        [Fact]
        public void Studio_ContinuousSupplyAndExtractBalance()
        {
            PartFDwellingResult dwellingResult = Reference();

            Assert.Equal(30, dwellingResult.TotalSupply_Lps, tolerance);
            Assert.Equal(30, dwellingResult.TotalExtract_Lps, tolerance);
        }

        /// <summary>
        /// Table 1.2 note 1: where the continuous rate is already at or above the minimum high rate, no
        /// boost is needed. Both terminals are, so neither has to increase.
        /// </summary>
        [Fact]
        public void Studio_HighRatesMeetTheTable1_2Minimums()
        {
            PartFComplianceResult complianceResult = Reference().ComplianceResult;

            PartFVentilationTerminalRequirement terminal_Kitchen = Terminal(complianceResult, "Studio", PartFTerminalRole.LocalKitchenExtract);
            PartFVentilationTerminalRequirement terminal_Bathroom = Terminal(complianceResult, "Bathroom", PartFTerminalRole.GeneralExtract);

            Assert.Equal(13, terminal_Kitchen.MinimumRequiredFlowRate_Lps.Value, tolerance);
            Assert.Equal(22, terminal_Kitchen.HighFlowRate_Lps.Value, tolerance);
            Assert.False(terminal_Kitchen.HighRateIncreaseRequired);

            Assert.Equal(8, terminal_Bathroom.MinimumRequiredFlowRate_Lps.Value, tolerance);
            Assert.Equal(8, terminal_Bathroom.HighFlowRate_Lps.Value, tolerance);
            Assert.False(terminal_Bathroom.HighRateIncreaseRequired);
        }

        [Fact]
        public void Studio_HighSupplyBalancesHighExtract()
        {
            PartFDwellingResult dwellingResult = Reference();

            Assert.Equal(30, dwellingResult.TotalHighExtract_Lps, tolerance);
            Assert.Equal(30, dwellingResult.TotalHighSupply_Lps, tolerance);
        }

        /// <summary>Setback is 30% of continuous design, and is not a Part F condition.</summary>
        [Fact]
        public void Studio_ReferenceSetbackRates()
        {
            PartFComplianceResult complianceResult = Reference().ComplianceResult;

            Assert.Equal(9, Terminal(complianceResult, "Studio", PartFTerminalRole.Supply).SetbackFlowRate_Lps.Value, tolerance);
            Assert.Equal(6.6, Terminal(complianceResult, "Studio", PartFTerminalRole.LocalKitchenExtract).SetbackFlowRate_Lps.Value, tolerance);
            Assert.Equal(2.4, Terminal(complianceResult, "Bathroom", PartFTerminalRole.GeneralExtract).SetbackFlowRate_Lps.Value, tolerance);
        }

        // ------------------------------------------------------------------
        // Transfer air
        // ------------------------------------------------------------------

        /// <summary>
        /// The studio is +8 l/s net and the bathroom -8 l/s, and there is exactly one route between them,
        /// so conservation of air flow fixes the transfer at 8 l/s with no engineering choice involved.
        /// </summary>
        [Fact]
        public void Studio_TransfersEightLitresPerSecondToTheBathroom()
        {
            PartFComplianceResult complianceResult = Reference().ComplianceResult;

            PartFDoorTransferData partFDoorTransferData = Assert.Single(complianceResult.TransferPaths);

            Assert.Equal("Studio", partFDoorTransferData.UpstreamSpaceName);
            Assert.Equal("Bathroom", partFDoorTransferData.DownstreamSpaceName);
            Assert.Equal(8, partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps.Value, tolerance);
            Assert.Equal(2.4, partFDoorTransferData.SetbackTransferFlowRate_Lps.Value, tolerance);
            Assert.Equal(PartFTransferRouteStatus.UniquelyDetermined, partFDoorTransferData.RouteStatus);
        }

        /// <summary>
        /// Paragraph 1.25 requires a free area equivalent to a 10mm undercut in a 760mm wide door, which
        /// is 7,600mm2, achieved as 10mm above a fitted floor finish or 20mm above an unfinished one.
        /// Nothing in the model records what was actually provided, so the door cannot be passed.
        /// </summary>
        [Fact]
        public void Studio_DoorCarriesTheParagraph1_25RequirementAndCannotBeDetermined()
        {
            PartFDoorTransferData partFDoorTransferData = Assert.Single(Reference().ComplianceResult.TransferPaths);

            Assert.True(partFDoorTransferData.RequiresTransferAirPath);
            Assert.True(partFDoorTransferData.IsInternalDwellingDoor);

            Assert.Equal(7600, partFDoorTransferData.MinimumRequiredFreeArea_mm2.Value, tolerance);
            Assert.Equal(10, partFDoorTransferData.RequiredUndercutHeightFinished_mm.Value, tolerance);
            Assert.Equal(20, partFDoorTransferData.RequiredUndercutHeightBeforeFloorFinish_mm.Value, tolerance);

            Assert.Null(partFDoorTransferData.ProvidedUndercutHeight_mm);
            Assert.Equal(PartFComplianceStatus.CannotBeDetermined, partFDoorTransferData.ComplianceStatus);
            Assert.False(partFDoorTransferData.IsCompliant);
        }

        // ------------------------------------------------------------------
        // The schematic
        // ------------------------------------------------------------------

        /// <summary>
        /// The compact schematic, asserted verbatim. This is the diagram an engineer reads first, so its
        /// exact shape is part of the deliverable rather than an implementation detail.
        /// <para>
        /// The reference model carries NO door aperture between the studio and the bathroom, so the branch
        /// says "calculated transfer ?" and carries a caption. It used to say "through internal door",
        /// which contradicted the same assessment's own door schedule further down the report.
        /// </para>
        /// </summary>
        [Fact]
        public void Studio_CompactSchematic()
        {
            string schematic = PartFSchematic.Build(Reference().ComplianceResult, PartFOperatingMode.ContinuousDesign);

            string expected = string.Join("\r\n",
                "Outdoor supply",
                "      " + PartFSchematic.ArrowDown,
                "Studio: +30 l/s supply, " + PartFSchematic.Minus + "22 l/s local kitchen extract",
                "      " + PartFSchematic.Vertical,
                "      " + PartFSchematic.CornerLast + Repeat(PartFSchematic.Horizontal, 4) + " 8 l/s calculated transfer ? " + Repeat(PartFSchematic.Horizontal, 4) + PartFSchematic.ArrowRight + " Bathroom: " + PartFSchematic.Minus + "8 l/s extract",
                "             no modelled transfer opening");

            Assert.Contains(expected, schematic);
        }

        /// <summary>
        /// The schematic never says "through internal door" for a route the model carries no door for.
        /// This is the assertion that stops the diagram claiming an opening the door schedule denies.
        /// </summary>
        [Fact]
        public void Studio_Schematic_DoesNotClaimADoorThatIsNotModelled()
        {
            PartFComplianceResult complianceResult = Reference().ComplianceResult;

            Assert.All(complianceResult.TransferPaths, x => Assert.False(x.IsDoorRepresented));

            string schematic = PartFSchematic.Build(complianceResult, PartFOperatingMode.ContinuousDesign);

            Assert.DoesNotContain("through internal door", schematic);
            Assert.Contains("no modelled transfer opening", schematic);
        }

        /// <summary>The schematic names its own operating condition, so two modes can never be confused.</summary>
        [Theory]
        [InlineData(PartFOperatingMode.ContinuousDesign, "CONTINUOUS DESIGN")]
        [InlineData(PartFOperatingMode.HighBoost, "HIGH/BOOST")]
        [InlineData(PartFOperatingMode.Setback, "SETBACK")]
        [InlineData(PartFOperatingMode.MeasuredCommissioning, "MEASURED COMMISSIONING")]
        public void Schematic_NamesItsOperatingCondition(PartFOperatingMode partFOperatingMode, string expected)
        {
            string schematic = PartFSchematic.Build(Reference().ComplianceResult, partFOperatingMode);

            Assert.StartsWith("AIRFLOW SCHEMATIC " + PartFSchematic.EmDash + " " + expected, schematic);
        }

        /// <summary>The setback schematic carries the setback numbers, not the continuous design ones.</summary>
        [Fact]
        public void Studio_SetbackSchematicUsesTheSetbackRates()
        {
            string schematic = PartFSchematic.Build(Reference().ComplianceResult, PartFOperatingMode.Setback);

            Assert.Contains("Studio: +9 l/s supply, " + PartFSchematic.Minus + "6.6 l/s local kitchen extract", schematic);
            Assert.Contains("2.4 l/s calculated transfer ?", schematic);
            Assert.Contains("Bathroom: " + PartFSchematic.Minus + "2.4 l/s extract", schematic);
        }

        /// <summary>
        /// The measured schematic draws the rates recorded at commissioning under Appendix C Part 3. Those
        /// record fan and terminal rates, not door flows, so no transfer air is invented for a door nobody
        /// measured.
        /// </summary>
        [Fact]
        public void Studio_MeasuredSchematicUsesMeasuredRatesAndDoesNotInventATransferFlow()
        {
            PartFComplianceResult complianceResult = Reference().ComplianceResult;

            Terminal(complianceResult, "Studio", PartFTerminalRole.Supply).MeasuredContinuousFlowRate_Lps = 31;
            Terminal(complianceResult, "Studio", PartFTerminalRole.LocalKitchenExtract).MeasuredContinuousFlowRate_Lps = 22.5;
            Terminal(complianceResult, "Bathroom", PartFTerminalRole.GeneralExtract).MeasuredContinuousFlowRate_Lps = 8.5;

            string schematic = PartFSchematic.Build(complianceResult, PartFOperatingMode.MeasuredCommissioning);

            Assert.Contains("Studio: +31 l/s supply, " + PartFSchematic.Minus + "22.5 l/s local kitchen extract", schematic);
            Assert.Contains("Bathroom: " + PartFSchematic.Minus + "8.5 l/s extract", schematic);
            Assert.Contains("not measured calculated transfer ?", schematic);
        }

        /// <summary>
        /// With nothing measured, the measured schematic reports the spaces and no flows at all, rather
        /// than falling back to the design rates and presenting them as measurements.
        /// </summary>
        [Fact]
        public void Studio_MeasuredSchematicWithNoMeasurementsShowsNoRates()
        {
            string schematic = PartFSchematic.Build(Reference().ComplianceResult, PartFOperatingMode.MeasuredCommissioning);

            Assert.Contains("Studio", schematic);
            Assert.Contains("Bathroom", schematic);
            Assert.DoesNotContain("l/s", schematic);
        }

        // ------------------------------------------------------------------
        // The report
        // ------------------------------------------------------------------

        /// <summary>
        /// Table 1.2 and its note 1 are about EXTRACT ventilation in a room, so the report must not cite
        /// them against a supply terminal. A balanced system's supply is governed by paragraphs 1.67 to
        /// 1.69, and its high rate is whatever balances the dwelling's high extract total.
        /// </summary>
        [Fact]
        public void Report_SupplyHighRate_DoesNotCiteTable1_2Note1()
        {
            string report = PartFReport.Build([Reference()]);

            int index_Supply = report.IndexOf("SUPPLY TERMINAL SCHEDULE", System.StringComparison.Ordinal);
            int index_Next = report.IndexOf("GENERAL EXTRACT SCHEDULE", System.StringComparison.Ordinal);

            Assert.True(index_Supply >= 0 && index_Next > index_Supply);

            string section = report.Substring(index_Supply, index_Next - index_Supply);

            Assert.DoesNotContain("Table 1.2 note 1", section);
            Assert.Contains("balanced to the dwelling high/boost extract total", section);
            Assert.Contains("1.67 to 1.69", section);
        }

        /// <summary>
        /// The extract terminals keep the note 1 wording, because it does apply to them: a room already
        /// continuously at or above its own Table 1.2 minimum needs no extra ventilation.
        /// </summary>
        [Fact]
        public void Report_ExtractHighRate_KeepsTable1_2Note1()
        {
            string report = PartFReport.Build([Reference()]);

            int index = report.IndexOf("LOCAL KITCHEN EXTRACT SCHEDULE", System.StringComparison.Ordinal);

            Assert.True(index >= 0);
            Assert.Contains("Table 1.2 note 1", report.Substring(index));
        }

        /// <summary>
        /// The allocation note must not read as though 9 l/s were itself a Table 1.2 minimum. It is the
        /// surplus ABOVE the combined 21 l/s of the two rooms' minima.
        /// </summary>
        [Fact]
        public void Report_AllocationNote_NamesTheSurplusAndTheMinimaSeparately()
        {
            string report = PartFReport.Build([Reference()]);

            Assert.Contains("9 l/s surplus above the combined 21 l/s Table 1.2 high-rate minima", report);
            Assert.DoesNotContain("above the Approved Document F Table 1.2 minimums (", report);
        }

        /// <summary>The report opens with the assumptions, verbatim, before any number.</summary>
        [Fact]
        public void Report_BeginsWithTheRequiredAssumptions()
        {
            string report = PartFReport.Build([Reference()]);

            Assert.StartsWith("ASSUMPTIONS\r\n\r\nNew dwelling in England.\r\nApproved Document F, Volume 1, 2021 edition.\r\n", report);
        }

        [Fact]
        public void Report_ContainsTheCompactSchematic()
        {
            string report = PartFReport.Build([Reference()]);

            Assert.Contains("Studio: +30 l/s supply, " + PartFSchematic.Minus + "22 l/s local kitchen extract", report);
            Assert.Contains("8 l/s calculated transfer ?", report);
            Assert.Contains("no modelled transfer opening", report);
        }

        /// <summary>Every schedule the assessment produces reaches the report.</summary>
        [Theory]
        [InlineData("DWELLING SUMMARY")]
        [InlineData("SUPPLY TERMINAL SCHEDULE")]
        [InlineData("GENERAL EXTRACT SCHEDULE")]
        [InlineData("LOCAL KITCHEN EXTRACT SCHEDULE")]
        [InlineData("INTERNAL TRANSFER AIR ROUTING (CALCULATED)")]
        [InlineData("DOOR UNDERCUT AND FREE AREA SCHEDULE (PARAGRAPH 1.25 ASSESSMENT)")]
        [InlineData("PURGE VENTILATION ASSESSMENT")]
        [InlineData("COMMISSIONING STATUS")]
        [InlineData("FAILED CHECKS")]
        [InlineData("UNRESOLVED CHECKS")]
        [InlineData("ENGINEERING REVIEW REQUIRED")]
        [InlineData("REGULATORY REFERENCES")]
        [InlineData("OVERALL PART F CONFORMANCE ASSESSMENT")]
        public void Report_ContainsEverySection(string title)
        {
            Assert.Contains(title, PartFReport.Build([Reference()]));
        }

        /// <summary>The result is an assessment. It is never described as a certificate.</summary>
        [Fact]
        public void Report_IsNeverCalledACertificate()
        {
            string report = PartFReport.Build([Reference()]);

            Assert.Contains("Part F conformance assessment", report);
            Assert.DoesNotContain("certificate of compliance", report);
            Assert.DoesNotContain("certifies", report);
        }

        // ------------------------------------------------------------------
        // Overall status
        // ------------------------------------------------------------------

        /// <summary>
        /// A dwelling with no commissioning record, no recorded door undercut and no confirmed manual
        /// checks cannot pass. Nothing here has failed; a great deal is simply unanswered, and unanswered
        /// is not compliance.
        /// </summary>
        [Fact]
        public void Studio_CannotPassWhileMandatoryChecksAreUnresolved()
        {
            PartFComplianceResult complianceResult = Reference().ComplianceResult;

            Assert.Empty(complianceResult.FailedChecks);
            Assert.NotEmpty(complianceResult.UnresolvedChecks);
            Assert.Equal(PartFOverallStatus.CannotBeDetermined, complianceResult.OverallStatus);
        }

        // ------------------------------------------------------------------
        // The alternative allocation strategy
        // ------------------------------------------------------------------

        /// <summary>
        /// The volume-weighted strategy is retained and produces a different, equally Part F compliant
        /// split: both terminals still reach their Table 1.2 minimums and the total still reaches 30 l/s,
        /// but the 9 l/s surplus is shared by volume, 300 m3 against 100 m3, so the studio takes 6.75 and
        /// the bathroom 2.25.
        /// </summary>
        [Fact]
        public void Studio_VolumeWeightedStrategyProducesADifferentButValidSplit()
        {
            PartFCalculator partFCalculator = Calculator(PartFExtractAllocationStrategy.VolumeWeighted);

            PartFComplianceResult complianceResult = Assert.Single(partFCalculator.DwellingResults).ComplianceResult;

            Assert.Equal(19.75, Terminal(complianceResult, "Studio", PartFTerminalRole.LocalKitchenExtract).ContinuousDesignFlowRate_Lps.Value, tolerance);
            Assert.Equal(10.25, Terminal(complianceResult, "Bathroom", PartFTerminalRole.GeneralExtract).ContinuousDesignFlowRate_Lps.Value, tolerance);

            //Still balanced, still at the dwelling design rate, still above every Table 1.2 minimum.
            Assert.Equal(30, complianceResult.TotalContinuousExtract_Lps, tolerance);
            Assert.Equal(30, complianceResult.TotalContinuousSupply_Lps, tolerance);

            //And the transfer air follows the terminals: the studio is now +30 - 19.75 = +10.25 l/s net.
            Assert.Equal(10.25, Assert.Single(complianceResult.TransferPaths).ContinuousDesignTransferFlowRate_Lps.Value, tolerance);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static string Repeat(string text, int count)
        {
            return string.Concat(Enumerable.Repeat(text, count));
        }

        private static PartFVentilationTerminalRequirement Terminal(PartFComplianceResult partFComplianceResult, string spaceName, PartFTerminalRole partFTerminalRole)
        {
            PartFVentilationTerminalRequirement result = partFComplianceResult.Terminals.Find(x => x.SpaceName == spaceName && x.TerminalRole == partFTerminalRole);

            Assert.NotNull(result);

            return result;
        }

        internal static PartFCalculator Calculator(PartFExtractAllocationStrategy? partFExtractAllocationStrategy = null)
        {
            PartFModel partFModel = new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom")
                .ExternalWall("Studio");

            PartFCalculator result = new(Analytical.Create.PartFData(Fixtures.GetPath(dataFileName)))
            {
                AdjacencyCluster = partFModel.AdjacencyCluster,
            };

            if (partFExtractAllocationStrategy is not null)
            {
                result.ExtractAllocationStrategy = partFExtractAllocationStrategy.Value;
            }

            Assert.True(result.Calculate());

            return result;
        }

        private static PartFDwellingResult Reference()
        {
            return Assert.Single(Calculator().DwellingResults);
        }
    }
}
