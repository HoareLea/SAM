// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>A native acceptance screenshot raised a concern</b> that both the Part F and Design transfer-air
    /// arrows might be pointing the wrong way in a straight three-room chain - a supplied room, a
    /// pass-through room, and an extracted room. That screenshot was traced to an invalid throwaway
    /// fixture (an unbalanced Design duty with no confirmed pass-through room) and cannot be read as
    /// evidence of a direction defect either way - see <c>transfer-air-direction-acceptance</c> in the
    /// project's own notes.
    /// <para>
    /// What the investigation DID find real: every existing transfer-air fixture
    /// (<see cref="PartFTransferAirRealizationTests"/>, <see cref="PartFTransferAirDwellingScopeTests"/>)
    /// wires its rooms through <c>Helpers.DwellingPartitions.Star</c> - every room adjacent to one hub,
    /// never to another non-hub room. That topology can never produce a connection whose Upstream space is
    /// itself the Downstream space of another connection, which is exactly the shape of a genuine chain and
    /// exactly the shape the screenshot's fixture was trying, and failing, to build. This fixture is a
    /// straight line instead, built from nothing but terminal duties and the production solve, so a
    /// direction defect that only shows up over two hops in series has somewhere real to fail.
    /// </para>
    /// <para>
    /// The middle room is named <c>Hall</c> deliberately: Approved Document F's own rule set classifies a
    /// hall as <c>Category: Transfer</c>, <c>IsTerminalSpace: false</c> - a genuine pass-through, confirmed
    /// below from the CALCULATED state rather than assumed from the room's name.
    /// </para>
    /// </summary>
    [Collection("SAM.Analytical.ActiveSetting default Part F data")]
    public class PartFTransferAirChainDirectionTests
    {
        private const string name_Supply = "Living Room";

        private const string name_Through = "Hall";

        private const string name_Extract = "Bathroom";

        private const string name_Zone = "Flat 1";

        [Fact]
        public void PartF_UpstreamAndDownstream_FollowTheChainInOrder()
        {
            (PartOIterationPreparation preparation, PartFCalculator partFCalculator) = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Supply = SpaceByName(adjacencyCluster, name_Supply);
            Space space_Through = SpaceByName(adjacencyCluster, name_Through);
            Space space_Extract = SpaceByName(adjacencyCluster, name_Extract);

            PartFComplianceResult complianceResult = ComplianceResult(partFCalculator, space_Supply);

            //The pass-through claim, proved from the CALCULATED state rather than assumed from the room's
            //name: Hall carries no Part F terminal requirement of its own, so every litre it moves is
            //transfer air passing through it, never air it is itself the source or sink for.
            Assert.DoesNotContain(complianceResult.Terminals ?? [], x => x.SpaceGuid == space_Through.Guid);

            PartFDoorTransferData leg_1 = Leg(complianceResult, space_Supply, space_Through);
            PartFDoorTransferData leg_2 = Leg(complianceResult, space_Through, space_Extract);

            Assert.True(leg_1.UpstreamSpaceGuid == space_Supply.Guid,
                string.Format("Part F: the {0} -> {1} leg should carry air FROM {0}, but UpstreamSpaceGuid resolves to {2}.",
                    name_Supply, name_Through, NameOf(adjacencyCluster, leg_1.UpstreamSpaceGuid)));
            Assert.True(leg_1.DownstreamSpaceGuid == space_Through.Guid,
                string.Format("Part F: the {0} -> {1} leg should carry air TO {1}, but DownstreamSpaceGuid resolves to {2}.",
                    name_Supply, name_Through, NameOf(adjacencyCluster, leg_1.DownstreamSpaceGuid)));

            Assert.True(leg_2.UpstreamSpaceGuid == space_Through.Guid,
                string.Format("Part F: the {0} -> {1} leg should carry air FROM {0}, but UpstreamSpaceGuid resolves to {2}.",
                    name_Through, name_Extract, NameOf(adjacencyCluster, leg_2.UpstreamSpaceGuid)));
            Assert.True(leg_2.DownstreamSpaceGuid == space_Extract.Guid,
                string.Format("Part F: the {0} -> {1} leg should carry air TO {1}, but DownstreamSpaceGuid resolves to {2}.",
                    name_Through, name_Extract, NameOf(adjacencyCluster, leg_2.DownstreamSpaceGuid)));
        }

        [Fact]
        public void Design_FromAndTo_FollowTheChainInOrder()
        {
            (PartOIterationPreparation preparation, _) = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Supply = SpaceByName(adjacencyCluster, name_Supply);
            Space space_Through = SpaceByName(adjacencyCluster, name_Through);
            Space space_Extract = SpaceByName(adjacencyCluster, name_Extract);

            //The Design authority's own pass-through proof: Hall carries no Design supply or extract
            //terminal, in either direction - confirmed from the prepared model, not assumed.
            List<VentilationTerminal> ventilationTerminals_Through = adjacencyCluster.VentilationTerminals(space_Through);
            Assert.Null(Analytical.Query.VentilationTerminalDesignDuty_Lps(ventilationTerminals_Through, FlowClassification.Supply));
            Assert.Null(Analytical.Query.VentilationTerminalDesignDuty_Lps(ventilationTerminals_Through, FlowClassification.Extract));

            //PreparePartOIteration has already realized the transfer-air SpaceAirMovements onto this
            //adjacencyCluster (see PartFTransferAirRealizationTests) - nothing more to add here.
            double? flowRate_1 = adjacencyCluster.DesignTransferFlowRate_Lps(space_Supply.Guid, space_Through.Guid, out Guid from_1, out Guid to_1);
            double? flowRate_2 = adjacencyCluster.DesignTransferFlowRate_Lps(space_Through.Guid, space_Extract.Guid, out Guid from_2, out Guid to_2);

            Assert.True(flowRate_1 is not null, string.Format("No Design transfer movement was found between {0} and {1}.", name_Supply, name_Through));
            Assert.True(flowRate_2 is not null, string.Format("No Design transfer movement was found between {0} and {1}.", name_Through, name_Extract));

            Assert.True(from_1 == space_Supply.Guid,
                string.Format("Design: the {0} <-> {1} connection should carry air FROM {0}, but From resolves to {2}.",
                    name_Supply, name_Through, NameOf(adjacencyCluster, from_1)));
            Assert.True(to_1 == space_Through.Guid,
                string.Format("Design: the {0} <-> {1} connection should carry air TO {1}, but To resolves to {2}.",
                    name_Supply, name_Through, NameOf(adjacencyCluster, to_1)));

            Assert.True(from_2 == space_Through.Guid,
                string.Format("Design: the {0} <-> {1} connection should carry air FROM {0}, but From resolves to {2}.",
                    name_Through, name_Extract, NameOf(adjacencyCluster, from_2)));
            Assert.True(to_2 == space_Extract.Guid,
                string.Format("Design: the {0} <-> {1} connection should carry air TO {1}, but To resolves to {2}.",
                    name_Through, name_Extract, NameOf(adjacencyCluster, to_2)));
        }

        [Fact]
        public void PartFAndDesign_AgreeOnDirection_ForEveryLegOfTheChain()
        {
            (PartOIterationPreparation preparation, PartFCalculator partFCalculator) = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Supply = SpaceByName(adjacencyCluster, name_Supply);
            Space space_Through = SpaceByName(adjacencyCluster, name_Through);
            Space space_Extract = SpaceByName(adjacencyCluster, name_Extract);

            PartFComplianceResult complianceResult = ComplianceResult(partFCalculator, space_Supply);

            foreach ((Space upstream, Space downstream) in new[] { (space_Supply, space_Through), (space_Through, space_Extract) })
            {
                PartFDoorTransferData leg = Leg(complianceResult, upstream, downstream);

                adjacencyCluster.DesignTransferFlowRate_Lps(upstream.Guid, downstream.Guid, out Guid guid_From, out Guid guid_To);

                Assert.True(leg.UpstreamSpaceGuid == guid_From,
                    string.Format("Part F says {0} -> {1}; Design says {2} -> {3}. The two authorities must agree on physical direction even though they may disagree on rate.",
                        NameOf(adjacencyCluster, leg.UpstreamSpaceGuid), NameOf(adjacencyCluster, leg.DownstreamSpaceGuid),
                        NameOf(adjacencyCluster, guid_From), NameOf(adjacencyCluster, guid_To)));
            }
        }

        // =================================================================================================
        // Fixture
        // =================================================================================================

        private static string NameOf(AdjacencyCluster adjacencyCluster, Guid guid)
        {
            return adjacencyCluster.GetObject<Space>(guid)?.Name ?? guid.ToString();
        }

        private static PartFDoorTransferData Leg(PartFComplianceResult complianceResult, Space space_1, Space space_2)
        {
            PartFDoorTransferData result = (complianceResult.TransferPaths ?? []).Find(x =>
                (x.UpstreamSpaceGuid == space_1.Guid && x.DownstreamSpaceGuid == space_2.Guid) ||
                (x.UpstreamSpaceGuid == space_2.Guid && x.DownstreamSpaceGuid == space_1.Guid));

            Assert.True(result is not null, string.Format("No Part F transfer route was found between {0} and {1}.", space_1.Name, space_2.Name));

            return result;
        }

        private static PartFComplianceResult ComplianceResult(PartFCalculator partFCalculator, Space space_Supply)
        {
            PartFDwellingResult dwellingResult = partFCalculator.DwellingResults.Find(x => x.ComplianceResult?.TransferPaths is not null
                && x.ComplianceResult.TransferPaths.Exists(y => y.UpstreamSpaceGuid == space_Supply.Guid || y.DownstreamSpaceGuid == space_Supply.Guid));

            Assert.True(dwellingResult is not null, "No dwelling result carried a transfer route touching the supplied room.");

            return dwellingResult.ComplianceResult;
        }

        private static Space SpaceByName(AdjacencyCluster adjacencyCluster, string name)
        {
            return adjacencyCluster.GetSpaces().Find(x => x.Name == name);
        }

        private static (PartOIterationPreparation, PartFCalculator) Prepared()
        {
            (AnalyticalModel analyticalModel, PartFCalculator partFCalculator) = Model();

            List<Zone> zones = analyticalModel.GetZones();
            Assert.NotEmpty(zones);

            Dictionary<Guid, string> dictionary = [];
            foreach (Zone zone in zones)
            {
                dictionary[zone.Guid] = "MVRE";
            }

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, dictionary);

            Assert.Null(preparation.Refusal);
            Assert.NotNull(preparation.AnalyticalModel);

            return (preparation, partFCalculator);
        }

        /// <summary>
        /// A straight chain, not a star: Living Room (supplied, not extracted) is adjacent only to Hall (a
        /// circulation space Approved Document F gives no terminal to - see the class summary), which is
        /// adjacent only to Bathroom (extracted, not supplied). Living Room and Bathroom share no partition
        /// of their own, so the only route between them crosses two connections in series - the one
        /// topology a hub-and-spoke fixture can never produce.
        /// </summary>
        private static (AnalyticalModel, PartFCalculator) Model()
        {
            AdjacencyCluster adjacencyCluster = new();

            Dictionary<string, double> dictionary = new()
            {
                { name_Supply, 30.0 },
                { name_Through, 16.0 },
                { name_Extract, 6.0 },
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

            Helpers.DwellingPartitions.Partition(adjacencyCluster, name_Supply, name_Through, 0);
            Helpers.DwellingPartitions.Partition(adjacencyCluster, name_Through, name_Extract, 10);

            AnalyticalModel analyticalModel = new("Part F Transfer Air Chain Dwelling", null, null, null, adjacencyCluster, null, new ProfileLibrary("Part F Transfer Air Chain Fixture"));

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();
            Assert.NotNull(partFCalculator);

            partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate(), "The Part F calculation did not run, so every test resting on it would be meaningless.");

            AdjacencyCluster adjacencyCluster_Sized = partFCalculator.AdjacencyCluster;

            Zone zone = new(name_Zone);
            adjacencyCluster_Sized.AddObject(zone);

            foreach (Space space_Existing in adjacencyCluster_Sized.GetSpaces())
            {
                adjacencyCluster_Sized.AddRelation(zone, space_Existing);
            }

            return (new AnalyticalModel(analyticalModel, adjacencyCluster_Sized), partFCalculator);
        }
    }
}
