// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Tests for the SAM design convention covering studios and open plan living kitchens under
    /// Approved Document F, Volume 1: Dwellings (2021 edition, for use in England).
    /// </summary>
    /// <remarks>
    /// Both rooms are habitable under Appendix A, because neither is <i>solely</i> a kitchen, so
    /// paragraph 1.67 requires mechanical supply to them; and both contain the cooking function, so
    /// paragraph 1.17a and Table 1.2 require kitchen extract from them as well. Both requirements now
    /// produce a terminal on the same space, and these tests lock that.
    /// <para>
    /// Earlier versions of the calculation could assign only one terminal role per space, so both rooms
    /// took the supply role alone and their kitchen extract was raised as an ENGINEERING CHECK REQUIRED
    /// warning rather than modelled. That limitation, and the tests that locked it, are superseded.
    /// </para>
    /// </remarks>
    public class PartFStudioTests
    {
        private const string dataFileName = "SAM_PartFSpaceRulesUKDwellingsMVHR.json";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // Intended supply / extract arrangement
        // ------------------------------------------------------------------

        /// <summary>
        /// The intended arrangement: studio, living kitchen, bedroom and living room take mechanical
        /// supply; bathroom, ensuite, utility room and sanitary accommodation take mechanical extract.
        /// </summary>
        [Theory]
        [InlineData("Studio", PartFVentilationType.supply)]
        [InlineData("Living Kitchen", PartFVentilationType.supply)]
        [InlineData("Bedroom", PartFVentilationType.supply)]
        [InlineData("Living Room", PartFVentilationType.supply)]
        [InlineData("Bathroom", PartFVentilationType.extract)]
        [InlineData("Ensuite", PartFVentilationType.extract)]
        [InlineData("Utility Room", PartFVentilationType.extract)]
        [InlineData("WC", PartFVentilationType.extract)]
        public void IntendedArrangement_AssignsTheExpectedTerminalRole(string name, PartFVentilationType expected)
        {
            PartFCategory partFCategory = DataFile().GetPartFCategory(name);

            Assert.NotNull(partFCategory);
            Assert.Equal(expected, partFCategory.PartFVentilationType);
        }

        /// <summary>
        /// A studio and a living kitchen are habitable rooms, not wet rooms, so they carry no Table 1.2
        /// minimum extract rate of their own.
        /// </summary>
        [Theory]
        [InlineData("Studio")]
        [InlineData("Living Kitchen")]
        public void StudioAndLivingKitchen_AreHabitableWithNoExtractMinimum(string name)
        {
            PartFCategory partFCategory = DataFile().GetPartFCategory(name);

            Assert.NotNull(partFCategory);
            Assert.Equal(PartFType.Habitable, partFCategory.PartFType);
            Assert.True(partFCategory.IsCookingSpace);
            Assert.True(partFCategory.MinFlowRate_Lps is null || partFCategory.MinFlowRate_Lps.Value == 0);
        }

        /// <summary>
        /// A living kitchen receives supply as a habitable room AND its own local kitchen extract. The
        /// bathroom's extract is general extract, held separately, and the two together make up the
        /// dwelling's continuous extract.
        /// </summary>
        [Fact]
        public void LivingKitchen_ReceivesSupplyAndItsOwnLocalKitchenExtract()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Living Kitchen", 30, 75),
                ("Bedroom 1", 14, 35),
                ("Bathroom", 6, 15));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);
            PartFComplianceResult complianceResult = dwellingResult.ComplianceResult;

            Assert.True(Rate(partFCalculator, "Living Kitchen") > 0);

            Assert.Contains(complianceResult.SupplyTerminals, x => x.SpaceName == "Living Kitchen");
            Assert.Contains(complianceResult.LocalKitchenExtractTerminals, x => x.SpaceName == "Living Kitchen");
            Assert.Contains(complianceResult.GeneralExtractTerminals, x => x.SpaceName == "Bathroom");

            //Supply and extract each equal the design system rate.
            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps, dwellingResult.TotalSupply_Lps, tolerance);
            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps, dwellingResult.TotalExtract_Lps, tolerance);

            //Extract is now shared between the living kitchen and the bathroom rather than carried by the
            //bathroom alone.
            Assert.Equal(
                dwellingResult.TotalExtract_Lps,
                complianceResult.LocalKitchenExtractTerminals.Sum(x => x.ContinuousDesignFlowRate_Lps!.Value) + Rate(partFCalculator, "Bathroom"),
                tolerance);
        }

        /// <summary>
        /// A studio receives supply as a habitable room and its own local kitchen extract, with the
        /// bathroom providing the general extract. Every Table 1.2 minimum is met AT THE HIGH RATE, which
        /// is the condition Table 1.2's per-room figures apply to, and the totals balance.
        /// <para>
        /// The dwelling here is 46 m2, so the whole dwelling rate is 13.8 l/s, below the 21 l/s total of
        /// the two rooms' Table 1.2 minimum high rates. That is a normal outcome, not a failure: nothing
        /// in the Approved Document requires the continuous dwelling rate to reach the sum of the
        /// per-room high-rate minimums.
        /// </para>
        /// </summary>
        [Fact]
        public void Studio_ReceivesSupplyAndItsOwnLocalKitchenExtract()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 40, 100),
                ("Bathroom", 6, 15));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);
            PartFComplianceResult complianceResult = dwellingResult.ComplianceResult;

            //The studio takes the whole dwelling supply, being the only habitable room.
            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps, Rate(partFCalculator, "Studio"), tolerance);

            PartFVentilationTerminalRequirement terminal_Kitchen = Assert.Single(complianceResult.LocalKitchenExtractTerminals);
            PartFVentilationTerminalRequirement terminal_Bathroom = Assert.Single(complianceResult.GeneralExtractTerminals);

            Assert.Equal("Studio", terminal_Kitchen.SpaceName);

            //Each room reaches its own Table 1.2 minimum at the high rate, by boosting.
            Assert.Equal(13, terminal_Kitchen.HighFlowRate_Lps!.Value, tolerance);
            Assert.Equal(8, terminal_Bathroom.HighFlowRate_Lps!.Value, tolerance);
            Assert.True(terminal_Kitchen.HighRateIncreaseRequired);
            Assert.True(terminal_Bathroom.HighRateIncreaseRequired);

            //The continuous total is the whole dwelling rate and is not raised to the sum of those two.
            Assert.Equal(13.8, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(21, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps, dwellingResult.TotalExtract_Lps, tolerance);

            Assert.Equal(dwellingResult.TotalSupply_Lps, dwellingResult.TotalExtract_Lps, tolerance);
        }

        // ------------------------------------------------------------------
        // Habitable-room count, bedroom count and the whole dwelling rate
        // ------------------------------------------------------------------

        /// <summary>A studio counts as one bedroom.</summary>
        [Fact]
        public void Studio_CountsAsOneBedroom()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10));

            Assert.Equal(1, Assert.Single(partFCalculator.DwellingResults).BedroomCount);
        }

        /// <summary>
        /// A studio counts as one bedroom equivalent AND is one habitable room, so a studio flat sizes
        /// from Table 1.3 note 1 rather than the one bedroom figure of Table 1.3.
        /// </summary>
        [Fact]
        public void Studio_IsOneHabitableRoomAndOneBedroomEquivalent()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.Equal(1, dwellingResult.BedroomCount);
            Assert.True(dwellingResult.OneHabitableRoomRuleApplied);
        }

        /// <summary>
        /// The previous SAM convention of a fixed 19 l/s studio minimum is removed: the rate set by the
        /// dwelling's rooms is the Table 1.3 note 1 figure of 13 l/s, not 19 l/s, and that is also the
        /// continuous design rate here because the 24 m2 floor area rate of 7.2 l/s is lower.
        /// </summary>
        [Fact]
        public void Studio_NoLongerUsesTheFixedNineteenLitrePerSecondMinimum()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
            Assert.NotEqual(19, dwellingResult.BedroomOrHabitableRate_Lps);

            Assert.Equal(13, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
        }

        /// <summary>
        /// Adding a second habitable room takes the dwelling out of Table 1.3 note 1 and back onto the
        /// bedroom table, where a studio counts as one bedroom: 19 l/s.
        /// </summary>
        [Theory]
        [InlineData("Living Room")]
        [InlineData("Study")]
        public void StudioPlusASecondHabitableRoom_ReturnsToTheBedroomTable(string name_Second)
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 20, 50),
                (name_Second, 10, 25),
                ("Bathroom", 4, 10));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(2, dwellingResult.HabitableRoomCount);
            Assert.False(dwellingResult.OneHabitableRoomRuleApplied);
            Assert.Equal(19, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);

            //The Table 1.3 one bedroom rate of 19 l/s governs, above the floor area rate of
            //0.3 x 34 = 10.2 l/s. The 21 l/s total of the Table 1.2 per-room minimum HIGH rates is
            //reported but does not raise it.
            Assert.Equal(19, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(21, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
        }

        /// <summary>
        /// The floor area rate of paragraph 1.24a governs where it exceeds the note 1 rate:
        /// 0.3 x 100 m2 = 30 l/s beats 13 l/s.
        /// </summary>
        [Fact]
        public void Studio_FloorAreaRateGovernsAboveTheNote1Rate()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 96, 240),
                ("Bathroom", 4, 10));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(100, dwellingResult.InternalFloorArea_M2, tolerance);
            Assert.Equal(30, dwellingResult.AreaBasedRate_Lps, tolerance);
            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
            Assert.Equal(30, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
        }

        /// <summary>
        /// The total of the Table 1.2 per-room minimum HIGH rates does NOT govern the continuous design
        /// rate, even where it is far above it. Here studio kitchen 13 + bathroom 8 + WC 6 + utility 8 =
        /// 35 l/s against a whole dwelling rate of 13 l/s, and the continuous rate stays at 13 l/s.
        /// <para>
        /// Table 1.2 requires the TOTAL of continuous extract to reach the whole dwelling rate, and EACH
        /// room to reach its own figure at the HIGH rate. Summing the per-room high-rate minimums into
        /// the continuous rate would size this small flat's normal continuous operation at nearly three
        /// times what the Approved Document asks of it.
        /// </para>
        /// </summary>
        [Fact]
        public void WetRoomHighRateMinimums_DoNotGovernTheContinuousRate()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10),
                ("WC", 2, 5),
                ("Utility Room", 4, 10));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(35, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
            Assert.True(dwellingResult.AreaBasedRate_Lps < 13);

            Assert.Equal(13, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(13, dwellingResult.TotalExtract_Lps, tolerance);

            //Every room still reaches its own Table 1.2 minimum, by boosting to its high rate.
            List<PartFVentilationTerminalRequirement> terminals = [.. dwellingResult.ComplianceResult.Terminals.Where(x => x.IsExtract)];

            Assert.Equal(4, terminals.Count);
            Assert.All(terminals, x => Assert.Equal(x.MinimumRequiredFlowRate_Lps!.Value, x.HighFlowRate_Lps!.Value, tolerance));
            Assert.All(terminals, x => Assert.True(x.HighRateIncreaseRequired));
            Assert.Equal(35, dwellingResult.TotalHighExtract_Lps, tolerance);
        }

        /// <summary>
        /// The governing continuous design rate is the greater of the bedroom or one-habitable-room rate
        /// and the paragraph 1.24a floor area rate, whichever that happens to be - and nothing else.
        /// </summary>
        [Fact]
        public void ContinuousDesignRate_IsTheGreaterOfTheTwoWholeDwellingRates()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 96, 240),
                ("Bathroom", 4, 10),
                ("WC", 2, 5));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            double expected = System.Math.Max(dwellingResult.BedroomOrHabitableRate_Lps, dwellingResult.AreaBasedRate_Lps);

            Assert.Equal(expected, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(dwellingResult.WholeDwellingRate_Lps, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
        }

        // ------------------------------------------------------------------
        // LOCAL kitchen extract as a terminal of its own
        // ------------------------------------------------------------------
        //
        // Local kitchen extract and general wet-room extract are different terminal roles with different
        // source paragraphs. Extract from a bathroom or ensuite may balance the dwelling airflow, but it
        // is not local kitchen extract and never satisfies paragraph 1.17a. What has changed is that the
        // cooking space's own extract is now modelled rather than reported as unrepresentable.

        /// <summary>
        /// A studio with a separate bathroom: supply and extract balance, and the studio now carries its
        /// own local kitchen extract terminal, held separately from the bathroom's general extract.
        /// </summary>
        [Fact]
        public void StudioPlusBathroom_BalancesAndCarriesItsOwnLocalKitchenExtract()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 40, 100),
                ("Bathroom", 6, 15));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            //Balanced at both conditions.
            Assert.True(dwellingResult.TotalExtract_Lps > 0);
            Assert.Equal(dwellingResult.TotalSupply_Lps, dwellingResult.TotalExtract_Lps, tolerance);
            Assert.Equal(dwellingResult.TotalSetbackSupply_Lps, dwellingResult.TotalSetbackExtract_Lps, tolerance);

            //The two extract roles are separate and both present.
            Assert.Contains(dwellingResult.ComplianceResult.LocalKitchenExtractTerminals, x => x.SpaceName == "Studio");
            Assert.Contains(dwellingResult.ComplianceResult.GeneralExtractTerminals, x => x.SpaceName == "Bathroom");

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("ENGINEERING CHECK REQUIRED"));
        }

        /// <summary>The same applies to an open plan living kitchen with an ensuite.</summary>
        [Fact]
        public void LivingKitchenPlusEnsuite_BalancesAndCarriesItsOwnLocalKitchenExtract()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Living Kitchen", 30, 75),
                ("Bedroom 1", 14, 35),
                ("Ensuite", 5, 12.5));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.True(dwellingResult.TotalExtract_Lps > 0);
            Assert.Equal(dwellingResult.TotalSupply_Lps, dwellingResult.TotalExtract_Lps, tolerance);
            Assert.Equal(dwellingResult.TotalSetbackSupply_Lps, dwellingResult.TotalSetbackExtract_Lps, tolerance);

            Assert.Contains(dwellingResult.ComplianceResult.LocalKitchenExtractTerminals, x => x.SpaceName == "Living Kitchen");
            Assert.Contains(dwellingResult.ComplianceResult.GeneralExtractTerminals, x => x.SpaceName == "Ensuite");

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("ENGINEERING CHECK REQUIRED"));
        }

        /// <summary>
        /// The warning must NOT claim the dwelling has no extract terminal when wet room extract terminals
        /// exist. That was the defect in the previous wording.
        /// </summary>
        [Fact]
        public void LocalKitchenExtractWarning_DoesNotClaimThereIsNoExtractTerminal()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Studio", 40, 100),
                ("Bathroom", 6, 15));

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("no space that takes an extract terminal"));
            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("NO extract ventilation at all"));
            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("has no wet room that takes an extract terminal"));
        }

        /// <summary>
        /// A room classified as solely a kitchen DOES take an extract terminal, so it represents the
        /// configured local kitchen extract and raises no missing-local-kitchen-extract warning.
        /// </summary>
        [Fact]
        public void SolelyKitchenExtractTerminal_RaisesNoLocalKitchenExtractWarning()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Living Room", 20, 50),
                ("Bedroom 1", 14, 35),
                ("Kitchen", 12, 30),
                ("Bathroom", 6, 15));

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("ENGINEERING CHECK REQUIRED"));
            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("local kitchen or cooker extract"));

            //And it reaches its Table 1.2 kitchen minimum at the high rate, which is the condition that
            //figure applies to. The whole dwelling rate here is 19 l/s against a 21 l/s total of the two
            //rooms' high-rate minimums, so the kitchen sits below 13 l/s continuously and boosts.
            PartFVentilationTerminalRequirement terminal_Kitchen = Assert.Single(Assert.Single(partFCalculator.DwellingResults).ComplianceResult.LocalKitchenExtractTerminals);

            Assert.Equal(13, terminal_Kitchen.HighFlowRate_Lps!.Value, tolerance);
            Assert.True(terminal_Kitchen.HighRateIncreaseRequired);
        }

        /// <summary>
        /// A studio on its own is now a complete little dwelling: it takes supply as its only habitable
        /// room and its own local kitchen extract as the room holding the cooking function, and the two
        /// balance. Nothing about it needs an engineering check for a missing terminal, because none is
        /// missing.
        /// </summary>
        [Fact]
        public void StudioAlone_SuppliesAndExtractsItselfWithoutAnyWetRoom()
        {
            PartFCalculator partFCalculator = Calculate(("Studio", 40, 100));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            //Note 1 gives 13 l/s and the floor area rate 0.3 x 40 = 12 l/s, so the dwelling sizes at
            //13 l/s. The studio's own Table 1.2 kitchen high-rate minimum is also 13 l/s, so the single
            //extract terminal happens to meet it continuously and needs no boost (Table 1.2 note 1).
            Assert.Equal(13, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(13, dwellingResult.TotalSupply_Lps, tolerance);
            Assert.Equal(13, dwellingResult.TotalExtract_Lps, tolerance);

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("ENGINEERING CHECK REQUIRED"));
            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("no extract terminal that forms part of the balanced continuous system"));
        }

        /// <summary>
        /// A dwelling with no cooking space at all raises no kitchen extract warning of either kind - it
        /// raises the separate "no cooking space" warning instead.
        /// </summary>
        [Fact]
        public void DwellingWithoutACookingSpace_RaisesNoKitchenExtractWarning()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Bedroom 1", 14, 35),
                ("Living Room", 20, 50),
                ("Bathroom", 6, 15));

            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("ENGINEERING CHECK REQUIRED"));
            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("local kitchen or cooker extract"));

            Assert.Contains(partFCalculator.Warnings, x => x.Contains("contains no kitchen, open plan living kitchen or studio"));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static PartFData DataFile()
        {
            return Analytical.Create.PartFData(Fixtures.GetPath(dataFileName));
        }

        private static double Rate(PartFCalculator partFCalculator, string name)
        {
            return partFCalculator.AdjacencyCluster.GetSpaces().Find(x => x.Name == name)
                ?.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)?.ContinuousDesignFlowRate_Lps ?? 0;
        }

        private static PartFCalculator Calculate(params (string Name, double Area_M2, double Volume_M3)[] spaces)
        {
            AdjacencyCluster adjacencyCluster = new();

            for (int i = 0; i < spaces.Length; i++)
            {
                Space space = new(spaces[i].Name, new Point3D(i * 10, 0, 1.5));
                space.SetValue(SpaceParameter.Area, spaces[i].Area_M2);
                space.SetValue(SpaceParameter.Volume, spaces[i].Volume_M3);
                adjacencyCluster.AddObject(space);
            }

            PartFCalculator result = new(DataFile()) { AdjacencyCluster = adjacencyCluster };

            Assert.True(result.Calculate());

            return result;
        }
    }
}
