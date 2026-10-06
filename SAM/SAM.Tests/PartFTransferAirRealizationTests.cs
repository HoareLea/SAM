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
    /// <b>Where the air a balanced heat recovery design moves actually goes, and the conservation TAS
    /// refuses to simulate without.</b>
    /// <para>
    /// TAS will not simulate a building in which any one zone's inter-zone air movements do not balance: a
    /// zone that gains air it never loses is refused outright, and the EDSL documentation states the rule
    /// as "any air flow imbalance will be reported as a Max Pressure Exceeded error". That is the whole
    /// reason Iteration 1a's first licensed run produced <c>Simulation Failed</c> rather than a result -
    /// every room of a balanced heat recovery dwelling is individually out of balance, because the design
    /// balances at the SYSTEM and not in each room.
    /// </para>
    /// <para>
    /// Two objects close it, and these tests pin both: the dwelling's internal <b>transfer air</b>, routed
    /// by the same Approved Document F airflow network the door schedule is assessed over, and the unit's
    /// <b>exhaust</b>, which is where the extract air leaves the building.
    /// </para>
    /// <para>
    /// <b>What must not happen to make them balance.</b> No design terminal duty is adjusted, no route is
    /// invented where the model has no internal adjacency to carry one, and no room is quietly given a
    /// connection to outside it does not have. Where the dwelling cannot be balanced the preparation
    /// refuses and says which room, which is the difference between a diagnosis an engineer can act on and
    /// an export that reports success having produced a file TAS will not read.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Shares a collection with the other readers of the default Part F rule set, so the two never run at
    /// the same time: the rule set is reached through the process-wide <c>ActiveSetting.Setting</c> and its
    /// stored <c>PartFData</c> is shared by reference between every <c>PartFCalculator</c> built from it.
    /// </remarks>
    [Collection("SAM.Analytical.ActiveSetting default Part F data")]
    public class PartFTransferAirRealizationTests
    {
        private const string name_LivingRoom = "Living Room";

        private const string name_Bedroom = "Bedroom 1";

        private const string name_Kitchen = "Kitchen";

        private const string name_Bathroom = "Bathroom";

        private const string name_Zone = "Flat 1";

        private const double tolerance = 1e-9;

        // =================================================================================================
        // 1. Conservation
        // =================================================================================================

        /// <summary>
        /// <b>The assertion the licensed failure came down to.</b> Every space, and the unit, passes on
        /// exactly what it receives. Summed at each node over every movement touching it - not matched route
        /// by route, because these movements form a network in which flows split and recombine and no
        /// movement has a partner.
        /// </summary>
        [Fact]
        public void EveryNode_PassesOnExactlyWhatItReceives()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Dictionary<Guid, double> dictionary = adjacencyCluster.AirMovementResidual(
                adjacencyCluster.GetObjects<SpaceAirMovement>(),
                new List<AirHandlingUnit>() { preparation.AirHandlingUnit });

            Assert.NotEmpty(dictionary);

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                Assert.True(dictionary.TryGetValue(space.Guid, out double residual), string.Format("{0} carries no air movement at all.", space.Name));
                Assert.True(System.Math.Abs(residual) <= tolerance, string.Format("{0} is out of balance by {1:0.######} l/s.", space.Name, residual * 1000));
            }

            Assert.True(dictionary.TryGetValue(preparation.AirHandlingUnit.Guid, out double residual_Unit));
            Assert.True(System.Math.Abs(residual_Unit) <= tolerance, string.Format("The unit is out of balance by {0:0.######} l/s.", residual_Unit * 1000));
        }

        /// <summary>
        /// The design duties are what they were. Balancing decides where the air goes and never how much of
        /// it there is - a room rescaled to make its own sums work would be a different building from the
        /// one Approved Document F sized.
        /// </summary>
        [Fact]
        public void TheDesignTerminalDuties_AreNotAdjustedToMakeARoomBalance()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            //A habitable room is supplied and not extracted, and stays that way.
            Assert.Equal(Duty(adjacencyCluster, name_Bedroom, FlowClassification.Supply), preparation.DesignSupplyDuty_Lps - Duty(adjacencyCluster, name_LivingRoom, FlowClassification.Supply), 6);

            Assert.Null(Analytical.Query.VentilationTerminalDesignDuty_Lps(adjacencyCluster.VentilationTerminals(SpaceByName(adjacencyCluster, name_Bedroom)), FlowClassification.Extract));
            Assert.Null(Analytical.Query.VentilationTerminalDesignDuty_Lps(adjacencyCluster.VentilationTerminals(SpaceByName(adjacencyCluster, name_Bathroom)), FlowClassification.Supply));
        }

        // =================================================================================================
        // 2. The transfer air itself
        // =================================================================================================

        /// <summary>
        /// A supplied room with no extract passes its air on through the dwelling rather than being given a
        /// synthetic extract of its own. The movement runs from the room that has air to give to the room
        /// that has to draw it in.
        /// </summary>
        [Fact]
        public void TheSuppliedRoom_PassesItsAirOnThroughTheDwelling()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Bedroom = SpaceByName(adjacencyCluster, name_Bedroom);

            List<SpaceAirMovement> outward = Transfers(adjacencyCluster, space_Bedroom, true);

            Assert.NotEmpty(outward);

            double total = 0;
            foreach (SpaceAirMovement spaceAirMovement in outward)
            {
                total += spaceAirMovement.AirFlow;

                //Never to outside, and never to the unit: transfer air stays inside the dwelling.
                Assert.NotNull(spaceAirMovement.To);
                Assert.IsType<Space>(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.To));
            }

            double supply_Lps = Analytical.Query.VentilationTerminalDesignDuty_Lps(adjacencyCluster.VentilationTerminals(space_Bedroom), FlowClassification.Supply).Value;

            Assert.Equal(supply_Lps / 1000.0, total, 9);
        }

        /// <summary>
        /// An extracted room with no supply draws its make-up air from inside the dwelling, not from outside.
        /// Air arriving from outside would be untempered, which is the opposite of what a heat recovery
        /// design does and would flatter the overheating result it is assessed on.
        /// </summary>
        [Fact]
        public void TheExtractedRoom_DrawsItsMakeUpAirFromInsideTheDwelling()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_Bathroom = SpaceByName(adjacencyCluster, name_Bathroom);

            List<SpaceAirMovement> inward = Transfers(adjacencyCluster, space_Bathroom, false);

            Assert.NotEmpty(inward);

            double total = 0;
            foreach (SpaceAirMovement spaceAirMovement in inward)
            {
                total += spaceAirMovement.AirFlow;

                Assert.NotNull(spaceAirMovement.From);
                Assert.IsType<Space>(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.From));
            }

            double extract_Lps = Analytical.Query.VentilationTerminalDesignDuty_Lps(adjacencyCluster.VentilationTerminals(space_Bathroom), FlowClassification.Extract).Value;

            Assert.Equal(extract_Lps / 1000.0, total, 9);
        }

        /// <summary>
        /// <b>A network, not a set of journeys.</b> The living room of this flat is the only route between
        /// every other room, so it both receives from the bedroom and passes air on to the kitchen and the
        /// bathroom - several movements in and several out, splitting and recombining, with no movement
        /// matching any other. Nothing downstream may assume one flow has one end-to-end route.
        /// </summary>
        [Fact]
        public void OneSpace_CarriesSeveralMovementsInAndSeveralOut()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space_LivingRoom = SpaceByName(adjacencyCluster, name_LivingRoom);

            List<SpaceAirMovement> inward = Transfers(adjacencyCluster, space_LivingRoom, false);
            List<SpaceAirMovement> outward = Transfers(adjacencyCluster, space_LivingRoom, true);

            Assert.NotEmpty(inward);
            Assert.True(outward.Count > 1, "The only route between the rooms of this flat divides its outgoing air between more than one destination.");
        }

        // =================================================================================================
        // 3. The unit's exhaust
        // =================================================================================================

        /// <summary>
        /// The unit takes the extract air out of the building. Its destination is <b>null</b> - which is how
        /// outside is said, and what the TBD writer turns into a movement on the unit's zone with no source
        /// zone and no from-outside flag.
        /// </summary>
        [Fact]
        public void TheUnit_ExhaustsTheExtractAirToOutside()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            List<SpaceAirMovement> exhaust = [];

            ObjectReference objectReference = new(preparation.AirHandlingUnit);

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(preparation.AirHandlingUnit))
            {
                if (string.IsNullOrWhiteSpace(spaceAirMovement.To) && objectReference == Core.Convert.ComplexReference<ObjectReference>(spaceAirMovement.From))
                {
                    exhaust.Add(spaceAirMovement);
                }
            }

            Assert.Single(exhaust);
            Assert.Equal(preparation.DesignExtractDuty_Lps / 1000.0, exhaust[0].AirFlow, 9);
        }

        /// <summary>
        /// <b>All four legs of the physical MVHR route, asserted together, because something now depends on
        /// them being here.</b>
        /// <para>
        /// <c>Outside -&gt; unit</c>, <c>unit -&gt; supplied room</c>, <c>extracted room -&gt; unit</c>,
        /// <c>unit -&gt; Outside</c>. That is what an MVHR unit does, and this model says so.
        /// </para>
        /// <para>
        /// <b>The TAS export deliberately writes something else</b> - it flattens the last two into
        /// <c>extracted room -&gt; Outside</c>, because TAS represents the unit as one well-mixed thermal
        /// zone and would otherwise recover heat nobody specified (see <c>SAM_Tas</c>
        /// <c>Query.DesignTerminalExtractFlattening</c> and
        /// <c>documentation/PartO-TAS-VALIDATION.md</c>). That is a limitation of the target format, and it
        /// is handled at the boundary, on the way out. It must never be solved by making this model less
        /// true: the physical topology is what every other consumer reads, and a later iteration modelling a
        /// real unit with real heat recovery needs the extract to arrive at the unit.
        /// </para>
        /// <para>
        /// So this test exists to fail if the flattening is ever pushed back up into SAM.
        /// </para>
        /// </summary>
        [Fact]
        public void TheModel_StatesAllFourLegsOfThePhysicalMVHRRoute()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            ObjectReference objectReference_Unit = new(preparation.AirHandlingUnit);

            // 1. Outside -> unit. The intake is an AirHandlingUnitAirMovement, and its flow is derived from
            //    what the unit delivers.
            AirHandlingUnitAirMovement airHandlingUnitAirMovement = Assert.Single(adjacencyCluster.GetRelatedObjects<AirHandlingUnitAirMovement>(preparation.AirHandlingUnit));

            Assert.Equal(preparation.DesignSupplyDuty_Lps / 1000.0, Analytical.Query.AirFlow(adjacencyCluster, airHandlingUnitAirMovement, out Profile _), 9);

            double supply = 0;
            double extract = 0;
            double exhaust_Unit = 0;

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>())
            {
                ObjectReference objectReference_From = Core.Convert.ComplexReference<ObjectReference>(spaceAirMovement.From);
                ObjectReference objectReference_To = Core.Convert.ComplexReference<ObjectReference>(spaceAirMovement.To);

                // 2. unit -> supplied room.
                if (objectReference_Unit == objectReference_From && !string.IsNullOrWhiteSpace(spaceAirMovement.To))
                {
                    Assert.IsType<Space>(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.To));

                    supply += spaceAirMovement.AirFlow;
                }
                // 3. extracted room -> unit. THE LEG THE TAS EXPORT FLATTENS, and it is here.
                else if (objectReference_Unit == objectReference_To)
                {
                    Assert.IsType<Space>(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.From));

                    extract += spaceAirMovement.AirFlow;
                }
                // 4. unit -> Outside. THE OTHER LEG THE TAS EXPORT FLATTENS, and it is here too.
                else if (objectReference_Unit == objectReference_From)
                {
                    exhaust_Unit += spaceAirMovement.AirFlow;
                }
            }

            Assert.Equal(preparation.DesignSupplyDuty_Lps / 1000.0, supply, 9);
            Assert.Equal(preparation.DesignExtractDuty_Lps / 1000.0, extract, 9);
            Assert.Equal(preparation.DesignExtractDuty_Lps / 1000.0, exhaust_Unit, 9);

            //No room takes its extract straight outside: in the physical model that air goes to the unit.
            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>())
            {
                if (string.IsNullOrWhiteSpace(spaceAirMovement.To))
                {
                    Assert.Equal(objectReference_Unit, Core.Convert.ComplexReference<ObjectReference>(spaceAirMovement.From));
                }
            }
        }

        /// <summary>
        /// A model whose spaces carry no design terminals produces no extract back to the unit, so it gets no
        /// exhaust either - the branch that predates all of this is left exactly as it was.
        /// </summary>
        [Fact]
        public void AModelWithNoDesignTerminals_GetsNoExhaust()
        {
            AdjacencyCluster adjacencyCluster = new();

            Space space = new(name_LivingRoom) { InternalCondition = new InternalCondition(name_LivingRoom + " IC") };
            space.SetValue(SpaceParameter.Area, 30.0);
            space.SetValue(SpaceParameter.Volume, 75.0);

            AirHandlingUnit airHandlingUnit = Analytical.Create.AirHandlingUnit("AHU-01");

            VentilationSystem ventilationSystem = Analytical.Create.MechanicalSystem(new VentilationSystemType("MV", "Mechanical ventilation"), null, "1") as VentilationSystem;
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);

            adjacencyCluster.AddObject(space);
            adjacencyCluster.AddObject(airHandlingUnit);
            adjacencyCluster.AddObject(ventilationSystem);
            adjacencyCluster.AddRelation(ventilationSystem, space);

            List<IAirMovementObject> airMovementObjects = adjacencyCluster.AddAirMovementObjects(null, ventilationSystem);

            foreach (IAirMovementObject airMovementObject in airMovementObjects)
            {
                if (airMovementObject is SpaceAirMovement spaceAirMovement)
                {
                    Assert.NotEqual(string.Format("{0} exhaust", airHandlingUnit.Name), spaceAirMovement.Name);
                }
            }
        }

        // =================================================================================================
        // 4. What it refuses to do
        // =================================================================================================

        /// <summary>
        /// <b>No route is invented.</b> A dwelling whose rooms share no internal separating element cannot
        /// move transfer air between them, and this refuses and names the rooms rather than quietly
        /// connecting each of them to outside - which would put untempered outside air into the wet rooms of
        /// a heat recovery dwelling and change the answer the assessment turns on.
        /// </summary>
        [Fact]
        public void ADwellingWithNoInternalAdjacency_IsRefusedAndTheRoomsAreNamed()
        {
            PartOIterationPreparation preparation = Prepare(Model(false));

            Assert.NotNull(preparation.Refusal);
            Assert.Contains(name_Bathroom, preparation.Refusal);
        }

        /// <summary>
        /// Preparing the same model twice produces the same model. The transfer movements are related to a
        /// space, so the existing removal pass reaches them and replaces them rather than adding a second
        /// dwelling's worth beside the first.
        /// </summary>
        [Fact]
        public void PreparingTwice_ReplacesTheTransferAirRatherThanAddingToIt()
        {
            PartOIterationPreparation preparation = Prepared();

            int count = preparation.AnalyticalModel.AdjacencyCluster.GetObjects<SpaceAirMovement>().Count;

            PartOIterationPreparation preparation_Again = Prepare(preparation.AnalyticalModel);

            Assert.Null(preparation_Again.Refusal);
            Assert.Equal(count, preparation_Again.AnalyticalModel.AdjacencyCluster.GetObjects<SpaceAirMovement>().Count);
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

                ObjectReference objectReference_End = new(outward ? sAMObject_From : sAMObject_To);

                if (objectReference == objectReference_End)
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
            PartOIterationPreparation result = Prepare(Model(true));

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
        /// One flat: a living room every other room opens off, a bedroom Approved Document F supplies and
        /// does not extract, and a kitchen and bathroom it extracts and does not supply.
        /// </summary>
        /// <param name="partitioned">
        /// False builds the same rooms with no separating elements at all - a bag of rooms rather than a
        /// dwelling, which is what the refusal is pinned on.
        /// </param>
        private static AnalyticalModel Model(bool partitioned)
        {
            AdjacencyCluster adjacencyCluster = new();

            //Named so the shared space-use classification recognises them.
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

            if (partitioned)
            {
                Helpers.DwellingPartitions.Star(adjacencyCluster, name_LivingRoom, name_Bedroom, name_Kitchen, name_Bathroom);
            }

            AnalyticalModel analyticalModel = new("Part F Transfer Air Dwelling", null, null, null, adjacencyCluster, null, new ProfileLibrary("Part F Transfer Air Fixture"));

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();

            Assert.NotNull(partFCalculator);

            partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate(), "The Part F calculation did not run, so every test resting on it would be meaningless.");

            AdjacencyCluster adjacencyCluster_Sized = partFCalculator.AdjacencyCluster;

            Zone zone = new(name_Zone);
            adjacencyCluster_Sized.AddObject(zone);

            //Related to every space this fixture builds, matching how a real model's own zoning relates a
            //dwelling zone to its rooms - Modify.PrepareBaseMVHR partitions the assessed scope by exactly
            //this relation (Query.PartFDwellingZones plus the zone -> space relation).
            foreach (Space space_Existing in adjacencyCluster_Sized.GetSpaces())
            {
                adjacencyCluster_Sized.AddRelation(zone, space_Existing);
            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster_Sized);
        }
    }
}
