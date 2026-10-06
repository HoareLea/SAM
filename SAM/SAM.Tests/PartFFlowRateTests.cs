// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Tests for the separation of the Approved Document F design condition from the background
    /// operating condition.
    /// </summary>
    /// <remarks>
    /// The design condition is the regulatory sizing case that every minimum is checked against. The
    /// background condition is an operating mode obtained by scaling the design rates by the rule set's
    /// background factor (0.30 by default, so 30% of the design rate). The background factor must never reduce
    /// or replace the design calculation, and both conditions must stay balanced.
    /// </remarks>
    public class PartFFlowRateTests
    {
        private const string dataFileName = "SAM_PartFSpaceRulesUKDwellingsMVHR.json";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // The default background rate: 30% of design
        // ------------------------------------------------------------------

        /// <summary>The shipped rule set carries the documented 0.30 default.</summary>
        [Fact]
        public void DataFile_CarriesTheDefaultBackgroundFactor()
        {
            Assert.Equal(0.3, DataFile().SetbackFlowRateFactor, tolerance);
            Assert.Equal(0.3, PartFData.DefaultSetbackFlowRateFactor, tolerance);
        }

        /// <summary>The whole dwelling background rate is exactly 30% of the design rate by default.</summary>
        [Fact]
        public void WholeDwellingBackgroundRate_IsThirtyPercentOfDesign()
        {
            PartFCalculator partFCalculator = Calculate(House());

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps * 0.3, dwellingResult.SetbackSystemRate_Lps, tolerance);
            Assert.Equal(0.3, dwellingResult.SetbackFlowRateFactor, tolerance);
        }

        /// <summary>Every room's background rate is exactly 30% of that room's design rate.</summary>
        [Fact]
        public void EveryRoomBackgroundRate_IsThirtyPercentOfItsDesignRate()
        {
            PartFCalculator partFCalculator = Calculate(House());

            List<PartFSpaceData> partFSpaceDatas = SpaceDatas(partFCalculator);

            Assert.NotEmpty(partFSpaceDatas);

            foreach (PartFSpaceData partFSpaceData in partFSpaceDatas)
            {
                Assert.NotNull(partFSpaceData.ContinuousDesignFlowRate_Lps);
                Assert.NotNull(partFSpaceData.SetbackFlowRate_Lps);
                Assert.Equal(partFSpaceData.ContinuousDesignFlowRate_Lps!.Value * 0.3, partFSpaceData.SetbackFlowRate_Lps!.Value, tolerance);
            }
        }

        /// <summary>Both conditions balance: total supply equals total extract at design and background.</summary>
        [Fact]
        public void SupplyAndExtract_BalanceAtBothConditions()
        {
            PartFCalculator partFCalculator = Calculate(House());

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(dwellingResult.TotalSupply_Lps, dwellingResult.TotalExtract_Lps, tolerance);
            Assert.Equal(dwellingResult.TotalSetbackSupply_Lps, dwellingResult.TotalSetbackExtract_Lps, tolerance);

            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps, dwellingResult.TotalSupply_Lps, tolerance);
            Assert.Equal(dwellingResult.SetbackSystemRate_Lps, dwellingResult.TotalSetbackSupply_Lps, tolerance);
        }

        // ------------------------------------------------------------------
        // Specific worked values
        // ------------------------------------------------------------------

        /// <summary>
        /// 100 l/s continuous design gives 30 l/s setback at the default factor. Two habitable rooms so
        /// note 1 does not apply; 333.33 m2 x 0.3 = 100 l/s governs over the 19 l/s bedroom rate and the
        /// 13 l/s kitchen minimum.
        /// </summary>
        [Fact]
        public void OneHundredLitresPerSecondContinuousDesign_GivesThirtyLitresPerSecondSetback()
        {
            PartFCalculator partFCalculator = Calculate(
            [
                ("Bedroom 1", 200, 500),
                ("Living Room", 100, 250),
                ("Kitchen", 100.0 / 3.0, 80),
            ]);

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(100, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(30, dwellingResult.SetbackSystemRate_Lps, tolerance);
        }

        /// <summary>
        /// 63 l/s continuous design gives 18.9 l/s setback at the default factor. This is the Flat 2
        /// arrangement of the example model: 210 m2 x 0.3 = 63 l/s governs.
        /// </summary>
        [Fact]
        public void SixtyThreeLitresPerSecondContinuousDesign_GivesEighteenPointNineSetback()
        {
            PartFCalculator partFCalculator = Calculate(
            [
                ("Bedroom 2_3", 105, 420),
                ("Kitchen_4", 75, 300),
                ("Ensuite_5", 30, 120),
            ]);

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(63, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(18.9, dwellingResult.SetbackSystemRate_Lps, tolerance);
        }

        /// <summary>A custom factor of 0.50 gives a setback rate of exactly half the continuous design.</summary>
        [Fact]
        public void CustomFactorOfAHalf_GivesHalfTheContinuousDesignRate()
        {
            PartFData partFData = DataFile();
            partFData.SetbackFlowRateFactor = 0.5;

            PartFCalculator partFCalculator = Calculate(House(), partFData);
            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(0.5, dwellingResult.SetbackFlowRateFactor, tolerance);
            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps / 2, dwellingResult.SetbackSystemRate_Lps, tolerance);

            foreach (PartFSpaceData partFSpaceData in SpaceDatas(partFCalculator))
            {
                Assert.Equal(partFSpaceData.ContinuousDesignFlowRate_Lps!.Value / 2, partFSpaceData.SetbackFlowRate_Lps!.Value, tolerance);
            }
        }

        // ------------------------------------------------------------------
        // The design calculation is untouched by the reduction
        // ------------------------------------------------------------------

        /// <summary>
        /// Changing the background factor must not move any design rate. The background reduction is an
        /// operating mode, not part of the regulatory sizing calculation.
        /// </summary>
        [Theory]
        [InlineData(0.3)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public void ChangingTheBackgroundFactor_LeavesEveryDesignRateUnchanged(double factor)
        {
            PartFDwellingResult dwellingResult_Default = Assert.Single(Calculate(House()).DwellingResults);

            PartFData partFData = DataFile();
            partFData.SetbackFlowRateFactor = factor;

            PartFCalculator partFCalculator = Calculate(House(), partFData);
            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(dwellingResult_Default.ContinuousDesignSystemRate_Lps, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(dwellingResult_Default.BedroomBasedRate_Lps, dwellingResult.BedroomBasedRate_Lps, tolerance);
            Assert.Equal(dwellingResult_Default.AreaBasedRate_Lps, dwellingResult.AreaBasedRate_Lps, tolerance);
            Assert.Equal(dwellingResult_Default.WetRoomMinimumTotal_Lps, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(dwellingResult_Default.TotalSupply_Lps, dwellingResult.TotalSupply_Lps, tolerance);
            Assert.Equal(dwellingResult_Default.TotalExtract_Lps, dwellingResult.TotalExtract_Lps, tolerance);
        }

        /// <summary>A custom valid factor is applied to the whole dwelling rate and to every room.</summary>
        [Theory]
        [InlineData(0.2)]
        [InlineData(0.5)]
        [InlineData(0.85)]
        [InlineData(1.0)]
        public void CustomValidFactor_IsApplied(double factor)
        {
            PartFData partFData = DataFile();
            partFData.SetbackFlowRateFactor = factor;

            PartFCalculator partFCalculator = Calculate(House(), partFData);
            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(factor, dwellingResult.SetbackFlowRateFactor, tolerance);
            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps * factor, dwellingResult.SetbackSystemRate_Lps, tolerance);

            foreach (PartFSpaceData partFSpaceData in SpaceDatas(partFCalculator))
            {
                Assert.Equal(partFSpaceData.ContinuousDesignFlowRate_Lps!.Value * factor, partFSpaceData.SetbackFlowRate_Lps!.Value, tolerance);
            }
        }

        /// <summary>Both conditions stay balanced at a custom factor too.</summary>
        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public void CustomValidFactor_KeepsBothConditionsBalanced(double factor)
        {
            PartFData partFData = DataFile();
            partFData.SetbackFlowRateFactor = factor;

            PartFDwellingResult dwellingResult = Assert.Single(Calculate(House(), partFData).DwellingResults);

            Assert.Equal(dwellingResult.TotalSetbackSupply_Lps, dwellingResult.TotalSetbackExtract_Lps, tolerance);
        }

        // ------------------------------------------------------------------
        // Factor validation
        // ------------------------------------------------------------------

        /// <summary>A factor greater than 0 and no greater than 1 is valid.</summary>
        [Theory]
        [InlineData(0.0001)]
        [InlineData(0.3)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        public void ValidFactor_IsAccepted(double factor)
        {
            Assert.True(PartFData.IsValidSetbackFlowRateFactor(factor));

            PartFData partFData = DataFile();
            partFData.SetbackFlowRateFactor = factor;

            Assert.Equal(factor, partFData.SetbackFlowRateFactor, tolerance);
        }

        /// <summary>
        /// A negative factor, zero, a factor above 1, NaN and infinity are all rejected. None of them may
        /// be silently accepted: each would produce a nonsensical background rate.
        /// </summary>
        [Theory]
        [InlineData(-0.5)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1.0001)]
        [InlineData(2)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void InvalidFactor_IsRejectedAndReplacedByTheDocumentedDefault(double factor)
        {
            Assert.False(PartFData.IsValidSetbackFlowRateFactor(factor));

            PartFData partFData = DataFile();
            partFData.SetbackFlowRateFactor = factor;

            Assert.Equal(PartFData.DefaultSetbackFlowRateFactor, partFData.SetbackFlowRateFactor, tolerance);
        }

        /// <summary>
        /// An invalid factor must not leak into the results: the calculation still produces finite,
        /// balanced background rates at the documented default.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(5)]
        [InlineData(double.NaN)]
        public void InvalidFactor_StillProducesFiniteBalancedBackgroundRates(double factor)
        {
            PartFData partFData = DataFile();
            partFData.SetbackFlowRateFactor = factor;

            PartFCalculator partFCalculator = Calculate(House(), partFData);
            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(PartFData.DefaultSetbackFlowRateFactor, dwellingResult.SetbackFlowRateFactor, tolerance);
            Assert.False(double.IsNaN(dwellingResult.SetbackSystemRate_Lps));
            Assert.False(double.IsInfinity(dwellingResult.SetbackSystemRate_Lps));
            Assert.Equal(dwellingResult.TotalSetbackSupply_Lps, dwellingResult.TotalSetbackExtract_Lps, tolerance);
        }

        /// <summary>A data file carrying an invalid factor falls back to the documented default.</summary>
        [Fact]
        public void DataFileWithAnInvalidFactor_FallsBackToTheDefault()
        {
            PartFData partFData = new()
            {
                SetbackFlowRateFactor = -3,
            };

            Assert.Equal(PartFData.DefaultSetbackFlowRateFactor, partFData.SetbackFlowRateFactor, tolerance);
        }

        // ------------------------------------------------------------------
        // No NaN or infinity anywhere
        // ------------------------------------------------------------------

        /// <summary>No design or background rate may ever be NaN or infinite.</summary>
        [Fact]
        public void NoFlowRate_IsNaNOrInfinite()
        {
            PartFCalculator partFCalculator = Calculate(House());

            foreach (PartFSpaceData partFSpaceData in SpaceDatas(partFCalculator))
            {
                Assert.False(double.IsNaN(partFSpaceData.ContinuousDesignFlowRate_Lps!.Value));
                Assert.False(double.IsInfinity(partFSpaceData.ContinuousDesignFlowRate_Lps!.Value));
                Assert.False(double.IsNaN(partFSpaceData.SetbackFlowRate_Lps!.Value));
                Assert.False(double.IsInfinity(partFSpaceData.SetbackFlowRate_Lps!.Value));
            }

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.False(double.IsNaN(dwellingResult.ContinuousDesignSystemRate_Lps));
            Assert.False(double.IsNaN(dwellingResult.SetbackSystemRate_Lps));
            Assert.False(double.IsInfinity(dwellingResult.ContinuousDesignSystemRate_Lps));
            Assert.False(double.IsInfinity(dwellingResult.SetbackSystemRate_Lps));
        }

        /// <summary>
        /// A dwelling whose rooms carry no volume cannot have its air distributed, but it must still
        /// produce finite rates rather than a NaN from a division by zero.
        /// </summary>
        [Fact]
        public void MissingVolumes_StillProduceFiniteRatesAtBothConditions()
        {
            PartFCalculator partFCalculator = Calculate(
                ("Bedroom 1", 14, 0),
                ("Living Room", 20, 0),
                ("Kitchen", 12, 0),
                ("Bathroom", 6, 0));

            foreach (PartFSpaceData partFSpaceData in SpaceDatas(partFCalculator))
            {
                Assert.False(double.IsNaN(partFSpaceData.ContinuousDesignFlowRate_Lps ?? 0));
                Assert.False(double.IsInfinity(partFSpaceData.ContinuousDesignFlowRate_Lps ?? 0));
                Assert.False(double.IsNaN(partFSpaceData.SetbackFlowRate_Lps ?? 0));
                Assert.False(double.IsInfinity(partFSpaceData.SetbackFlowRate_Lps ?? 0));
            }
        }

        // ------------------------------------------------------------------
        // Backward compatibility
        // ------------------------------------------------------------------

        /// <summary>
        /// CalculatedFlowRate_Lps has always held the design rate and must continue to, so existing
        /// scripts and Grasshopper definitions keep their original meaning.
        /// </summary>
        [Fact]
        public void CalculatedFlowRate_StillMeansTheDesignFlowRate()
        {
            PartFCalculator partFCalculator = Calculate(House());

            foreach (PartFSpaceData partFSpaceData in SpaceDatas(partFCalculator))
            {
                Assert.Equal(partFSpaceData.ContinuousDesignFlowRate_Lps, partFSpaceData.CalculatedFlowRate_Lps);
            }
        }

        /// <summary>FinalSystemRate_Lps likewise still means the design system rate.</summary>
        [Fact]
        public void FinalSystemRate_StillMeansTheDesignSystemRate()
        {
            PartFCalculator partFCalculator = Calculate(House());

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps, dwellingResult.FinalSystemRate_Lps, tolerance);
            Assert.Equal(dwellingResult.ContinuousDesignSystemRate_Lps, partFCalculator.FinalSystemRate_Lps!.Value, tolerance);
        }

        /// <summary>
        /// A model serialised before the design and background rates were separated carries only
        /// CalculatedFlowRate_Lps, and that value must be read back as the design rate.
        /// </summary>
        [Fact]
        public void LegacyJson_WithOnlyCalculatedFlowRate_ReadsBackAsTheDesignRate()
        {
            System.Text.Json.Nodes.JsonObject jsonObject = new()
            {
                ["_type"] = "SAM.Analytical.PartFSpaceData,SAM.Analytical",
                ["Name"] = "Sleeping",
                ["CalculatedFlowRate_Lps"] = 12.5,
            };

            PartFSpaceData partFSpaceData = new();
            Assert.True(partFSpaceData.FromJsonObject(jsonObject));

            Assert.Equal(12.5, partFSpaceData.ContinuousDesignFlowRate_Lps!.Value, tolerance);
            Assert.Equal(12.5, partFSpaceData.CalculatedFlowRate_Lps!.Value, tolerance);
        }

        /// <summary>
        /// A model written now must still be readable by an earlier SAM build, which only knows
        /// CalculatedFlowRate_Lps, so both keys are written.
        /// </summary>
        [Fact]
        public void NewJson_WritesBothTheDesignAndTheLegacyKey()
        {
            PartFSpaceData partFSpaceData = new()
            {
                ContinuousDesignFlowRate_Lps = 21,
                SetbackFlowRate_Lps = 6.3,
            };

            System.Text.Json.Nodes.JsonObject jsonObject = partFSpaceData.ToJsonObject();

            Assert.Equal(21, jsonObject["ContinuousDesignFlowRate_Lps"]!.GetValue<double>(), tolerance);
            Assert.Equal(21, jsonObject["CalculatedFlowRate_Lps"]!.GetValue<double>(), tolerance);
            Assert.Equal(6.3, jsonObject["SetbackFlowRate_Lps"]!.GetValue<double>(), tolerance);
        }

        /// <summary>Design and background rates survive a full round trip.</summary>
        [Fact]
        public void DesignAndBackgroundRates_RoundTripThroughJson()
        {
            PartFSpaceData partFSpaceData = new()
            {
                ContinuousDesignFlowRate_Lps = 31,
                SetbackFlowRate_Lps = 9.3,
            };

            PartFSpaceData partFSpaceData_RoundTrip = new();
            Assert.True(partFSpaceData_RoundTrip.FromJsonObject(partFSpaceData.ToJsonObject()));

            Assert.Equal(31, partFSpaceData_RoundTrip.ContinuousDesignFlowRate_Lps!.Value, tolerance);
            Assert.Equal(9.3, partFSpaceData_RoundTrip.SetbackFlowRate_Lps!.Value, tolerance);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static PartFData DataFile()
        {
            return Analytical.Create.PartFData(Fixtures.GetPath(dataFileName));
        }

        private static List<PartFSpaceData> SpaceDatas(PartFCalculator partFCalculator)
        {
            return [.. partFCalculator.AdjacencyCluster.GetSpaces()
                .Select(x => x.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData))
                .Where(x => x is not null)];
        }

        /// <summary>A three bedroom house with a kitchen, bathroom and WC.</summary>
        private static (string Name, double Area_M2, double Volume_M3)[] House()
        {
            return
            [
                ("Bedroom 1", 14, 35),
                ("Bedroom 2", 12, 30),
                ("Bedroom 3", 10, 25),
                ("Living Room", 28, 70),
                ("Kitchen", 16, 40),
                ("Bathroom", 6, 15),
                ("WC", 3, 7.5),
            ];
        }

        private static PartFCalculator Calculate((string Name, double Area_M2, double Volume_M3)[] spaces, PartFData partFData = null)
        {
            AdjacencyCluster adjacencyCluster = new();

            for (int i = 0; i < spaces.Length; i++)
            {
                Space space = new(spaces[i].Name, new Point3D(i * 10, 0, 1.5));
                space.SetValue(SpaceParameter.Area, spaces[i].Area_M2);
                space.SetValue(SpaceParameter.Volume, spaces[i].Volume_M3);
                adjacencyCluster.AddObject(space);
            }

            PartFCalculator result = new(partFData ?? DataFile()) { AdjacencyCluster = adjacencyCluster };

            Assert.True(result.Calculate());

            return result;
        }

        private static PartFCalculator Calculate(params (string Name, double Area_M2, double Volume_M3)[] spaces)
        {
            return Calculate(spaces, null);
        }
    }
}
