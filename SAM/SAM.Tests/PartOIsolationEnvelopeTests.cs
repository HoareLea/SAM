// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    /// <summary>
    /// <b>What isolation may and may not turn adiabatic.</b>
    /// <para>
    /// An isolation cut is one thing only: a panel that separated two thermal spaces in the SOURCE model and
    /// has exactly one of them retained in the isolated scope. A panel that was ALREADY a boundary to the
    /// outside in the source model is not a cut and must keep the envelope it had - its type, construction,
    /// apertures, exposure and orientation - because the flat still has that roof, that façade and that
    /// ground floor whether or not its neighbours are being simulated.
    /// </para>
    /// <para>
    /// The distinction cannot be drawn from the derived cluster alone. After filtering, a genuine roof and a
    /// selected-to-excluded cut BOTH have exactly one adjacent space; only the source adjacency separates
    /// them. These tests state that truth table on every envelope type the Part O models carry, which
    /// <c>PartOIsolationTests</c>' fixture - external walls and internal partitions only - could not reach.
    /// </para>
    /// </summary>
    public class PartOIsolationEnvelopeTests
    {
        // ---- The envelope survives isolation ----------------------------------------------------------

        [Theory]
        [InlineData(PanelType.Roof)]
        [InlineData(PanelType.FloorExposed)]
        [InlineData(PanelType.SlabOnGrade)]
        [InlineData(PanelType.WallExternal)]
        [InlineData(PanelType.CurtainWall)]
        [InlineData(PanelType.UndergroundWall)]
        public void AGenuineExternalPanelOfASelectedSpace_IsNotTurnedAdiabatic(PanelType panelType)
        {
            PartFModel partFModel = Fixture();
            Panel panel = Envelope(partFModel, "Studio", panelType, "Studio " + panelType);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel_Isolated = Panel_Named(spaceIsolation.AdjacencyCluster, panel.Construction.Name);

            Assert.False(
                panel_Isolated.GetValue<bool>(PanelParameter.Adiabatic),
                string.Format(
                    "A {0} of a selected space bounded no excluded space in the source model, so isolation has cut nothing and it must keep its external boundary.",
                    panelType));
        }

        [Theory]
        [InlineData(PanelType.Roof)]
        [InlineData(PanelType.FloorExposed)]
        [InlineData(PanelType.SlabOnGrade)]
        [InlineData(PanelType.WallExternal)]
        public void AGenuineExternalPanelOfASelectedSpace_KeepsItsTypeAndConstruction(PanelType panelType)
        {
            PartFModel partFModel = Fixture();
            Panel panel = Envelope(partFModel, "Studio", panelType, "Studio " + panelType);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel_Isolated = Panel_Named(spaceIsolation.AdjacencyCluster, panel.Construction.Name);

            Assert.Equal(panelType, panel_Isolated.PanelType);
            Assert.Equal(panel.Construction.Guid, panel_Isolated.Construction.Guid);
            Assert.Equal(panel.GetFace3D().GetArea(), panel_Isolated.GetFace3D().GetArea(), 6);
        }

        /// <summary>
        /// A window in a genuine external wall opens onto the outside in the isolated model exactly as it did
        /// in the whole building. Only an aperture on a CUT has nothing left to open onto.
        /// </summary>
        [Fact]
        public void AnApertureInAGenuineExternalWall_Remains()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = Panel_External(spaceIsolation.AdjacencyCluster, "Studio");

            Assert.NotNull(panel.Apertures);
            Assert.NotEmpty(panel.Apertures);
        }

        // ---- The cut, which is the only thing that may become adiabatic -------------------------------

        [Fact]
        public void ASelectedToExcludedPartition_BecomesTheAdiabaticCut()
        {
            PartFModel partFModel = Fixture();

            //Studio to Corridor: two spaces in the source, one retained.
            Guid guid = Panel_Between(partFModel.AdjacencyCluster, "Studio", "Corridor").Guid;

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = Panel_Guid(spaceIsolation.AdjacencyCluster, guid);

            Assert.True(panel.GetValue<bool>(PanelParameter.Adiabatic));
        }

        [Fact]
        public void ASelectedToSelectedPartition_StaysInternalAndIsNotACut()
        {
            PartFModel partFModel = Fixture();

            //Studio to Bathroom: both retained, so nothing was cut.
            Guid guid = Panel_Between(partFModel.AdjacencyCluster, "Studio", "Bathroom").Guid;

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = Panel_Guid(spaceIsolation.AdjacencyCluster, guid);

            Assert.False(panel.GetValue<bool>(PanelParameter.Adiabatic));
            Assert.Equal(PanelType.WallInternal, panel.PanelType);
            Assert.Equal(2, spaceIsolation.AdjacencyCluster.GetRelatedObjects<Space>(panel).Count);
        }

        /// <summary>
        /// <b>The reported defect.</b> A genuine external boundary modelled as an <c>Air</c> panel - the shape
        /// a roof arrives in from a model whose top surface was left unzoned - is re-typed by the filter to
        /// the real envelope type its normal implies, and given that type's construction. It must not then
        /// also be marked adiabatic: the two statements contradict each other, and an adiabatic roof takes no
        /// solar gain and loses no heat.
        /// </summary>
        [Fact]
        public void AGenuineExternalAirPanel_IsRetypedButNotTurnedAdiabatic()
        {
            PartFModel partFModel = Fixture();
            Panel panel = Envelope(partFModel, "Studio", PanelType.Air, "Studio Air Roof");

            //One adjacent space in the SOURCE model: it opens onto the outside, not onto another space.
            Assert.Single(partFModel.AdjacencyCluster.GetRelatedObjects<Space>(panel));

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel_Isolated = Panel_Guid(spaceIsolation.AdjacencyCluster, panel.Guid);

            Assert.Equal(PanelType.Roof, panel_Isolated.PanelType);
            Assert.False(
                panel_Isolated.GetValue<bool>(PanelParameter.Adiabatic),
                "It was external in the source model, so isolation cut nothing here and it must keep its external boundary.");
        }

        /// <summary>
        /// Isolating a whole building cuts nothing at all - every space is retained - so no panel may come
        /// back adiabatic that was not authored that way.
        /// </summary>
        [Fact]
        public void SelectingEverySpace_TurnsNothingAdiabatic()
        {
            PartFModel partFModel = Fixture();
            Envelope(partFModel, "Studio", PanelType.Roof, "Studio Roof");
            Envelope(partFModel, "Bedroom", PanelType.Roof, "Bedroom Roof");

            List<Space> spaces = [.. Flat1(partFModel), .. Flat2(partFModel), partFModel.Get("Corridor")];

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(spaces);

            foreach (Panel panel in spaceIsolation.AdjacencyCluster.GetPanels() ?? [])
            {
                Assert.False(
                    panel.GetValue<bool>(PanelParameter.Adiabatic),
                    string.Format("'{0}' was turned adiabatic although no space was excluded.", panel.Construction?.Name));
            }
        }

        // ---- An authored adiabatic panel is not this run's doing --------------------------------------

        /// <summary>
        /// A panel a person - or a TBD/gbXML import - already marked adiabatic keeps that state, and is not
        /// counted as an interface to an excluded space in the disclosure note.
        /// </summary>
        [Fact]
        public void AnAuthoredAdiabaticExternalPanel_KeepsItsStateAndIsNotCountedAsACut()
        {
            PartFModel partFModel = Fixture();
            Panel panel = Envelope(partFModel, "Studio", PanelType.Roof, "Studio Roof");

            panel.SetValue(PanelParameter.Adiabatic, true);
            partFModel.AdjacencyCluster.AddObject(panel);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel_Isolated = Panel_Named(spaceIsolation.AdjacencyCluster, "Studio Roof");

            Assert.True(panel_Isolated.GetValue<bool>(PanelParameter.Adiabatic), "The authored state is preserved.");

            //One cut only - Studio to Corridor. The authored roof is not an interface to an excluded space.
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);
        }

        // ---- The invariant, stated over every panel ---------------------------------------------------

        /// <summary>
        /// <b>The acceptance criterion itself.</b> For every panel isolation turned adiabatic, the source
        /// model must show it separating a SELECTED space from an EXCLUDED one. Stated as a sweep rather than
        /// case by case, so an envelope type nobody thought to write a case for cannot slip through.
        /// </summary>
        [Fact]
        public void EveryPanelTurnedAdiabatic_SeparatedASelectedSpaceFromAnExcludedOneInTheSource()
        {
            PartFModel partFModel = Fixture();
            Envelope(partFModel, "Studio", PanelType.Roof, "Studio Roof");
            Envelope(partFModel, "Studio", PanelType.Air, "Studio Air Roof");
            Envelope(partFModel, "Bathroom", PanelType.SlabOnGrade, "Bathroom Ground Floor");
            Envelope(partFModel, "Bathroom", PanelType.FloorExposed, "Bathroom Soffit");

            List<Space> spaces_Selected = Flat1(partFModel);
            List<Guid> guids_Selected = spaces_Selected.ConvertAll(x => x.Guid);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(spaces_Selected);

            foreach (Panel panel in spaceIsolation.AdjacencyCluster.GetPanels() ?? [])
            {
                if (!panel.GetValue<bool>(PanelParameter.Adiabatic))
                {
                    continue;
                }

                List<Space> spaces_Source = partFModel.AdjacencyCluster.GetRelatedObjects<Space>(
                    Panel_Guid(partFModel.AdjacencyCluster, panel.Guid));

                bool selected = false;
                bool excluded = false;

                foreach (Space space in spaces_Source ?? [])
                {
                    if (guids_Selected.Contains(space.Guid))
                    {
                        selected = true;
                    }
                    else
                    {
                        excluded = true;
                    }
                }

                Assert.True(
                    selected && excluded,
                    string.Format(
                        "'{0}' ({1}) was turned adiabatic but bounded {2} space(s) in the source model, selected={3}, excluded={4}. Only a selected-to-excluded boundary is an isolation cut.",
                        panel.Construction?.Name,
                        panel.PanelType,
                        spaces_Source?.Count ?? 0,
                        selected,
                        excluded));
            }
        }

        /// <summary>
        /// The envelope classification before and after isolation, as the acceptance report asks for it. The
        /// external envelope of the retained spaces is carried across unchanged and in full; the only
        /// difference isolation makes is the cut.
        /// </summary>
        [Fact]
        public void TheRetainedEnvelope_IsCarriedAcrossUnchanged()
        {
            PartFModel partFModel = Fixture();
            Envelope(partFModel, "Studio", PanelType.Roof, "Studio Roof");
            Envelope(partFModel, "Studio", PanelType.Air, "Studio Air Roof");
            Envelope(partFModel, "Bathroom", PanelType.SlabOnGrade, "Bathroom Ground Floor");
            Envelope(partFModel, "Bathroom", PanelType.FloorExposed, "Bathroom Soffit");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            AdjacencyCluster adjacencyCluster = spaceIsolation.AdjacencyCluster;

            //The Air roof is re-typed to Roof, so Flat 1's two roofs both count as Roof here.
            Assert.Equal(2, Count(adjacencyCluster, PanelType.Roof, adiabatic: false));
            Assert.Equal(1, Count(adjacencyCluster, PanelType.SlabOnGrade, adiabatic: false));
            Assert.Equal(1, Count(adjacencyCluster, PanelType.FloorExposed, adiabatic: false));
            Assert.Equal(2, Count(adjacencyCluster, PanelType.WallExternal, adiabatic: false));

            //Exactly one cut - Studio to Corridor - and nothing else adiabatic.
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);
            Assert.Equal(1, Count(adjacencyCluster, PanelType.WallInternal, adiabatic: true));

            //Studio to Bathroom stays a live internal partition.
            Assert.Equal(1, Count(adjacencyCluster, PanelType.WallInternal, adiabatic: false));

            //Nothing of the envelope was lost on the way.
            Assert.Equal(0, Count(adjacencyCluster, PanelType.Air, adiabatic: false));
            Assert.Equal(0, Count(adjacencyCluster, PanelType.Air, adiabatic: true));
        }

        private static int Count(AdjacencyCluster adjacencyCluster, PanelType panelType, bool adiabatic)
        {
            int result = 0;

            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                //Shade is the excluded building's envelope, kept as context and not part of this count.
                if (panel.PanelType == panelType && panel.GetValue<bool>(PanelParameter.Adiabatic) == adiabatic)
                {
                    result++;
                }
            }

            return result;
        }

        // ---- The source model is not touched ----------------------------------------------------------

        [Fact]
        public void TheSourceModel_IsUnchangedByIsolation()
        {
            PartFModel partFModel = Fixture();
            Panel panel = Envelope(partFModel, "Studio", PanelType.Roof, "Studio Roof");

            int count_Space = partFModel.AdjacencyCluster.GetSpaces().Count;
            int count_Panel = partFModel.AdjacencyCluster.GetPanels().Count;

            partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.Equal(count_Space, partFModel.AdjacencyCluster.GetSpaces().Count);
            Assert.Equal(count_Panel, partFModel.AdjacencyCluster.GetPanels().Count);

            Panel panel_Source = Panel_Named(partFModel.AdjacencyCluster, "Studio Roof");
            Assert.False(panel_Source.GetValue<bool>(PanelParameter.Adiabatic));
            Assert.Equal(PanelType.Roof, panel_Source.PanelType);

            Panel panel_Cut = Panel_Between(partFModel.AdjacencyCluster, "Studio", "Corridor");
            Assert.False(panel_Cut.GetValue<bool>(PanelParameter.Adiabatic), "The cut exists only in the derived model.");

            Panel panel_External = Panel_External(partFModel.AdjacencyCluster, "Studio");
            Assert.NotEmpty(panel_External.Apertures);
        }

        // ---- Fixture ----------------------------------------------------------------------------------

        /// <summary>
        /// The acceptance building of <c>PartOIsolationTests</c>: Flat 1 (Studio, Bathroom), Flat 2 (Bedroom,
        /// Kitchen), and a Corridor joining them, each flat on its own MVHR.
        /// </summary>
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

            RealConstructions(partFModel.AdjacencyCluster);

            return partFModel;
        }

        /// <summary>
        /// One genuine envelope surface on a space: a single adjacent space, a real construction and, where
        /// the type is horizontal, a face whose normal points the way that type does.
        /// </summary>
        private static Panel Envelope(PartFModel partFModel, string name_Space, PanelType panelType, string name_Construction)
        {
            Construction construction = new(
                Guid.NewGuid(),
                name_Construction,
                [new ConstructionLayer("Concrete", 0.2)]);

            Panel panel = AnalyticalCreate.Panel(construction, panelType, Face(panelType));

            partFModel.AdjacencyCluster.AddObject(panel);
            partFModel.AdjacencyCluster.AddRelation(partFModel.Get(name_Space), panel);

            return panel;
        }

        /// <summary>A horizontal face for the roof and floor types, and a vertical one for the wall types.</summary>
        private static Face3D Face(PanelType panelType)
        {
            switch (panelType)
            {
                //Air stands in for the unzoned top surface the defect was reported on, so it is horizontal
                //and faces up - which is what makes the filter re-type it to a Roof.
                case PanelType.Air:
                case PanelType.Roof:
                    return Horizontal(3, true);

                case PanelType.FloorExposed:
                case PanelType.SlabOnGrade:
                    return Horizontal(0, false);

                default:
                    return new Face3D(new Polygon3D(
                    [
                        new Point3D(100, 0, 0),
                        new Point3D(104, 0, 0),
                        new Point3D(104, 0, 3),
                        new Point3D(100, 0, 3),
                    ]));
            }
        }

        /// <summary>A horizontal face at <paramref name="z"/>, wound so its normal points up or down.</summary>
        private static Face3D Horizontal(double z, bool up)
        {
            List<Point3D> point3Ds =
            [
                new Point3D(100, 0, z),
                new Point3D(104, 0, z),
                new Point3D(104, 4, z),
                new Point3D(100, 4, z),
            ];

            if (!up)
            {
                point3Ds.Reverse();
            }

            return new Face3D(new Polygon3D(point3Ds));
        }

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

        private static List<Space> Flat1(PartFModel partFModel)
        {
            return [partFModel.Get("Studio"), partFModel.Get("Bathroom")];
        }

        private static List<Space> Flat2(PartFModel partFModel)
        {
            return [partFModel.Get("Bedroom"), partFModel.Get("Kitchen")];
        }

        /// <summary>The panel of the given identity. Filter clones panels, so the guid is what carries across.</summary>
        private static Panel Panel_Guid(AdjacencyCluster adjacencyCluster, Guid guid)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                if (panel.Guid == guid)
                {
                    return panel;
                }
            }

            throw new ArgumentException(string.Format("No panel has the guid '{0}'.", guid));
        }

        /// <summary>The one panel adjacent to both named spaces.</summary>
        private static Panel Panel_Between(AdjacencyCluster adjacencyCluster, string name_1, string name_2)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(panel);
                if (spaces is null || spaces.Count != 2)
                {
                    continue;
                }

                List<string> names = [spaces[0].Name, spaces[1].Name];
                if (names.Contains(name_1) && names.Contains(name_2))
                {
                    return panel;
                }
            }

            throw new ArgumentException(string.Format("No panel joins '{0}' and '{1}'.", name_1, name_2));
        }

        /// <summary>The one panel whose construction carries the given name.</summary>
        private static Panel Panel_Named(AdjacencyCluster adjacencyCluster, string name_Construction)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                if (panel.Construction?.Name == name_Construction)
                {
                    return panel;
                }
            }

            throw new ArgumentException(string.Format("No panel has the construction '{0}'.", name_Construction));
        }

        private static Panel Panel_External(AdjacencyCluster adjacencyCluster, string name_Space)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                if (panel.PanelType != PanelType.WallExternal)
                {
                    continue;
                }

                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(panel);
                if (spaces != null && spaces.Count == 1 && spaces[0].Name == name_Space)
                {
                    return panel;
                }
            }

            throw new ArgumentException(string.Format("No external panel belongs to '{0}'.", name_Space));
        }
    }
}
