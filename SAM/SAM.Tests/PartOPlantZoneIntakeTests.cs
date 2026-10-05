// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    /// <summary>
    /// <b>The generated TAS plant zone has to balance, and an isolated model used to hand it a zone that
    /// could not.</b>
    ///
    /// <para><b>What the plant zone is, and what balances it</b></para>
    /// <para>
    /// <c>SAM.Analytical.Tas.Modify.UpdateIZAMs</c> builds one small TAS zone per air handling unit - the
    /// unit's own plant zone, named after it ("MVHR-01") - and writes it one inter-zone air movement per
    /// room the unit <b>supplies</b>, plus one <c>"IZAM &lt;unit&gt; FROM OUTSIDE"</c> that brings in what
    /// the unit therefore has to draw. That intake is sized by <c>Analytical.Query.AirFlow</c>, which reads the
    /// unit's own deliveries; where it resolves nothing, <b>the intake is simply not written</b> and the
    /// zone delivers the dwelling's whole supply while gaining nothing. TAS refuses to simulate a zone
    /// whose air movements do not balance, and reports it as nothing more informative than
    /// <i>"Simulation Failed"</i>.
    /// </para>
    ///
    /// <para><b>The isolation defect</b></para>
    /// <para>
    /// <c>Modify.IsolateSpaces</c> depended on two <c>IJSAMObject</c> lookups that cannot work on an
    /// <see cref="AdjacencyCluster"/>: <c>AdjacencyCluster.IsValid(Type)</c> asks whether a type is
    /// assignable TO one of the analytical families it admits, which <c>IJSAMObject</c> - broader than all
    /// of them - is not, and both <c>RelationCluster.GetObjects(Type)</c> and
    /// <c>RelationCluster.GetObject(Type, Guid)</c> gate on it. So
    /// <c>result.GetObjects&lt;IJSAMObject&gt;()</c> answered null however much the cluster held, and
    /// <c>result.GetObject&lt;IJSAMObject&gt;(guid)</c> answered null for every guid in it.
    /// </para>
    /// <para>
    /// Two consequences, both fixed here. The relation restoration returned at its first line and restored
    /// <b>nothing</b>; and the plant carry's existence guard was unsatisfiable, so the unit arrived in the
    /// derived model related to nothing but its own <see cref="AirHandlingUnitAirMovement"/>. On the
    /// licensed acceptance model that produced a plant zone delivering 36.3 l/s and taking in nothing, and
    /// TAS refused the isolated Flat 1 run - with a 22 KB stub of a results file where a full year belongs.
    /// </para>
    /// <para>
    /// A third gap in the same family: the unit's own exhaust is a <see cref="SpaceAirMovement"/> related to
    /// <b>no space at all</b>, so nothing reachable from the selection carried it.
    /// </para>
    ///
    /// <para><b>And SAM Check now sees it first</b></para>
    /// <para>
    /// The state is deterministic and visible in the analytical model long before a TBD exists, so it is a
    /// <c>Create.Log</c> rule rather than something to rediscover in TAS. See the Part O pre-simulation gate
    /// in SAM_UI (<c>PartOPreSimulationCheck</c>), which refuses the run on it.
    /// </para>
    /// </summary>
    public class PartOPlantZoneIntakeTests
    {
        // ---- A. The Check rule -------------------------------------------------------------------------

        /// <summary>
        /// A unit that delivers supply somewhere resolves an intake, so the plant zone will balance and
        /// nothing is reported.
        /// </summary>
        [Fact]
        public void AUnitThatDeliversSupply_ReportsNothing()
        {
            AdjacencyCluster adjacencyCluster = Plant(out AirHandlingUnitAirMovement airHandlingUnitAirMovement, supply: true, exhaust: true);

            Assert.Empty(AnalyticalCreate.Log(airHandlingUnitAirMovement, adjacencyCluster));
        }

        /// <summary>
        /// <b>A unit that supplies a room but is not RELATED to that movement is an error</b> - which is
        /// the state an isolated model used to arrive in. The model says the unit delivers; the relation
        /// graph does not, so nothing can size the intake, and the plant zone loses air it never gains.
        /// The record names the movement, the unit and both Guids.
        /// </summary>
        [Fact]
        public void AUnitSupplyingAMovementItIsNotRelatedTo_IsAnError()
        {
            AdjacencyCluster adjacencyCluster = Plant(out AirHandlingUnitAirMovement airHandlingUnitAirMovement, supply: true, exhaust: true, relateSupply: false);

            LogRecord logRecord = Assert.Single(AnalyticalCreate.Log(airHandlingUnitAirMovement, adjacencyCluster));

            Assert.Equal(LogRecordType.Error, logRecord.LogRecordType);
            Assert.Contains("MVHR-01", logRecord.Text);
            Assert.Contains(airHandlingUnitAirMovement.Guid.ToString(), logRecord.Text);
            Assert.Contains("resolves no intake air flow", logRecord.Text);
            Assert.Contains("Studio supply", logRecord.Text);
            Assert.Contains("air movements do not balance", logRecord.Text);
        }

        /// <summary>
        /// <b>An extract-only unit is valid and must not be reported.</b>
        /// <para>
        /// It delivers to no room at all: its zone gains each room's extract and loses it again through the
        /// unit's own exhaust, so it balances with no outside intake, and <c>Analytical.Query.AirFlow</c>
        /// correctly answers nothing. "No intake" is only a fault where something is being delivered - so
        /// the rule asks whether the model says the unit delivers, and only then whether an intake can be
        /// sized.
        /// </para>
        /// <para>An earlier revision would have refused a real system.</para>
        /// </summary>
        [Fact]
        public void AnExtractOnlyUnit_ReportsNothing()
        {
            AdjacencyCluster adjacencyCluster = Plant(out AirHandlingUnitAirMovement airHandlingUnitAirMovement, supply: false, exhaust: true, extract: true);

            //It really is extract-only, and it really does have air movements - so this is not a fixture
            //that passes by being empty.
            Assert.Equal(2, (adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(Unit(adjacencyCluster)) ?? []).Count);
            Assert.Null(Analytical.Query.SpaceAirMovement_Delivered(adjacencyCluster, Unit(adjacencyCluster)));

            Assert.Empty(AnalyticalCreate.Log(airHandlingUnitAirMovement, adjacencyCluster));
        }

        /// <summary>
        /// <b>The unit's own exhaust is not a delivery.</b> It names the unit as its source and names no
        /// destination - it is air leaving the building - so counting it would have the unit draw its own
        /// duty twice. A unit with nothing but an exhaust delivers nothing, so it needs no intake.
        /// </summary>
        [Fact]
        public void AUnitWithOnlyAnExhaust_ReportsNothing()
        {
            AdjacencyCluster adjacencyCluster = Plant(out AirHandlingUnitAirMovement airHandlingUnitAirMovement, supply: false, exhaust: true);

            //The exhaust IS there - so this is not "the unit has no air movements at all".
            Assert.Single(adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(Unit(adjacencyCluster)) ?? []);
            Assert.Null(Analytical.Query.SpaceAirMovement_Delivered(adjacencyCluster, Unit(adjacencyCluster)));

            Assert.Empty(AnalyticalCreate.Log(airHandlingUnitAirMovement, adjacencyCluster));
        }

        /// <summary>
        /// An air movement related to no unit at all is a <b>warning</b>, not an error: nothing pairs it
        /// with a unit, so no plant zone is generated from it and it is inert. Worth saying; not a reason a
        /// model cannot be simulated.
        /// </summary>
        [Fact]
        public void AnAirMovementRelatedToNoUnit_IsAWarning()
        {
            AdjacencyCluster adjacencyCluster = new();

            AirHandlingUnitAirMovement airHandlingUnitAirMovement = new("MVHR-01");
            adjacencyCluster.AddObject(airHandlingUnitAirMovement);

            LogRecord logRecord = Assert.Single(AnalyticalCreate.Log(airHandlingUnitAirMovement, adjacencyCluster));

            Assert.Equal(LogRecordType.Warning, logRecord.LogRecordType);
            Assert.Contains("related to no AirHandlingUnit", logRecord.Text);
        }

        /// <summary>
        /// <b>Asked without a cluster, the rule says nothing about the intake.</b> Whether a unit delivers
        /// anything is a question about the model's relations, and an object handed over on its own cannot
        /// answer it - so it must not guess, in either direction.
        /// </summary>
        [Fact]
        public void WithNoCluster_NothingIsSaidAboutTheIntake()
        {
            Plant(out AirHandlingUnitAirMovement airHandlingUnitAirMovement, supply: false, exhaust: false);

            Assert.Empty(AnalyticalCreate.Log(airHandlingUnitAirMovement));
        }

        /// <summary>The model level Check reaches the rule, which is what the Part O gate runs.</summary>
        [Fact]
        public void TheModelCheck_ReportsAUnitThatDeliversNothing()
        {
            AdjacencyCluster adjacencyCluster = Plant(out AirHandlingUnitAirMovement _, supply: true, exhaust: true, relateSupply: false);

            AnalyticalModel analyticalModel = new("Block", null, null, null, adjacencyCluster, new MaterialLibrary("M"), new ProfileLibrary("P"));

            Assert.Contains(
                AnalyticalCreate.Log(analyticalModel),
                x => x.LogRecordType == LogRecordType.Error && x.Text.Contains("resolves no intake air flow", StringComparison.Ordinal));
        }

        // ---- B. The isolation regression ---------------------------------------------------------------

        /// <summary>
        /// <b>The derived unit keeps the air movements it supplies.</b> This is the relation the isolation
        /// lost, and losing it is what left the plant zone with no intake.
        /// </summary>
        [Fact]
        public void IsolatedUnit_KeepsTheAirMovementsItSupplies()
        {
            SpaceIsolation spaceIsolation = Isolated(out PartFModel partFModel);

            AdjacencyCluster adjacencyCluster = spaceIsolation.AdjacencyCluster;
            AirHandlingUnit airHandlingUnit = Unit(adjacencyCluster);

            Assert.NotNull(airHandlingUnit);

            List<string> names = [];
            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(airHandlingUnit) ?? [])
            {
                names.Add(spaceAirMovement.Name);
            }

            names.Sort(StringComparer.Ordinal);

            Assert.Equal(["Bathroom extract", "MVHR-01 exhaust", "Studio supply"], names);
        }

        /// <summary>
        /// <b>And therefore resolves an intake air flow.</b> This is the one assertion that stands between
        /// an isolated run and a TAS refusal: no profile here means no
        /// <c>"IZAM &lt;unit&gt; FROM OUTSIDE"</c> in the TBD.
        /// </summary>
        [Fact]
        public void IsolatedUnit_ResolvesAnIntakeAirFlow()
        {
            SpaceIsolation spaceIsolation = Isolated(out PartFModel _);

            AdjacencyCluster adjacencyCluster = spaceIsolation.AdjacencyCluster;

            AirHandlingUnitAirMovement airHandlingUnitAirMovement = Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>() ?? []);

            double airFlow = Analytical.Query.AirFlow(adjacencyCluster, airHandlingUnitAirMovement, out Profile profile);

            Assert.NotNull(profile);
            Assert.Equal(0.03, airFlow, 6);
        }

        /// <summary>
        /// <b>The unit's own plant side air movements come across.</b> The exhaust is related to no space,
        /// so nothing reachable from the selection carries it - and a unit that gains the extract duty and
        /// never loses it is exactly the zone TAS refuses.
        /// </summary>
        [Fact]
        public void IsolatedUnit_CarriesItsOwnPlantSideAirMovements()
        {
            SpaceIsolation spaceIsolation = Isolated(out PartFModel _);

            Assert.Contains(
                spaceIsolation.AdjacencyCluster.GetObjects<SpaceAirMovement>() ?? [],
                x => x.Name == "MVHR-01 exhaust");

            Assert.Contains(spaceIsolation.Notes, x => x.Contains("plant side air movement", StringComparison.Ordinal));
        }

        /// <summary>
        /// A movement that belongs to an <b>excluded</b> dwelling's rooms is not dragged in by the plant
        /// carry. Only movements related to no space at all are plant side.
        /// </summary>
        [Fact]
        public void ThePlantCarry_DoesNotBringInAnExcludedDwellingsRoomAirMovements()
        {
            SpaceIsolation spaceIsolation = Isolated(out PartFModel _);

            foreach (SpaceAirMovement spaceAirMovement in spaceIsolation.AdjacencyCluster.GetObjects<SpaceAirMovement>() ?? [])
            {
                Assert.DoesNotContain("Bedroom", spaceAirMovement.Name, StringComparison.Ordinal);
                Assert.DoesNotContain("Kitchen", spaceAirMovement.Name, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// <b>The derived relation graph is more than what the filter made.</b> The supply movement is
        /// related to its space, which the geometric filter did, <b>and</b> to its unit, which only the
        /// relation restoration can do.
        /// </summary>
        [Fact]
        public void IsolatedModel_RestoresSourceRelationsBetweenCarriedObjects()
        {
            SpaceIsolation spaceIsolation = Isolated(out PartFModel _);

            AdjacencyCluster adjacencyCluster = spaceIsolation.AdjacencyCluster;

            SpaceAirMovement spaceAirMovement = (adjacencyCluster.GetObjects<SpaceAirMovement>() ?? []).Find(x => x.Name == "Studio supply");

            Assert.NotNull(spaceAirMovement);
            Assert.NotEmpty(adjacencyCluster.GetRelatedObjects<Space>(spaceAirMovement) ?? []);
            Assert.NotEmpty(adjacencyCluster.GetRelatedObjects<AirHandlingUnit>(spaceAirMovement) ?? []);
        }

        /// <summary>The isolated model passes the intake rule, which is the regression stated as the Check sees it.</summary>
        [Fact]
        public void IsolatedModel_PassesTheIntakeCheck()
        {
            SpaceIsolation spaceIsolation = Isolated(out PartFModel _);

            AdjacencyCluster adjacencyCluster = spaceIsolation.AdjacencyCluster;

            foreach (AirHandlingUnitAirMovement airHandlingUnitAirMovement in adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>() ?? [])
            {
                Assert.Empty(AnalyticalCreate.Log(airHandlingUnitAirMovement, adjacencyCluster));
            }
        }

        /// <summary>The source model is untouched by any of it.</summary>
        [Fact]
        public void SourceModel_IsUnchangedByThePlantCarry()
        {
            PartFModel partFModel = PlantFixture();

            string before = partFModel.AdjacencyCluster.ToJsonObject().ToJsonString();

            partFModel.AdjacencyCluster.IsolateSpaces([partFModel.Get("Studio"), partFModel.Get("Bathroom")]);

            Assert.Equal(before, partFModel.AdjacencyCluster.ToJsonObject().ToJsonString());
        }

        // ---- C. A transfer air movement across the cut, which is related to one end only ---------------

        /// <summary>
        /// <b>A transfer air movement OUT of the selection into an excluded room refuses.</b>
        /// <para>
        /// <c>Modify.AddPartFTransferAirMovements</c> relates a transfer to the <b>downstream</b> space
        /// only - relating it to both would have the TBD writer write the dwelling two identical inter-zone
        /// air movements - so the upstream space exists on the object as a <c>From</c> reference and nowhere
        /// else. Read from the relations alone, this movement looks as though it touches nothing selected:
        /// it was neither refused nor carried, and the selected room passed its air on to a room that is
        /// not in the model.
        /// </para>
        /// </summary>
        [Fact]
        public void ATransferFromASelectedSpaceToAnExcludedOne_Refuses()
        {
            PartFModel partFModel = PlantFixture();

            Transfer(partFModel, "Studio", "Corridor");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces([partFModel.Get("Studio"), partFModel.Get("Bathroom")]);

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Null(spaceIsolation.AdjacencyCluster);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Studio to Corridor transfer", StringComparison.Ordinal) && x.Contains("crosses the isolation boundary", StringComparison.Ordinal));
        }

        /// <summary>
        /// <b>And one INTO the selection from an excluded room refuses too.</b> This is the mirror case:
        /// related to the selected space, so nothing looked excluded and nothing refused - and it was then
        /// carried into the derived model with a <c>From</c> naming a space that is not in it, which is air
        /// arriving from a room that does not exist.
        /// </summary>
        [Fact]
        public void ATransferFromAnExcludedSpaceIntoASelectedOne_Refuses()
        {
            PartFModel partFModel = PlantFixture();

            Transfer(partFModel, "Corridor", "Studio");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces([partFModel.Get("Studio"), partFModel.Get("Bathroom")]);

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Null(spaceIsolation.AdjacencyCluster);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Corridor to Studio transfer", StringComparison.Ordinal));
        }

        /// <summary>
        /// A transfer <b>within</b> the selection is not a cut and still isolates - the fixture's own
        /// Studio-to-Bathroom transfer, which every other test here relies on.
        /// </summary>
        [Fact]
        public void ATransferWithinTheSelection_StillIsolates()
        {
            SpaceIsolation spaceIsolation = Isolated(out PartFModel _);

            Assert.True(spaceIsolation.IsIsolated);
            Assert.Contains(
                spaceIsolation.AdjacencyCluster.GetObjects<SpaceAirMovement>() ?? [],
                x => x.Name == "Studio to Bathroom transfer");
        }

        /// <summary>
        /// The plant side movements are not read as boundary crossings. The unit's exhaust names the unit
        /// and no destination at all, and "no destination" is how outside is said - not a space outside the
        /// scope.
        /// </summary>
        [Fact]
        public void ThePlantSideMovements_AreNotReadAsBoundaryCrossings()
        {
            SpaceIsolation spaceIsolation = Isolated(out PartFModel _);

            Assert.True(spaceIsolation.IsIsolated);
            Assert.Empty(spaceIsolation.Refusals);
        }

        // ---- The fixture -------------------------------------------------------------------------------

        /// <summary>
        /// A unit, its air movement, and optionally the two movements a real one has: the supply it
        /// delivers to a room, and its own exhaust.
        /// </summary>
        private static AdjacencyCluster Plant(out AirHandlingUnitAirMovement airHandlingUnitAirMovement, bool supply, bool exhaust, bool extract = false, bool relateSupply = true)
        {
            AdjacencyCluster adjacencyCluster = new();

            Space space = new("Studio");
            adjacencyCluster.AddObject(space);

            AirHandlingUnit airHandlingUnit = new("MVHR-01", 20, 20);
            adjacencyCluster.AddObject(airHandlingUnit);

            airHandlingUnitAirMovement = new AirHandlingUnitAirMovement("MVHR-01");
            adjacencyCluster.AddObject(airHandlingUnitAirMovement);
            adjacencyCluster.AddRelation(airHandlingUnit, airHandlingUnitAirMovement);

            string reference_Unit = new ObjectReference(airHandlingUnit).ToString();
            string reference_Space = new ObjectReference(space).ToString();

            if (supply)
            {
                SpaceAirMovement spaceAirMovement = new("Studio supply", 0.03, reference_Unit, reference_Space);
                adjacencyCluster.AddObject(spaceAirMovement);
                adjacencyCluster.AddRelation(spaceAirMovement, space);

                //relateSupply: false is the isolation defect in one line. The movement is in the model and
                //names the unit as its source, and the unit does not know about it - so nothing can size
                //the intake from it.
                if (relateSupply)
                {
                    adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
                }
            }

            if (extract)
            {
                //Room to unit. The unit's zone GAINS this, which is how an extract-only unit balances
                //against its exhaust with no outside intake at all.
                SpaceAirMovement spaceAirMovement = new("Studio extract", 0.03, reference_Space, reference_Unit);
                adjacencyCluster.AddObject(spaceAirMovement);
                adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
                adjacencyCluster.AddRelation(spaceAirMovement, space);
            }

            if (exhaust)
            {
                //From the unit, to nowhere. That is how "leaving the building" is said, and it is why the
                //exhaust must not be read as a delivery.
                SpaceAirMovement spaceAirMovement = new("MVHR-01 exhaust", 0.03, reference_Unit, null);
                adjacencyCluster.AddObject(spaceAirMovement);
                adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
            }

            return adjacencyCluster;
        }

        private static AirHandlingUnit Unit(AdjacencyCluster adjacencyCluster)
        {
            return (adjacencyCluster?.GetObjects<AirHandlingUnit>() ?? []).Find(x => x.Name == "MVHR-01");
        }

        /// <summary>Flat 1 (Studio, Bathroom) isolated out of the plant fixture.</summary>
        private static SpaceIsolation Isolated(out PartFModel partFModel)
        {
            partFModel = PlantFixture();

            SpaceIsolation result = partFModel.AdjacencyCluster.IsolateSpaces([partFModel.Get("Studio"), partFModel.Get("Bathroom")]);

            Assert.True(result.IsIsolated, string.Join(" ", result.Refusals ?? []));

            return result;
        }

        /// <summary>
        /// Flat 1 (Studio, Bathroom) and Flat 2 (Bedroom, Kitchen) with a corridor between them, Flat 1 on
        /// MVHR-01 with the air movements a realized Part O dwelling has - the unit's supply into the
        /// Studio, the Bathroom's extract back to the unit, the transfer air between them, and the unit's
        /// own exhaust - and Flat 2 on MVHR-02, so the isolation has something to leave behind.
        /// </summary>
        private static PartFModel PlantFixture()
        {
            PartFModel partFModel = new PartFModel()
                .Space("Studio", 25, 62.5)
                .Space("Bathroom", 5, 12.5)
                .Space("Bedroom", 14, 35)
                .Space("Kitchen", 10, 25)
                .Space("Corridor", 20, 50)
                .Zone("Flat 1", "Dwelling", true, "Studio", "Bathroom")
                .Zone("Flat 2", "Dwelling", true, "Bedroom", "Kitchen")
                .Partition("Studio", "Bathroom", "Door Studio Bathroom")
                .Partition("Studio", "Corridor", "Door Flat 1 Corridor")
                .Partition("Bedroom", "Corridor", "Door Flat 2 Corridor")
                .Partition("Bedroom", "Kitchen", "Door Bedroom Kitchen")
                .ExternalWall("Studio")
                .ExternalWall("Bathroom", window: false)
                .ExternalWall("Bedroom")
                .ExternalWall("Kitchen");

            RealConstructions(partFModel.AdjacencyCluster);

            Dwelling(partFModel, "MVHR-01", "Studio", "Bathroom");
            Dwelling(partFModel, "MVHR-02", "Bedroom", "Kitchen");

            return partFModel;
        }

        /// <summary>
        /// One dwelling on one dedicated unit, realized: the system, the unit, the unit's supply condition,
        /// and the four air movements the TAS export reads.
        /// </summary>
        private static void Dwelling(PartFModel partFModel, string name_Unit, string name_Supplied, string name_Extracted)
        {
            AdjacencyCluster adjacencyCluster = partFModel.AdjacencyCluster;

            Space space_Supplied = partFModel.Get(name_Supplied);
            Space space_Extracted = partFModel.Get(name_Extracted);

            VentilationSystem ventilationSystem = new(name_Unit, new VentilationSystemType("MVHR", "Fixture"));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, name_Unit);
            adjacencyCluster.AddObject(ventilationSystem);
            adjacencyCluster.AddRelation(ventilationSystem, space_Supplied);
            adjacencyCluster.AddRelation(ventilationSystem, space_Extracted);

            AirHandlingUnit airHandlingUnit = new(name_Unit, 20, 20);
            adjacencyCluster.AddObject(airHandlingUnit);

            AirHandlingUnitAirMovement airHandlingUnitAirMovement = new(name_Unit);
            adjacencyCluster.AddObject(airHandlingUnitAirMovement);
            adjacencyCluster.AddRelation(airHandlingUnit, airHandlingUnitAirMovement);

            string reference_Unit = new ObjectReference(airHandlingUnit).ToString();
            string reference_Supplied = new ObjectReference(space_Supplied).ToString();
            string reference_Extracted = new ObjectReference(space_Extracted).ToString();

            Add(adjacencyCluster, string.Format("{0} supply", name_Supplied), 0.03, reference_Unit, reference_Supplied, airHandlingUnit, space_Supplied);
            Add(adjacencyCluster, string.Format("{0} extract", name_Extracted), 0.03, reference_Extracted, reference_Unit, airHandlingUnit, space_Extracted);
            Add(adjacencyCluster, string.Format("{0} to {1} transfer", name_Supplied, name_Extracted), 0.03, reference_Supplied, reference_Extracted, null, space_Extracted);

            //From the unit, to nowhere - the extract air leaving the building. Related to the unit and to
            //no space at all, which is exactly why a space-shaped filter cannot carry it.
            Add(adjacencyCluster, string.Format("{0} exhaust", name_Unit), 0.03, reference_Unit, null, airHandlingUnit, null);
        }

        /// <summary>
        /// One transfer air movement, built exactly as <c>Modify.AddPartFTransferAirMovements</c> builds
        /// one: <c>From</c> the upstream space, <c>To</c> the downstream space, and <b>related to the
        /// downstream space only</b>.
        /// </summary>
        private static void Transfer(PartFModel partFModel, string name_Upstream, string name_Downstream)
        {
            Space space_Upstream = partFModel.Get(name_Upstream);
            Space space_Downstream = partFModel.Get(name_Downstream);

            Add(
                partFModel.AdjacencyCluster,
                string.Format("{0} to {1} transfer", name_Upstream, name_Downstream),
                0.01,
                new ObjectReference(space_Upstream).ToString(),
                new ObjectReference(space_Downstream).ToString(),
                null,
                space_Downstream);
        }

        private static void Add(AdjacencyCluster adjacencyCluster, string name, double airFlow, string from, string to, AirHandlingUnit airHandlingUnit, Space space)
        {
            SpaceAirMovement spaceAirMovement = new(name, airFlow, from, to);
            adjacencyCluster.AddObject(spaceAirMovement);

            if (airHandlingUnit is not null)
            {
                adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
            }

            if (space is not null)
            {
                adjacencyCluster.AddRelation(spaceAirMovement, space);
            }
        }

        /// <summary>
        /// Gives every panel a construction with a real thickness, so <c>Query.Adiabatic</c> does not
        /// report every zero-thickness surface as already adiabatic and hide the isolation cut.
        /// </summary>
        private static void RealConstructions(AdjacencyCluster adjacencyCluster)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                Construction construction = new(
                    panel.Construction.Guid,
                    panel.Construction.Name,
                    [new ConstructionLayer("Concrete", 0.2)]);

                adjacencyCluster.AddObject(AnalyticalCreate.Panel(panel, construction));
            }
        }
    }
}
