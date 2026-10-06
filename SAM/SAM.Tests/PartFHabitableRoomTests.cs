// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Tests for the habitable-room count and for Approved Document F, Volume 1: Dwellings
    /// (2021 edition, for use in England) Table 1.3 note 1 (page 10): "If the dwelling only has one
    /// habitable room, a minimum ventilation rate of 13l/s should be used."
    /// </summary>
    /// <remarks>
    /// Note 1 is a REGULATORY requirement, not a SAM convention, and it keys off the habitable ROOM
    /// count rather than the bedroom count. Where it applies it replaces the Table 1.3 bedroom rate, so a
    /// studio flat is sized from 13 l/s rather than the one-bedroom figure of 19 l/s. The final continuous
    /// design rate may still be higher, because the floor-area rate of paragraph 1.24a and the total of
    /// the Table 1.2 wet-room minimums also apply.
    /// <para>
    /// Appendix A (page 36) defines a habitable room as one used for dwelling purposes but not SOLELY a
    /// kitchen, utility room, bathroom, cellar or sanitary accommodation.
    /// </para>
    /// </remarks>
    public class PartFHabitableRoomTests
    {
        private const string dataFileName = "SAM_PartFSpaceRulesUKDwellingsMVHR.json";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // What counts as a habitable room
        // ------------------------------------------------------------------

        /// <summary>A studio plus a bathroom is a ONE habitable room dwelling.</summary>
        [Fact]
        public void StudioPlusBathroom_IsOneHabitableRoom()
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10));

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.Equal(["Studio"], dwellingResult.HabitableRoomNames);
        }

        /// <summary>
        /// None of the non-habitable room uses increases the habitable-room count, however many of them
        /// the dwelling contains.
        /// </summary>
        [Theory]
        [InlineData("Bathroom")]
        [InlineData("Ensuite")]
        [InlineData("Corridor")]
        [InlineData("Utility Room")]
        [InlineData("WC")]
        [InlineData("Store")]
        [InlineData("Plant Room")]
        public void NonHabitableRoom_DoesNotIncreaseTheHabitableRoomCount(string name)
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Studio", 20, 50),
                (name, 4, 10));

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.True(dwellingResult.OneHabitableRoomRuleApplied);
        }

        /// <summary>
        /// A whole set of non-habitable rooms together still leaves one habitable room, so note 1 still
        /// applies. Bathroom 8 + ensuite 8 + WC 6 = 22 l/s of wet-room minimum governs the final rate,
        /// which is the correct outcome and does not change the habitable-room count.
        /// </summary>
        [Fact]
        public void ManyNonHabitableRooms_StillLeaveOneHabitableRoom()
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10),
                ("Ensuite", 3, 8),
                ("WC", 2, 5),
                ("Corridor", 5, 12));

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.True(dwellingResult.OneHabitableRoomRuleApplied);
            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
        }

        /// <summary>
        /// A room that is solely a kitchen is a wet room, not a habitable room (Appendix A), so a bedroom
        /// plus a kitchen is a one habitable room dwelling.
        /// </summary>
        [Fact]
        public void Kitchen_IsNotAHabitableRoom()
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Bedroom 1", 14, 35),
                ("Kitchen", 10, 25));

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.Equal(["Bedroom 1"], dwellingResult.HabitableRoomNames);
        }

        /// <summary>Every habitable room use is counted.</summary>
        [Theory]
        [InlineData("Bedroom 1")]
        [InlineData("Living Room")]
        [InlineData("Living Kitchen")]
        [InlineData("Study")]
        public void HabitableRoom_IsCounted(string name)
        {
            PartFDwellingResult dwellingResult = Calculate(
                (name, 20, 50),
                ("Bathroom", 4, 10));

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.Equal([name], dwellingResult.HabitableRoomNames);
        }

        /// <summary>More than one habitable room takes the dwelling out of note 1.</summary>
        [Fact]
        public void MoreThanOneHabitableRoom_DoesNotApplyNote1()
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Bedroom 1", 14, 35),
                ("Living Room", 20, 50),
                ("Kitchen", 10, 25),
                ("Bathroom", 4, 10));

            Assert.Equal(2, dwellingResult.HabitableRoomCount);
            Assert.False(dwellingResult.OneHabitableRoomRuleApplied);
            Assert.Equal(19, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
        }

        /// <summary>
        /// A dwelling with no habitable room at all cannot be assessed against note 1, and is reported:
        /// paragraph 1.67 requires mechanical supply to each habitable room, so there is no supply.
        /// </summary>
        [Fact]
        public void ZeroHabitableRooms_IsReportedAndDoesNotApplyNote1()
        {
            PartFCalculator partFCalculator = Calculator(
                ("Bathroom", 4, 10),
                ("WC", 2, 5));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(0, dwellingResult.HabitableRoomCount);
            Assert.False(dwellingResult.OneHabitableRoomRuleApplied);
            Assert.Contains(partFCalculator.Warnings, x => x.Contains("No space was classified as a habitable room"));
        }

        // ------------------------------------------------------------------
        // The note 1 rate and its precedence
        // ------------------------------------------------------------------

        /// <summary>The one habitable room rate is 13 l/s.</summary>
        [Fact]
        public void OneHabitableRoomRate_IsThirteenLitresPerSecond()
        {
            Assert.Equal(13, PartFData.DefaultOneHabitableRoomRate_Lps, tolerance);
            Assert.Equal(13, DataFile().OneHabitableRoomRate_Lps, tolerance);

            PartFDwellingResult dwellingResult = Calculate(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10));

            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
        }

        /// <summary>
        /// The rate selector is exactly: HabitableRoomCount == 1 ? 13 : Table1_3Rate(BedroomCount).
        /// </summary>
        [Theory]
        [InlineData(1, 1, 13)]
        [InlineData(1, 0, 13)]
        [InlineData(1, 3, 13)]
        [InlineData(0, 1, 19)]
        [InlineData(2, 1, 19)]
        [InlineData(2, 2, 25)]
        [InlineData(4, 3, 31)]
        [InlineData(5, 4, 37)]
        [InlineData(6, 5, 43)]
        [InlineData(7, 6, 49)]
        public void BedroomOrHabitableRate_SelectsNote1OnlyForExactlyOneHabitableRoom(int habitableRoomCount, int bedroomCount, double expected_Lps)
        {
            Assert.Equal(expected_Lps, DataFile().GetBedroomOrHabitableRate_Lps(habitableRoomCount, bedroomCount), tolerance);
        }

        /// <summary>
        /// Floor-area rate BELOW 13 l/s: the note 1 rate governs. 24 m2 gives 0.3 x 24 = 7.2 l/s, and the
        /// only extract minimum is the WC's 6 l/s, so 13 l/s is the greatest.
        /// </summary>
        /// <remarks>
        /// A living room rather than a studio, because a studio contains the cooking function and so
        /// carries a local kitchen extract terminal of its own at the Table 1.2 kitchen rate of 13 l/s.
        /// Adding that to a wet room's minimum always takes the extract total above 13, so no dwelling
        /// containing a cooking space can demonstrate note 1 governing on its own.
        /// </remarks>
        [Fact]
        public void FloorAreaRateBelowThirteen_TheNote1RateGoverns()
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Living Room", 20, 50),
                ("WC", 4, 10));

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.Equal(7.2, dwellingResult.AreaBasedRate_Lps, tolerance);
            Assert.Equal(6, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(13, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
        }

        /// <summary>
        /// A cooking space carries the Table 1.2 kitchen minimum HIGH rate of 13 l/s on its own local
        /// kitchen extract terminal, so a studio flat's reported high-rate minimum total is that 13 plus
        /// every wet room's. Before terminal-level sizing, the studio's kitchen extract was not modelled
        /// at all and only the bathroom's 8 l/s counted.
        /// <para>
        /// That total is reported and applies room by room at the high condition. It does not raise the
        /// continuous design rate, which stays at the whole dwelling rate of 13 l/s.
        /// </para>
        /// </summary>
        [Fact]
        public void CookingSpace_ContributesTheTable1_2KitchenMinimum()
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10));

            Assert.Equal(7.2, dwellingResult.AreaBasedRate_Lps, tolerance);
            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
            Assert.Equal(21, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);

            Assert.Equal(13, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(21, dwellingResult.TotalHighExtract_Lps, tolerance);
        }

        /// <summary>
        /// Floor-area rate EQUAL to 13 l/s: the boundary. 43.333... m2 gives exactly 13 l/s, so the two
        /// coincide and the result is still 13 l/s.
        /// </summary>
        [Fact]
        public void FloorAreaRateEqualToThirteen_GivesThirteen()
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Living Room", 40, 100),
                ("WC", 10.0 / 3.0, 8));

            Assert.Equal(13, dwellingResult.AreaBasedRate_Lps, tolerance);
            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
            Assert.Equal(6, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(13, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
        }

        /// <summary>
        /// Floor-area rate ABOVE 13 l/s: the area rate governs. 100 m2 gives 30 l/s.
        /// </summary>
        [Fact]
        public void FloorAreaRateAboveThirteen_TheAreaRateGoverns()
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Studio", 96, 240),
                ("Bathroom", 4, 10));

            Assert.Equal(30, dwellingResult.AreaBasedRate_Lps, tolerance);
            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
            Assert.Equal(30, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
        }

        /// <summary>
        /// High-rate minimum total ABOVE 13 l/s: it still does not govern the continuous rate. Studio
        /// kitchen 13 + bathroom 8 + WC 6 = 27 l/s, against a note 1 rate of 13 l/s and a floor-area rate
        /// of 0.3 x 26 = 7.8 l/s, and the continuous design rate stays at 13 l/s.
        /// </summary>
        [Fact]
        public void WetRoomHighRateMinimumAboveThirteen_StillDoesNotGovern()
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10),
                ("WC", 2, 5));

            Assert.Equal(27, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(13, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
            Assert.True(dwellingResult.AreaBasedRate_Lps < 13);

            Assert.Equal(13, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(27, dwellingResult.TotalHighExtract_Lps, tolerance);
        }

        /// <summary>
        /// Note 1 applying never lowers the continuous design rate below the paragraph 1.24a floor area
        /// rate: it is always the greater of the two whole dwelling rates, whichever that is, and never
        /// the sum of the Table 1.2 per-room high-rate minimums.
        /// </summary>
        [Theory]
        [InlineData(20, 4)]
        [InlineData(40, 10)]
        [InlineData(96, 4)]
        [InlineData(200, 20)]
        public void Note1_NeverLowersTheRateBelowTheFloorAreaRate(double area_Studio, double area_Bathroom)
        {
            PartFDwellingResult dwellingResult = Calculate(
                ("Studio", area_Studio, area_Studio * 2.5),
                ("Bathroom", area_Bathroom, area_Bathroom * 2.5));

            double expected = System.Math.Max(dwellingResult.BedroomOrHabitableRate_Lps, dwellingResult.AreaBasedRate_Lps);

            Assert.Equal(expected, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.True(dwellingResult.ContinuousDesignSystemRate_Lps >= 13 - tolerance);
        }

        /// <summary>Applying note 1 is reported, so the engineer can see which provision was used.</summary>
        [Fact]
        public void Note1_IsReported()
        {
            PartFCalculator partFCalculator = Calculator(
                ("Studio", 20, 50),
                ("Bathroom", 4, 10));

            Assert.Contains(partFCalculator.Remarks, x => x.Contains("exactly one habitable room") && x.Contains("note 1"));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static PartFData DataFile()
        {
            return Analytical.Create.PartFData(Fixtures.GetPath(dataFileName));
        }

        private static PartFCalculator Calculator(params (string Name, double Area_M2, double Volume_M3)[] spaces)
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

        private static PartFDwellingResult Calculate(params (string Name, double Area_M2, double Volume_M3)[] spaces)
        {
            return Assert.Single(Calculator(spaces).DwellingResults);
        }
    }
}
