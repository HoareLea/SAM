// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// What happens to the dwelling's internal transfer air when an Approved Document O round raises a
    /// room's DESIGN airflow above what Approved Document F originally sized it at.
    /// <para>
    /// <b>The gap this closes.</b> <c>PartFTransferAirRealizationTests</c> proves the transfer network
    /// balances correctly, but every one of its fixtures is prepared once, at Approved Document F's own
    /// sizing - it never raises a design duty above that baseline and re-checks. That is exactly the
    /// scenario a floor-plan reading "Design SUP 150.0 l/s" against a "TRA 63.0 l/s" Approved Document F tag
    /// exposed: nothing had ever pinned that the network re-solves against the NEW design duty when the
    /// preparation is re-run, or that Approved Document F's own figure stays exactly where it was.
    /// </para>
    /// <para>
    /// <b>What these tests prove, and what they deliberately do not touch.</b> Raising one room's design
    /// supply duty and re-preparing must: (1) route MORE transfer air to match, solved by the same
    /// <see cref="PartFAirflowNetwork"/> as before; (2) leave every node in balance at the NEW magnitude;
    /// (3) leave Approved Document F's own calculated requirement - a fresh <see cref="PartFCalculator"/>
    /// run over the SAME model - completely unchanged, because nothing about raising a design duty asks
    /// Approved Document F to recalculate anything. No terminal duty is adjusted to make a room balance,
    /// exactly as <c>PartFTransferAirRealizationTests.TheDesignTerminalDuties_AreNotAdjustedToMakeARoomBalance</c>
    /// already pins for the unraised case.
    /// </para>
    /// </summary>
    [Collection("SAM.Analytical.ActiveSetting default Part F data")]
    public class PartFDesignTransferDivergenceTests
    {
        private const string name_LivingRoom = "Living Room";
        private const string name_Bedroom = "Bedroom 1";
        private const string name_Kitchen = "Kitchen";
        private const string name_Bathroom = "Bathroom";
        private const string name_Zone = "Flat 1";

        private const double tolerance = 1e-9;

        /// <summary>
        /// Raising the bedroom's design supply well above its Approved Document F sizing, then re-preparing,
        /// routes the FULL raised duty through the dwelling - not the original Approved Document F figure -
        /// while every node stays in balance at the new magnitude.
        /// </summary>
        [Fact]
        public void RaisedDesignSupply_RoutesTheRaisedDutyThroughTheDwelling_NodesStayBalanced()
        {
            (AnalyticalModel analyticalModel, double partF_Bedroom_Supply_Lps) = PreparedWithPartFBaseline();

            //Well above the Approved Document F figure - an Approved Document O round headroom, not a
            //rounding difference.
            double designSupply_Lps = partF_Bedroom_Supply_Lps + 87.0;

            PartOIterationPreparation preparation = RePrepare(WithRaisedDesignSupply(analyticalModel, name_Bedroom, designSupply_Lps));

            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            //The bedroom's outward transfer now totals the RAISED duty, not the Approved Document F figure
            //that used to sit there.
            double outwardTotal_Lps = OutwardTransferTotal_Lps(adjacencyCluster, name_Bedroom);

            Assert.Equal(designSupply_Lps, outwardTotal_Lps, 6);
            Assert.True(System.Math.Abs(outwardTotal_Lps - partF_Bedroom_Supply_Lps) > 1.0, "The transfer air stayed at the Approved Document F figure instead of following the raised design duty.");

            //And every node is still exactly in balance, at the new magnitude.
            Dictionary<Guid, double> residual = adjacencyCluster.AirMovementResidual(adjacencyCluster.GetObjects<SpaceAirMovement>(), new List<AirHandlingUnit> { preparation.AirHandlingUnit });

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                Assert.True(residual.TryGetValue(space.Guid, out double value), string.Format("{0} carries no air movement at all after the raised duty was re-prepared.", space.Name));
                Assert.True(System.Math.Abs(value) <= tolerance, string.Format("{0} is out of balance by {1:0.######} l/s after the raised duty was re-prepared.", space.Name, value * 1000));
            }
        }

        /// <summary>
        /// The same raise, read back through the new query this task adds -
        /// <see cref="Query.DesignTransferFlowRate_Lps"/> - agrees with the raw movement totals, and reports
        /// which way the air actually goes.
        /// </summary>
        [Fact]
        public void DesignTransferFlowRate_Lps_AgreesWithTheRawMovementsAfterARaise()
        {
            (AnalyticalModel analyticalModel, double partF_Bedroom_Supply_Lps) = PreparedWithPartFBaseline();

            double designSupply_Lps = partF_Bedroom_Supply_Lps + 40.0;

            PartOIterationPreparation preparation = RePrepare(WithRaisedDesignSupply(analyticalModel, name_Bedroom, designSupply_Lps));
            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_LivingRoom = SpaceByName(adjacencyCluster, name_LivingRoom);
            Space space_Bedroom = SpaceByName(adjacencyCluster, name_Bedroom);

            //The star topology's only route out of the bedroom is through the living room.
            double? flow_Lps = adjacencyCluster.DesignTransferFlowRate_Lps(space_Bedroom.Guid, space_LivingRoom.Guid, out Guid guid_From, out Guid guid_To);

            Assert.NotNull(flow_Lps);
            Assert.Equal(designSupply_Lps, flow_Lps.Value, 6);

            //And it goes bedroom -> living room, never the other way, whichever order the two guids were asked in.
            Assert.Equal(space_Bedroom.Guid, guid_From);
            Assert.Equal(space_LivingRoom.Guid, guid_To);

            double? flow_Lps_Reversed = adjacencyCluster.DesignTransferFlowRate_Lps(space_LivingRoom.Guid, space_Bedroom.Guid, out Guid guid_From_Reversed, out Guid guid_To_Reversed);

            Assert.Equal(flow_Lps, flow_Lps_Reversed);
            Assert.Equal(guid_From, guid_From_Reversed);
            Assert.Equal(guid_To, guid_To_Reversed);
        }

        /// <summary>
        /// Two spaces with no transfer movement between them at all - here, the bedroom and the bathroom,
        /// which the star topology never connects directly - report null, never an invented zero.
        /// </summary>
        [Fact]
        public void DesignTransferFlowRate_Lps_IsNull_WhereNoMovementConnectsTheTwoSpaces()
        {
            (AnalyticalModel analyticalModel, double _) = PreparedWithPartFBaseline();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            Space space_Bedroom = SpaceByName(adjacencyCluster, name_Bedroom);
            Space space_Bathroom = SpaceByName(adjacencyCluster, name_Bathroom);

            Assert.Null(adjacencyCluster.DesignTransferFlowRate_Lps(space_Bedroom.Guid, space_Bathroom.Guid, out Guid guid_From, out Guid guid_To));
            Assert.Equal(Guid.Empty, guid_From);
            Assert.Equal(Guid.Empty, guid_To);
        }

        /// <summary>
        /// <b>The invariant this whole task rests on.</b> Raising the bedroom's design supply and
        /// re-preparing must not move Approved Document F's own figure by a single l/s - a fresh
        /// <see cref="PartFCalculator"/> run over the SAME model reports exactly what it reported before
        /// the design duty was ever touched. Design airflow and the Approved Document F requirement are
        /// two independent derivations, and neither may correct the other.
        /// </summary>
        [Fact]
        public void RaisedDesignSupply_LeavesThePartFRequirement_CompletelyUnchanged()
        {
            (AnalyticalModel analyticalModel, double partF_Bedroom_Supply_Lps) = PreparedWithPartFBaseline();

            PartOIterationPreparation preparation = RePrepare(WithRaisedDesignSupply(analyticalModel, name_Bedroom, partF_Bedroom_Supply_Lps + 200.0));
            Assert.Null(preparation.Refusal);

            //A completely independent Part F calculation, over the SAME (re-prepared) model, must land on
            //the exact same requirement it landed on before any design duty was raised.
            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();
            partFCalculator.AdjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate());

            Space space_Bedroom_Reassessed = SpaceByName(partFCalculator.AdjacencyCluster, name_Bedroom);
            PartFSpaceData partFSpaceData = space_Bedroom_Reassessed.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.NotNull(partFSpaceData);
            Assert.Equal(partF_Bedroom_Supply_Lps, partFSpaceData.ContinuousSupplyFlowRate_Lps.Value, 6);
        }

        /// <summary>
        /// Preparing twice at the raised duty is still idempotent - the same guarantee
        /// <c>PartFTransferAirRealizationTests.PreparingTwice_ReplacesTheTransferAirRatherThanAddingToIt</c>
        /// pins at baseline, now checked at a design magnitude Approved Document F never sized.
        /// </summary>
        [Fact]
        public void PreparingTwiceAtTheRaisedDuty_ReplacesRatherThanAdds()
        {
            (AnalyticalModel analyticalModel, double partF_Bedroom_Supply_Lps) = PreparedWithPartFBaseline();

            PartOIterationPreparation preparation = RePrepare(WithRaisedDesignSupply(analyticalModel, name_Bedroom, partF_Bedroom_Supply_Lps + 50.0));
            Assert.Null(preparation.Refusal);

            int count = preparation.AnalyticalModel.AdjacencyCluster.GetObjects<SpaceAirMovement>().Count;

            PartOIterationPreparation preparation_Again = RePrepare(preparation.AnalyticalModel);

            Assert.Null(preparation_Again.Refusal);
            Assert.Equal(count, preparation_Again.AnalyticalModel.AdjacencyCluster.GetObjects<SpaceAirMovement>().Count);

            //And it is still the same raised total, not a second copy's worth.
            Assert.Equal(
                partF_Bedroom_Supply_Lps + 50.0,
                OutwardTransferTotal_Lps(preparation_Again.AnalyticalModel.AdjacencyCluster, name_Bedroom),
                6);
        }

        // =================================================================================================
        // Fixture
        // =================================================================================================

        /// <summary>
        /// The model with one room's design supply duty raised to <paramref name="designSupply_Lps"/> and
        /// the dwelling rebalanced around it - the same transaction an Approved Document O round performs.
        /// <para>
        /// <b>Why the targeted transaction, and not <c>Modify.SetSpaceDesignFlowRate</c>.</b> The primitive
        /// writes exactly the terminal it is told to and rebalances nothing, so raising a bedroom's supply
        /// through it alone leaves the dwelling gaining air it never loses - which
        /// <c>Modify.PreparePartOIteration</c>'s conservation check rightly refuses, as
        /// <c>PartOVentilationUnitSelectionTests.ASupplyOnlyDesignChange_RefusesRatherThanUnbalancingTheDwelling</c>
        /// already pins. A test that raised a duty that way would be measuring an invalid design, not a
        /// raised one. <see cref="Modify.ApplyTargetedDesignAirFlow"/> is the operation Approved Document O
        /// optimisation is actually built on: it raises the targeted room and derives the matching extract
        /// in one all-or-nothing step, leaving a dwelling that still balances at the new magnitude.
        /// </para>
        /// <para>
        /// <b>Why this hands a model back rather than writing in place.</b>
        /// <see cref="AnalyticalModel.AdjacencyCluster"/> returns a fresh copy on every read, so writing to
        /// <c>analyticalModel.AdjacencyCluster</c> inline mutates a throwaway that is never put back - the
        /// raise would silently never reach the model, and every assertion resting on it would quietly
        /// re-measure the Approved Document F baseline it started from. The cluster is taken once, written
        /// once and put back once, exactly as <c>Modify.PrepareBaseMVHR</c> does for the same reason.
        /// </para>
        /// </summary>
        private static AnalyticalModel WithRaisedDesignSupply(AnalyticalModel analyticalModel, string spaceName, double designSupply_Lps)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            DwellingDesignAirFlowChange dwellingDesignAirFlowChange = adjacencyCluster.ApplyTargetedDesignAirFlow(SpaceByName(adjacencyCluster, spaceName), FlowClassification.Supply, designSupply_Lps);

            Assert.NotNull(dwellingDesignAirFlowChange);
            Assert.Empty(dwellingDesignAirFlowChange.Refusals);
            Assert.True(dwellingDesignAirFlowChange.Successful);

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        private static double OutwardTransferTotal_Lps(AdjacencyCluster adjacencyCluster, string spaceName)
        {
            Space space = SpaceByName(adjacencyCluster, spaceName);

            ObjectReference objectReference = new(space);

            double total_Lps = 0;

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>())
            {
                if (adjacencyCluster.AirMovementEndpoint(spaceAirMovement.From) is not Space || adjacencyCluster.AirMovementEndpoint(spaceAirMovement.To) is not Space)
                {
                    //Supply, extract or exhaust leg, not a transfer movement.
                    continue;
                }

                if (new ObjectReference(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.From)) == objectReference)
                {
                    total_Lps += spaceAirMovement.AirFlow * 1000.0;
                }
            }

            return total_Lps;
        }

        private static Space SpaceByName(AdjacencyCluster adjacencyCluster, string name)
        {
            return adjacencyCluster.GetSpaces().Find(x => x.Name == name);
        }

        private static PartOIterationPreparation RePrepare(AnalyticalModel analyticalModel)
        {
            List<Zone> zones = analyticalModel.GetZones();

            Dictionary<Guid, string> dictionary = [];
            foreach (Zone zone in zones)
            {
                dictionary[zone.Guid] = "MVRE";
            }

            return analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, dictionary);
        }

        /// <summary>
        /// The same star-topology flat <c>PartFTransferAirRealizationTests</c> uses, prepared once at
        /// Approved Document F's own baseline sizing, with that baseline bedroom supply figure returned
        /// alongside the model so a test can raise it by a stated amount and know exactly what it started
        /// from.
        /// </summary>
        private static (AnalyticalModel AnalyticalModel, double PartFBedroomSupply_Lps) PreparedWithPartFBaseline()
        {
            AdjacencyCluster adjacencyCluster = new();

            Dictionary<string, double> dictionary = new()
            {
                { name_LivingRoom, 30.0 },
                { name_Bedroom, 16.0 },
                { name_Kitchen, 12.0 },
                { name_Bathroom, 6.0 },
            };

            foreach (KeyValuePair<string, double> keyValuePair in dictionary)
            {
                Space space = new(keyValuePair.Key);

                space.SetValue(SpaceParameter.Area, keyValuePair.Value);
                space.SetValue(SpaceParameter.Volume, keyValuePair.Value * 2.5);

                InternalCondition internalCondition = new(keyValuePair.Key + " IC");
                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, "MVRE");

                space.InternalCondition = internalCondition;

                adjacencyCluster.AddObject(space);
            }

            Helpers.DwellingPartitions.Star(adjacencyCluster, name_LivingRoom, name_Bedroom, name_Kitchen, name_Bathroom);

            AnalyticalModel analyticalModel_Fixture = new("Part F Design Transfer Divergence Dwelling", null, null, null, adjacencyCluster, null, new ProfileLibrary("Part F Design Transfer Divergence Fixture"));

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();
            partFCalculator.AdjacencyCluster = analyticalModel_Fixture.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate(), "The Part F calculation did not run, so every test resting on it would be meaningless.");

            AdjacencyCluster adjacencyCluster_Sized = partFCalculator.AdjacencyCluster;

            Zone zone = new(name_Zone);
            adjacencyCluster_Sized.AddObject(zone);

            foreach (Space space_Existing in adjacencyCluster_Sized.GetSpaces())
            {
                adjacencyCluster_Sized.AddRelation(zone, space_Existing);
            }

            AnalyticalModel analyticalModel = new(analyticalModel_Fixture, adjacencyCluster_Sized);

            Space space_Bedroom = SpaceByName(adjacencyCluster_Sized, name_Bedroom);
            PartFSpaceData partFSpaceData = space_Bedroom.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.NotNull(partFSpaceData);
            Assert.NotNull(partFSpaceData.ContinuousSupplyFlowRate_Lps);

            double partF_Bedroom_Supply_Lps = partFSpaceData.ContinuousSupplyFlowRate_Lps.Value;

            PartOIterationPreparation preparation = RePrepare(analyticalModel);

            Assert.Null(preparation.Refusal);
            Assert.NotNull(preparation.AnalyticalModel);

            //The unraised baseline preparation's bedroom design duty must equal what Approved Document F
            //just sized, or raising it "above baseline" later would not test what it claims to.
            double designSupply_AtBaseline_Lps = Analytical.Query.VentilationTerminalDesignDuty_Lps(
                preparation.AnalyticalModel.AdjacencyCluster.VentilationTerminals(SpaceByName(preparation.AnalyticalModel.AdjacencyCluster, name_Bedroom)),
                FlowClassification.Supply) ?? 0;

            Assert.Equal(partF_Bedroom_Supply_Lps, designSupply_AtBaseline_Lps, 6);

            return (preparation.AnalyticalModel, partF_Bedroom_Supply_Lps);
        }
    }
}
