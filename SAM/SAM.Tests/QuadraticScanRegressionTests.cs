// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Behaviour locks for the workflows touched by the quadratic-scan audit. These were
    /// written against the pre-optimisation implementations and must keep passing unchanged
    /// afterwards - the optimisations are broad-phase filtering and indexing only, they are
    /// not allowed to move a single result.
    /// </summary>
    //MergeOverlapPanels_KeepsApertureFromRedundantPanel reads ActiveSetting's default aperture construction
    //library, and PartFTransferAirDoorTests temporarily replaces it to prove a refusal. Sharing a collection
    //keeps the two classes from running at the same time; nothing else in the suite reads that library.
    [Collection("SAM.Analytical.ActiveSetting default aperture construction library")]
    public class QuadraticScanRegressionTests
    {
        // --- Modify.SplitFace3Ds(List<Shell>) -------------------------------------------------

        [Fact]
        public void SplitFace3Ds_DisjointShells_LeavesFaceCountsUnchanged()
        {
            List<Shell> shells = QuadraticScanFixtures.DisjointBoxShells(9);
            List<int> before = shells.ConvertAll(x => x.Face3Ds.Count);

            shells.SplitFace3Ds(Core.Tolerance.MacroDistance, Core.Tolerance.Angle, Core.Tolerance.Distance);

            Assert.Equal(9, shells.Count);
            Assert.Equal(before, shells.ConvertAll(x => x.Face3Ds.Count));
        }

        [Fact]
        public void SplitFace3Ds_TouchingShells_SplitsSharedWallOfLargerShell()
        {
            // A 4x4 box against a 4x2 box: the tall neighbour's shared wall covers only half of
            // the big box's wall, so the big box's wall gets split and the small one does not.
            Shell shell_Large = QuadraticScanFixtures.BoxShell(0, 0, 0, 4, 4, 3);
            Shell shell_Small = QuadraticScanFixtures.BoxShell(4, 0, 0, 4, 2, 3);

            List<Shell> shells = new List<Shell> { shell_Large, shell_Small };
            shells.SplitFace3Ds(Core.Tolerance.MacroDistance, Core.Tolerance.Angle, Core.Tolerance.Distance);

            Assert.Equal(2, shells.Count);
            Assert.True(shells[0].Face3Ds.Count >= 6);
            Assert.True(shells[1].Face3Ds.Count >= 6);

            // Total surface area is conserved by a split.
            Assert.Equal(TotalArea(shell_Large) + TotalArea(shell_Small), TotalArea(shells[0]) + TotalArea(shells[1]), 6);
        }

        [Fact]
        public void SplitFace3Ds_PreservesInputOrder()
        {
            List<Shell> shells = new List<Shell>
            {
                QuadraticScanFixtures.BoxShell(0, 0, 0, 4, 4, 3),
                QuadraticScanFixtures.BoxShell(100, 0, 0, 4, 4, 3),
                QuadraticScanFixtures.BoxShell(4, 0, 0, 4, 2, 3),
            };

            List<BoundingBox3D> before = shells.ConvertAll(x => x.GetBoundingBox());

            shells.SplitFace3Ds(Core.Tolerance.MacroDistance, Core.Tolerance.Angle, Core.Tolerance.Distance);

            for (int i = 0; i < before.Count; i++)
            {
                Assert.True(before[i].Min.AlmostEquals(shells[i].GetBoundingBox().Min, 1e-9));
                Assert.True(before[i].Max.AlmostEquals(shells[i].GetBoundingBox().Max, 1e-9));
            }
        }

        // --- Create.AdjacencyCluster(shells, spaces) ------------------------------------------

        [Fact]
        public void AdjacencyCluster_ByShells_DisjointBoxes_OneSpaceAndSixPanelsEach()
        {
            List<Shell> shells = QuadraticScanFixtures.DisjointBoxShells(4);

            AdjacencyCluster adjacencyCluster = Analytical.Create.AdjacencyCluster(shells, null);

            Assert.NotNull(adjacencyCluster);
            Assert.Equal(4, adjacencyCluster.GetSpaces().Count);
            Assert.Equal(24, adjacencyCluster.GetPanels().Count);
        }

        [Fact]
        public void AdjacencyCluster_ByShells_AdjacentBoxes_ShareTheCommonPanel()
        {
            // Two boxes sharing the x=4 wall: the shared face must resolve to a single panel
            // related to both spaces, which is exactly what the pairwise face scan is for.
            List<Shell> shells = new List<Shell>
            {
                QuadraticScanFixtures.BoxShell(0, 0, 0, 4, 4, 3),
                QuadraticScanFixtures.BoxShell(4, 0, 0, 4, 4, 3),
            };

            AdjacencyCluster adjacencyCluster = Analytical.Create.AdjacencyCluster(shells, null);

            Assert.NotNull(adjacencyCluster);

            List<Space> spaces = adjacencyCluster.GetSpaces();
            Assert.Equal(2, spaces.Count);

            List<Panel> panels = adjacencyCluster.GetPanels();
            Assert.Equal(11, panels.Count);

            List<Panel> shared = panels.FindAll(x => adjacencyCluster.GetRelatedObjects<Space>(x).Count == 2);
            Assert.Single(shared);
        }

        [Fact]
        public void AdjacencyCluster_ByShells_NamesGeneratedSpacesFromSuppliedNamesOnly()
        {
            // Documented current behaviour: the running index is probed against the *supplied*
            // space list only - generated spaces are never added back to it - so with no
            // supplied spaces every generated space is called "Space 1". The optimisation must
            // not change this.
            List<Shell> shells = QuadraticScanFixtures.DisjointBoxShells(3);

            AdjacencyCluster adjacencyCluster = Analytical.Create.AdjacencyCluster(shells, null);

            List<string> names = adjacencyCluster.GetSpaces().ConvertAll(x => x.Name);
            names.Sort(StringComparer.Ordinal);

            Assert.Equal(new List<string> { "Space 1", "Space 1", "Space 1" }, names);
        }

        [Fact]
        public void AdjacencyCluster_ByShells_ReusesSuppliedSpaceAndSkipsItsName()
        {
            // "Space 1" is taken by the supplied space, so the generated one must be "Space 2".
            List<Shell> shells = new List<Shell>
            {
                QuadraticScanFixtures.BoxShell(0, 0, 0, 4, 4, 3),
                QuadraticScanFixtures.BoxShell(100, 0, 0, 4, 4, 3),
            };

            List<Space> spaces = new List<Space> { new Space("Space 1", new Point3D(102, 2, 1.5)) };

            AdjacencyCluster adjacencyCluster = Analytical.Create.AdjacencyCluster(shells, spaces);

            List<string> names = adjacencyCluster.GetSpaces().ConvertAll(x => x.Name);
            names.Sort(StringComparer.Ordinal);

            Assert.Equal(new List<string> { "Space 1", "Space 2" }, names);
        }

        // --- Query.MergeOverlapPanels ---------------------------------------------------------

        [Fact]
        public void MergeOverlapPanels_DisjointFloors_KeepsEveryPanel()
        {
            List<Panel> panels = QuadraticScanFixtures.CoplanarFloorPanels(9);
            List<Panel> redundantPanels = new List<Panel>();

            List<Panel> result = Analytical.Query.MergeOverlapPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Equal(9, result.Count);
            Assert.Empty(redundantPanels);
            Assert.Equal(9 * 16, result.Sum(x => x.GetArea()), 6);
        }

        [Fact]
        public void MergeOverlapPanels_StackedFloors_CollapsesToOne()
        {
            // Three identical floor slabs within the offset band collapse to a single panel and
            // the other two are reported redundant.
            List<Panel> panels = new List<Panel>
            {
                FloorPanel(0, 0, 0.00, 4),
                FloorPanel(0, 0, 0.02, 4),
                FloorPanel(0, 0, 0.04, 4),
            };

            List<Panel> redundantPanels = new List<Panel>();
            List<Panel> result = Analytical.Query.MergeOverlapPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(16, result[0].GetArea(), 4);

            // Documented current behaviour: a superseded panel is only reported as redundant
            // when it carries apertures, so an aperture-free stack reports none.
            Assert.Empty(redundantPanels);
        }

        [Fact]
        public void MergeOverlapPanels_PartiallyOverlappingFloors_SplitsIntoDisjointPieces()
        {
            List<Panel> panels = new List<Panel>
            {
                FloorPanel(0, 0, 0, 4),
                FloorPanel(2, 0, 0.01, 4),
            };

            List<Panel> redundantPanels = new List<Panel>();
            List<Panel> result = Analytical.Query.MergeOverlapPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);

            // The union of the two 4x4 slabs offset by 2 in x is 6x4 = 24.
            Assert.Equal(24, result.Sum(x => x.GetArea()), 4);
        }

        [Fact]
        public void MergeOverlapPanels_DisjointWalls_KeepsEveryPanel()
        {
            List<Panel> panels = QuadraticScanFixtures.DisjointWallPanels(9);
            List<Panel> redundantPanels = new List<Panel>();

            List<Panel> result = Analytical.Query.MergeOverlapPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Equal(9, result.Count);
            Assert.Equal(9 * 9, result.Sum(x => x.GetArea()), 6);
        }

        [Fact]
        public void MergeOverlapPanels_StackedWalls_CollapsesToOne()
        {
            List<Panel> panels = new List<Panel>
            {
                WallPanel(0.00, 3),
                WallPanel(0.02, 3),
                WallPanel(0.04, 3),
            };

            List<Panel> redundantPanels = new List<Panel>();
            List<Panel> result = Analytical.Query.MergeOverlapPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(9, result[0].GetArea(), 4);
        }

        [Fact]
        public void MergeOverlapPanels_KeepsApertureFromRedundantPanel()
        {
            // Both slabs on the same plane, so the aperture of the superseded one stays valid
            // on the merged face and must survive the merge.
            Panel panel_Keep = FloorPanel(0, 0, 0, 4);
            Panel panel_Drop = FloorPanel(0, 0, 0, 4);

            ApertureConstruction apertureConstruction = Analytical.Query.DefaultApertureConstruction(PanelType.Floor, ApertureType.Window);
            Aperture aperture = new Aperture(apertureConstruction, new Polygon3D(new Point3D[]
            {
                new Point3D(1, 1, 0),
                new Point3D(2, 1, 0),
                new Point3D(2, 2, 0),
                new Point3D(1, 2, 0),
            }));

            Assert.True(panel_Drop.AddAperture(aperture));

            List<Panel> redundantPanels = new List<Panel>();
            List<Panel> result = Analytical.Query.MergeOverlapPanels(new List<Panel> { panel_Keep, panel_Drop }, 0.1, ref redundantPanels, false, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].Apertures?.Count ?? -1);
            Assert.Single(redundantPanels);
        }

        // --- Query.MergeCoplanarPanels --------------------------------------------------------

        [Fact]
        public void MergeCoplanarPanels_TwoAbuttingFloors_MergeIntoOne()
        {
            List<Panel> panels = new List<Panel>
            {
                FloorPanel(0, 0, 0, 4),
                FloorPanel(4, 0, 0, 4),
            };

            List<Panel> redundantPanels = new List<Panel>();
            List<Panel> result = Analytical.Query.MergeCoplanarPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.MacroDistance, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(32, result[0].GetArea(), 4);
            Assert.Single(redundantPanels);
        }

        [Fact]
        public void MergeCoplanarPanels_SeparatedFloors_StayApart()
        {
            List<Panel> panels = new List<Panel>
            {
                FloorPanel(0, 0, 0, 4),
                FloorPanel(100, 0, 0, 4),
            };

            List<Panel> redundantPanels = new List<Panel>();
            List<Panel> result = Analytical.Query.MergeCoplanarPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.MacroDistance, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(32, result.Sum(x => x.GetArea()), 4);
            Assert.Empty(redundantPanels);
        }

        [Fact]
        public void MergeCoplanarPanels_NonCoplanarFloors_StayApart()
        {
            List<Panel> panels = new List<Panel>
            {
                FloorPanel(0, 0, 0, 4),
                FloorPanel(4, 0, 5, 4),
            };

            List<Panel> redundantPanels = new List<Panel>();
            List<Panel> result = Analytical.Query.MergeCoplanarPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.MacroDistance, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(32, result.Sum(x => x.GetArea()), 4);
        }

        [Fact]
        public void MergeCoplanarPanels_GridOfFloors_MergesToSingleSlab()
        {
            List<Panel> panels = QuadraticScanFixtures.CoplanarFloorPanels(9, 5, 5);

            List<Panel> redundantPanels = new List<Panel>();
            List<Panel> result = Analytical.Query.MergeCoplanarPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.MacroDistance, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(225, result[0].GetArea(), 4);
        }

        // --- Modify.RemoveAlmostSimilar_NTS --------------------------------------------------

        [Fact]
        public void RemoveAlmostSimilar_NTS_NaNTolerance_KeepsOnlyFirstGeometry()
        {
            // Documented historical behaviour: Query.AlmostSimilar rejects a pair with
            // `distance > tolerance`, which is false for every pair when tolerance is NaN,
            // so the original exhaustive scan removed everything after the first geometry.
            // The bucket optimisation must not reinterpret NaN as a finite cell size and
            // quietly keep far-apart geometry.
            List<NetTopologySuite.Geometries.Polygon> polygons = new List<NetTopologySuite.Geometries.Polygon>
            {
                Rectangle(0, 0, 1, 1),
                Rectangle(100, 0, 1, 1),
                Rectangle(0, 0, 1, 1),
            };

            Geometry.Planar.Modify.RemoveAlmostSimilar_NTS(polygons, double.NaN);

            Assert.Single(polygons);
            Assert.Equal(0, polygons[0].EnvelopeInternal.MinX);
            Assert.Equal(1, polygons[0].EnvelopeInternal.MaxX);
        }

        // --- Non-finite bounds in the private grid helpers ------------------------------------

        [Fact]
        public void SplitFace3Ds_Grid_NonFiniteBounds_RemainDiscoverable()
        {
            // The grid helper is private, so it is exercised by reflection rather than by
            // weakening production validation to construct invalid geometry. A box whose
            // max is +infinity produces kx2 < kx1 when quantised; a NaN box cannot be
            // quantised at all. Both must be held aside as fallback entries and offered to
            // every query instead of silently disappearing.
            BoundingBox3D finite = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
            BoundingBox3D infiniteMax = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(double.PositiveInfinity, 1, 1));
            BoundingBox3D nan = new BoundingBox3D(new Point3D(double.NaN, 0, 0), new Point3D(double.NaN, 1, 1));

            Type type = typeof(Geometry.Spatial.Modify).GetNestedType("BoundingBox3DGrid", BindingFlags.NonPublic);
            Assert.NotNull(type);

            object grid = Activator.CreateInstance(type, new object[] { new BoundingBox3D[] { finite, infiniteMax, nan }, Core.Tolerance.Distance });

            List<int> candidates = (List<int>)type.GetMethod("Candidates").Invoke(grid, new object[] { finite });
            Assert.Equal(new List<int> { 0, 1, 2 }, candidates);

            // A non-finite query box falls back to the full candidate set.
            List<int> candidates_NaN = (List<int>)type.GetMethod("Candidates").Invoke(grid, new object[] { nan });
            Assert.Equal(new List<int> { 0, 1, 2 }, candidates_NaN);
        }

        [Fact]
        public void AdjacencyCluster_FaceIndex_NonFiniteBounds_RemainDiscoverable()
        {
            // Same contract as SplitFace3Ds_Grid_NonFiniteBounds_RemainDiscoverable, for the
            // face lookup used by Create.AdjacencyCluster: an entry whose bounding box cannot
            // be quantised must still reach the exact predicate. Here the exact predicate
            // accepts (the point is on the face and inside the infinite box), so the entry
            // must be found; before the fix it was dropped from the index entirely.
            Face3D face3D = QuadraticScanFixtures.Quad(
                new Point3D(0, 0, 0),
                new Point3D(1, 0, 0),
                new Point3D(1, 1, 0),
                new Point3D(0, 1, 0));

            BoundingBox3D infiniteMax = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(double.PositiveInfinity, 1, 1));

            Type type = typeof(SAM.Analytical.Create).GetNestedType("Face3DIndex", BindingFlags.NonPublic);
            Assert.NotNull(type);

            object index = Activator.CreateInstance(type, new object[] { new List<Shell>(), Core.Tolerance.Distance });

            List<Tuple<BoundingBox3D, Face3D, Panel>> tuples = new List<Tuple<BoundingBox3D, Face3D, Panel>>
            {
                new Tuple<BoundingBox3D, Face3D, Panel>(infiniteMax, face3D, null),
            };

            type.GetMethod("Add").Invoke(index, new object[] { 0, infiniteMax });

            int found = (int)type.GetMethod("Find").Invoke(index, new object[] { new Point3D(0.5, 0.5, 0), tuples, Core.Tolerance.Distance });
            Assert.Equal(0, found);
        }

        private static NetTopologySuite.Geometries.Polygon Rectangle(double x, double y, double width, double height)
        {
            return new NetTopologySuite.Geometries.Polygon(new NetTopologySuite.Geometries.LinearRing(new[]
            {
                new NetTopologySuite.Geometries.Coordinate(x, y),
                new NetTopologySuite.Geometries.Coordinate(x + width, y),
                new NetTopologySuite.Geometries.Coordinate(x + width, y + height),
                new NetTopologySuite.Geometries.Coordinate(x, y + height),
                new NetTopologySuite.Geometries.Coordinate(x, y),
            }));
        }

        // --- Greedy-walk snap semantics ----------------------------------------------------

        [Fact]
        public void SnapFace2D_GreedyWalk_ReachesBeyondToleranceFromStart()
        {
            // Documented legacy semantics: the point Snap accepts an eligible snap point,
            // moves there, and keeps scanning with a strictly decreasing hop distance, so a
            // snap point farther than tolerance from the START is reachable through an
            // intermediate point. (0,0) -> (-0.09,0) -> (-0.15,0) with tolerance 0.1.
            Geometry.Planar.Face2D face2D = new Geometry.Planar.Face2D(new Geometry.Planar.Polygon2D(new List<Geometry.Planar.Point2D>
            {
                new Geometry.Planar.Point2D(0, 0),
                new Geometry.Planar.Point2D(4, 0),
                new Geometry.Planar.Point2D(4, 4),
                new Geometry.Planar.Point2D(0, 4),
            }));

            List<Geometry.Planar.Point2D> snapPoints = new List<Geometry.Planar.Point2D>
            {
                new Geometry.Planar.Point2D(-0.09, 0),
                new Geometry.Planar.Point2D(-0.15, 0),
            };

            Geometry.Planar.Face2D result = Geometry.Planar.Query.Snap(face2D, snapPoints, 0.1);

            List<Geometry.Planar.Point2D> points = ((Geometry.Planar.Polygon2D)result.ExternalEdge2D).Points;
            Assert.Contains(points, x => System.Math.Abs(x.X - (-0.15)) < 1e-9 && System.Math.Abs(x.Y) < 1e-9);
        }

        [Fact]
        public void SnapFace2D_FilteredSnapSet_LosesSecondHop()
        {
            // Companion to the walk test: removing the far point from the snap set changes
            // the result to the intermediate point. This is exactly why the bounding-box
            // candidate filter around Snap was not semantics-preserving.
            Geometry.Planar.Face2D face2D = new Geometry.Planar.Face2D(new Geometry.Planar.Polygon2D(new List<Geometry.Planar.Point2D>
            {
                new Geometry.Planar.Point2D(0, 0),
                new Geometry.Planar.Point2D(4, 0),
                new Geometry.Planar.Point2D(4, 4),
                new Geometry.Planar.Point2D(0, 4),
            }));

            List<Geometry.Planar.Point2D> snapPoints = new List<Geometry.Planar.Point2D>
            {
                new Geometry.Planar.Point2D(-0.09, 0),
            };

            Geometry.Planar.Face2D result = Geometry.Planar.Query.Snap(face2D, snapPoints, 0.1);

            List<Geometry.Planar.Point2D> points = ((Geometry.Planar.Polygon2D)result.ExternalEdge2D).Points;
            Assert.Contains(points, x => System.Math.Abs(x.X - (-0.09)) < 1e-9 && System.Math.Abs(x.Y) < 1e-9);
        }

        [Fact]
        public void MergeOverlapPanels_GreedySnapChain_DeduplicatedSetMatchesLegacy()
        {
            // Behaviour lock for the greedy-snap correction. The legacy path de-duplicates the
            // snap set with the same tolerance Snap uses, so surviving snap points are pairwise
            // MORE than tolerance apart; the greedy walk then provably performs at most one hop
            // per vertex (a second hop would need two survivors closer than tolerance). In this
            // fixture the intermediate point (-0.09,0) is removed by the de-duplication itself
            // and the far point (-0.15,0) survives but is out of reach, so legacy, filtered and
            // restored implementations all leave the corner at (0,0).
            Panel panel_Island = FloorPanel(0, 0, 0, 4);
            Panel panel_Hop1 = Hop1Panel();
            Panel panel_Hop2 = TrapezoidPanel();

            List<Panel> panels = new List<Panel> { panel_Island, panel_Hop1, panel_Hop2 };
            List<Panel> redundantPanels = new List<Panel>();

            List<Panel> result = Analytical.Query.MergeOverlapPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(panels.Count, result.Select(x => x.Guid).Distinct().Count());

            Panel panel_Merged = result.Find(x => x.Guid == panel_Island.Guid);
            Assert.NotNull(panel_Merged);
            Assert.True(HasVertex(panel_Merged, 0, 0), "corner must remain at (0,0): the same-tolerance dedup removes the intermediate point");
            Assert.False(HasVertex(panel_Merged, -1.5e-6, 0), "the far point must not replace the corner through a multi-hop walk");
        }

        [Fact]
        public void MergeCoplanarPanels_GreedySnapChain_DeduplicatedSetMatchesLegacy()
        {
            Panel panel_Island = FloorPanel(0, 0, 0, 4);
            Panel panel_Hop1 = Hop1Panel();
            Panel panel_Hop2 = TrapezoidPanel();

            List<Panel> panels = new List<Panel> { panel_Island, panel_Hop1, panel_Hop2 };
            List<Panel> redundantPanels = new List<Panel>();

            List<Panel> result = Analytical.Query.MergeCoplanarPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.MacroDistance, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(panels.Count, result.Select(x => x.Guid).Distinct().Count());

            Panel panel_Merged = result.Find(x => x.Guid == panel_Island.Guid);
            Assert.NotNull(panel_Merged);
            // In this path the popped panel is appended AFTER its coplanar partners, so the
            // de-duplication keeps the intermediate point and drops (0,0): the corner makes a
            // single legitimate hop. The far point is removed by the same de-duplication and
            // can never be reached, because that would need a second hop.
            Assert.True(HasVertex(panel_Merged, -9e-7, 0), "corner must make the single legitimate hop to the surviving intermediate point");
            Assert.False(HasVertex(panel_Merged, -1.5e-6, 0), "the far point must not replace the corner through a multi-hop walk");
        }

        private static Panel Hop1Panel()
        {
            // Corner (-9e-7, 0) is the intermediate hop: within Core.Tolerance.Distance of the
            // island corner (0,0), so the same-tolerance de-duplication removes it before Snap
            // ever runs. The slanted far edge keeps the panel disjoint from its neighbours.
            Face3D face3D = QuadraticScanFixtures.Quad(
                new Point3D(-0.5, -0.01, 0),
                new Point3D(-1, -1, 0),
                new Point3D(-9e-7, -1, 0),
                new Point3D(-9e-7, 0, 0));

            return Analytical.Create.Panel(Analytical.Query.DefaultConstruction(PanelType.Floor), PanelType.Floor, face3D);
        }

        private static Panel TrapezoidPanel()
        {
            // Corner (-1.5e-6, 0) is the unreachable far point: beyond Core.Tolerance.Distance
            // from the island corner (0,0) and outside its bbox grown by tolerance.
            Face3D face3D = QuadraticScanFixtures.Quad(
                new Point3D(-0.5, 0.01, 0),
                new Point3D(-1.5e-6, 0, 0),
                new Point3D(-0.14, 0.3, 0),
                new Point3D(-0.5, 0.3, 0));

            return Analytical.Create.Panel(Analytical.Query.DefaultConstruction(PanelType.Floor), PanelType.Floor, face3D);
        }

        private static Panel FloorPanel(double x, double y, double z, double width, double depth)
        {
            Face3D face3D = QuadraticScanFixtures.Quad(
                new Point3D(x, y, z),
                new Point3D(x + width, y, z),
                new Point3D(x + width, y + depth, z),
                new Point3D(x, y + depth, z));

            return Analytical.Create.Panel(Analytical.Query.DefaultConstruction(PanelType.Floor), PanelType.Floor, face3D);
        }

        private static bool HasVertex(Panel panel, double x, double y)
        {
            ISegmentable3D segmentable3D = panel?.GetFace3D()?.GetExternalEdge3D() as ISegmentable3D;
            List<Point3D> point3Ds = segmentable3D?.GetPoints();
            if (point3Ds == null)
            {
                return false;
            }

            return point3Ds.Exists(p => System.Math.Abs(p.X - x) < 1e-9 && System.Math.Abs(p.Y - y) < 1e-9);
        }

        private static double TotalArea(Shell shell)
        {
            return shell.Face3Ds.Sum(x => x.GetArea());
        }

        private static Panel FloorPanel(double x, double y, double z, double size)
        {
            Face3D face3D = QuadraticScanFixtures.Quad(
                new Point3D(x, y, z),
                new Point3D(x + size, y, z),
                new Point3D(x + size, y + size, z),
                new Point3D(x, y + size, z));

            return Analytical.Create.Panel(Analytical.Query.DefaultConstruction(PanelType.Floor), PanelType.Floor, face3D);
        }

        private static Panel WallPanel(double y, double size)
        {
            Face3D face3D = QuadraticScanFixtures.Quad(
                new Point3D(0, y, 0),
                new Point3D(size, y, 0),
                new Point3D(size, y, size),
                new Point3D(0, y, size));

            return Analytical.Create.Panel(Analytical.Query.DefaultConstruction(PanelType.Wall), PanelType.Wall, face3D);
        }
    }
}
