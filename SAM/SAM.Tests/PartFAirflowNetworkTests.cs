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
    /// Tests for the internal transfer air network: which spaces are connected, how much air crosses each
    /// internal route, and where a route is deliberately not created.
    /// </summary>
    /// <remarks>
    /// Approved Document F, Volume 1: Dwellings (2021 edition, for use in England) paragraph 1.25
    /// (page 10) requires internal doors to allow air to flow through the dwelling. Under a balanced
    /// mechanical ventilation with heat recovery design, supply enters the habitable rooms
    /// (paragraph 1.67) and extract leaves from the wet rooms (paragraph 1.70), so every litre supplied
    /// has to cross the dwelling to reach an extract terminal.
    /// <para>
    /// Where the dwelling's connections form a tree, conservation of air flow fixes every transfer flow
    /// exactly and no engineering choice is involved. Where they contain a loop, more than one valid split
    /// exists, Approved Document F does not choose between them, and the deterministic allocation strategy
    /// is applied and reported as such.
    /// </para>
    /// </remarks>
    public class PartFAirflowNetworkTests
    {
        private const string dataFileName = "SAM_PartFSpaceRulesUKDwellingsMVHR.json";

        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // Route topology
        // ------------------------------------------------------------------

        /// <summary>
        /// A single route between one net-supply space and one net-extract space is uniquely determined,
        /// and carries exactly the net flow of either end.
        /// </summary>
        [Fact]
        public void UniqueRoute_IsUniquelyDeterminedAndCarriesTheWholeNetFlow()
        {
            PartFComplianceResult complianceResult = Calculate(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01"));

            PartFDoorTransferData partFDoorTransferData = Assert.Single(complianceResult.TransferPaths);

            Assert.Equal(PartFTransferRouteStatus.UniquelyDetermined, partFDoorTransferData.RouteStatus);
            Assert.Equal(8, partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps.Value, tolerance);
            Assert.Contains("conservation of air flow", partFDoorTransferData.CalculationSource);
        }

        /// <summary>
        /// A chain of rooms is still a tree, so every flow along it is fixed. Bedroom +63, kitchen -55,
        /// ensuite -8: the bedroom passes its whole 63 l/s to the kitchen, which extracts 55 and passes
        /// the remaining 8 on to the ensuite.
        /// </summary>
        [Fact]
        public void ChainOfRooms_IsUniquelyDeterminedAtEveryStep()
        {
            PartFComplianceResult complianceResult = Calculate(new PartFModel()
                .Space("Bedroom 1", 105, 420)
                .Space("Kitchen", 75, 300)
                .Space("Ensuite", 30, 120)
                .Partition("Bedroom 1", "Kitchen", "D01")
                .Partition("Kitchen", "Ensuite", "D02"));

            Assert.Equal(63, complianceResult.ContinuousDesignSystemRate_Lps, tolerance);

            Assert.Equal(63, Flow(complianceResult, "Bedroom 1", "Kitchen"), tolerance);
            Assert.Equal(8, Flow(complianceResult, "Kitchen", "Ensuite"), tolerance);

            Assert.All(complianceResult.TransferPaths, x => Assert.Equal(PartFTransferRouteStatus.UniquelyDetermined, x.RouteStatus));
        }

        /// <summary>
        /// An internal corridor takes no terminal of its own, but air flows through it, which is exactly
        /// what paragraph 1.25 is about. It must be a node in the network.
        /// </summary>
        [Fact]
        public void InternalCorridor_CarriesTransferAirWithoutTakingATerminal()
        {
            PartFComplianceResult complianceResult = Calculate(new PartFModel()
                .Space("Bedroom 1", 40, 100)
                .Space("Hall", 10, 25)
                .Space("Bathroom", 8, 20)
                .Partition("Bedroom 1", "Hall", "D01")
                .Partition("Hall", "Bathroom", "D02"));

            //The hall takes no terminal at all.
            Assert.DoesNotContain(complianceResult.Terminals, x => x.SpaceName == "Hall");

            //But it carries the whole dwelling's transfer air.
            double rate = complianceResult.ContinuousDesignSystemRate_Lps;

            Assert.Equal(rate, Flow(complianceResult, "Bedroom 1", "Hall"), tolerance);
            Assert.Equal(rate, Flow(complianceResult, "Hall", "Bathroom"), tolerance);
        }

        /// <summary>
        /// Two parallel paths between the same supply and extract spaces put a loop in the network, so
        /// more than one valid split exists. The deterministic allocation strategy is applied and every
        /// route says so, because that split is a design decision rather than a regulatory value.
        /// </summary>
        [Fact]
        public void MultipleRoutes_UseTheAllocationStrategyAndSaySo()
        {
            PartFComplianceResult complianceResult = Calculate(new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Hall", 10, 25)
                .Space("Utility Room", 8, 20)
                .Partition("Living Room", "Hall", "D01")
                .Partition("Hall", "Utility Room", "D02")
                .Partition("Living Room", "Utility Room", "D03"));

            Assert.Contains(complianceResult.TransferPaths, x => x.RouteStatus == PartFTransferRouteStatus.AllocationStrategy);

            //The total still reaches the utility room: the split is uncertain, the total is not.
            double intoUtility = complianceResult.TransferPaths
                .Where(x => x.DownstreamSpaceName == "Utility Room")
                .Sum(x => x.ContinuousDesignTransferFlowRate_Lps ?? 0);

            Assert.Equal(complianceResult.ContinuousDesignSystemRate_Lps, intoUtility, tolerance);

            Assert.Contains(complianceResult.TransferPaths, x => x.CalculationSource.Contains("more than one valid split"));
        }

        /// <summary>
        /// Two doors between the same two rooms share the route's air. Approved Document F says nothing
        /// about how it divides, so the split is equal and both are reported as ambiguous - and each still
        /// has to provide the paragraph 1.25 free area in its own right.
        /// </summary>
        [Fact]
        public void MultipleDoorsOnOneRoute_ShareTheFlowAndAreReportedAsAmbiguous()
        {
            PartFComplianceResult complianceResult = Calculate(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01")
                .Partition("Studio", "Bathroom", "D02"));

            Assert.Equal(2, complianceResult.TransferPaths.Count);
            Assert.All(complianceResult.TransferPaths, x => Assert.Equal(PartFTransferRouteStatus.Ambiguous, x.RouteStatus));
            Assert.All(complianceResult.TransferPaths, x => Assert.Equal(4, x.ContinuousDesignTransferFlowRate_Lps.Value, tolerance));

            //Both still carry the full paragraph 1.25 requirement.
            Assert.All(complianceResult.TransferPaths, x => Assert.Equal(7600, x.MinimumRequiredFreeArea_mm2.Value, tolerance));
        }

        /// <summary>
        /// The engineer can fix a transfer flow explicitly, which is why the override exists: Approved
        /// Document F does not specify a unique value where several paths exist. The override wins over
        /// the calculated allocation, and the setback flow follows it.
        /// </summary>
        [Fact]
        public void UserDefinedSplit_OverridesTheCalculatedAllocation()
        {
            PartFComplianceResult complianceResult = Calculate(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01")
                .DoorInput("D01", transferFlowRateOverride_Lps: 6.5));

            PartFDoorTransferData partFDoorTransferData = Assert.Single(complianceResult.TransferPaths);

            Assert.Equal(PartFTransferRouteStatus.UserOverridden, partFDoorTransferData.RouteStatus);
            Assert.Equal(6.5, partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps.Value, tolerance);
            Assert.Equal(6.5 * 0.3, partFDoorTransferData.SetbackTransferFlowRate_Lps.Value, tolerance);
        }

        // ------------------------------------------------------------------
        // What must NOT become a route
        // ------------------------------------------------------------------

        /// <summary>
        /// An external element has one adjacent space, so it is never an internal transfer route. An
        /// external door is not a paragraph 1.25 internal door.
        /// </summary>
        [Fact]
        public void ExternalWall_IsNeverATransferRoute()
        {
            PartFComplianceResult complianceResult = Calculate(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01")
                .ExternalWall("Studio")
                .ExternalWall("Bathroom"));

            Assert.Single(complianceResult.TransferPaths);
            Assert.Equal("D01", Assert.Single(complianceResult.TransferPaths).Name);
        }

        /// <summary>
        /// A communal corridor is shared between dwellings and belongs to none of them, so it is excluded
        /// from the dwelling entirely and can never carry its transfer air.
        /// </summary>
        [Fact]
        public void CommunalCorridor_IsExcludedFromTheNetwork()
        {
            PartFCalculator partFCalculator = Calculator(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Space("Communal Corridor", 40, 160)
                .Partition("Studio", "Communal Corridor", "D01")
                .Partition("Bathroom", "Communal Corridor", "D02")
                .Partition("Studio", "Bathroom", "D03"));

            PartFDwellingResult dwellingResult = Assert.Single(partFCalculator.DwellingResults);

            Assert.Contains("Communal Corridor", dwellingResult.NonDwellingSpaceNames);

            //Only the route between the two dwelling spaces survives.
            PartFDoorTransferData partFDoorTransferData = Assert.Single(dwellingResult.ComplianceResult.TransferPaths);
            Assert.Equal("D03", partFDoorTransferData.Name);
        }

        /// <summary>
        /// A partition between two separate flats is never an edge in either flat's network, so no
        /// dwelling's transfer air can cross into another's. This is what stops a block being solved as
        /// one large dwelling.
        /// </summary>
        [Fact]
        public void PartitionBetweenTwoFlats_IsNeverATransferRoute()
        {
            PartFCalculator partFCalculator = Calculator(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Space("Bedroom 2", 105, 420)
                .Space("Kitchen", 75, 300)
                .Partition("Studio", "Bathroom", "D01")
                .Partition("Bedroom 2", "Kitchen", "D02")
                .Partition("Bathroom", "Bedroom 2", "D03")
                .Zone("Flat 1", "Flats", true, "Studio", "Bathroom")
                .Zone("Flat 2", "Flats", true, "Bedroom 2", "Kitchen"), "Flats");

            foreach (PartFDwellingResult dwellingResult in partFCalculator.DwellingResults)
            {
                Assert.DoesNotContain(dwellingResult.ComplianceResult.TransferPaths, x => x.Name == "D03");
            }

            Assert.Equal("D01", Assert.Single(Dwelling(partFCalculator, "Flat 1").ComplianceResult.TransferPaths).Name);
            Assert.Equal("D02", Assert.Single(Dwelling(partFCalculator, "Flat 2").ComplianceResult.TransferPaths).Name);
        }

        /// <summary>
        /// A room with a terminal but no connection to the rest of its dwelling is reported: paragraph
        /// 1.25 requires air to flow THROUGH the dwelling, and it cannot reach that room.
        /// </summary>
        [Fact]
        public void DisconnectedRoom_IsReported()
        {
            PartFCalculator partFCalculator = Calculator(new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Bathroom", 8, 20)
                .Space("Utility Room", 8, 20)
                .Partition("Living Room", "Bathroom", "D01"));

            //The utility room takes an extract terminal but adjoins nothing.
            Assert.Contains(partFCalculator.Warnings, x =>
                x.Contains("cannot reach anywhere it could go") &&
                x.Contains("Utility Room"));
        }

        /// <summary>
        /// A dwelling of several rooms with no internal separating element at all is a gap in the model,
        /// not a dwelling whose rooms do not adjoin. It is reported once, as a note, rather than as one
        /// warning per room, and the paragraph 1.25 checks report that they could not be assessed rather
        /// than passing or failing.
        /// </summary>
        [Fact]
        public void DwellingWithNoModelledPartitions_IsReportedOnceAndCannotBeAssessed()
        {
            PartFCalculator partFCalculator = Calculator(new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Bedroom 1", 20, 50)
                .Space("Kitchen", 12, 30)
                .Space("Bathroom", 8, 20));

            PartFComplianceResult complianceResult = Assert.Single(partFCalculator.DwellingResults).ComplianceResult;

            Assert.True(complianceResult.HasNoInternalAdjacency);
            Assert.Empty(complianceResult.TransferPaths);

            Assert.Contains(partFCalculator.Remarks, x => x.Contains("No internal separating element was found"));
            Assert.DoesNotContain(partFCalculator.Warnings, x => x.Contains("cannot reach anywhere it could go"));

            PartFComplianceCheck check = Check(complianceResult, "Internal doors allow air to flow through the dwelling");

            Assert.Equal(PartFComplianceStatus.CannotBeDetermined, check.Status);
        }

        // ------------------------------------------------------------------
        // High and setback conditions
        // ------------------------------------------------------------------

        /// <summary>
        /// The transfer network is re-solved at the high rate rather than scaled, because boosting the
        /// extract terminals changes the balance between rooms, not just its magnitude.
        /// </summary>
        [Fact]
        public void HighRateTransfer_IsSolvedSeparatelyFromTheContinuousCondition()
        {
            PartFComplianceResult complianceResult = Calculate(new PartFModel()
                .Space("Living Room", 30, 75)
                .Space("Bedroom 1", 20, 50)
                .Space("Bathroom", 5, 12)
                .Space("WC", 3, 7)
                .Partition("Living Room", "Bathroom", "D01")
                .Partition("Bedroom 1", "WC", "D02"));

            Assert.All(complianceResult.TransferPaths, x => Assert.NotNull(x.HighTransferFlowRate_Lps));

            //Total transfer at the high rate matches the total high extract, exactly as it does at the
            //continuous condition.
            double transfer_High = complianceResult.TransferPaths.Sum(x => x.HighTransferFlowRate_Lps ?? 0);

            Assert.Equal(complianceResult.TotalHighExtract_Lps, transfer_High, tolerance);
        }

        /// <summary>Setback transfer is the continuous transfer scaled by the setback factor, nothing more.</summary>
        [Fact]
        public void SetbackTransfer_IsTheContinuousTransferScaled()
        {
            PartFComplianceResult complianceResult = Calculate(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01"));

            PartFDoorTransferData partFDoorTransferData = Assert.Single(complianceResult.TransferPaths);

            Assert.Equal(partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps.Value * 0.3, partFDoorTransferData.SetbackTransferFlowRate_Lps.Value, tolerance);
        }

        // ------------------------------------------------------------------
        // Determinism
        // ------------------------------------------------------------------

        /// <summary>
        /// The same model gives the same door flows every run. An allocation that depended on dictionary
        /// ordering would give a different answer each time the model was opened.
        /// </summary>
        [Fact]
        public void Solve_IsDeterministic()
        {
            List<string> results = [];

            for (int i = 0; i < 5; i++)
            {
                PartFComplianceResult complianceResult = Calculate(new PartFModel()
                    .Space("Living Room", 40, 100)
                    .Space("Hall", 10, 25)
                    .Space("Bathroom", 8, 20)
                    .Space("WC", 3, 8)
                    .Partition("Living Room", "Hall", "D01")
                    .Partition("Hall", "Bathroom", "D02")
                    .Partition("Hall", "WC", "D03")
                    .Partition("Bathroom", "WC", "D04"));

                results.Add(string.Join("|", complianceResult.TransferPaths.ConvertAll(x => string.Format("{0}:{1:0.000000}", x.Name, x.ContinuousDesignTransferFlowRate_Lps))));
            }

            Assert.Single(results.Distinct());
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        internal static PartFCalculator Calculator(PartFModel partFModel, string zoneCategoryName = null)
        {
            PartFCalculator result = new(Analytical.Create.PartFData(Fixtures.GetPath(dataFileName)))
            {
                AdjacencyCluster = partFModel.AdjacencyCluster,
            };

            Assert.True(zoneCategoryName is null ? result.Calculate() : result.Calculate(zoneCategoryName));

            return result;
        }

        internal static PartFComplianceResult Calculate(PartFModel partFModel)
        {
            return Assert.Single(Calculator(partFModel).DwellingResults).ComplianceResult;
        }

        internal static PartFDwellingResult Dwelling(PartFCalculator partFCalculator, string name)
        {
            PartFDwellingResult result = partFCalculator.DwellingResults.Find(x => x.Name == name);

            Assert.NotNull(result);

            return result;
        }

        internal static PartFComplianceCheck Check(PartFComplianceResult partFComplianceResult, string name)
        {
            PartFComplianceCheck result = partFComplianceResult.Checks.Find(x => x.Name == name);

            Assert.NotNull(result);

            return result;
        }

        private static double Flow(PartFComplianceResult partFComplianceResult, string name_Upstream, string name_Downstream)
        {
            PartFDoorTransferData result = partFComplianceResult.TransferPaths.Find(x => x.UpstreamSpaceName == name_Upstream && x.DownstreamSpaceName == name_Downstream);

            Assert.NotNull(result);

            return result.ContinuousDesignTransferFlowRate_Lps.Value;
        }
    }
}
