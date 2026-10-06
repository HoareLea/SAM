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
    /// <c>Query.UpdateConstructionLayersByPanelType</c> - filling in the fabric of panels and apertures that
    /// have none.
    /// <para>
    /// It runs on every guided Part O run, on the prepared model, immediately before conversion. With
    /// <c>emptyOnly</c> at its default it is a <b>gap filler and not a rewrite</b>, and these pin both
    /// halves of that: what it must leave alone, and what it must say when it cannot fill a gap.
    /// </para>
    /// <para>
    /// It used to <b>throw</b> instead. <c>GetConstructions</c> and <c>GetApertureConstructions</c> answer
    /// null rather than an empty list, and <c>FirstOrDefault</c> over null is an
    /// <see cref="ArgumentNullException"/> - reached by an ordinary internal door whose aperture
    /// construction has no pane layers, which is what the Part F preparation builds.
    /// </para>
    /// </summary>
    public class ConstructionLayerFillTests
    {
        // ---- A. Established fabric is not touched ------------------------------------------------------

        /// <summary>A construction that already has layers is left exactly as it was, library or no library.</summary>
        [Fact]
        public void ExistingLayeredConstruction_IsUnchanged()
        {
            AnalyticalModel analyticalModel = Model(layered: true, apertureLayers: true);

            List<string> before = Fabric(analyticalModel);

            AnalyticalModel result = analyticalModel.UpdateConstructionLayersByPanelType(
                out List<string> unresolved, Library(), ApertureLibrary(), Materials());

            Assert.Equal(before, Fabric(result));
            Assert.Empty(unresolved);
        }

        /// <summary>
        /// And it is still left alone when the library has a candidate that differs - the fill is for gaps,
        /// never for replacing what somebody authored.
        /// </summary>
        [Fact]
        public void ExistingLayeredConstruction_IsNotReplacedByALibraryCandidate()
        {
            AnalyticalModel analyticalModel = Model(layered: true, apertureLayers: true);

            List<string> before = Fabric(analyticalModel);

            AnalyticalModel result = analyticalModel.UpdateConstructionLayersByPanelType(
                out _, LibraryFor(PanelType.WallInternal, PanelType.WallExternal), ApertureLibrary(), Materials());

            Assert.Equal(before, Fabric(result));
        }

        // ---- B. A missing match does not throw ---------------------------------------------------------

        /// <summary>A construction library with nothing for this panel type is not an error.</summary>
        [Fact]
        public void NoConstructionLibraryMatch_DoesNotThrow()
        {
            AnalyticalModel analyticalModel = Model(layered: false, apertureLayers: true);

            AnalyticalModel result = analyticalModel.UpdateConstructionLayersByPanelType(
                out List<string> unresolved, Library(), ApertureLibrary(), Materials());

            Assert.NotNull(result);
            Assert.NotEmpty(unresolved);
        }

        /// <summary>
        /// Nor are the machine's own configured libraries, whatever they happen to contain.
        /// <para>
        /// Passing null asks for the libraries in <c>ActiveSetting</c>, so what this resolves depends on the
        /// machine: a configured install fills the fixture, a bare one - a CI runner, a fresh checkout -
        /// fills nothing. Both must return a model and neither may throw, which is the whole of what is
        /// asserted here. What happens to an unresolved surface is pinned by the tests below, against
        /// libraries this fixture owns.
        /// </para>
        /// </summary>
        [Fact]
        public void ConfiguredLibraries_WhateverTheyContain_DoNotThrow()
        {
            AnalyticalModel analyticalModel = Model(layered: false, apertureLayers: false);

            AnalyticalModel result = analyticalModel.UpdateConstructionLayersByPanelType(
                out List<string> unresolved, null, null, null);

            Assert.NotNull(result);
            Assert.NotNull(unresolved);
        }

        /// <summary>
        /// A library that exists but offers nothing for this type is the crash path, and it is not the same
        /// as a null library: <c>GetConstructions</c> answers <b>null</b> rather than an empty list, so
        /// <c>FirstOrDefault</c> over it threw <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public void EmptyLibraries_DoNotThrow_AndReportEverythingUnresolved()
        {
            AnalyticalModel analyticalModel = Model(layered: false, apertureLayers: false);

            AnalyticalModel result = analyticalModel.UpdateConstructionLayersByPanelType(
                out List<string> unresolved, Library(), ApertureLibrary(), Materials());

            Assert.NotNull(result);

            //Every panel, and the door.
            Assert.Equal(result.AdjacencyCluster.GetPanels().Count, unresolved.FindAll(x => x.StartsWith("Panel ", StringComparison.Ordinal)).Count);
            Assert.Contains(unresolved, x => x.StartsWith("Aperture ", StringComparison.Ordinal));
        }

        /// <summary>
        /// The case that actually crashed: an ordinary internal door, whose aperture construction carries no
        /// pane layers, on a model whose panels are fully layered.
        /// <para>
        /// This is what <c>PartFModel.Partition</c> builds and what the Part F preparation puts in a
        /// dwelling, so it reached the guided Part O path directly.
        /// </para>
        /// </summary>
        [Fact]
        public void OrdinaryDoor_WithNoPaneLayers_DoesNotThrow()
        {
            AnalyticalModel analyticalModel = Model(layered: true, apertureLayers: false);

            AnalyticalModel result = analyticalModel.UpdateConstructionLayersByPanelType(
                out List<string> unresolved, Library(), ApertureLibrary(), Materials());

            Assert.NotNull(result);

            //The panels resolved; the door did not, and says so by name.
            Assert.Contains(unresolved, x => x.Contains("Door Studio Bathroom", StringComparison.Ordinal));
        }

        /// <summary>The same door, with an aperture library that does have a candidate, is filled.</summary>
        [Fact]
        public void OrdinaryDoor_WithALibraryCandidate_IsFilled()
        {
            AnalyticalModel analyticalModel = Model(layered: true, apertureLayers: false);

            AnalyticalModel result = analyticalModel.UpdateConstructionLayersByPanelType(
                out List<string> unresolved, Library(), ApertureLibrary(withDoor: true), Materials());

            Assert.Empty(unresolved);

            foreach (Panel panel in result.AdjacencyCluster.GetPanels() ?? [])
            {
                foreach (Aperture aperture in panel.Apertures ?? [])
                {
                    Assert.True(aperture.ApertureConstruction.HasPaneConstructionLayers());
                    Assert.True(aperture.GetThickness(AperturePart.Pane) > 0);
                }
            }
        }

        // ---- C. Nothing unresolved is quietly made adiabatic --------------------------------------------

        /// <summary>
        /// A panel the fill could not resolve keeps the construction it had, and is <b>reported</b>.
        /// <para>
        /// It is not given a default, and nothing marks it adiabatic to let the run continue. That matters
        /// because a construction with no layers has zero thickness, and <c>Query.Adiabatic</c> reports a
        /// zero thickness construction as adiabatic in its own right - so an unresolved wall reaches TAS as
        /// an adiabatic boundary. The disclosure is what stops that being silent.
        /// </para>
        /// </summary>
        [Fact]
        public void UnresolvedPanel_KeepsItsConstruction_AndIsDisclosed()
        {
            AnalyticalModel analyticalModel = Model(layered: false, apertureLayers: true);

            string name_Before = null;
            foreach (Panel panel in analyticalModel.AdjacencyCluster.GetPanels() ?? [])
            {
                name_Before ??= panel.Construction?.Name;
            }

            AnalyticalModel result = analyticalModel.UpdateConstructionLayersByPanelType(
                out List<string> unresolved, Library(), ApertureLibrary(), Materials());

            List<Panel> panels = result.AdjacencyCluster.GetPanels();

            //Every panel that had no fabric still has none - none was invented - and every one is disclosed.
            int count_Empty = 0;
            foreach (Panel panel in panels)
            {
                if (panel.Construction is null || !panel.Construction.HasConstructionLayers())
                {
                    count_Empty++;

                    //It kept its own construction rather than being handed a default.
                    Assert.NotNull(panel.Construction);
                }
            }

            Assert.Equal(count_Empty, unresolved.FindAll(x => x.StartsWith("Panel ", StringComparison.Ordinal)).Count);
            Assert.Equal(name_Before, panels[0].Construction?.Name);
        }

        /// <summary>
        /// The disclosure is the whole point: an unresolved panel IS adiabatic as far as SAM is concerned,
        /// so a run that did not report it would be simulating a different thermal case.
        /// </summary>
        [Fact]
        public void UnresolvedPanel_ReadsAsAdiabatic_WhichIsWhyItMustBeDisclosed()
        {
            AnalyticalModel analyticalModel = Model(layered: false, apertureLayers: true);

            AnalyticalModel result = analyticalModel.UpdateConstructionLayersByPanelType(
                out List<string> unresolved, Library(), ApertureLibrary(), Materials());

            Assert.NotEmpty(unresolved);

            bool any_Adiabatic = false;
            foreach (Panel panel in result.AdjacencyCluster.GetPanels() ?? [])
            {
                if (panel.PanelType != PanelType.Air && panel.PanelType != PanelType.Shade && Analytical.Query.Adiabatic(panel))
                {
                    any_Adiabatic = true;
                }
            }

            Assert.True(any_Adiabatic);
        }

        // ---- Helpers -----------------------------------------------------------------------------------

        private static List<string> Fabric(AnalyticalModel analyticalModel)
        {
            List<string> result = [];
            foreach (Panel panel in analyticalModel.AdjacencyCluster.GetPanels() ?? [])
            {
                string apertures = string.Empty;
                foreach (Aperture aperture in panel.Apertures ?? [])
                {
                    apertures += string.Format(
                        ";{0}/{1}/{2:F4}",
                        aperture.Guid,
                        aperture.ApertureConstruction?.Name,
                        aperture.GetThickness(AperturePart.Pane));
                }

                result.Add(string.Format(
                    "{0}|{1}|{2}|{3}|{4:F4}{5}",
                    panel.Guid,
                    panel.PanelType,
                    panel.Construction?.Name,
                    panel.Construction?.ConstructionLayers?.Count ?? -1,
                    panel.Construction?.GetThickness() ?? double.NaN,
                    apertures));
            }

            result.Sort(StringComparer.Ordinal);

            return result;
        }

        /// <summary>An empty library - configured, but with nothing for any panel type in the fixture.</summary>
        private static ConstructionLibrary Library()
        {
            return new ConstructionLibrary("Fixture");
        }

        /// <summary>A library carrying one candidate for each named panel type.</summary>
        private static ConstructionLibrary LibraryFor(params PanelType[] panelTypes)
        {
            ConstructionLibrary result = new("Fixture");

            foreach (PanelType panelType in panelTypes)
            {
                Construction construction = new(
                    Guid.NewGuid(),
                    "Library " + panelType.ToString(),
                    [new ConstructionLayer("Library Block", 0.15)]);

                //How ConstructionLibrary.GetConstructions(PanelType) matches: the construction states the
                //panel type it is the default for.
                construction.SetValue(ConstructionParameter.DefaultPanelType, panelType.ToString());

                result.Add(construction);
            }

            return result;
        }

        private static ApertureConstructionLibrary ApertureLibrary(bool withDoor = false)
        {
            ApertureConstructionLibrary result = new("Fixture");

            if (withDoor)
            {
                ApertureConstruction apertureConstruction = new(
                    Guid.NewGuid(),
                    "Library Door",
                    ApertureType.Door,
                    [new ConstructionLayer("Library Timber", 0.044)],
                    [new ConstructionLayer("Library Timber", 0.044)]);

                apertureConstruction.SetValue(ApertureConstructionParameter.DefaultPanelType, PanelType.WallInternal.ToString());

                result.Add(apertureConstruction);
            }

            return result;
        }

        private static SAM.Core.MaterialLibrary Materials()
        {
            SAM.Core.MaterialLibrary result = new("Fixture");

            result.Add(new SAM.Core.OpaqueMaterial(Guid.NewGuid(), "Library Block", "Library Block", "Fixture", 0.5, 1000, 1000));
            result.Add(new SAM.Core.OpaqueMaterial(Guid.NewGuid(), "Library Timber", "Library Timber", "Fixture", 0.14, 500, 1600));

            return result;
        }

        /// <summary>
        /// Two rooms, a door between them and an external wall - the smallest thing that carries both a
        /// panel construction and an aperture construction.
        /// </summary>
        private static AnalyticalModel Model(bool layered, bool apertureLayers)
        {
            PartFModel partFModel = new PartFModel()
                .Space("Studio", 25, 62.5)
                .Space("Bathroom", 5, 12.5)
                .Partition("Studio", "Bathroom", "Door Studio Bathroom")
                .ExternalWall("Studio", window: false);

            foreach (Panel panel in partFModel.AdjacencyCluster.GetPanels() ?? [])
            {
                Panel panel_New = panel;

                if (layered)
                {
                    Construction construction = new(
                        panel.Construction.Guid,
                        panel.Construction.Name,
                        [new ConstructionLayer("Concrete", 0.2)]);

                    panel_New = AnalyticalCreate.Panel(panel, construction);
                }

                if (apertureLayers && panel_New.HasApertures)
                {
                    panel_New = AnalyticalCreate.Panel(panel_New);

                    foreach (Aperture aperture in panel_New.Apertures)
                    {
                        ApertureConstruction apertureConstruction_Old = aperture.ApertureConstruction;

                        ApertureConstruction apertureConstruction = new(
                            apertureConstruction_Old.Guid,
                            apertureConstruction_Old.Name,
                            apertureConstruction_Old.ApertureType,
                            [new ConstructionLayer("Timber", 0.044)],
                            [new ConstructionLayer("Timber", 0.044)]);

                        panel_New.RemoveAperture(aperture.Guid);
                        panel_New.AddAperture(new Aperture(aperture, apertureConstruction));
                    }
                }

                partFModel.AdjacencyCluster.AddObject(panel_New);
            }

            SAM.Core.MaterialLibrary materialLibrary = new("Fixture");
            materialLibrary.Add(new SAM.Core.OpaqueMaterial(Guid.NewGuid(), "Concrete", "Concrete", "Fixture", 2.3, 2300, 1000));
            materialLibrary.Add(new SAM.Core.OpaqueMaterial(Guid.NewGuid(), "Timber", "Timber", "Fixture", 0.14, 500, 1600));

            return new AnalyticalModel("Fixture", null, null, null, partFModel.AdjacencyCluster, materialLibrary, null);
        }
    }
}
