// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Conformance tests for <see cref="PartFCalculator"/> and <see cref="PartFData"/> against
    /// Approved Document F, Volume 1: Dwellings, 2021 edition, for use in England (in effect
    /// 15 June 2022).
    /// </summary>
    /// <remarks>
    /// The component under test sizes a balanced mechanical ventilation with heat recovery system
    /// (paragraphs 1.67 to 1.73). Purge ventilation, background ventilators, intermittent extract
    /// (Table 1.1), airtightness and Section 3 work on existing dwellings are outside its scope and
    /// are therefore not covered here.
    /// </remarks>
    public class PartFTests
    {
        private const string dataFileName = "SAM_PartFSpaceRulesUKDwellingsMVHR.json";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // Table 1.3 (page 10) - minimum whole dwelling rate by number of bedrooms
        // ------------------------------------------------------------------

        /// <summary>ADF F Vol 1 (2021) Table 1.3, page 10: 19, 25, 31, 37, 43 l/s for 1 to 5 bedrooms.</summary>
        [Theory]
        [InlineData(1, 19)]
        [InlineData(2, 25)]
        [InlineData(3, 31)]
        [InlineData(4, 37)]
        [InlineData(5, 43)]
        public void Table1_3_TabulatedBedroomRates_MatchApprovedDocument(int bedroomCount, double expected_Lps)
        {
            Assert.Equal(expected_Lps, DataFile().GetWholeDwellingRates_Lps(bedroomCount), tolerance);
        }

        /// <summary>ADF F Vol 1 (2021) Table 1.3 note 2, page 10: add 6 l/s for each additional bedroom.</summary>
        [Theory]
        [InlineData(6, 49)]
        [InlineData(7, 55)]
        [InlineData(10, 73)]
        public void Table1_3Note2_AboveFiveBedrooms_AddsSixLitresPerSecondEach(int bedroomCount, double expected_Lps)
        {
            Assert.Equal(expected_Lps, DataFile().GetWholeDwellingRates_Lps(bedroomCount), tolerance);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) Table 1.3, page 10, gives no value below one bedroom. A dwelling with no
        /// space classified as a bedroom is sized as a one bedroom dwelling rather than extrapolated
        /// below the table (which previously produced 13 l/s).
        /// </summary>
        [Fact]
        public void Table1_3_ZeroBedrooms_IsClampedToTheOneBedroomRate()
        {
            Assert.Equal(19, DataFile().GetWholeDwellingRates_Lps(0), tolerance);
        }

        /// <summary>The Table 1.3 increment is taken from the data file rather than hard coded.</summary>
        [Fact]
        public void Table1_3Note2_IncrementIsReadFromTheDataFile()
        {
            PartFData partFData = new()
            {
                WholeDwellingRates_Lps = new Dictionary<int, double> { { 1, 19 }, { 2, 25 } },
                IncrementAbove5 = 10,
            };

            Assert.Equal(45, partFData.GetWholeDwellingRates_Lps(4), tolerance);
        }

        /// <summary>Missing input: with no rate table loaded the bedroom based rate is not available.</summary>
        [Fact]
        public void Table1_3_MissingRateTable_ReturnsNaN()
        {
            Assert.True(double.IsNaN(new PartFData().GetWholeDwellingRates_Lps(3)));
        }

        // ------------------------------------------------------------------
        // Table 1.2 (page 10) - minimum continuous extract high rates
        // ------------------------------------------------------------------

        /// <summary>
        /// ADF F Vol 1 (2021) Table 1.2, page 10: kitchen 13 l/s, utility room 8 l/s, bathroom 8 l/s,
        /// sanitary accommodation 6 l/s.
        /// </summary>
        [Theory]
        [InlineData("Kitchen", 13)]
        [InlineData("Utility", 8)]
        [InlineData("Bathroom", 8)]
        [InlineData("WC", 6)]
        public void Table1_2_WetRoomMinimumExtractRates_MatchApprovedDocument(string spaceName, double expected_Lps)
        {
            PartFCategory? partFCategory = DataFile().GetPartFCategory(spaceName);

            Assert.NotNull(partFCategory);
            Assert.Equal(PartFType.WetRoom, partFCategory!.PartFType);
            Assert.Equal(PartFVentilationType.extract, partFCategory.PartFVentilationType);
            Assert.Equal(expected_Lps, partFCategory.MinFlowRate_Lps);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) paragraphs 1.63 and 1.70, pages 16 and 17: every wet room extract
        /// terminal achieves at least its Table 1.2 minimum high rate.
        /// </summary>
        [Fact]
        public void Paragraph1_70_EveryWetRoom_ReceivesAtLeastItsTable1_2Minimum()
        {
            Dictionary<string, double> rates = Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom 1", 12, 30),
                ("Bedroom 2", 12, 30),
                ("Bedroom 3", 12, 30),
                ("Living Room", 24, 60),
                ("Kitchen", 12, 30),
                ("Bathroom", 6, 15),
                ("WC", 3, 7.5));

            Assert.True(rates["Kitchen"] >= 13);
            Assert.True(rates["Bathroom"] >= 8);
            Assert.True(rates["WC"] >= 6);
            Assert.Empty(partFCalculator.UnclassifiedSpaceNames);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) Table 1.2, page 10: where the sum of the per-room minimum HIGH rates
        /// exceeds the whole dwelling rate, the continuous system rate does NOT rise to that sum. The
        /// continuous total stays at the whole dwelling rate and is shared out; each room reaches its own
        /// Table 1.2 figure by boosting to its high rate.
        /// <para>
        /// Table 1.2's continuous requirement is on the TOTAL of continuous extract, not on each room,
        /// and note 1 says only that a room already continuously at or above its own minimum needs no
        /// further increase. Sizing continuous operation at 35 l/s here would be almost twice what the
        /// Approved Document asks of this dwelling.
        /// </para>
        /// </summary>
        [Fact]
        public void Table1_2_HighRateMinimums_DoNotRaiseTheContinuousSystemRate()
        {
            // The bedroom is the only habitable room, so Table 1.3 note 1 gives 13 l/s, above the 20 m2
            // floor area rate of 6 l/s - against per-room high-rate minimums of 13 + 8 + 8 + 6 = 35 l/s.
            Dictionary<string, double> rates = Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom", 10, 25),
                ("Kitchen", 4, 10),
                ("Bathroom", 3, 7.5),
                ("Utility", 2, 5),
                ("WC", 1, 2.5));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(13, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
            Assert.Equal(35, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);

            //The whole dwelling rate is shared in proportion to each room's Table 1.2 minimum.
            Assert.Equal(13 * 13 / 35.0, rates["Kitchen"], tolerance);
            Assert.Equal(13 * 8 / 35.0, rates["Bathroom"], tolerance);
            Assert.Equal(13 * 8 / 35.0, rates["Utility"], tolerance);
            Assert.Equal(13 * 6 / 35.0, rates["WC"], tolerance);
            Assert.Equal(13, rates["Bedroom"], tolerance);

            //And every room still reaches its own Table 1.2 minimum at the high rate.
            Assert.Equal(35, dwellingResult.TotalHighExtract_Lps, tolerance);
            Assert.All(
                dwellingResult.ComplianceResult.Terminals.Where(x => x.IsExtract),
                x => Assert.Equal(x.MinimumRequiredFlowRate_Lps!.Value, x.HighFlowRate_Lps!.Value, tolerance));
        }

        // ------------------------------------------------------------------
        // Paragraph 1.24 (page 10) - the greater of the two minima applies
        // ------------------------------------------------------------------

        /// <summary>
        /// ADF F Vol 1 (2021) paragraph 1.24, page 10: the whole dwelling rate meets both the
        /// bedroom based minimum and 0.3 l/s per m2, so the greater governs. Here 3 bedrooms
        /// (31 l/s) beats 60 m2 (18 l/s).
        /// </summary>
        [Fact]
        public void Paragraph1_24_BedroomRateGoverns_WhenGreaterThanTheAreaRate()
        {
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom 1", 12, 30),
                ("Bedroom 2", 12, 30),
                ("Bedroom 3", 12, 30),
                ("Living Room", 20, 50),
                ("Kitchen", 4, 10));

            Assert.Equal(31, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) paragraph 1.24a, page 10: 0.3 l/s per m2 of internal floor area
        /// governs where it exceeds the bedroom based rate. Here 1 bedroom (19 l/s) against
        /// 200 m2 (60 l/s).
        /// </summary>
        [Fact]
        public void Paragraph1_24a_AreaRateGoverns_WhenGreaterThanTheBedroomRate()
        {
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom", 60, 150),
                ("Living Room", 120, 300),
                ("Kitchen", 15, 37.5),
                ("Bathroom", 5, 12.5));

            Assert.Equal(60, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
        }

        /// <summary>
        /// Boundary behaviour of paragraph 1.24: immediately below, at and above the floor area at
        /// which the area based rate overtakes the 2 bedroom rate of 25 l/s, i.e. 83.333 m2.
        /// </summary>
        [Theory]
        [InlineData(80, 25)]           // below - bedroom rate governs
        [InlineData(83.3333333333, 25)] // at the crossover - both give 25 l/s
        [InlineData(90, 27)]           // above - area rate governs
        public void Paragraph1_24_FloorAreaBoundary_SelectsTheGreaterRate(double area_M2, double expected_Lps)
        {
            // Two bedrooms plus a living room; the living room carries the balance of the floor area.
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom 1", 12, 30),
                ("Bedroom 2", 12, 30),
                ("Living Room", area_M2 - 24, (area_M2 - 24) * 2.5));

            Assert.Equal(expected_Lps, partFCalculator.FinalSystemRate_Lps!.Value, 1e-4);
        }

        /// <summary>The 0.3 l/s per m2 area rate is taken from the data file rather than hard coded.</summary>
        [Fact]
        public void Paragraph1_24a_AreaRateIsReadFromTheDataFile()
        {
            PartFData partFData = DataFile();
            partFData.AreaRate_LpsPerM2 = 1.0;

            Calculate(partFData, out PartFCalculator partFCalculator,
                ("Bedroom", 10, 25),
                ("Living Room", 40, 100),
                ("Kitchen", 10, 25));

            Assert.Equal(60, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) paragraph 1.24a, page 10, counts internal floor area. Void and
        /// open-to-below spaces carry IncludeInFloorAreaCheck = false and are excluded.
        /// </summary>
        [Fact]
        public void Paragraph1_24a_VoidSpaces_AreExcludedFromTheFloorArea()
        {
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom", 10, 25),
                ("Living Room", 40, 100),
                ("Kitchen", 10, 25),
                ("Void", 200, 500));

            // 60 m2 x 0.3 = 18 l/s, below the one bedroom rate of 19 l/s. Including the 200 m2
            // void would have produced 78 l/s.
            Assert.Equal(19, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
        }

        // ------------------------------------------------------------------
        // Paragraph 1.67 (page 16) - supply distributed by habitable room volume
        // ------------------------------------------------------------------

        /// <summary>
        /// ADF F Vol 1 (2021) paragraph 1.67, page 16: the total supply air flow is distributed
        /// proportionately to the volume of each habitable room.
        /// </summary>
        [Fact]
        public void Paragraph1_67_SupplyIsDistributedInProportionToHabitableRoomVolume()
        {
            Dictionary<string, double> rates = Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom", 10, 25),
                ("Living Room", 30, 75),
                ("Kitchen", 10, 25),
                ("Bathroom", 5, 12.5));

            double finalSystemRate = partFCalculator.FinalSystemRate_Lps!.Value;

            Assert.Equal(finalSystemRate * 25.0 / 100.0, rates["Bedroom"], tolerance);
            Assert.Equal(finalSystemRate * 75.0 / 100.0, rates["Living Room"], tolerance);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) paragraphs 1.69 and 1.70: total supply and total extract are both
        /// balanced to the whole dwelling ventilation rate.
        /// </summary>
        [Fact]
        public void Paragraphs1_69And1_70_TotalSupplyAndTotalExtract_BothEqualTheSystemRate()
        {
            Dictionary<string, double> rates = Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom 1", 12, 30),
                ("Bedroom 2", 12, 30),
                ("Living Room", 26, 65),
                ("Kitchen", 12, 30),
                ("Bathroom", 6, 15),
                ("WC", 2, 5),
                ("Hall", 10, 25));

            double finalSystemRate = partFCalculator.FinalSystemRate_Lps!.Value;

            double supply = rates["Bedroom 1"] + rates["Bedroom 2"] + rates["Living Room"];
            double extract = rates["Kitchen"] + rates["Bathroom"] + rates["WC"];

            Assert.Equal(finalSystemRate, supply, tolerance);
            Assert.Equal(finalSystemRate, extract, tolerance);
        }

        /// <summary>Transfer spaces have no terminal and carry no flow rate.</summary>
        [Fact]
        public void TransferSpaces_CarryNoFlowRate()
        {
            Dictionary<string, double> rates = Calculate(DataFile(), out _,
                ("Bedroom", 10, 25),
                ("Living Room", 30, 75),
                ("Kitchen", 10, 25),
                ("Hall", 8, 20),
                ("Void", 5, 12.5));

            Assert.Equal(0, rates["Hall"], tolerance);
            Assert.Equal(0, rates["Void"], tolerance);
        }

        // ------------------------------------------------------------------
        // Regressions on the extract distribution
        // ------------------------------------------------------------------

        /// <summary>
        /// Regression: an extract terminal that does not take a share of the balance
        /// (ScaleExtractAboveMinimum = false) must still be given its Table 1.2 minimum on the continuous
        /// rate wherever the whole dwelling rate can carry every room's minimum. It previously received
        /// no flow rate at all while its minimum was still deducted from the balance shared out to the
        /// other wet rooms.
        /// </summary>
        [Fact]
        public void Table1_2_NonScalingExtractTerminal_StillReceivesItsMinimum()
        {
            PartFData partFData = CustomData();
            partFData.PartFCategories["Fixed"] = new PartFCategory("Fixed", PartFType.WetRoom, PartFVentilationType.extract,
                false, 6, true, true, false, false, "RoomVolume", ["fixed"]);

            //90 m2 gives a floor area rate of 27 l/s, comfortably above the 6 l/s minimum of the fixed
            //terminal, so this exercises the minimum-first path rather than the deficit path.
            Dictionary<string, double> rates = Calculate(partFData, out PartFCalculator partFCalculator,
                ("Sleeping", 60, 150),
                ("Extract", 20, 50),
                ("Fixed", 10, 25));

            Assert.Equal(6, rates["Fixed"], tolerance);
            Assert.Equal(partFCalculator.FinalSystemRate_Lps!.Value - 6, rates["Extract"], tolerance);
        }

        /// <summary>
        /// The mirror of the test above: where the whole dwelling rate is BELOW the total of the Table 1.2
        /// per-room minimums, no room can be held at its minimum on the continuous rate, and none is. The
        /// continuous total is the whole dwelling rate, shared in proportion to each room's minimum, and
        /// every room reaches its own figure by boosting to its high rate.
        /// </summary>
        [Fact]
        public void Table1_2_BelowTheMinimumTotal_SharesTheWholeDwellingRateAndBoosts()
        {
            PartFData partFData = CustomData();
            partFData.PartFCategories["Fixed"] = new PartFCategory("Fixed", PartFType.WetRoom, PartFVentilationType.extract,
                false, 6, true, true, false, false, "RoomVolume", ["fixed"]);

            //A 20 m2 dwelling with one habitable room: the floor area rate is 6 l/s and Table 1.3 note 1
            //gives 13 l/s, so the whole dwelling rate is 13 l/s - against per-room minimums totalling
            //8 + 8 + 6 = 22 l/s.
            Dictionary<string, double> rates = Calculate(partFData, out PartFCalculator partFCalculator,
                ("Sleeping", 10, 25),
                ("Extract 1", 4, 10),
                ("Extract 2", 3, 7.5),
                ("Fixed", 3, 7.5));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(22, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(13, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);

            //The whole dwelling rate is shared in proportion to each room's Table 1.2 minimum. A terminal
            //that does not scale above its minimum is still included here: there is no surplus to withhold
            //from it, and leaving it out would put the shortfall on the other rooms.
            Assert.Equal(13 * 6 / 22.0, rates["Fixed"], tolerance);
            Assert.Equal(13 * 8 / 22.0, rates["Extract 1"], tolerance);
            Assert.Equal(13 * 8 / 22.0, rates["Extract 2"], tolerance);
            Assert.Equal(13, dwellingResult.TotalExtract_Lps, tolerance);

            //Every room reaches its own Table 1.2 minimum at the high rate instead.
            Assert.Equal(22, dwellingResult.TotalHighExtract_Lps, tolerance);
            Assert.All(
                dwellingResult.ComplianceResult.Terminals.Where(x => x.IsExtract),
                x => Assert.True(x.HighRateIncreaseRequired));

            Assert.Contains(dwellingResult.Remarks, x => x.Contains("is above the whole dwelling ventilation rate"));
        }

        /// <summary>
        /// Regression: an extract terminal with no Table 1.2 minimum must receive its share of the
        /// balance. Operator precedence in the previous expression collapsed the whole result to
        /// zero whenever the minimum was null.
        /// </summary>
        [Fact]
        public void ExtractTerminalWithoutAMinimum_StillReceivesItsShareOfTheBalance()
        {
            PartFData partFData = CustomData();
            partFData.PartFCategories["NoMinimum"] = new PartFCategory("NoMinimum", PartFType.WetRoom, PartFVentilationType.extract,
                false, null, true, true, false, true, "RoomVolume", ["nominimum"]);

            Dictionary<string, double> rates = Calculate(partFData, out PartFCalculator partFCalculator,
                ("Sleeping", 20, 50),
                ("NoMinimum", 10, 25));

            Assert.Equal(partFCalculator.FinalSystemRate_Lps!.Value, rates["NoMinimum"], tolerance);
            Assert.True(rates["NoMinimum"] > 0);
        }

        // ------------------------------------------------------------------
        // Studio dwellings
        // ------------------------------------------------------------------

        /// <summary>
        /// A studio combines sleeping, living and cooking in one room, so it is counted as one
        /// bedroom for ADF F Vol 1 (2021) Table 1.3 (page 10) and takes supply as a habitable room
        /// under paragraph 1.67 (page 16).
        /// </summary>
        [Fact]
        public void Table1_3_Studio_CountsAsOneBedroomAndIsHabitable()
        {
            PartFCategory? partFCategory = DataFile().GetPartFCategory("Studio");

            Assert.NotNull(partFCategory);
            Assert.Equal("Studio", partFCategory!.Name);
            Assert.True(partFCategory.IsBedroom);
            Assert.True(partFCategory.IsCookingSpace);
            Assert.Equal(PartFType.Habitable, partFCategory.PartFType);
            Assert.Equal(PartFVentilationType.supply, partFCategory.PartFVentilationType);
            Assert.True(partFCategory.IncludeInFloorAreaCheck);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) Table 1.3 note 1 (page 10): a studio flat has exactly ONE habitable room -
        /// the shower room is not habitable - so it is sized from the 13 l/s note 1 rate, not the one
        /// bedroom figure of 19 l/s. The studio still counts as one bedroom, and no missing-bedroom
        /// warning is raised.
        /// </summary>
        [Fact]
        public void Table1_3Note1_StudioFlat_IsSizedFromTheOneHabitableRoomRate()
        {
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Studio", 30, 75),
                ("Shower Room", 5, 12.5));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(1, dwellingResult.BedroomCount);
            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.True(dwellingResult.OneHabitableRoomRuleApplied);
            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);

            //The note 1 rate is 13 l/s and the floor-area rate is 35 x 0.3 = 10.5 l/s, so the dwelling is
            //sized at 13 l/s. The two rooms' Table 1.2 minimum HIGH rates total 21 l/s - the studio's own
            //local kitchen extract carries the 13 l/s kitchen figure and the shower room 8 l/s - and that
            //total is reported without raising the continuous rate, because Table 1.2's per-room figures
            //apply room by room at the high condition and not to the continuous dwelling total.
            Assert.Equal(21, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(13, dwellingResult.FinalSystemRate_Lps, tolerance);
            Assert.Equal(21, dwellingResult.TotalHighExtract_Lps, tolerance);

            Assert.DoesNotContain(dwellingResult.Warnings, x => x.Contains("sized at the one bedroom rate"));
        }

        /// <summary>
        /// ADF F Vol 1 (2021) paragraph 1.17a (page 8) and Table 1.2 (page 10): a studio holds the cooking
        /// function, so it takes a LOCAL KITCHEN EXTRACT terminal of its own carrying the Table 1.2 kitchen
        /// rate, in addition to the supply terminal paragraph 1.67 requires of it as a habitable room.
        /// <para>
        /// This replaces the previous behaviour, where SAM could assign only one terminal role per space
        /// and had to report the missing local kitchen extract as an ENGINEERING CHECK REQUIRED warning.
        /// The shower room's extract is still general extract and still does not satisfy paragraph 1.17a.
        /// </para>
        /// </summary>
        [Fact]
        public void Paragraph1_17a_StudioWithoutSeparateKitchen_TakesItsOwnLocalKitchenExtractTerminal()
        {
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Studio", 30, 75),
                ("Shower Room", 5, 12.5));

            PartFComplianceResult complianceResult = Assert.Single(partFCalculator.DwellingResults).ComplianceResult;

            PartFVentilationTerminalRequirement terminal = Assert.Single(complianceResult.LocalKitchenExtractTerminals);

            Assert.Equal("Studio", terminal.SpaceName);
            Assert.True(terminal.IsLocalExtract);
            Assert.True(terminal.IsInBalancedFlow);
            Assert.Equal(13, terminal.MinimumRequiredFlowRate_Lps.Value, tolerance);

            //The studio also keeps its supply role: the two requirements are independent.
            Assert.Contains(complianceResult.SupplyTerminals, x => x.SpaceName == "Studio");

            //And the shower room's extract is general extract, held separately.
            Assert.Contains(complianceResult.GeneralExtractTerminals, x => x.SpaceName == "Shower Room");

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("ENGINEERING CHECK REQUIRED"));
        }

        /// <summary>
        /// The ENGINEERING CHECK REQUIRED warning is still raised, but now only where the design actually
        /// says there is no local kitchen extract, rather than because SAM could not represent one.
        /// </summary>
        [Fact]
        public void Paragraph1_17a_StudioWithNoLocalExtractRepresented_IsReportedAsAFailure()
        {
            PartFModel partFModel = new PartFModel()
                .Space("Studio", 30, 75)
                .Space("Shower Room", 5, 12.5)
                .LocalExtractMethod("Studio", PartFExtractMethod.NotRepresented);

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = partFModel.AdjacencyCluster };
            Assert.True(partFCalculator.Calculate());

            Assert.Contains(partFCalculator.Warnings, x =>
                x.Contains("ENGINEERING CHECK REQUIRED") &&
                x.Contains("Studio") &&
                x.Contains("13 l/s"));

            PartFComplianceResult complianceResult = Assert.Single(partFCalculator.DwellingResults).ComplianceResult;

            Assert.Equal(PartFComplianceStatus.Fail, Assert.Single(complianceResult.LocalKitchenExtractTerminals).ComplianceStatus);
            Assert.Equal(PartFOverallStatus.Fail, complianceResult.OverallStatus);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) Diagram 1.2 note 1 (page 9): "A recirculating cooker hood on its own does not
        /// provide a means of ventilation that complies with Part F of the Building Regulations." It is
        /// therefore never accepted as external extract and contributes to no design flow.
        /// </summary>
        [Fact]
        public void Diagram1_2_RecirculatingCookerHood_IsNotAcceptedAsExternalExtract()
        {
            PartFModel partFModel = new PartFModel()
                .Space("Studio", 30, 75)
                .Space("Shower Room", 5, 12.5)
                .LocalExtractMethod("Studio", PartFExtractMethod.RecirculatingCookerHood);

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = partFModel.AdjacencyCluster };
            Assert.True(partFCalculator.Calculate());

            PartFComplianceResult complianceResult = Assert.Single(partFCalculator.DwellingResults).ComplianceResult;

            PartFVentilationTerminalRequirement terminal = Assert.Single(complianceResult.LocalKitchenExtractTerminals);

            Assert.Equal(PartFComplianceStatus.Fail, terminal.ComplianceStatus);
            Assert.False(terminal.IsInBalancedFlow);
            Assert.Null(terminal.ContinuousDesignFlowRate_Lps);

            Assert.Equal(PartFOverallStatus.Fail, complianceResult.OverallStatus);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) Table 1.1 and Diagram 1.1 (pages 8 and 9): a cooker hood extracting to the
        /// outside is assessed at 30 l/s intermittent. It runs intermittently, so it stays outside the
        /// balanced continuous flow and the dwelling's continuous extract has to come from elsewhere.
        /// </summary>
        [Fact]
        public void Table1_1_CookerHoodExtractingOutside_IsAssessedIntermittentlyAndOutsideTheBalance()
        {
            PartFModel partFModel = new PartFModel()
                .Space("Studio", 30, 75)
                .Space("Shower Room", 5, 12.5)
                .LocalExtractMethod("Studio", PartFExtractMethod.CookerHoodExtractingOutside);

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = partFModel.AdjacencyCluster };
            Assert.True(partFCalculator.Calculate());

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            PartFVentilationTerminalRequirement terminal = Assert.Single(dwellingResult.ComplianceResult.LocalKitchenExtractTerminals);

            Assert.Equal(30, terminal.HighFlowRate_Lps.Value, tolerance);
            Assert.False(terminal.IsInBalancedFlow);
            Assert.Null(terminal.ContinuousDesignFlowRate_Lps);

            //Only the shower room's 8 l/s is in the continuous minimum total now.
            Assert.Equal(8, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(30, dwellingResult.TotalIntermittentExtract_Lps, tolerance);
        }

        /// <summary>
        /// ADF F Vol 1 (2021) paragraph 1.17 (page 8): a dwelling with no cooking space at all is
        /// reported. This implements the MissingCookingSpace rule declared in the rule set.
        /// </summary>
        [Fact]
        public void Paragraph1_17_DwellingWithNoCookingSpace_IsReported()
        {
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom", 12, 30),
                ("Living Room", 20, 50),
                ("Bathroom", 5, 12.5));

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("contains no kitchen"));
        }

        /// <summary>A separate kitchen satisfies paragraph 1.17, so no cooking warning is raised.</summary>
        [Fact]
        public void Paragraph1_17_DwellingWithASeparateKitchen_RaisesNoCookingWarning()
        {
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom", 12, 30),
                ("Living Room", 20, 50),
                ("Kitchen", 10, 25),
                ("Bathroom", 5, 12.5));

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("contains no kitchen"));
            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("13 l/s"));
        }

        // ------------------------------------------------------------------
        // Unsupported, missing and invalid inputs
        // ------------------------------------------------------------------

        /// <summary>
        /// A space whose name matches no Part F room category is reported and excluded from the
        /// dwelling, rather than being silently dropped.
        /// </summary>
        [Fact]
        public void UnsupportedRoomType_IsReportedAndExcludedFromTheDwelling()
        {
            AdjacencyCluster adjacencyCluster = Cluster(
                ("Bedroom", 10, 25),
                ("Living Room", 30, 75),
                ("Kitchen", 10, 25),
                ("Zzqx", 500, 1250));

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };
            Assert.True(partFCalculator.Calculate());

            Assert.Equal("Zzqx", Assert.Single(partFCalculator.UnclassifiedSpaceNames));
            Assert.Contains(partFCalculator.Warnings, x => x.Contains("Zzqx"));

            // Excluded from the floor area check: 50 m2 x 0.3 = 15 l/s, so the one bedroom rate governs.
            Assert.Equal(19, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);

            Space? space = partFCalculator.AdjacencyCluster.GetSpaces().Find(x => x.Name == "Zzqx");
            Assert.NotNull(space);
            Assert.Null(space!.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData));
        }

        /// <summary>Missing input: no rate table means no bedroom based minimum, reported as a warning.</summary>
        [Fact]
        public void MissingRateTable_FallsBackToTheAreaRateAndWarns()
        {
            PartFData partFData = DataFile();
            partFData.WholeDwellingRates_Lps = [];

            Calculate(partFData, out PartFCalculator partFCalculator,
                ("Bedroom", 10, 25),
                ("Living Room", 30, 75),
                ("Kitchen", 10, 25));

            // 50 m2 x 0.3 = 15 l/s, but the kitchen minimum of 13 l/s does not exceed it.
            Assert.Equal(15, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
            Assert.Contains(partFCalculator.Warnings, x => x.Contains("Table 1.3"));
        }

        /// <summary>Invalid input: spaces without an Area parameter must not silently under-size the dwelling.</summary>
        [Fact]
        public void MissingFloorArea_IsReportedAsAWarning()
        {
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom", 0, 25),
                ("Living Room", 0, 75),
                ("Kitchen", 0, 25));

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("internal floor area"));
            Assert.Equal(19, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
        }

        /// <summary>Invalid input: zero room volumes must not produce NaN or infinite flow rates.</summary>
        [Fact]
        public void MissingRoomVolumes_ProduceFiniteFlowRatesAndAWarning()
        {
            Dictionary<string, double> rates = Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Bedroom", 10, 0),
                ("Living Room", 30, 0),
                ("Kitchen", 10, 0),
                ("Bathroom", 5, 0));

            foreach (double rate in rates.Values)
            {
                Assert.False(double.IsNaN(rate));
                Assert.False(double.IsInfinity(rate));
            }

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("paragraph 1.67"));

            // The whole dwelling rate of 19 l/s is below the 21 l/s total of the two rooms' Table 1.2
            // minimum high rates, so it is shared in proportion to them and each room reaches its own
            // figure by boosting. Zero volumes must not disturb that: the deficit split is weighted on the
            // minimums, not on volume.
            Assert.Equal(19 * 13 / 21.0, rates["Kitchen"], tolerance);
            Assert.Equal(19 * 8 / 21.0, rates["Bathroom"], tolerance);

            PartFComplianceResult complianceResult = Assert.Single(partFCalculator.DwellingResults).ComplianceResult;

            Assert.Equal(13, Assert.Single(complianceResult.LocalKitchenExtractTerminals).HighFlowRate_Lps!.Value, tolerance);
            Assert.Equal(8, Assert.Single(complianceResult.GeneralExtractTerminals).HighFlowRate_Lps!.Value, tolerance);
        }

        /// <summary>
        /// A dwelling with no bedroom but exactly one habitable room takes the Table 1.3 note 1 rate of
        /// 13 l/s. Note 1 keys off the habitable room count, so it applies here even though the bedroom
        /// count is zero, and the no-bedroom warning is therefore not raised: the rate did not fall back
        /// to the bottom of Table 1.3.
        /// </summary>
        [Fact]
        public void NoBedroomButOneHabitableRoom_UsesTheOneHabitableRoomRate()
        {
            // 25 m2 x 0.3 = 7.5 l/s and a single 6 l/s wet room minimum, so the note 1 rate governs.
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Living Room", 20, 50),
                ("WC", 5, 12.5));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(0, dwellingResult.BedroomCount);
            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.True(dwellingResult.OneHabitableRoomRuleApplied);
            Assert.Equal(13, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("sized at the one bedroom rate"));
        }

        /// <summary>
        /// A dwelling with no bedroom AND more than one habitable room falls back to the bottom of
        /// Table 1.3 (19 l/s) and is flagged, because note 1 does not apply.
        /// </summary>
        [Fact]
        public void NoBedroomAndSeveralHabitableRooms_IsSizedAsOneBedroomAndWarns()
        {
            Calculate(DataFile(), out PartFCalculator partFCalculator,
                ("Living Room", 20, 50),
                ("Study", 10, 25),
                ("WC", 5, 12.5));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(0, dwellingResult.BedroomCount);
            Assert.Equal(2, dwellingResult.HabitableRoomCount);
            Assert.False(dwellingResult.OneHabitableRoomRuleApplied);
            Assert.Equal(19, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
            Assert.Contains(partFCalculator.Warnings, x => x.Contains("sized at the one bedroom rate"));
        }

        /// <summary>Missing input: without an AdjacencyCluster or a rule set the calculation does not run.</summary>
        [Fact]
        public void MissingInputs_ReturnFalse()
        {
            Assert.False(new PartFCalculator(DataFile()).Calculate());
            Assert.False(new PartFCalculator(null) { AdjacencyCluster = Cluster(("Bedroom", 10, 25)) }.Calculate());
        }

        /// <summary>The supplied model is not modified; the calculation returns a new cluster.</summary>
        [Fact]
        public void Calculate_DoesNotModifyTheSuppliedModel()
        {
            AdjacencyCluster adjacencyCluster = Cluster(("Bedroom", 10, 25), ("Kitchen", 10, 25));

            PartFCalculator partFCalculator = new(DataFile()) { AdjacencyCluster = adjacencyCluster };
            Assert.True(partFCalculator.Calculate());

            Assert.NotSame(adjacencyCluster, partFCalculator.AdjacencyCluster);
            Assert.All(adjacencyCluster.GetSpaces(), x => Assert.Null(x.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)));
            Assert.All(partFCalculator.AdjacencyCluster.GetSpaces(), x => Assert.NotNull(x.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static PartFData DataFile()
        {
            return Analytical.Create.PartFData(Fixtures.GetPath(dataFileName));
        }

        /// <summary>A minimal two category rule set used to exercise paths the shipped data file does not reach.</summary>
        private static PartFData CustomData()
        {
            return new PartFData
            {
                WholeDwellingRates_Lps = new Dictionary<int, double> { { 1, 19 }, { 2, 25 } },
                AreaRate_LpsPerM2 = 0.3,
                IncrementAbove5 = 6,
                PartFCategories = new Dictionary<string, PartFCategory>
                {
                    ["Sleeping"] = new PartFCategory("Sleeping", PartFType.Habitable, PartFVentilationType.supply,
                        true, null, true, true, true, false, "RoomVolume", ["sleeping"]),
                    ["Extract"] = new PartFCategory("Extract", PartFType.WetRoom, PartFVentilationType.extract,
                        false, 8, true, true, false, true, "RoomVolume", ["extract"]),
                },
            };
        }

        private static AdjacencyCluster Cluster(params (string Name, double Area_M2, double Volume_M3)[] spaces)
        {
            AdjacencyCluster result = new();

            for (int i = 0; i < spaces.Length; i++)
            {
                Space space = new(spaces[i].Name, new Point3D(i * 10, 0, 1.5));
                space.SetValue(SpaceParameter.Area, spaces[i].Area_M2);
                space.SetValue(SpaceParameter.Volume, spaces[i].Volume_M3);
                result.AddObject(space);
            }

            return result;
        }

        private static Dictionary<string, double> Calculate(PartFData partFData, out PartFCalculator partFCalculator, params (string Name, double Area_M2, double Volume_M3)[] spaces)
        {
            partFCalculator = new PartFCalculator(partFData) { AdjacencyCluster = Cluster(spaces) };

            Assert.True(partFCalculator.Calculate());

            return partFCalculator.AdjacencyCluster.GetSpaces().ToDictionary(
                x => x.Name,
                x => x.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)?.CalculatedFlowRate_Lps ?? 0);
        }
    }
}
