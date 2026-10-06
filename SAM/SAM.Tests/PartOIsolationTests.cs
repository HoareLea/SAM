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
    /// <c>Modify.IsolateSpaces</c> - building the derived model that simulates only the selected dwellings.
    /// <para>
    /// The fixture throughout is the acceptance building: <b>Flat 1</b> (Studio, Bathroom), <b>Flat 2</b>
    /// (Bedroom, Kitchen) and a <b>Corridor</b> between them, each flat on its own MVHR. Isolating Flat 1
    /// must simulate two spaces, cut to the corridor adiabatically, keep Flat 1's real external envelope,
    /// keep Flat 2's façades only as shade, and leave the source model exactly as it was.
    /// </para>
    /// </summary>
    public class PartOIsolationTests
    {
        // ---- A. Extraction -----------------------------------------------------------------------------

        /// <summary>One dwelling out of several leaves only that dwelling's spaces as thermal spaces.</summary>
        [Fact]
        public void OneSelectedDwelling_LeavesOnlyItsSpaces()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.True(spaceIsolation.IsIsolated);
            Assert.Equal(
                ["Bathroom", "Studio"],
                Names(spaceIsolation.AdjacencyCluster.GetSpaces()));
        }

        /// <summary>Two selected dwellings leave both dwellings' spaces, and nothing else.</summary>
        [Fact]
        public void TwoSelectedDwellings_LeaveBothDwellingsSpaces()
        {
            PartFModel partFModel = Fixture();

            List<Space> spaces = [.. Flat1(partFModel), .. Flat2(partFModel)];

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(spaces);

            Assert.True(spaceIsolation.IsIsolated);
            Assert.Equal(
                ["Bathroom", "Bedroom", "Kitchen", "Studio"],
                Names(spaceIsolation.AdjacencyCluster.GetSpaces()));
        }

        /// <summary>
        /// Two dwellings may legitimately share a display name. Selecting one must not drag the other in:
        /// the selection is the guid, and the name is not consulted at all.
        /// </summary>
        [Fact]
        public void DuplicateDwellingNames_IdentityWins()
        {
            PartFModel partFModel = Fixture();

            //A second space also called "Studio", belonging to the EXCLUDED flat. Selecting Flat 1 must
            //bring Flat 1's Studio and not this one - the names are identical, so only identity can tell
            //them apart.
            Space space_Duplicate = new("Studio", new Point3D(500, 0, 1.5));
            partFModel.AdjacencyCluster.AddObject(space_Duplicate);

            Space space_Studio = partFModel.Get("Studio");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.True(spaceIsolation.IsIsolated);
            Assert.NotNull(spaceIsolation.AdjacencyCluster.GetObject<Space>(space_Studio.Guid));
            Assert.Null(spaceIsolation.AdjacencyCluster.GetObject<Space>(space_Duplicate.Guid));

            //And exactly one space named "Studio" came across, not both.
            Assert.Single(spaceIsolation.AdjacencyCluster.GetSpaces(), x => x.Name == "Studio");
        }

        /// <summary>
        /// The source model is not touched. Isolation derives a new model; the building the user still has
        /// open keeps every space, every panel and every unit it had.
        /// </summary>
        [Fact]
        public void SourceModel_IsUnchanged()
        {
            PartFModel partFModel = Fixture();

            string before = partFModel.AdjacencyCluster.ToJsonObject().ToJsonString();

            partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.Equal(before, partFModel.AdjacencyCluster.ToJsonObject().ToJsonString());
        }

        /// <summary>Space guids survive - TAS zone mapping, TM59 result mapping and provenance all read them.</summary>
        [Fact]
        public void SelectedSpaceGuids_Survive()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.NotNull(spaceIsolation.AdjacencyCluster.GetObject<Space>(partFModel.Get("Studio").Guid));
            Assert.NotNull(spaceIsolation.AdjacencyCluster.GetObject<Space>(partFModel.Get("Bathroom").Guid));
        }

        // ---- B. Panels ---------------------------------------------------------------------------------

        /// <summary>Studio to Bathroom - both selected - stays the ordinary internal partition it is.</summary>
        [Fact]
        public void SelectedToSelectedPanel_StaysInternal()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = Panel_Between(spaceIsolation.AdjacencyCluster, "Studio", "Bathroom");

            Assert.Equal(PanelType.WallInternal, panel.PanelType);
            Assert.False(Analytical.Query.Adiabatic(panel));
            Assert.Equal(2, spaceIsolation.AdjacencyCluster.GetRelatedObjects<Space>(panel).Count);
        }

        /// <summary>Studio to Corridor - the corridor is excluded - becomes the adiabatic isolation cut.</summary>
        [Fact]
        public void SelectedToExcludedPanel_BecomesAdiabatic()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = Assert.Single(spaceIsolation.AdjacencyCluster.GetPanels(), x => Analytical.Query.Adiabatic(x));

            //The cut is a boundary of the selected space, not a deletion and not a new external wall.
            Assert.Equal(PanelType.WallInternal, panel.PanelType);
            Assert.Single(spaceIsolation.AdjacencyCluster.GetRelatedObjects<Space>(panel));
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);
        }

        /// <summary>A genuinely external wall of a selected space stays exactly what it was.</summary>
        [Fact]
        public void SelectedToExternalPanel_StaysExternal()
        {
            PartFModel partFModel = Fixture();

            Panel panel_Source = Panel_External(partFModel.AdjacencyCluster, "Studio");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = spaceIsolation.AdjacencyCluster.GetObject<Panel>(panel_Source.Guid);

            Assert.Equal(PanelType.WallExternal, panel.PanelType);
            Assert.False(Analytical.Query.Adiabatic(panel));
            Assert.Equal(panel_Source.Construction?.Name, panel.Construction?.Name);
        }

        /// <summary>
        /// An internal partition buried inside the excluded building never becomes shade. Turning every
        /// removed panel into a shade would build a large, physically meaningless solar model.
        /// </summary>
        [Fact]
        public void ExcludedInternalPartition_DoesNotBecomeShade()
        {
            PartFModel partFModel = Fixture();

            Panel panel_Source = Panel_Between(partFModel.AdjacencyCluster, "Bedroom", "Kitchen");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.Null(spaceIsolation.AdjacencyCluster.GetObject<Panel>(panel_Source.Guid));
        }

        /// <summary>An excluded façade does survive - as shading context, and only that.</summary>
        [Fact]
        public void ExcludedExternalWall_BecomesShadingContext()
        {
            PartFModel partFModel = Fixture();

            Panel panel_Source = Panel_External(partFModel.AdjacencyCluster, "Bedroom");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = spaceIsolation.AdjacencyCluster.GetObject<Panel>(panel_Source.Guid);

            Assert.Equal(PanelType.Shade, panel.PanelType);

            //Shade belongs to no space - it obstructs the sun, it does not bound a zone.
            List<Space> spaces = spaceIsolation.AdjacencyCluster.GetRelatedObjects<Space>(panel);
            Assert.True(spaces is null || spaces.Count == 0);

            //Both excluded flat façades, and nothing from inside the excluded building.
            Assert.Equal(2, spaceIsolation.Count_ShadePanel);
        }

        /// <summary>
        /// A shading model is built once from the geometry, not once per selected space: no panel appears
        /// twice, however many dwellings are selected.
        /// </summary>
        [Fact]
        public void ShadeAndPanelGeometry_IsNotDuplicated()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            List<Panel> panels = spaceIsolation.AdjacencyCluster.GetPanels();

            HashSet<Guid> guids = [];
            foreach (Panel panel in panels)
            {
                Assert.True(guids.Add(panel.Guid), string.Format("Panel '{0}' appears more than once.", panel.Name));
            }
        }

        // ---- C. Apertures ------------------------------------------------------------------------------

        /// <summary>A window on the selected flat's own external wall is untouched.</summary>
        [Fact]
        public void ExternalAperture_OnSelectedEnvelope_Remains()
        {
            PartFModel partFModel = Fixture();

            Panel panel_Source = Panel_External(partFModel.AdjacencyCluster, "Studio");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = spaceIsolation.AdjacencyCluster.GetObject<Panel>(panel_Source.Guid);

            Assert.Single(panel.Apertures);
            Assert.Equal(panel_Source.Apertures[0].Guid, panel.Apertures[0].Guid);
        }

        /// <summary>
        /// The door from the studio into the corridor is removed from the DERIVED model. An aperture left
        /// on the cut has nothing to open onto: the panel has one adjacent space there, so the conversion
        /// would export it as an external window and give the flat a door to outside.
        /// </summary>
        [Fact]
        public void CutAperture_IsRemovedFromTheDerivedModel()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = Assert.Single(spaceIsolation.AdjacencyCluster.GetPanels(), x => Analytical.Query.Adiabatic(x));

            Assert.True(panel.Apertures is null || panel.Apertures.Count == 0);
            Assert.Equal(1, spaceIsolation.Count_RemovedCutAperture);
        }

        /// <summary>And the source model still has its door.</summary>
        [Fact]
        public void CutAperture_SurvivesInTheSourceModel()
        {
            PartFModel partFModel = Fixture();

            partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = Panel_Between(partFModel.AdjacencyCluster, "Studio", "Corridor");

            Assert.Single(panel.Apertures);
        }

        /// <summary>
        /// <b>A surface that was already adiabatic in the source building is not this run's cut.</b>
        ///
        /// <para>
        /// <c>PanelParameter.Adiabatic</c> is not evidence of anything this method did.
        /// <c>SAM.Analytical.Tas.Convert.ToSAM</c> sets it on every surface a TBD calls adiabatic,
        /// <c>SAM.Analytical.gbXML.Convert.ToSAM</c> sets it wherever a gbXML surface names an adjacent
        /// space the file does not contain, and the <c>SAMAnalyticalSetAdiabatic</c> component sets it
        /// because a person asked for it - so a real project model, which is how these arrive, carries the
        /// flag on ordinary walls. <c>Filter</c> copies a panel's parameters into the derived cluster, so
        /// reading the flag back would take every one of them for a cut.
        /// </para>
        /// <para>
        /// Here Studio-to-Bathroom is flagged in the source and both rooms are selected, so it is an
        /// ordinary partition of the isolated flat. It must keep its door - the whole-building run would -
        /// and it must not be counted in the disclosure note as an interface to an excluded space.
        /// </para>
        /// </summary>
        [Fact]
        public void SourceAdiabaticPanel_BetweenTwoSelectedSpaces_KeepsItsApertures()
        {
            PartFModel partFModel = Fixture();

            SetAdiabatic(partFModel, "Studio", "Bathroom");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = Panel_Between(spaceIsolation.AdjacencyCluster, "Studio", "Bathroom");

            Assert.Single(panel.Apertures);

            //Studio-to-Corridor is the only cut, and the only aperture removed.
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);
            Assert.Equal(1, spaceIsolation.Count_RemovedCutAperture);
        }

        /// <summary>
        /// The mirror of it: a source surface that was already adiabatic <b>and</b> divides a selected space
        /// from an excluded one is still a cut. Its door is removed like any other cut's, because in the
        /// derived model it has nothing left to open onto - being flagged in the source neither makes a
        /// panel a cut nor excuses it from being one.
        /// </summary>
        [Fact]
        public void SourceAdiabaticPanel_OnTheCut_IsStillCut()
        {
            PartFModel partFModel = Fixture();

            SetAdiabatic(partFModel, "Studio", "Corridor");

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = spaceIsolation.AdjacencyCluster.GetObject<Panel>(
                Panel_Between(partFModel.AdjacencyCluster, "Studio", "Corridor").Guid);

            Assert.True(panel.Apertures is null || panel.Apertures.Count == 0);
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);
            Assert.Equal(1, spaceIsolation.Count_RemovedCutAperture);
        }

        /// <summary>
        /// And an already adiabatic <b>external</b> wall of a selected space is not a cut either - it has
        /// one adjacent space in the source, so no adjacency of it changed. It keeps its window.
        /// </summary>
        [Fact]
        public void SourceAdiabaticExternalWall_IsNotACut()
        {
            PartFModel partFModel = Fixture();

            Panel panel_Source = Panel_External(partFModel.AdjacencyCluster, "Studio");
            panel_Source.SetValue(PanelParameter.Adiabatic, true);
            partFModel.AdjacencyCluster.AddObject(panel_Source);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Panel panel = spaceIsolation.AdjacencyCluster.GetObject<Panel>(panel_Source.Guid);

            Assert.Single(panel.Apertures);
            Assert.Equal(1, spaceIsolation.Count_AdiabaticPanel);
        }

        // ---- D. Systems --------------------------------------------------------------------------------

        /// <summary>The selected dwelling's own MVHR - system and unit - comes across.</summary>
        [Fact]
        public void DedicatedMVHR_Survives()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            VentilationSystem ventilationSystem = Assert.Single(spaceIsolation.AdjacencyCluster.GetObjects<VentilationSystem>());
            Assert.Equal("AHU 1", ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName));

            AirHandlingUnit airHandlingUnit = Assert.Single(spaceIsolation.AdjacencyCluster.GetObjects<AirHandlingUnit>());
            Assert.Equal("AHU 1", airHandlingUnit.Name);
        }

        /// <summary>The excluded dwelling's MVHR does not.</summary>
        [Fact]
        public void ExcludedDwellingMVHR_IsRemoved()
        {
            PartFModel partFModel = Fixture();

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            foreach (AirHandlingUnit airHandlingUnit in spaceIsolation.AdjacencyCluster.GetObjects<AirHandlingUnit>())
            {
                Assert.NotEqual("AHU 2", airHandlingUnit.Name);
            }
        }

        /// <summary>
        /// A system serving both a selected and an excluded space is refused, not silently narrowed.
        /// Continuing would keep the whole unit's duty while dropping the branches that justified it.
        /// </summary>
        [Fact]
        public void SharedVentilationSystem_Refuses()
        {
            PartFModel partFModel = Fixture();

            //The corridor is put on Flat 1's system, so that system now straddles the isolation boundary.
            partFModel.AdjacencyCluster.AddRelation(
                VentilationSystem_Named(partFModel.AdjacencyCluster, "AHU 1"),
                partFModel.Get("Corridor"));

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Null(spaceIsolation.AdjacencyCluster);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("Corridor"));
        }

        /// <summary>
        /// One unit reached through two systems - one in scope, one not - is refused too. The per-system
        /// check cannot see that; the unit is asked in its own right.
        /// </summary>
        [Fact]
        public void SharedAirHandlingUnit_AcrossTwoSystems_Refuses()
        {
            PartFModel partFModel = Fixture();

            //A second system on Flat 1's unit, serving the excluded corridor.
            VentilationSystem ventilationSystem = new("Corridor MV", new VentilationSystemType("MVHR", "Fixture"));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, "AHU 1");
            partFModel.AdjacencyCluster.AddObject(ventilationSystem);
            partFModel.AdjacencyCluster.AddRelation(ventilationSystem, partFModel.Get("Corridor"));

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("AHU 1"));
        }

        /// <summary>An airflow path crossing the cut is refused rather than quietly left pointing at nothing.</summary>
        [Fact]
        public void CrossCutAirMovement_Refuses()
        {
            PartFModel partFModel = Fixture();

            SpaceAirMovement spaceAirMovement = new("Corridor to Studio", 10, "Corridor", "Studio");
            partFModel.AdjacencyCluster.AddObject(spaceAirMovement);
            partFModel.AdjacencyCluster.AddRelation(spaceAirMovement, partFModel.Get("Corridor"));
            partFModel.AdjacencyCluster.AddRelation(spaceAirMovement, partFModel.Get("Studio"));

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.False(spaceIsolation.IsIsolated);
            Assert.Contains(spaceIsolation.Refusals, x => x.Contains("crosses the isolation boundary"));
        }

        /// <summary>A transfer path wholly inside the selection is normal and survives.</summary>
        [Fact]
        public void SelectedToSelectedAirMovement_Survives()
        {
            PartFModel partFModel = Fixture();

            SpaceAirMovement spaceAirMovement = new("Studio to Bathroom", 10, "Studio", "Bathroom");
            partFModel.AdjacencyCluster.AddObject(spaceAirMovement);
            partFModel.AdjacencyCluster.AddRelation(spaceAirMovement, partFModel.Get("Studio"));
            partFModel.AdjacencyCluster.AddRelation(spaceAirMovement, partFModel.Get("Bathroom"));

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Assert.True(spaceIsolation.IsIsolated);
            Assert.Single(spaceIsolation.AdjacencyCluster.GetObjects<SpaceAirMovement>());
        }

        // ---- E. Part F ---------------------------------------------------------------------------------

        /// <summary>
        /// Isolation is a thermal-model scope option. It is not a ventilation sizing method, and it does not
        /// touch what Approved Document F requires of the selected spaces.
        /// </summary>
        [Fact]
        public void PartFSpaceData_IsUnchangedByIsolation()
        {
            PartFModel partFModel = Fixture();

            Space space_Source = partFModel.Get("Studio");
            space_Source.SetValue(SpaceParameter.PartFSpaceData, new PartFSpaceData { ContinuousDesignFlowRate_Lps = 31.5 });
            partFModel.AdjacencyCluster.AddObject(space_Source);

            SpaceIsolation spaceIsolation = partFModel.AdjacencyCluster.IsolateSpaces(Flat1(partFModel));

            Space space = spaceIsolation.AdjacencyCluster.GetObject<Space>(space_Source.Guid);

            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.NotNull(partFSpaceData);
            Assert.Equal(31.5, partFSpaceData.ContinuousDesignFlowRate_Lps);
        }

        // ---- G. Persistence ----------------------------------------------------------------------------

        /// <summary>The isolation context round-trips through JSON, so it survives into the run's .sam.</summary>
        [Fact]
        public void IsolationContext_RoundTripsThroughJson()
        {
            Guid guid_Space = Guid.NewGuid();
            Guid guid_Zone = Guid.NewGuid();

            PartOIsolationContext partOIsolationContext = new([guid_Space], [guid_Zone], ["Flat 1"]);

            PartOIsolationContext read = new(partOIsolationContext.ToJsonObject());

            Assert.Equal([guid_Space], read.Guids_Space);
            Assert.Equal([guid_Zone], read.Guids_Zone);
            Assert.Equal(["Flat 1"], read.Names_Dwelling);
            Assert.Equal(partOIsolationContext.ScopeToken, read.ScopeToken);
            Assert.True(read.IsValid);
        }

        /// <summary>The isolation context survives being stamped on a model and read back off it.</summary>
        [Fact]
        public void IsolationContext_SurvivesTheModelsJsonRoundTrip()
        {
            PartFModel partFModel = Fixture();

            AnalyticalModel analyticalModel = new("Fixture", null, null, null, partFModel.AdjacencyCluster);

            PartOIsolationContext partOIsolationContext = new([partFModel.Get("Studio").Guid], [], ["Flat 1"]);
            analyticalModel.SetValue(AnalyticalModelParameter.PartOIsolationContext, partOIsolationContext);

            AnalyticalModel analyticalModel_Read = new(analyticalModel.ToJsonObject());

            PartOIsolationContext read = analyticalModel_Read.GetValue<PartOIsolationContext>(AnalyticalModelParameter.PartOIsolationContext);

            Assert.NotNull(read);
            Assert.True(read.IsValid);
            Assert.Equal(partOIsolationContext.ScopeToken, read.ScopeToken);
            Assert.Equal(["Flat 1"], read.Names_Dwelling);
        }

        // ---- H. Artifact naming ------------------------------------------------------------------------

        /// <summary>
        /// The scope token is a function of the selection, not of the order it was enumerated in - so one
        /// selection always names the same run, and a different selection names a different one.
        /// </summary>
        [Fact]
        public void ScopeToken_IsStableAndDistinguishesScopes()
        {
            Guid guid_1 = Guid.NewGuid();
            Guid guid_2 = Guid.NewGuid();

            Assert.Equal(PartOIsolationContext.Token([guid_1, guid_2]), PartOIsolationContext.Token([guid_2, guid_1]));
            Assert.NotEqual(PartOIsolationContext.Token([guid_1]), PartOIsolationContext.Token([guid_1, guid_2]));
            Assert.Equal(8, PartOIsolationContext.Token([guid_1]).Length);
        }

        // ---- I. Large model ----------------------------------------------------------------------------

        /// <summary>
        /// A synthetic building of 2,000 spaces, isolating two of them. Asserted on shape rather than on a
        /// clock: only the selected spaces are thermal spaces, the shading context is bounded by the
        /// building's external geometry rather than multiplied per selected space, and nothing is
        /// duplicated. A wall-clock threshold would be brittle; these are the properties that make it fast.
        /// </summary>
        [Fact]
        public void LargeModel_ReturnsOnlySelectedSpacesAndBoundedContext()
        {
            const int count_Space = 2000;

            AdjacencyCluster adjacencyCluster = new();

            List<Space> spaces = [];

            for (int i = 0; i < count_Space; i++)
            {
                Space space = new(string.Format("Space {0}", i), new Point3D(i * 10, 0, 1.5));
                adjacencyCluster.AddObject(space);
                spaces.Add(space);

                //One external wall each - the building's envelope, and the only thing that can shade.
                Panel panel_External = AnalyticalCreate.Panel(new Construction(Guid.NewGuid(), "External Wall"), PanelType.WallExternal, Wall(i * 10));
                adjacencyCluster.AddObject(panel_External);
                adjacencyCluster.AddRelation(space, panel_External);

                //And an internal partition to its neighbour - the geometry that must NOT become shade.
                if (i != 0)
                {
                    Panel panel_Internal = AnalyticalCreate.Panel(new Construction(Guid.NewGuid(), "Internal Partition"), PanelType.WallInternal, Wall((i * 10) + 5));
                    adjacencyCluster.AddObject(panel_Internal);
                    adjacencyCluster.AddRelation(spaces[i - 1], panel_Internal);
                    adjacencyCluster.AddRelation(space, panel_Internal);
                }
            }

            RealConstructions(adjacencyCluster);

            SpaceIsolation spaceIsolation = adjacencyCluster.IsolateSpaces([spaces[10], spaces[11]]);

            Assert.True(spaceIsolation.IsIsolated);

            //Only the selection is simulated - the whole point of the feature.
            Assert.Equal(2, spaceIsolation.AdjacencyCluster.GetSpaces().Count);

            //The shading context is the rest of the envelope, once each: one external wall per excluded
            //space. Not one per excluded panel, and emphatically not one per selected space.
            Assert.Equal(count_Space - 2, spaceIsolation.Count_ShadePanel);

            //No internal partition became shade.
            foreach (Panel panel in spaceIsolation.AdjacencyCluster.GetPanels())
            {
                Assert.NotEqual("Internal Partition", panel.PanelType == PanelType.Shade ? panel.Construction?.Name : null);
            }

            //And nothing is duplicated.
            HashSet<Guid> guids = [];
            foreach (Panel panel in spaceIsolation.AdjacencyCluster.GetPanels())
            {
                Assert.True(guids.Add(panel.Guid));
            }
        }

        // ---- The fixture -------------------------------------------------------------------------------

        /// <summary>
        /// Flat 1 (Studio, Bathroom), Flat 2 (Bedroom, Kitchen), and a Corridor joining them - each flat on
        /// its own dedicated MVHR, exactly as the Part O workflow builds them.
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
        /// Gives every panel a construction with a real thickness.
        /// <para>
        /// <c>Query.Adiabatic</c> reports a construction of zero thickness as adiabatic in its own right, so
        /// a fixture built from bare named constructions would report every surface adiabatic and the test
        /// for the isolation cut would pass without the isolation having done anything. These are the
        /// constructions a real model has.
        /// </para>
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

        /// <summary>One dedicated MVHR: a system related to the dwelling's spaces, and the unit it names.</summary>
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

        private static VentilationSystem VentilationSystem_Named(AdjacencyCluster adjacencyCluster, string name_Unit)
        {
            return adjacencyCluster.GetObjects<VentilationSystem>()
                .Find(x => x.GetValue<string>(VentilationSystemParameter.SupplyUnitName) == name_Unit);
        }

        /// <summary>The one panel adjacent to both named spaces.</summary>
        /// <summary>
        /// Flags the partition between two named spaces adiabatic in the SOURCE model, the way a model
        /// imported from a TBD or a gbXML - or one a person set adiabatic - arrives carrying it.
        /// </summary>
        private static void SetAdiabatic(PartFModel partFModel, string name_1, string name_2)
        {
            Panel panel = Panel_Between(partFModel.AdjacencyCluster, name_1, name_2);

            panel.SetValue(PanelParameter.Adiabatic, true);

            partFModel.AdjacencyCluster.AddObject(panel);
        }

        private static Panel Panel_Between(AdjacencyCluster adjacencyCluster, string name_1, string name_2)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels())
            {
                List<string> names = Names(adjacencyCluster.GetRelatedObjects<Space>(panel));
                if (names.Count == 2 && names.Contains(name_1) && names.Contains(name_2))
                {
                    return panel;
                }
            }

            throw new ArgumentException(string.Format("No panel joins '{0}' and '{1}'.", name_1, name_2));
        }

        /// <summary>The one external panel of the named space.</summary>
        private static Panel Panel_External(AdjacencyCluster adjacencyCluster, string name)
        {
            foreach (Panel panel in adjacencyCluster.GetPanels())
            {
                if (panel.PanelType != PanelType.WallExternal)
                {
                    continue;
                }

                List<string> names = Names(adjacencyCluster.GetRelatedObjects<Space>(panel));
                if (names.Count == 1 && names[0] == name)
                {
                    return panel;
                }
            }

            throw new ArgumentException(string.Format("No external panel belongs to '{0}'.", name));
        }

        private static List<string> Names(IEnumerable<Space> spaces)
        {
            List<string> result = [];
            foreach (Space space in spaces ?? [])
            {
                result.Add(space.Name);
            }

            result.Sort(StringComparer.Ordinal);

            return result;
        }

        private static Face3D Wall(double x)
        {
            return new Face3D(new Polygon3D(
            [
                new Point3D(x, 0, 0),
                new Point3D(x + 4, 0, 0),
                new Point3D(x + 4, 0, 3),
                new Point3D(x, 0, 3),
            ]));
        }
    }
}
