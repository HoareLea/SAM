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
    /// <b>Which spaces the dwelling's transfer air is allowed to cross, and which it is not.</b>
    /// <para>
    /// A balanced heat recovery dwelling supplies its habitable rooms and extracts its wet rooms, so the air
    /// has to cross the flat to get from one to the other - and in almost every real plan it crosses an
    /// internal hall on the way. That hall carries no design ventilation terminal, because Approved Document
    /// F sizes nothing for circulation, so it is not a space the ventilation system serves and it is
    /// correctly absent from the system's own space relation.
    /// </para>
    /// <para>
    /// It is not, however, absent from the dwelling. Solving the paragraph 1.25 airflow network over the
    /// served spaces alone deletes the middle of every route through it, and a bedroom and a bathroom that
    /// open off the same hall are then reported as having no internal connection at all. These tests pin the
    /// hall into the network - and pin the boundary that stops the network swallowing the building: a
    /// communal corridor belongs to no dwelling, and no flat's air may be routed through one.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Shares a collection with the other readers of the default Part F rule set, so the two never run at
    /// the same time: the rule set is reached through the process-wide <c>ActiveSetting.Setting</c> and its
    /// stored <c>PartFData</c> is shared by reference between every <c>PartFCalculator</c> built from it.
    /// </remarks>
    [Collection("SAM.Analytical.ActiveSetting default Part F data")]
    public class PartFTransferAirDwellingScopeTests
    {
        private const string name_Bedroom = "Bedroom 1";

        /// <summary>Maps to <c>SpaceUse.Circulation</c>, which Approved Document F sizes no terminal for.</summary>
        private const string name_Hall = "Hall";

        private const string name_Bathroom = "Bathroom";

        private const string name_Kitchen = "Kitchen";

        /// <summary>Maps to <c>SpaceUse.CommunalCirculation</c> - outside every dwelling.</summary>
        private const string name_CommunalCorridor = "Communal Corridor";

        private const string name_Zone_Dwelling = "Flat 1";

        private const string name_Zone_Common = "Common Parts";

        private const double tolerance = 1e-9;

        // =================================================================================================
        // 1. The zero-terminal hall is part of the dwelling's transfer network
        // =================================================================================================

        /// <summary>
        /// <b>Transfer air passes through a room with no terminal of its own.</b> The hall receives the
        /// bedroom's supply and passes it on; it is neither a source nor a sink, and it conserves exactly.
        /// </summary>
        [Fact]
        public void TransferAir_PassesThroughAZeroTerminalInternalHall()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Hall = SpaceByName(adjacencyCluster, name_Hall);

            List<SpaceAirMovement> inward = Transfers(adjacencyCluster, space_Hall, false);
            List<SpaceAirMovement> outward = Transfers(adjacencyCluster, space_Hall, true);

            Assert.NotEmpty(inward);
            Assert.NotEmpty(outward);

            //Every one of them has a space at both ends: the hall is an internal node, not a way outside.
            foreach (SpaceAirMovement spaceAirMovement in inward)
            {
                Assert.IsType<Space>(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.From));
            }

            foreach (SpaceAirMovement spaceAirMovement in outward)
            {
                Assert.IsType<Space>(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.To));
            }

            Dictionary<Guid, double> dictionary = adjacencyCluster.AirMovementResidual(
                adjacencyCluster.GetObjects<SpaceAirMovement>(),
                new List<AirHandlingUnit>() { preparation.AirHandlingUnit });

            Assert.True(dictionary.TryGetValue(space_Hall.Guid, out double residual), "The hall carries no air movement at all, so the transfer air is not routed through it.");
            Assert.True(System.Math.Abs(residual) <= tolerance, string.Format("The hall is out of balance by {0:0.######} l/s. TAS refuses a zone that gains air it never loses, whether or not that zone has a terminal.", residual * 1000));
        }

        /// <summary>
        /// <b>The flow divides at the hall.</b> One movement arrives carrying the bedroom's whole supply and
        /// two leave, each carrying the duty of the wet room it feeds. Nothing downstream may assume a
        /// transfer movement has a matching partner.
        /// </summary>
        [Fact]
        public void TheFlow_SplitsAtTheHallBetweenTheWetRooms()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Hall = SpaceByName(adjacencyCluster, name_Hall);

            List<SpaceAirMovement> inward = Transfers(adjacencyCluster, space_Hall, false);
            List<SpaceAirMovement> outward = Transfers(adjacencyCluster, space_Hall, true);

            SpaceAirMovement spaceAirMovement_In = Assert.Single(inward);

            Assert.Equal(name_Bedroom, adjacencyCluster.AirMovementEndpoint(spaceAirMovement_In.From).Name);
            Assert.Equal(Duty(adjacencyCluster, name_Bedroom, FlowClassification.Supply) / 1000.0, spaceAirMovement_In.AirFlow, 9);

            Assert.Equal(2, outward.Count);

            double total = 0;
            List<string> names = [];

            foreach (SpaceAirMovement spaceAirMovement in outward)
            {
                string name = adjacencyCluster.AirMovementEndpoint(spaceAirMovement.To).Name;

                names.Add(name);
                total += spaceAirMovement.AirFlow;

                //Each branch carries exactly the room it feeds, because that room has no supply of its own.
                Assert.Equal(Duty(adjacencyCluster, name, FlowClassification.Extract) / 1000.0, spaceAirMovement.AirFlow, 9);
            }

            names.Sort(StringComparer.Ordinal);

            Assert.Equal(new List<string>() { name_Bathroom, name_Kitchen }, names);

            //What arrives leaves: the split is an allocation of one flow, not two new ones.
            Assert.Equal(spaceAirMovement_In.AirFlow, total, 9);
        }

        // =================================================================================================
        // 2. Being a transfer node is not being served
        // =================================================================================================

        /// <summary>
        /// <b>The hall is not claimed as mechanically ventilated.</b> It carries no design terminal and the
        /// Base MVHR system is not related to it - the unit moves no air into or out of it, and saying it did
        /// would be a false statement about the building on a model an assessment is filed from.
        /// </summary>
        [Fact]
        public void TheHall_IsATransferNodeAndNotAServedSpace()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Hall = SpaceByName(adjacencyCluster, name_Hall);

            Assert.Empty(adjacencyCluster.VentilationTerminals(space_Hall) ?? []);

            foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(preparation.VentilationSystem) ?? [])
            {
                Assert.NotEqual(space_Hall.Guid, space.Guid);
            }

            //And no supply or extract of its own reaches it either - only the transfer air passing through.
            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(space_Hall) ?? [])
            {
                Assert.IsType<Space>(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.From));
                Assert.IsType<Space>(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.To));
            }
        }

        /// <summary>
        /// The scope query itself, asked directly: the served spaces come back with the dwelling's own
        /// zero-terminal rooms added and the communal corridor left out, and it says which is which.
        /// </summary>
        [Fact]
        public void TheScope_AddsTheDwellingsOwnRoomsAndExcludesTheCommonParts()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            List<Space> spaces = adjacencyCluster.PartFTransferAirSpaces(adjacencyCluster.GetRelatedObjects<Space>(preparation.VentilationSystem), out List<string> notes);

            List<string> names = spaces.ConvertAll(x => x.Name);
            names.Sort(StringComparer.Ordinal);

            Assert.Equal(new List<string>() { name_Bathroom, name_Bedroom, name_Hall, name_Kitchen }, names);

            Assert.DoesNotContain(name_CommunalCorridor, names);

            Assert.Contains(notes, x => x.Contains(name_Hall));
        }

        // =================================================================================================
        // 3. The boundary of the dwelling
        // =================================================================================================

        /// <summary>
        /// <b>A communal corridor is not a route between a flat's rooms.</b> Here it is the only thing the
        /// bathroom touches, so using it would balance the dwelling perfectly - and it is refused instead,
        /// naming the room. Routing a flat's supply air out into the common parts and back is not a design
        /// this may quietly produce, and a corridor that joined two flats would let one dwelling's air be
        /// solved as the other's make-up.
        /// </summary>
        [Fact]
        public void ACommunalCorridor_CannotBecomeATransferRouteBetweenTheDwellingsRooms()
        {
            PartOIterationPreparation preparation = Prepare(Model(bathroomOnlyOffTheCommunalCorridor: true));

            Assert.NotNull(preparation.Refusal);
            Assert.Contains(name_Bathroom, preparation.Refusal);
            Assert.DoesNotContain(name_CommunalCorridor, preparation.Refusal);
        }

        /// <summary>
        /// And the corridor keeps out of the answer when the dwelling can be solved without it: no movement
        /// anywhere in the model touches it.
        /// </summary>
        [Fact]
        public void TheCommunalCorridor_CarriesNoneOfTheDwellingsAir()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Corridor = SpaceByName(adjacencyCluster, name_CommunalCorridor);

            Assert.Empty(adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(space_Corridor) ?? []);

            ObjectReference objectReference = new(space_Corridor);

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>())
            {
                Assert.NotEqual(objectReference, Core.Convert.ComplexReference<ObjectReference>(spaceAirMovement.From));
                Assert.NotEqual(objectReference, Core.Convert.ComplexReference<ObjectReference>(spaceAirMovement.To));
            }
        }

        // =================================================================================================
        // Fixture
        // =================================================================================================

        private static List<SpaceAirMovement> Transfers(AdjacencyCluster adjacencyCluster, Space space, bool outward)
        {
            ObjectReference objectReference = new(space);

            List<SpaceAirMovement> result = [];

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>())
            {
                SAMObject sAMObject_From = adjacencyCluster.AirMovementEndpoint(spaceAirMovement.From);
                SAMObject sAMObject_To = adjacencyCluster.AirMovementEndpoint(spaceAirMovement.To);

                //A transfer movement is the one with a Space at BOTH ends. Anything touching the unit is
                //supply, extract or the exhaust.
                if (sAMObject_From is not Space || sAMObject_To is not Space)
                {
                    continue;
                }

                if (objectReference == new ObjectReference(outward ? sAMObject_From : sAMObject_To))
                {
                    result.Add(spaceAirMovement);
                }
            }

            return result;
        }

        private static double Duty(AdjacencyCluster adjacencyCluster, string name, FlowClassification flowClassification)
        {
            return Analytical.Query.VentilationTerminalDesignDuty_Lps(adjacencyCluster.VentilationTerminals(SpaceByName(adjacencyCluster, name)), flowClassification) ?? 0;
        }

        private static Space SpaceByName(AdjacencyCluster adjacencyCluster, string name)
        {
            return adjacencyCluster.GetSpaces().Find(x => x.Name == name);
        }

        private static PartOIterationPreparation Prepared()
        {
            PartOIterationPreparation result = Prepare(Model(bathroomOnlyOffTheCommunalCorridor: false));

            Assert.Null(result.Refusal);
            Assert.NotNull(result.AnalyticalModel);

            return result;
        }

        private static PartOIterationPreparation Prepare(AnalyticalModel analyticalModel)
        {
            List<Zone> zones = analyticalModel.GetZones();

            Assert.NotEmpty(zones);

            Dictionary<Guid, string> dictionary = [];
            foreach (Zone zone in zones)
            {
                dictionary[zone.Guid] = "MVRE";
            }

            return analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, dictionary);
        }

        /// <summary>
        /// One flat whose rooms all open off an internal hall Approved Document F sizes nothing for, beside a
        /// communal corridor that belongs to no dwelling.
        /// </summary>
        /// <param name="bathroomOnlyOffTheCommunalCorridor">
        /// True moves the bathroom's only internal connection onto the communal corridor, so the dwelling can
        /// be balanced only by routing through the common parts - which is the thing that must be refused
        /// rather than done.
        /// </param>
        private static AnalyticalModel Model(bool bathroomOnlyOffTheCommunalCorridor)
        {
            AdjacencyCluster adjacencyCluster = new();

            //Named so the shared space-use classification recognises them: "Hall" is Circulation and
            //"Communal Corridor" is CommunalCirculation, and the Part F rule set sizes no terminal for
            //either.
            Dictionary<string, double> dictionary = new()
            {
                { name_Bedroom, 16.0 },
                { name_Hall, 8.0 },
                { name_Bathroom, 6.0 },
                { name_Kitchen, 12.0 },
                { name_CommunalCorridor, 20.0 },
            };

            foreach (KeyValuePair<string, double> keyValuePair in dictionary)
            {
                Space space = new(keyValuePair.Key);

                space.SetValue(SpaceParameter.Area, keyValuePair.Value);
                space.SetValue(SpaceParameter.Volume, keyValuePair.Value * 2.5);

                space.InternalCondition = new InternalCondition(keyValuePair.Key + " IC");

                adjacencyCluster.AddObject(space);
            }

            //The dwelling: every room opens off the hall and off nothing else, so the hall is the whole of
            //the internal route and a network without it has none.
            Helpers.DwellingPartitions.Partition(adjacencyCluster, name_Hall, name_Bedroom, 0);
            Helpers.DwellingPartitions.Partition(adjacencyCluster, name_Hall, name_Kitchen, 10);

            if (!bathroomOnlyOffTheCommunalCorridor)
            {
                Helpers.DwellingPartitions.Partition(adjacencyCluster, name_Hall, name_Bathroom, 20);
            }

            //The common parts, next to the flat but not part of it. A shortcut, if anything were allowed to
            //use it.
            Helpers.DwellingPartitions.Partition(adjacencyCluster, name_CommunalCorridor, name_Bedroom, 30);
            Helpers.DwellingPartitions.Partition(adjacencyCluster, name_CommunalCorridor, name_Bathroom, 40);

            AnalyticalModel analyticalModel = new("Part F Transfer Air Dwelling Scope", null, null, null, adjacencyCluster, null, new ProfileLibrary("Part F Transfer Air Scope Fixture"));

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();

            Assert.NotNull(partFCalculator);

            partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate(), "The Part F calculation did not run, so every test resting on it would be meaningless.");

            AdjacencyCluster adjacencyCluster_Sized = partFCalculator.AdjacencyCluster;

            //The dwelling boundary the transfer air scope is read from - the model's own statement about
            //which rooms are somebody's home and which are the block's.
            Zone zone_Dwelling = new(name_Zone_Dwelling);
            zone_Dwelling.SetValue(ZoneParameter.IsDwelling, true);
            zone_Dwelling.SetValue(ZoneParameter.ZoneCategory, "Flats");

            Zone zone_Common = new(name_Zone_Common);
            zone_Common.SetValue(ZoneParameter.IsDwelling, false);
            zone_Common.SetValue(ZoneParameter.ZoneCategory, "Flats");

            adjacencyCluster_Sized.AddObject(zone_Dwelling);
            adjacencyCluster_Sized.AddObject(zone_Common);

            foreach (string name in new string[] { name_Bedroom, name_Hall, name_Bathroom, name_Kitchen })
            {
                adjacencyCluster_Sized.AddRelation(zone_Dwelling, SpaceByName(adjacencyCluster_Sized, name));
            }

            adjacencyCluster_Sized.AddRelation(zone_Common, SpaceByName(adjacencyCluster_Sized, name_CommunalCorridor));

            return new AnalyticalModel(analyticalModel, adjacencyCluster_Sized);
        }
    }
}
