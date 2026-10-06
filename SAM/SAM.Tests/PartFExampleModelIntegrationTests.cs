// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Tests.Helpers;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// End to end tests over the room mix, zoning and adjacency of the SAM_zoningAM_v1 example model.
    /// </summary>
    /// <remarks>
    /// The example model is a block of three flats around a communal corridor, and its partitions connect
    /// rooms ACROSS flat boundaries as well as within them - the studio of Flat 1 adjoins a bedroom of
    /// Flat 2, and the communal corridor adjoins almost everything. That is what makes it a good test of
    /// the transfer air network: a solver that treated adjacency alone as connectivity would route Flat 1's
    /// supply air into Flat 2 and out through the communal corridor.
    /// <para>
    /// The topology and room dimensions are reproduced here as a synthetic model. The example file itself
    /// is read-only project data and is never opened, written or depended on by the test suite.
    /// </para>
    /// <para>
    /// Areas and volumes, from the example model: Studio 1_0 75 m2 / 300 m3, Bathroom_2 25 / 100,
    /// Bedroom 2_3 105 / 420, Kitchen_4 75 / 300, Ensuite_5 30 / 120, Bedroom 2_6 105 / 420,
    /// Kitchen_7 75 / 300, Ensuite_8 30 / 120, Corridor_1 366 / 1464.
    /// </para>
    /// </remarks>
    public class PartFExampleModelIntegrationTests
    {
        private const string zoneCategoryName = "Flats";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // Flat 1: the studio reference case
        // ------------------------------------------------------------------

        [Fact]
        public void Flat1_MatchesTheStudioReferenceCalculation()
        {
            PartFDwellingResult dwellingResult = Dwelling("Flat 1");

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.Equal(100, dwellingResult.InternalFloorArea_M2, tolerance);
            Assert.Equal(30, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);

            Assert.Equal(30, Terminal(dwellingResult, "Studio 1_0", PartFTerminalRole.Supply).ContinuousDesignFlowRate_Lps.Value, tolerance);
            Assert.Equal(22, Terminal(dwellingResult, "Studio 1_0", PartFTerminalRole.LocalKitchenExtract).ContinuousDesignFlowRate_Lps.Value, tolerance);
            Assert.Equal(8, Terminal(dwellingResult, "Bathroom_2", PartFTerminalRole.GeneralExtract).ContinuousDesignFlowRate_Lps.Value, tolerance);

            Assert.Equal(30, dwellingResult.TotalSupply_Lps, tolerance);
            Assert.Equal(30, dwellingResult.TotalExtract_Lps, tolerance);
        }

        /// <summary>
        /// Flat 1's only internal route is the studio to its bathroom, carrying 8 l/s. The studio also
        /// adjoins the communal corridor and a Flat 2 bedroom in the model, and neither may become a route.
        /// </summary>
        [Fact]
        public void Flat1_TransfersEightLitresPerSecondAndNowhereElse()
        {
            PartFDwellingResult dwellingResult = Dwelling("Flat 1");

            PartFDoorTransferData partFDoorTransferData = Assert.Single(dwellingResult.ComplianceResult.TransferPaths);

            Assert.Equal("Studio 1_0", partFDoorTransferData.UpstreamSpaceName);
            Assert.Equal("Bathroom_2", partFDoorTransferData.DownstreamSpaceName);
            Assert.Equal(8, partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps.Value, tolerance);
            Assert.Equal(PartFTransferRouteStatus.UniquelyDetermined, partFDoorTransferData.RouteStatus);
        }

        // ------------------------------------------------------------------
        // Flat 2 and Flat 3
        // ------------------------------------------------------------------

        /// <summary>
        /// Flat 2: bedroom supply, kitchen extract, ensuite extract. 210 m2 gives 63 l/s, which governs
        /// over the one-habitable-room rate of 13 l/s. The Table 1.2 per-room high-rate minimums total
        /// 13 + 8 = 21 l/s and are reported without entering the governing rate.
        /// </summary>
        [Theory]
        [InlineData("Flat 2", "Bedroom 2_3", "Kitchen_4", "Ensuite_5")]
        [InlineData("Flat 3", "Bedroom 2_6", "Kitchen_7", "Ensuite_8")]
        public void Flat2AndFlat3_SizeAndBalanceIndependently(string name_Dwelling, string name_Bedroom, string name_Kitchen, string name_Ensuite)
        {
            PartFDwellingResult dwellingResult = Dwelling(name_Dwelling);

            Assert.Equal(1, dwellingResult.HabitableRoomCount);
            Assert.Equal(210, dwellingResult.InternalFloorArea_M2, tolerance);
            Assert.Equal(63, dwellingResult.AreaBasedRate_Lps, tolerance);
            Assert.Equal(21, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(63, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);

            //Bedroom supply, kitchen local extract, ensuite general extract.
            Assert.Equal(63, Terminal(dwellingResult, name_Bedroom, PartFTerminalRole.Supply).ContinuousDesignFlowRate_Lps.Value, tolerance);
            Assert.Equal(55, Terminal(dwellingResult, name_Kitchen, PartFTerminalRole.LocalKitchenExtract).ContinuousDesignFlowRate_Lps.Value, tolerance);
            Assert.Equal(8, Terminal(dwellingResult, name_Ensuite, PartFTerminalRole.GeneralExtract).ContinuousDesignFlowRate_Lps.Value, tolerance);

            Assert.Equal(63, dwellingResult.TotalSupply_Lps, tolerance);
            Assert.Equal(63, dwellingResult.TotalExtract_Lps, tolerance);

            //High rate: every terminal is already above its Table 1.2 minimum at the continuous rate, so
            //Table 1.2 note 1 applies and nothing has to boost.
            Assert.Equal(63, dwellingResult.TotalHighExtract_Lps, tolerance);
            Assert.Equal(63, dwellingResult.TotalHighSupply_Lps, tolerance);
        }

        /// <summary>
        /// Flat 2's rooms form a chain: the bedroom passes its whole 63 l/s to the kitchen, which extracts
        /// 55 and passes the remaining 8 on to the ensuite. Both routes are fixed by conservation of air
        /// flow, so neither involves an engineering choice.
        /// </summary>
        [Theory]
        [InlineData("Flat 2", "Bedroom 2_3", "Kitchen_4", "Ensuite_5")]
        [InlineData("Flat 3", "Bedroom 2_6", "Kitchen_7", "Ensuite_8")]
        public void Flat2AndFlat3_InternalTransferRoutesAreUniquelyDetermined(string name_Dwelling, string name_Bedroom, string name_Kitchen, string name_Ensuite)
        {
            PartFComplianceResult complianceResult = Dwelling(name_Dwelling).ComplianceResult;

            Assert.Equal(2, complianceResult.TransferPaths.Count);

            Assert.Equal(63, Flow(complianceResult, name_Bedroom, name_Kitchen), tolerance);
            Assert.Equal(8, Flow(complianceResult, name_Kitchen, name_Ensuite), tolerance);

            Assert.All(complianceResult.TransferPaths, x => Assert.Equal(PartFTransferRouteStatus.UniquelyDetermined, x.RouteStatus));

            //Every internal door in the flat carries the paragraph 1.25 free area requirement.
            Assert.All(complianceResult.TransferPaths, x => Assert.Equal(7600, x.MinimumRequiredFreeArea_mm2.Value, tolerance));
        }

        // ------------------------------------------------------------------
        // Dwelling boundaries
        // ------------------------------------------------------------------

        /// <summary>
        /// The communal corridor adjoins nearly every room in the block, and belongs to no dwelling. No
        /// flat's transfer air may pass through it, and it is sized to Volume 2 rather than here.
        /// </summary>
        [Fact]
        public void CommunalCorridor_CarriesNoDwellingTransferAir()
        {
            PartFCalculator partFCalculator = Calculate();

            foreach (PartFDwellingResult dwellingResult in partFCalculator.DwellingResults)
            {
                Assert.DoesNotContain(dwellingResult.ComplianceResult.TransferPaths, x =>
                    x.UpstreamSpaceName == "Corridor_1" || x.DownstreamSpaceName == "Corridor_1");
            }

            Assert.Contains(partFCalculator.ExcludedZoneNames, x => x == "Corridor");
        }

        /// <summary>
        /// The model's partitions connect rooms across flat boundaries. None of them may become a transfer
        /// route, or one flat's ventilation would be solved through another's.
        /// </summary>
        [Fact]
        public void NoTransferRouteCrossesADwellingBoundary()
        {
            PartFCalculator partFCalculator = Calculate();

            System.Collections.Generic.Dictionary<string, string> dictionary_Dwelling = new()
            {
                { "Studio 1_0", "Flat 1" },
                { "Bathroom_2", "Flat 1" },
                { "Bedroom 2_3", "Flat 2" },
                { "Kitchen_4", "Flat 2" },
                { "Ensuite_5", "Flat 2" },
                { "Bedroom 2_6", "Flat 3" },
                { "Kitchen_7", "Flat 3" },
                { "Ensuite_8", "Flat 3" },
            };

            foreach (PartFDwellingResult dwellingResult in partFCalculator.DwellingResults)
            {
                Assert.All(dwellingResult.ComplianceResult.TransferPaths, x =>
                {
                    Assert.Equal(dwellingResult.Name, dictionary_Dwelling[x.UpstreamSpaceName]);
                    Assert.Equal(dwellingResult.Name, dictionary_Dwelling[x.DownstreamSpaceName]);
                });
            }
        }

        /// <summary>Each flat is sized on its own rooms only, so the three results differ where the rooms do.</summary>
        [Fact]
        public void EachFlat_IsSizedIndependently()
        {
            PartFCalculator partFCalculator = Calculate();

            Assert.Equal(3, partFCalculator.DwellingResults.Count);

            Assert.Equal(30, Dwelling(partFCalculator, "Flat 1").ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(63, Dwelling(partFCalculator, "Flat 2").ContinuousDesignSystemRate_Lps, tolerance);
            Assert.Equal(63, Dwelling(partFCalculator, "Flat 3").ContinuousDesignSystemRate_Lps, tolerance);
        }

        // ------------------------------------------------------------------
        // The report
        // ------------------------------------------------------------------

        /// <summary>
        /// One report covers the whole block, opens with the assumptions and carries a schematic for every
        /// flat with that flat's own room names and numbers.
        /// </summary>
        [Fact]
        public void Report_CoversEveryFlatWithItsOwnSchematic()
        {
            string report = PartFReport.Build(Calculate());

            Assert.StartsWith("ASSUMPTIONS\r\n\r\nNew dwelling in England.\r\nApproved Document F, Volume 1, 2021 edition.\r\n", report);

            Assert.Contains("DWELLING: Flat 1", report);
            Assert.Contains("DWELLING: Flat 2", report);
            Assert.Contains("DWELLING: Flat 3", report);

            Assert.Contains("Studio 1_0: +30 l/s supply, " + PartFSchematic.Minus + "22 l/s local kitchen extract", report);
            Assert.Contains("Bedroom 2_3: +63 l/s supply", report);
            Assert.Contains("Kitchen_4: " + PartFSchematic.Minus + "55 l/s local kitchen extract", report);

            //The communal corridor never appears as part of a dwelling's airflow.
            Assert.DoesNotContain("Corridor_1: +", report);
        }

        /// <summary>A large report is never truncated: every dwelling's every schedule is present.</summary>
        [Fact]
        public void Report_IsNotTruncated()
        {
            string report = PartFReport.Build(Calculate());

            Assert.Equal(3, Count(report, "OVERALL PART F CONFORMANCE ASSESSMENT"));
            Assert.Equal(3, Count(report, "DOOR UNDERCUT AND FREE AREA SCHEDULE (PARAGRAPH 1.25 ASSESSMENT)"));
            Assert.Equal(3, Count(report, "PURGE VENTILATION ASSESSMENT"));
        }

        /// <summary>
        /// No rate anywhere in the assessment is NaN or infinite. A single NaN propagates silently through
        /// every sum it touches and turns a schedule into nonsense that still looks like numbers, so this
        /// sweeps the whole result rather than spot-checking.
        /// </summary>
        [Fact]
        public void NoRate_IsNaNOrInfinite()
        {
            PartFCalculator partFCalculator = Calculate();

            foreach (PartFDwellingResult dwellingResult in partFCalculator.DwellingResults)
            {
                Finite(dwellingResult.ContinuousDesignSystemRate_Lps);
                Finite(dwellingResult.SetbackSystemRate_Lps);
                Finite(dwellingResult.BedroomOrHabitableRate_Lps);
                Finite(dwellingResult.BedroomBasedRate_Lps);
                Finite(dwellingResult.AreaBasedRate_Lps);
                Finite(dwellingResult.WholeDwellingRate_Lps);
                Finite(dwellingResult.WetRoomMinimumTotal_Lps);
                Finite(dwellingResult.InternalFloorArea_M2);
                Finite(dwellingResult.TotalSupply_Lps);
                Finite(dwellingResult.TotalExtract_Lps);
                Finite(dwellingResult.TotalHighSupply_Lps);
                Finite(dwellingResult.TotalHighExtract_Lps);
                Finite(dwellingResult.TotalSetbackSupply_Lps);
                Finite(dwellingResult.TotalSetbackExtract_Lps);
                Finite(dwellingResult.TotalIntermittentExtract_Lps);

                PartFComplianceResult complianceResult = dwellingResult.ComplianceResult;

                foreach (PartFVentilationTerminalRequirement terminal in complianceResult.Terminals)
                {
                    Finite(terminal.ContinuousDesignFlowRate_Lps);
                    Finite(terminal.HighFlowRate_Lps);
                    Finite(terminal.SetbackFlowRate_Lps);
                    Finite(terminal.MinimumRequiredFlowRate_Lps);

                    //A rate that exists is never negative either: air moving the wrong way through a
                    //terminal would be a sign error, not a design.
                    Assert.True((terminal.ContinuousDesignFlowRate_Lps ?? 0) >= 0);
                    Assert.True((terminal.HighFlowRate_Lps ?? 0) >= 0);
                }

                foreach (PartFDoorTransferData partFDoorTransferData in complianceResult.TransferPaths)
                {
                    Finite(partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps);
                    Finite(partFDoorTransferData.HighTransferFlowRate_Lps);
                    Finite(partFDoorTransferData.SetbackTransferFlowRate_Lps);
                    Finite(partFDoorTransferData.MinimumRequiredFreeArea_mm2);
                }

                foreach (PartFPurgeVentilationData partFPurgeVentilationData in complianceResult.PurgeVentilation)
                {
                    Finite(partFPurgeVentilationData.RequiredPurgeRate_Lps);
                    Finite(partFPurgeVentilationData.RequiredOpeningArea_M2);
                    Finite(partFPurgeVentilationData.RoomVolume_M3);
                    Finite(partFPurgeVentilationData.RoomFloorArea_M2);
                }
            }
        }

        private static void Finite(double? value)
        {
            if (value is null)
            {
                return;
            }

            Assert.False(double.IsNaN(value.Value));
            Assert.False(double.IsInfinity(value.Value));
        }

        // ------------------------------------------------------------------
        // Single house mode
        // ------------------------------------------------------------------

        /// <summary>
        /// A complete house with no zones at all: the whole model is one dwelling, which is the normal
        /// single-house workflow and is not itself reported as a problem.
        /// </summary>
        [Fact]
        public void CompleteHouseWithoutZones_IsSizedAsOneDwelling()
        {
            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(new PartFModel()
                .Space("Living Room", 25, 62.5)
                .Space("Bedroom 1", 15, 37.5)
                .Space("Bedroom 2", 12, 30)
                .Space("Bedroom 3", 10, 25)
                .Space("Hall", 10, 25)
                .Space("Kitchen", 12, 30)
                .Space("Bathroom", 6, 15)
                .Space("WC", 3, 7.5)
                .Partition("Living Room", "Hall", "D01")
                .Partition("Bedroom 1", "Hall", "D02")
                .Partition("Bedroom 2", "Hall", "D03")
                .Partition("Bedroom 3", "Hall", "D04")
                .Partition("Kitchen", "Hall", "D05")
                .Partition("Bathroom", "Hall", "D06")
                .Partition("WC", "Hall", "D07")
                .ExternalWall("Living Room")
                .ExternalWall("Bedroom 1")
                .ExternalWall("Bedroom 2")
                .ExternalWall("Bedroom 3"));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Equal(4, dwellingResult.HabitableRoomCount);
            Assert.Equal(3, dwellingResult.BedroomCount);
            Assert.Equal(93, dwellingResult.InternalFloorArea_M2, tolerance);

            //Table 1.3 gives 31 l/s for three bedrooms and 0.3 x 93 = 27.9 l/s, so the bedroom table
            //governs at 31 l/s. The Table 1.2 per-room high-rate minimums total kitchen 13 + bathroom 8 +
            //WC 6 = 27 l/s, which is reported and, being a per-room high-rate figure, does not govern.
            Assert.Equal(31, dwellingResult.BedroomOrHabitableRate_Lps, tolerance);
            Assert.Equal(27, dwellingResult.WetRoomMinimumTotal_Lps, tolerance);
            Assert.Equal(31, dwellingResult.ContinuousDesignSystemRate_Lps, tolerance);

            Assert.Equal(31, dwellingResult.TotalSupply_Lps, tolerance);
            Assert.Equal(31, dwellingResult.TotalExtract_Lps, tolerance);

            //Every habitable room has supply, every wet room has extract, and the hall carries the transfer
            //air between them without taking a terminal of its own.
            Assert.Equal(4, dwellingResult.ComplianceResult.SupplyTerminals.Count);
            Assert.Equal(7, dwellingResult.ComplianceResult.TransferPaths.Count);
            Assert.DoesNotContain(dwellingResult.ComplianceResult.Terminals, x => x.SpaceName == "Hall");
        }

        // ------------------------------------------------------------------
        // The model
        // ------------------------------------------------------------------

        /// <summary>
        /// The room mix, dimensions, zoning and partition topology of the example model, reproduced
        /// synthetically. Several pairs are separated by two partitions in the original, which is faithful
        /// here too, because two partitions between the same rooms are one adjacency and must not become
        /// two transfer routes.
        /// </summary>
        private static PartFModel Model()
        {
            return new PartFModel()
                .Space("Studio 1_0", 75, 300)
                .Space("Corridor_1", 366, 1464)
                .Space("Bathroom_2", 25, 100)
                .Space("Bedroom 2_3", 105, 420)
                .Space("Kitchen_4", 75, 300)
                .Space("Ensuite_5", 30, 120)
                .Space("Bedroom 2_6", 105, 420)
                .Space("Kitchen_7", 75, 300)
                .Space("Ensuite_8", 30, 120)

                //Within Flat 1, twice, exactly as the example model has it.
                .Partition("Studio 1_0", "Bathroom_2")
                .Partition("Studio 1_0", "Bathroom_2")

                //Within Flat 2 and Flat 3.
                .Partition("Bedroom 2_3", "Kitchen_4")
                .Partition("Kitchen_4", "Ensuite_5")
                .Partition("Kitchen_4", "Ensuite_5")
                .Partition("Bedroom 2_6", "Kitchen_7")
                .Partition("Kitchen_7", "Ensuite_8")
                .Partition("Kitchen_7", "Ensuite_8")

                //Onto the communal corridor.
                .Partition("Studio 1_0", "Corridor_1")
                .Partition("Bathroom_2", "Corridor_1")
                .Partition("Bedroom 2_3", "Corridor_1")
                .Partition("Kitchen_4", "Corridor_1")
                .Partition("Ensuite_5", "Corridor_1")
                .Partition("Bedroom 2_6", "Corridor_1")
                .Partition("Kitchen_7", "Corridor_1")
                .Partition("Ensuite_8", "Corridor_1")

                //Across flat boundaries.
                .Partition("Studio 1_0", "Bedroom 2_3")
                .Partition("Bathroom_2", "Bedroom 2_3")
                .Partition("Kitchen_4", "Bedroom 2_6")
                .Partition("Ensuite_5", "Bedroom 2_6")

                .Zone("Flat 1", zoneCategoryName, true, "Studio 1_0", "Bathroom_2")
                .Zone("Corridor", zoneCategoryName, false, "Corridor_1")
                .Zone("Flat 2", zoneCategoryName, true, "Bedroom 2_3", "Kitchen_4", "Ensuite_5")
                .Zone("Flat 3", zoneCategoryName, true, "Bedroom 2_6", "Kitchen_7", "Ensuite_8");
        }

        private static PartFCalculator Calculate()
        {
            return PartFAirflowNetworkTests.Calculator(Model(), zoneCategoryName);
        }

        private static PartFDwellingResult Dwelling(string name)
        {
            return Dwelling(Calculate(), name);
        }

        private static PartFDwellingResult Dwelling(PartFCalculator partFCalculator, string name)
        {
            return PartFAirflowNetworkTests.Dwelling(partFCalculator, name);
        }

        private static PartFVentilationTerminalRequirement Terminal(PartFDwellingResult partFDwellingResult, string spaceName, PartFTerminalRole partFTerminalRole)
        {
            PartFVentilationTerminalRequirement result = partFDwellingResult.ComplianceResult.Terminals.Find(x => x.SpaceName == spaceName && x.TerminalRole == partFTerminalRole);

            Assert.NotNull(result);

            return result;
        }

        private static double Flow(PartFComplianceResult partFComplianceResult, string name_Upstream, string name_Downstream)
        {
            PartFDoorTransferData result = partFComplianceResult.TransferPaths.Find(x => x.UpstreamSpaceName == name_Upstream && x.DownstreamSpaceName == name_Downstream);

            Assert.NotNull(result);

            return result.ContinuousDesignTransferFlowRate_Lps.Value;
        }

        private static int Count(string text, string value)
        {
            int result = 0;
            int index = 0;

            while ((index = text.IndexOf(value, index, System.StringComparison.Ordinal)) != -1)
            {
                result++;
                index += value.Length;
            }

            return result;
        }
    }
}
