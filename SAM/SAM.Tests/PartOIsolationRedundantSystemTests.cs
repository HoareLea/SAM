// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <c>Modify.IsolateSpaces</c> and the estate-wide ventilation system a building is often drawn with.
    /// <para>
    /// The real model behind this: three flats, each on its own MVHR with its own design terminals, and
    /// beside them one <c>MV</c> system related to the rooms of two of those flats, naming a central unit
    /// with nothing on it at all. Isolating either flat was refused - "ventilation system 'MV 1' also
    /// serves 3 space(s) outside the isolation scope" - although that system supplies no terminal, moves no
    /// air, and has no product selected against its unit. Every one of its rooms is already served by the
    /// MVHR that carries that room's design.
    /// </para>
    /// <para>
    /// <b>The rule is redundancy, not bareness.</b> A bare system whose rooms nothing else serves is plant
    /// somebody has still to detail, and it must go on refusing - which is what
    /// <c>PartOIsolationTests.SharedVentilationSystem_Refuses</c> and
    /// <c>SharedAirHandlingUnit_AcrossTwoSystems_Refuses</c> assert, unchanged by this. The tests here are
    /// the other half: the two sides of that line, and every way a system can be real enough to keep
    /// refusing.
    /// </para>
    /// </summary>
    public class PartOIsolationRedundantSystemTests
    {
        // ---- The case this exists for --------------------------------------------------------------

        /// <summary>
        /// The estate-wide system, exactly as the real model carries it: related to the rooms of both
        /// flats, no terminal, no air movement, its named unit bare. Both flats are already on MVHRs that
        /// carry design terminals, so it states nothing about their air that is not already stated, and
        /// isolating one flat proceeds.
        /// </summary>
        [Fact]
        public void EstateWideSystem_OverRoomsAlreadyOnDesignTerminals_DoesNotRefuse()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Bedroom", "Kitchen");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.Empty(spaceIsolation.Refusals);
            Assert.True(spaceIsolation.IsIsolated);
        }

        /// <summary>
        /// The other flat isolates too - the estate-wide system straddles the cut whichever side is
        /// selected, so ignoring it has to be symmetric.
        /// </summary>
        [Fact]
        public void EstateWideSystem_EitherDwellingIsolates()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Bedroom", "Kitchen");

            Assert.True(partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel)).IsIsolated);
            Assert.True(partFModel.AdjacencyCluster.IsolateSpaces(Flat2(partFModel)).IsIsolated);
        }

        /// <summary>
        /// Ignoring it is a refusal decision and nothing else: the system is still extracted into the
        /// derived model, related to the rooms of the selected dwelling that were retained. Dropping it
        /// would be an edit to the model, which this is not allowed to be.
        /// </summary>
        [Fact]
        public void TheIgnoredSystem_IsStillCarriedIntoTheDerivedModel()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Bedroom", "Kitchen");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            VentilationSystem ventilationSystem = VentilationSystem_Named(spaceIsolation.AdjacencyCluster, "Central AHU");

            Assert.NotNull(ventilationSystem);
            Assert.Equal(
                ["Bathroom", "Studio"],
                Names(spaceIsolation.AdjacencyCluster.GetRelatedObjects<Space>(ventilationSystem)));
        }

        /// <summary>The source model is not touched by any of it.</summary>
        [Fact]
        public void TheSourceModel_IsUnchanged()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Bedroom", "Kitchen");

            int count_Space = partFModel.AdjacencyCluster.GetSpaces().Count;
            int count_System = partFModel.AdjacencyCluster.GetObjects<VentilationSystem>().Count;
            int count_Unit = partFModel.AdjacencyCluster.GetObjects<AirHandlingUnit>().Count;
            int count_Related = partFModel.AdjacencyCluster
                .GetRelatedObjects<Space>(VentilationSystem_Named(partFModel.AdjacencyCluster, "Central AHU")).Count;

            partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.Equal(count_Space, partFModel.AdjacencyCluster.GetSpaces().Count);
            Assert.Equal(count_System, partFModel.AdjacencyCluster.GetObjects<VentilationSystem>().Count);
            Assert.Equal(count_Unit, partFModel.AdjacencyCluster.GetObjects<AirHandlingUnit>().Count);
            Assert.Equal(count_Related, partFModel.AdjacencyCluster
                .GetRelatedObjects<Space>(VentilationSystem_Named(partFModel.AdjacencyCluster, "Central AHU")).Count);
        }

        // ---- Fail closed: every way a shared system stays real -------------------------------------

        /// <summary>
        /// The same estate-wide system over a room nothing else serves - the corridor. That room's
        /// ventilation is stated by this system alone, so it is plant awaiting detail rather than a second
        /// statement about air already accounted for, and the refusal stands.
        /// </summary>
        [Fact]
        public void SharedSystem_OverARoomNothingElseServes_StillRefuses()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Corridor");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Null(spaceIsolation.AdjacencyCluster);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Estate MV") && x.Contains("Corridor"));
        }

        /// <summary>
        /// A shared system that carries design terminals of its own is the design chain, however many other
        /// systems its rooms are also on. Refuses.
        /// </summary>
        [Fact]
        public void SharedSystem_WithItsOwnDesignTerminals_StillRefuses()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            VentilationSystem ventilationSystem = EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Bedroom", "Kitchen");

            VentilationTerminal ventilationTerminal = new("Central supply terminal", FlowClassification.Supply, 50);
            partFModel.AdjacencyCluster.AddObject(ventilationTerminal);
            partFModel.AdjacencyCluster.AddRelation(ventilationSystem, ventilationTerminal);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Estate MV"));
        }

        /// <summary>
        /// A shared system whose named unit has had a product selected against it has a capacity somebody
        /// chose, so splitting it between dwellings is a real decision. Refuses.
        /// </summary>
        [Fact]
        public void SharedSystem_WhoseUnitHasASelectedProduct_StillRefuses()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Bedroom", "Kitchen");

            AirHandlingUnit airHandlingUnit = AirHandlingUnit_Named(partFModel.AdjacencyCluster, "Central AHU");
            airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, new VentilationUnitReference("Manufacturer", "Model 500", "MAN-500"));
            partFModel.AdjacencyCluster.AddObject(airHandlingUnit);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Estate MV") || x.Contains("Central AHU"));
        }

        /// <summary>
        /// A shared system whose named unit carries its own supply condition - the object that becomes the
        /// unit's generated TAS plant zone - is plant that reaches the simulation. Refuses.
        /// </summary>
        [Fact]
        public void SharedSystem_WhoseUnitCarriesItsOwnAirMovement_StillRefuses()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Bedroom", "Kitchen");

            AirHandlingUnit airHandlingUnit = AirHandlingUnit_Named(partFModel.AdjacencyCluster, "Central AHU");
            AirHandlingUnitAirMovement airHandlingUnitAirMovement = new("Central AHU");
            partFModel.AdjacencyCluster.AddObject(airHandlingUnitAirMovement);
            partFModel.AdjacencyCluster.AddRelation(airHandlingUnit, airHandlingUnitAirMovement);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Estate MV") || x.Contains("Central AHU"));
        }

        /// <summary>
        /// A room whose only other system is itself bare is not covered: two weak statements about a room
        /// do not make either of them redundant. Refuses.
        /// </summary>
        [Fact]
        public void SharedSystem_WhoseRoomsOnlyOtherSystemIsAlsoBare_StillRefuses()
        {
            //Deliberately the fixture WITHOUT design terminals: the per-dwelling MVHRs are bare too.
            PartFModel partFModel = Fixture_WithoutDesignTerminals();

            EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Bedroom", "Kitchen");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Estate MV") || x.Contains("Central AHU"));
        }

        /// <summary>
        /// A shared unit reached through two bare systems, whose rooms ARE otherwise covered, is still
        /// refused where the unit itself is real - here through a design terminal hanging off it.
        /// </summary>
        [Fact]
        public void SharedUnit_ThatIsRealPlant_StillRefuses()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            EstateWideSystem(partFModel, "Central AHU", "Studio", "Bathroom", "Bedroom", "Kitchen");

            AirHandlingUnit airHandlingUnit = AirHandlingUnit_Named(partFModel.AdjacencyCluster, "Central AHU");
            VentilationTerminal ventilationTerminal = new("Central plant terminal", FlowClassification.Extract, 50);
            partFModel.AdjacencyCluster.AddObject(ventilationTerminal);
            partFModel.AdjacencyCluster.AddRelation(airHandlingUnit, ventilationTerminal);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Estate MV") || x.Contains("Central AHU"));
        }

        /// <summary>
        /// The dwelling's own MVHR straddling the cut is untouched by any of this: it carries the design
        /// terminals, so it is never redundant and never ignored.
        /// </summary>
        [Fact]
        public void ADesignSystem_StraddlingTheCut_StillRefuses()
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();

            //Flat 1's own MVHR is put on the corridor too.
            partFModel.AdjacencyCluster.AddRelation(
                VentilationSystem_Named(partFModel.AdjacencyCluster, "AHU 1"),
                partFModel.Get("Corridor"));

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Corridor"));
        }

        // ---- The boundary: coverage alone must never suppress a refusal ----------------------------

        /// <summary>
        /// <b>The regression that pins the limit of the redundancy rule.</b>
        ///
        /// <para>
        /// Every room here carries a per-dwelling Part O design terminal on its own MVHR, and a central
        /// system spans the selected dwelling and the excluded one on top of them. That is the exact
        /// arrangement the redundancy rule was written for - and the one where getting it wrong is worst,
        /// because a central plant that genuinely serves both dwellings would be silently split.
        /// </para>
        ///
        /// <para>
        /// So the rule has TWO conditions, and this proves the second one is load-bearing: coverage of the
        /// rooms is <b>not</b> sufficient on its own. Whatever else is true, a central system that is itself
        /// in the design chain - by any of the markers below - keeps refusing, however thoroughly its rooms
        /// are already served.
        /// </para>
        ///
        /// <para>
        /// <b>Where the line falls, stated plainly.</b> A central system carrying <i>none</i> of these
        /// markers, over rooms that are all covered, IS ignored - that is
        /// <see cref="EstateWideSystem_OverRoomsAlreadyOnDesignTerminals_DoesNotRefuse"/>, and it is the
        /// real model's <c>MV 1</c>. Nothing in the model distinguishes such a system from central plant
        /// nobody has detailed yet; both are a bare <see cref="VentilationSystem"/> naming a bare
        /// <see cref="AirHandlingUnit"/>. Detailing the plant even slightly - one terminal, one air
        /// movement, one selected product - puts it back on the refusing side, which is what this asserts.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData("the central system carries its own design terminal")]
        [InlineData("the central system moves air between rooms")]
        [InlineData("the central unit has a product selected against it")]
        [InlineData("the central unit carries its own supply condition")]
        [InlineData("the central unit carries a design terminal")]
        [InlineData("the central system names a dwelling unit that is real plant")]
        public void GenuineSharedCentralPlant_OverRoomsThatAlsoHaveDwellingTerminals_StillRefuses(string marker)
        {
            PartFModel partFModel = Fixture_WithDesignTerminals();
            AdjacencyCluster adjacencyCluster = partFModel.AdjacencyCluster;

            //Studio and Bathroom are Flat 1 (selected); Bedroom and Kitchen are Flat 2 (excluded). All four
            //carry a per-dwelling design terminal already, so the coverage condition holds for every room.
            string name_Unit = marker == "the central system names a dwelling unit that is real plant" ? "AHU 1" : "Central AHU";

            VentilationSystem ventilationSystem = EstateWideSystem(
                partFModel,
                name_Unit,
                createUnit: name_Unit != "AHU 1",
                "Studio", "Bathroom", "Bedroom", "Kitchen");

            AirHandlingUnit airHandlingUnit = AirHandlingUnit_Named(adjacencyCluster, name_Unit);

            switch (marker)
            {
                case "the central system carries its own design terminal":
                    {
                        VentilationTerminal ventilationTerminal = new("Central supply terminal", FlowClassification.Supply, 50);
                        adjacencyCluster.AddObject(ventilationTerminal);
                        adjacencyCluster.AddRelation(ventilationSystem, ventilationTerminal);
                        break;
                    }

                case "the central system moves air between rooms":
                    {
                        //Deliberately BETWEEN TWO SELECTED ROOMS, so Refusals_AirMovementScope cannot fire
                        //and the refusal can only have come from the ventilation scope check under test.
                        SpaceAirMovement spaceAirMovement = new("Studio to Bathroom central", 10, "Studio", "Bathroom");
                        adjacencyCluster.AddObject(spaceAirMovement);
                        adjacencyCluster.AddRelation(ventilationSystem, spaceAirMovement);
                        break;
                    }

                case "the central unit has a product selected against it":
                    {
                        airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, new VentilationUnitReference("Manufacturer", "Central 900", "MAN-900"));
                        adjacencyCluster.AddObject(airHandlingUnit);
                        break;
                    }

                case "the central unit carries its own supply condition":
                    {
                        AirHandlingUnitAirMovement airHandlingUnitAirMovement = new(name_Unit);
                        adjacencyCluster.AddObject(airHandlingUnitAirMovement);
                        adjacencyCluster.AddRelation(airHandlingUnit, airHandlingUnitAirMovement);
                        break;
                    }

                case "the central unit carries a design terminal":
                    {
                        VentilationTerminal ventilationTerminal = new("Central plant terminal", FlowClassification.Extract, 50);
                        adjacencyCluster.AddObject(ventilationTerminal);
                        adjacencyCluster.AddRelation(airHandlingUnit, ventilationTerminal);
                        break;
                    }

                case "the central system names a dwelling unit that is real plant":
                    {
                        //The realistic shape: the central system names Flat 1's OWN unit, and that unit is
                        //real plant the way every unit the Part O preparation builds is - it carries its own
                        //supply condition. Flat 1's unit is then genuinely serving Flat 2's rooms.
                        AirHandlingUnitAirMovement airHandlingUnitAirMovement = new("AHU 1");
                        adjacencyCluster.AddObject(airHandlingUnitAirMovement);
                        adjacencyCluster.AddRelation(airHandlingUnit, airHandlingUnitAirMovement);
                        break;
                    }

                default:
                    throw new System.ArgumentException(marker);
            }

            //The premise: coverage really does hold for every room the central system claims, so if the
            //refusal survives it is the design-chain condition doing it and not a gap in the coverage test.
            foreach (string name_Space in new[] { "Studio", "Bathroom", "Bedroom", "Kitchen" })
            {
                Assert.Contains(
                    adjacencyCluster.GetRelatedObjects<VentilationTerminal>(partFModel.Get(name_Space)) ?? [],
                    x => x is not null);
            }

            SpaceIsolation spaceIsolation = adjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(
                spaceIsolation.IsIsolated,
                string.Format("Isolation proceeded although {0}. Coverage by per-dwelling terminals must not suppress a refusal over genuine central plant.", marker));

            Assert.Null(spaceIsolation.AdjacencyCluster);

            Assert.Contains(
                spaceIsolation.Refusals,
                x => x.Contains("Estate MV") || x.Contains(name_Unit));
        }

        // ---- Fixture -------------------------------------------------------------------------------

        /// <summary>
        /// Flat 1 (Studio, Bathroom), Flat 2 (Bedroom, Kitchen), Corridor - each flat on its own MVHR,
        /// and every one of those four rooms carrying a design ventilation terminal on its own system, the
        /// way the Part O preparation leaves the model. The corridor deliberately has none.
        /// </summary>
        private static PartFModel Fixture_WithDesignTerminals()
        {
            PartFModel partFModel = Fixture_WithoutDesignTerminals();

            DesignTerminal(partFModel, "AHU 1", "Studio", FlowClassification.Supply, 30);
            DesignTerminal(partFModel, "AHU 1", "Bathroom", FlowClassification.Extract, 8);
            DesignTerminal(partFModel, "AHU 2", "Bedroom", FlowClassification.Supply, 25);
            DesignTerminal(partFModel, "AHU 2", "Kitchen", FlowClassification.Extract, 13);

            return partFModel;
        }

        /// <summary>The same building with bare systems - no terminal anywhere.</summary>
        private static PartFModel Fixture_WithoutDesignTerminals()
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

            MVHR(partFModel, "AHU 1", "Studio", "Bathroom");
            MVHR(partFModel, "AHU 2", "Bedroom", "Kitchen");

            RealConstructions(partFModel.AdjacencyCluster);

            return partFModel;
        }

        private static void MVHR(PartFModel partFModel, string name_Unit, params string[] names_Space)
        {
            VentilationSystem ventilationSystem = new(name_Unit, new VentilationSystemType("MVHR", "Fixture"));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, name_Unit);

            partFModel.AdjacencyCluster.AddObject(ventilationSystem);

            foreach (string name_Space in names_Space)
            {
                partFModel.AdjacencyCluster.AddRelation(ventilationSystem, partFModel.Get(name_Space));
            }

            partFModel.AdjacencyCluster.AddObject(new AirHandlingUnit(name_Unit, 20, 20));
        }

        /// <summary>
        /// One design ventilation terminal, related to its room and to the system that serves it - the
        /// shape the Part O preparation builds, and the one the design chain is recognised by.
        /// </summary>
        private static void DesignTerminal(PartFModel partFModel, string name_System, string name_Space, FlowClassification flowClassification, double flow_Lps)
        {
            VentilationTerminal ventilationTerminal = new(string.Format("{0} - {1} terminal", name_Space, flowClassification), flowClassification, flow_Lps);

            partFModel.AdjacencyCluster.AddObject(ventilationTerminal);
            partFModel.AdjacencyCluster.AddRelation(VentilationSystem_Named(partFModel.AdjacencyCluster, name_System), ventilationTerminal);
            partFModel.AdjacencyCluster.AddRelation(partFModel.Get(name_Space), ventilationTerminal);
        }

        /// <summary>
        /// The estate-wide MV system: related to the named rooms, naming a unit that carries nothing at all.
        /// </summary>
        private static VentilationSystem EstateWideSystem(PartFModel partFModel, string name_Unit, params string[] names_Space)
        {
            return EstateWideSystem(partFModel, name_Unit, true, names_Space);
        }

        /// <summary>
        /// <paramref name="createUnit"/> is false where the system names a unit the fixture already holds -
        /// a central system on a dwelling's own unit - so the model keeps ONE unit of that name rather than
        /// two, which is what the shared-unit case is about.
        /// </summary>
        private static VentilationSystem EstateWideSystem(PartFModel partFModel, string name_Unit, bool createUnit, params string[] names_Space)
        {
            VentilationSystem ventilationSystem = new("Estate MV", new VentilationSystemType("MV", "Fixture"));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, name_Unit);
            ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, name_Unit);

            partFModel.AdjacencyCluster.AddObject(ventilationSystem);

            foreach (string name_Space in names_Space)
            {
                partFModel.AdjacencyCluster.AddRelation(ventilationSystem, partFModel.Get(name_Space));
            }

            if (createUnit)
            {
                partFModel.AdjacencyCluster.AddObject(new AirHandlingUnit(name_Unit, 20, 20));
            }

            return ventilationSystem;
        }

        private static void RealConstructions(AdjacencyCluster adjacencyCluster)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                Construction construction = new(
                    panel.Construction.Guid,
                    panel.Construction.Name,
                    [new ConstructionLayer("Concrete", 0.2)]);

                adjacencyCluster.AddObject(SAM.Analytical.Create.Panel(panel, construction));
            }
        }

        private static List<Space> Flat1(PartFModel partFModel)
        {
            return [partFModel.Get("Studio"), partFModel.Get("Bathroom")];
        }

        private static List<Space> Flat2(PartFModel partFModel)
        {
            return [partFModel.Get("Bedroom"), partFModel.Get("Kitchen")];
        }

        private static VentilationSystem VentilationSystem_Named(AdjacencyCluster adjacencyCluster, string name)
        {
            return adjacencyCluster.GetObjects<VentilationSystem>()
                .Find(x => x.GetValue<string>(VentilationSystemParameter.SupplyUnitName) == name);
        }

        private static AirHandlingUnit AirHandlingUnit_Named(AdjacencyCluster adjacencyCluster, string name)
        {
            return adjacencyCluster.GetObjects<AirHandlingUnit>().Find(x => x.Name == name);
        }

        private static List<string> Names(IEnumerable<Space> spaces)
        {
            List<string> result = [];

            foreach (Space space in spaces ?? [])
            {
                result.Add(space?.Name);
            }

            result.Sort();

            return result;
        }
    }
}
