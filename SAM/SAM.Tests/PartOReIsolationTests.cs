// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    /// <summary>
    /// <b>Isolating a model that has already been isolated.</b>
    /// <para>
    /// A person re-prepares a Part O run, or hands a saved isolated model back to <c>Prepare &amp; Run</c>,
    /// and the isolation is asked to do its work over a model already carrying its own cuts. The contract
    /// is re-entrance, not suppression: the second run does the same work over the same question and must
    /// arrive at the same model - the same panels under the same guids, the same adiabatic cut, the same
    /// shading context, the same geometry - and must state what it is carrying just as truthfully.
    /// </para>
    /// <para>
    /// The fixture is the acceptance building of <see cref="PartOIsolationTests"/>: <b>Flat 1</b> (Studio,
    /// Bathroom), <b>Flat 2</b> (Bedroom, Kitchen) and a <b>Corridor</b> between them, each flat on its own
    /// MVHR.
    /// </para>
    /// </summary>
    public class PartOReIsolationTests
    {
        // ---- A. The first isolation still does what it did ---------------------------------------------

        /// <summary>The intended cut: the interface to the excluded corridor, adiabatic and recorded as a cut.</summary>
        [Fact]
        public void FirstIsolation_CutsToTheExcludedSpace()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.True(spaceIsolation.IsIsolated);
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);
            Assert.Equal(1, spaceIsolation.Count_RemovedCutAperture);

            Panel panel_Cut = Cut(spaceIsolation.AdjacencyCluster);

            Assert.True(Analytical.Query.Adiabatic(panel_Cut));
            Assert.True(Analytical.Query.IsolationCut(panel_Cut));
            Assert.Empty(panel_Cut.Apertures ?? []);
        }

        /// <summary>An adiabatic wall somebody authored is not a cut, and does not claim to be one.</summary>
        [Fact]
        public void AuthoredAdiabaticExternalWall_IsNotRecordedAsACut()
        {
            PartFModel partFModel = Fixture();

            Panel panel_External = External(partFModel.AdjacencyCluster, "Studio");
            panel_External.SetValue(PanelParameter.Adiabatic, true);
            partFModel.AdjacencyCluster.AddObject(panel_External);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel_Result = spaceIsolation.AdjacencyCluster.GetObject<Panel>(panel_External.Guid);

            Assert.True(Analytical.Query.Adiabatic(panel_Result));
            Assert.False(Analytical.Query.IsolationCut(panel_Result));

            //And it is still counted as the one cut the model has, not two.
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);
        }

        // ---- B. The second isolation does not cut again ------------------------------------------------

        /// <summary>
        /// Isolating the derived model again with the same scope produces the same panels under the same
        /// guids: nothing is duplicated, nothing is added, nothing disappears.
        /// </summary>
        [Fact]
        public void SecondIsolation_SameScope_ProducesTheSamePanels()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));

            Assert.True(two.IsIsolated);
            Assert.Equal(Guids(one.AdjacencyCluster), Guids(two.AdjacencyCluster));
        }

        /// <summary>The cut is not cut again: one adiabatic interface after two isolations, not two panels standing for one.</summary>
        [Fact]
        public void SecondIsolation_DoesNotDoubleCut()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));

            Assert.Single(Cuts(one.AdjacencyCluster));
            Assert.Single(Cuts(two.AdjacencyCluster));
            Assert.Equal(Cuts(one.AdjacencyCluster)[0].Guid, Cuts(two.AdjacencyCluster)[0].Guid);
        }

        /// <summary>
        /// The second run's record states the cut the model carries rather than reporting none.
        /// <para>
        /// The second isolation makes no cut - there is nothing left to cut off - so the adjacency
        /// comparison finds nothing, and the count used to fall to zero. A re-prepared dwelling would then
        /// disclose "0 interface(s) to excluded spaces treated as adiabatic" over a model with one, in the
        /// evidence for a Part O submission.
        /// </para>
        /// </summary>
        [Fact]
        public void SecondIsolation_StatesTheCutItCarries()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));

            Assert.Equal(one.Count_AdiabaticPanel, two.Count_AdiabaticPanel);
            Assert.Equal(one.Count_ShadePanel, two.Count_ShadePanel);

            Assert.Contains(two.Notes, x => x.Contains("1 interface(s) to excluded spaces treated as adiabatic", StringComparison.Ordinal));

            //Nothing was stripped a second time - the cut lost its door on the first run.
            Assert.Equal(0, two.Count_RemovedCutAperture);
        }

        // ---- C. Geometry and identity ------------------------------------------------------------------

        /// <summary>Repeated isolation does not move, retype or reconstruct anything: three rounds, one model.</summary>
        [Fact]
        public void RepeatedIsolation_LeavesGeometryAndFabricUnchanged()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));
            SpaceIsolation three = two.AdjacencyCluster.IsolateSpaces(Selected(two, partFModel));

            Assert.Equal(Fabric(one.AdjacencyCluster), Fabric(two.AdjacencyCluster));
            Assert.Equal(Fabric(one.AdjacencyCluster), Fabric(three.AdjacencyCluster));
        }

        /// <summary>The selected spaces keep their guids, which is what every downstream result is keyed on.</summary>
        [Fact]
        public void RepeatedIsolation_KeepsSpaceIdentity()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));

            foreach (Space space in Flat1(partFModel))
            {
                Assert.NotNull(two.AdjacencyCluster.GetObject<Space>(space.Guid));
            }

            //And the scope token - which names the run's outputs - is the same both times.
            Assert.Equal(
                PartOIsolationContext.Token(SpaceGuids(one.AdjacencyCluster)),
                PartOIsolationContext.Token(SpaceGuids(two.AdjacencyCluster)));
        }

        /// <summary>The plant the dwelling depends on is carried once, not accumulated round after round.</summary>
        [Fact]
        public void RepeatedIsolation_DoesNotAccumulatePlant()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));

            Assert.Single(one.AdjacencyCluster.GetObjects<AirHandlingUnit>());
            Assert.Single(two.AdjacencyCluster.GetObjects<AirHandlingUnit>());
            Assert.Single(two.AdjacencyCluster.GetObjects<VentilationSystem>());
        }

        /// <summary>The shading context is restated, not stacked: no second copy of the neighbour facade.</summary>
        [Fact]
        public void RepeatedIsolation_DoesNotAccumulateShade()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));

            Assert.Equal(Shades(one.AdjacencyCluster), Shades(two.AdjacencyCluster));
        }

        // ---- D. The air boundary on the cut ------------------------------------------------------------

        /// <summary>
        /// An open interface between a flat and the corridor is the cut too, and has to be simulatable as
        /// one.
        /// <para>
        /// It used to be left exactly as it was: <c>PanelType.Air</c> with a single adjacent space, which
        /// <c>Query.Adiabatic</c> reports as not adiabatic whatever the flag says, so SAM_Tas never nulled
        /// its link and the opening reached the simulation as a hole onto nothing.
        /// </para>
        /// </summary>
        [Fact]
        public void AirBoundaryOnTheCut_BecomesTheAdiabaticCut()
        {
            PartFModel partFModel = Fixture();
            Air(partFModel, "Studio", "Corridor");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel_Cut = Cut(spaceIsolation.AdjacencyCluster);

            Assert.NotEqual(PanelType.Air, panel_Cut.PanelType);
            Assert.True(Analytical.Query.Adiabatic(panel_Cut));
            Assert.True(Analytical.Query.IsolationCut(panel_Cut));
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);

            //A re-typed surface keeps a fabric whether or not a default construction library is configured.
            Assert.NotNull(panel_Cut.Construction);
        }

        /// <summary>
        /// And it stays that boundary when the model is isolated again.
        /// <para>
        /// This is the failure that only appeared on the second run. An air boundary left uncut is external
        /// in the model being filtered as well by then, so nothing can tell it from a surface that was
        /// always a boundary to the outside, and it was silently re-typed into a genuine external wall with
        /// a solid default construction - the flat gaining an outside wall where the corridor opening used
        /// to be, with solar gain and heat loss behind it.
        /// </para>
        /// </summary>
        [Fact]
        public void AirBoundaryOnTheCut_DoesNotBecomeAnExternalWallOnTheSecondRun()
        {
            PartFModel partFModel = Fixture();
            Air(partFModel, "Studio", "Corridor");

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));

            Panel panel_One = Cut(one.AdjacencyCluster);
            Panel panel_Two = Cut(two.AdjacencyCluster);

            Assert.Equal(panel_One.Guid, panel_Two.Guid);
            Assert.Equal(panel_One.PanelType, panel_Two.PanelType);
            Assert.True(Analytical.Query.Adiabatic(panel_Two));

            //And it still HAS a fabric, both times and the same one. Re-typing an air boundary reaches for
            //the default construction library, which is a setting: on a machine where it is not configured
            //- a test host, a CI runner - it answers nothing, and assigning that would leave the surface
            //with no construction at all. Asserted rather than assumed, because the two environments
            //legitimately give different constructions here and only the invariant is common to both.
            Assert.NotNull(panel_One.Construction);
            Assert.NotNull(panel_Two.Construction);
            Assert.Equal(panel_One.Construction.Guid, panel_Two.Construction.Guid);
            Assert.Equal(panel_One.Construction.Name, panel_Two.Construction.Name);
        }

        /// <summary>An air boundary between two spaces that are both kept is still an air boundary.</summary>
        [Fact]
        public void AirBoundaryBetweenTwoSelectedSpaces_StaysAnAirBoundary()
        {
            PartFModel partFModel = Fixture();
            Air(partFModel, "Studio", "Bathroom");

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));

            foreach (AdjacencyCluster adjacencyCluster in new[] { one.AdjacencyCluster, two.AdjacencyCluster })
            {
                List<Panel> panels = adjacencyCluster.GetPanels().FindAll(x => x.PanelType == PanelType.Air);

                Assert.Single(panels);
                Assert.False(Analytical.Query.IsolationCut(panels[0]));
                Assert.Equal(2, adjacencyCluster.GetRelatedObjects<Space>(panels[0]).Count);
            }
        }

        // ---- D2. A cut this run did not make -----------------------------------------------------------

        /// <summary>
        /// A model filtered by <c>SAMAnalytical.FilterBySpaces</c> arrives with its cuts marked but their
        /// openings intact, and isolating it must still take the openings off.
        /// <para>
        /// <c>AdjacencyCluster.Filter</c> is a public API, and the component calls it directly: it makes the
        /// cut and marks it, but strips nothing, because removing a person openings is not what a geometric
        /// filter is for. The Part O isolation is what decides that a door onto an omitted space cannot
        /// stay - and it recognised this panel as a cut, counted it in the disclosure, and then left the
        /// door on it. The run would have stated the interface was treated as adiabatic while the derived
        /// model still opened onto the corridor, and the conversion would have exported that door as an
        /// opening to outside.
        /// </para>
        /// </summary>
        [Fact]
        public void CutFromFilterBySpaces_StillLosesItsApertures()
        {
            PartFModel partFModel = Fixture();

            //Exactly what the component does - Filter on its own, no isolation.
            AdjacencyCluster adjacencyCluster = partFModel.AdjacencyCluster.Filter(Flat1(partFModel), true);

            Panel panel_Filtered = Cut(adjacencyCluster);

            //The premise: the filter marked the cut and kept the door.
            Assert.NotEmpty(panel_Filtered.Apertures ?? []);

            List<Space> spaces = [];
            foreach (Space space in Flat1(partFModel))
            {
                spaces.Add(adjacencyCluster.GetObject<Space>(space.Guid));
            }

            SpaceIsolation spaceIsolation = adjacencyCluster.IsolateSpaces(spaces);

            Panel panel_Cut = Cut(spaceIsolation.AdjacencyCluster);

            Assert.Empty(panel_Cut.Apertures ?? []);
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);
            Assert.Equal(1, spaceIsolation.Count_RemovedCutAperture);
        }

        /// <summary>
        /// And an authored adiabatic wall handed in the same way keeps its openings: the marker is what
        /// decides, never the adiabatic flag.
        /// </summary>
        [Fact]
        public void AuthoredAdiabaticWall_KeepsItsAperturesThroughAReIsolation()
        {
            PartFModel partFModel = Fixture();

            Panel panel_External = External(partFModel.AdjacencyCluster, "Studio");
            panel_External.SetValue(PanelParameter.Adiabatic, true);
            partFModel.AdjacencyCluster.AddObject(panel_External);

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));
            SpaceIsolation two = one.AdjacencyCluster.IsolateSpaces(Selected(one, partFModel));

            Panel panel_Two = two.AdjacencyCluster.GetObject<Panel>(panel_External.Guid);

            Assert.True(Analytical.Query.Adiabatic(panel_Two));
            Assert.False(Analytical.Query.IsolationCut(panel_Two));
            Assert.NotEmpty(panel_Two.Apertures ?? []);
            Assert.Equal(0, two.Count_RemovedCutAperture);
        }

        // ---- E. Changed scope --------------------------------------------------------------------------

        /// <summary>
        /// Narrowing the scope on an already isolated model reaches the same model as isolating that
        /// narrower scope from the whole building - the same panels, the same cuts, the same shade.
        /// </summary>
        [Fact]
        public void NarrowedScope_FromDerived_MatchesIsolatingItFromTheSource()
        {
            PartFModel partFModel = Fixture();

            List<Space> both = Flat1(partFModel);
            both.AddRange([partFModel.Get("Bedroom"), partFModel.Get("Kitchen")]);

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(both);

            List<Space> flat1_Derived = [];
            foreach (Space space in Flat1(partFModel))
            {
                flat1_Derived.Add(one.AdjacencyCluster.GetObject<Space>(space.Guid));
            }

            SpaceIsolation narrowed = one.AdjacencyCluster.IsolateSpaces(flat1_Derived);
            SpaceIsolation direct = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.Equal(Guids(direct.AdjacencyCluster), Guids(narrowed.AdjacencyCluster));
            Assert.Equal(Fabric(direct.AdjacencyCluster), Fabric(narrowed.AdjacencyCluster));
            Assert.Equal(direct.Count_AdiabaticPanel, narrowed.Count_AdiabaticPanel);
        }

        // ---- F. The record survives the model json -----------------------------------------------------

        /// <summary>
        /// The cut record travels in the .sam, so a reopened isolated model is still recognisable as one.
        /// Without that, re-preparing a saved run would report no cut at all.
        /// </summary>
        [Fact]
        public void CutRecord_SurvivesTheJsonRoundTrip()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation one = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            AdjacencyCluster adjacencyCluster = Core.Query.Clone(one.AdjacencyCluster);

            Assert.True(Analytical.Query.IsolationCut(Cut(adjacencyCluster)));

            List<Space> spaces = [];
            foreach (Space space in Flat1(partFModel))
            {
                spaces.Add(adjacencyCluster.GetObject<Space>(space.Guid));
            }

            SpaceIsolation two = adjacencyCluster.IsolateSpaces(spaces);

            Assert.Equal(1, two.Count_AdiabaticPanel);
        }

        // ---- Helpers -----------------------------------------------------------------------------------

        /// <summary>The selected spaces as they exist in the derived model, which is what a re-run holds.</summary>
        private static List<Space> Selected(SpaceIsolation spaceIsolation, PartFModel partFModel)
        {
            List<Space> result = [];
            foreach (Space space in Flat1(partFModel))
            {
                result.Add(spaceIsolation.AdjacencyCluster.GetObject<Space>(space.Guid));
            }

            return result;
        }

        /// <summary>The one panel the isolation cut.</summary>
        private static Panel Cut(AdjacencyCluster adjacencyCluster)
        {
            return Assert.Single(Cuts(adjacencyCluster));
        }

        private static List<Panel> Cuts(AdjacencyCluster adjacencyCluster)
        {
            return (adjacencyCluster.GetPanels() ?? []).FindAll(x => Analytical.Query.IsolationCut(x));
        }

        /// <summary>Every panel guid, so two models can be compared for what they contain.</summary>
        private static List<Guid> Guids(AdjacencyCluster adjacencyCluster)
        {
            List<Guid> result = [];
            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                result.Add(panel.Guid);
            }

            result.Sort();

            return result;
        }

        private static List<Guid> SpaceGuids(AdjacencyCluster adjacencyCluster)
        {
            List<Guid> result = [];
            foreach (Space space in adjacencyCluster.GetSpaces() ?? [])
            {
                result.Add(space.Guid);
            }

            result.Sort();

            return result;
        }

        /// <summary>
        /// What each panel IS - identity, type, construction, area, adiabatic state and openings - as one
        /// comparable statement, so a round that quietly re-typed or re-built a surface is caught.
        /// </summary>
        private static List<string> Fabric(AdjacencyCluster adjacencyCluster)
        {
            List<string> result = [];
            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                result.Add(string.Format(
                    "{0}|{1}|{2}|{3:F6}|{4}|{5}|{6}|{7}",
                    panel.Guid,
                    panel.PanelType,
                    panel.Construction?.Name,
                    panel.GetArea(),
                    Analytical.Query.Adiabatic(panel),
                    Analytical.Query.IsolationCut(panel),
                    panel.Apertures?.Count ?? 0,
                    adjacencyCluster.GetRelatedObjects<Space>(panel)?.Count ?? 0));
            }

            result.Sort(StringComparer.Ordinal);

            return result;
        }

        private static List<Guid> Shades(AdjacencyCluster adjacencyCluster)
        {
            List<Guid> result = [];
            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                if (panel.PanelType == PanelType.Shade)
                {
                    result.Add(panel.Guid);
                }
            }

            result.Sort();

            return result;
        }

        /// <summary>Retypes the partition between two named spaces to an open air boundary.</summary>
        private static void Air(PartFModel partFModel, string name_1, string name_2)
        {
            Panel panel = Between(partFModel.AdjacencyCluster, name_1, name_2);

            partFModel.AdjacencyCluster.AddObject(AnalyticalCreate.Panel(panel, PanelType.Air));
        }

        private static Panel Between(AdjacencyCluster adjacencyCluster, string name_1, string name_2)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels())
            {
                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(panel);
                if (spaces is null || spaces.Count != 2)
                {
                    continue;
                }

                if ((spaces[0].Name == name_1 && spaces[1].Name == name_2) || (spaces[0].Name == name_2 && spaces[1].Name == name_1))
                {
                    return panel;
                }
            }

            throw new ArgumentException(string.Format("No panel joins '{0}' and '{1}'.", name_1, name_2));
        }

        private static Panel External(AdjacencyCluster adjacencyCluster, string name)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels())
            {
                if (panel.PanelType != PanelType.WallExternal)
                {
                    continue;
                }

                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(panel);
                if (spaces is not null && spaces.Count == 1 && spaces[0].Name == name)
                {
                    return panel;
                }
            }

            throw new ArgumentException(string.Format("No external panel belongs to '{0}'.", name));
        }

        private static List<Space> Flat1(PartFModel partFModel)
        {
            return [partFModel.Get("Studio"), partFModel.Get("Bathroom")];
        }

        /// <summary>The acceptance building of <see cref="PartOIsolationTests"/>.</summary>
        private static PartFModel Fixture()
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

            //Real thicknesses, or Query.Adiabatic reports every bare construction adiabatic in its own right
            //and the cut tests would pass without the isolation having done anything.
            foreach (Panel panel in partFModel.AdjacencyCluster.GetPanels() ?? [])
            {
                Construction construction = new(
                    panel.Construction.Guid,
                    panel.Construction.Name,
                    [new ConstructionLayer("Concrete", 0.2)]);

                partFModel.AdjacencyCluster.AddObject(AnalyticalCreate.Panel(panel, construction));
            }

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
    }
}
