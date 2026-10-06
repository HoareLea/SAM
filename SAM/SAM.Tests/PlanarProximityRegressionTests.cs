// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Behaviour locks for the planar proximity scans indexed by the planar-proximity
    /// optimisation branch: the point-to-point Snap, the averaging segment Snap, generic
    /// RemoveAlmostSimilar, the segment Split dedup and New.MergeCoplanar. These were written
    /// against the pre-optimisation implementations and must keep passing unchanged - the
    /// optimisations are broad-phase indexing only and may not move a single result.
    /// </summary>
    public class PlanarProximityRegressionTests
    {
        // --- Geometry.Planar.Query.Snap(IEnumerable<Point2D>, IEnumerable<Point2D>, double) ---------------

        [Fact]
        public void SnapPoints_NoPointWithinTolerance_LeavesPointsUnchanged()
        {
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0), new Point2D(10, 0) };
            List<Point2D> snap = new List<Point2D> { new Point2D(5, 5), new Point2D(-3, 2) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Equal(2, result.Count);
            Assert.Equal(0, result[0].X);
            Assert.Equal(10, result[1].X);
        }

        [Fact]
        public void SnapPoints_OnePointWithinTolerance_SnapsToIt()
        {
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { new Point2D(0.0005, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Single(result);
            Assert.Equal(0.0005, result[0].X, 12);
        }

        [Fact]
        public void SnapPoints_SeveralWithinTolerance_GreedyWalkKeepsLastImprovement()
        {
            // Documented current behaviour: the source point MUTATES as it snaps, each hop must
            // be strictly shorter than the previous one, and distances are measured from the
            // latest position. 0 -> 0.0008 (hop 8e-4) -> 0.0003 (hop 5e-4) -> 0.0006 (hop 3e-4).
            // The result is therefore the LAST improvement in list order, not the nearest point.
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { new Point2D(0.0008, 0), new Point2D(0.0003, 0), new Point2D(0.0006, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Equal(0.0006, result[0].X, 12);
        }

        [Fact]
        public void SnapPoints_ChainCanExceedToleranceFromSource()
        {
            // Documented current behaviour: because every hop is measured from the latest
            // position, a snap point farther than tolerance from the ORIGINAL source can still
            // be reached through an intermediate point. 0 -> 0.0009 -> 0.0015.
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { new Point2D(0.0009, 0), new Point2D(0.0015, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Equal(0.0015, result[0].X, 12);
        }

        [Fact]
        public void SnapPoints_NearestIsNotEarliest_NearestStillWins()
        {
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { new Point2D(0.0009, 0), new Point2D(0.0001, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Equal(0.0001, result[0].X, 12);
        }

        [Fact]
        public void SnapPoints_EqualDistanceCandidates_EarliestWins()
        {
            Point2D first = new Point2D(0.0005, 0);
            Point2D second = new Point2D(-0.0005, 0);
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { first, second };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            // Strictly-less comparison keeps the first point seen at the winning distance.
            Assert.Same(first, result[0]);
        }

        [Fact]
        public void SnapPoints_ExactToleranceBoundary_Snaps()
        {
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { new Point2D(1e-3, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Equal(1e-3, result[0].X, 12);
        }

        [Fact]
        public void SnapPoints_ImmediatelyAboveTolerance_DoesNotSnap()
        {
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { new Point2D(1e-3 + 1e-12, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Equal(0, result[0].X);
        }

        [Fact]
        public void SnapPoints_IdenticalSnapPoint_IsNotChosen()
        {
            // The exact rule requires distance > 0: a snap point coincident with the source
            // never replaces it.
            Point2D source = new Point2D(1, 1);
            Point2D identical = new Point2D(1, 1);
            List<Point2D> sources = new List<Point2D> { source };
            List<Point2D> snap = new List<Point2D> { identical };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Same(source, result[0]);
        }

        [Fact]
        public void SnapPoints_ZeroNegativeAndNaNTolerance_NeverSnaps()
        {
            foreach (double tolerance in new double[] { 0, -1e-3, double.NaN })
            {
                List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
                List<Point2D> snap = new List<Point2D> { new Point2D(0.0001, 0) };

                List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, tolerance);

                Assert.Equal(0, result[0].X);
            }
        }

        [Fact]
        public void SnapPoints_InfiniteTolerance_SnapsToNearestNonIdentical()
        {
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { new Point2D(100, 0), new Point2D(50, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, double.PositiveInfinity);

            Assert.Equal(50, result[0].X);
        }

        [Fact]
        public void SnapPoints_NegativeAndLargeCoordinates_Snap()
        {
            List<Point2D> sources = new List<Point2D> { new Point2D(-1e9, -1e9) };
            List<Point2D> snap = new List<Point2D> { new Point2D(-1e9 + 0.0005, -1e9) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Equal(-1e9 + 0.0005, result[0].X, 6);
        }

        [Fact]
        public void SnapPoints_DuplicateSnapPoints_FirstInstanceWins()
        {
            Point2D first = new Point2D(0.0005, 0);
            Point2D duplicate = new Point2D(0.0005, 0);
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, new List<Point2D> { first, duplicate }, 1e-3);

            Assert.Same(first, result[0]);
        }

        [Fact]
        public void SnapPoints_RepeatedSources_EachSnappedIndependently()
        {
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0), new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { new Point2D(0.0005, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Equal(2, result.Count);
            Assert.Equal(0.0005, result[0].X, 12);
            Assert.Equal(0.0005, result[1].X, 12);
        }

        [Fact]
        public void SnapPoints_EmptyAndSingleItemCollections()
        {
            Assert.Empty(Geometry.Planar.Query.Snap(new List<Point2D>(), new List<Point2D> { new Point2D(0, 0) }, 1e-3));

            List<Point2D> result = Geometry.Planar.Query.Snap(new List<Point2D> { new Point2D(0, 0) }, new List<Point2D>(), 1e-3);
            Assert.Single(result);
            Assert.Equal(0, result[0].X);

            List<Point2D> result_Single = Geometry.Planar.Query.Snap(new List<Point2D> { new Point2D(0, 0) }, new List<Point2D> { new Point2D(0.0005, 0) }, 1e-3);
            Assert.Equal(0.0005, result_Single[0].X, 12);
        }

        [Fact]
        public void SnapPoints_NullSnapPointsAreSkipped()
        {
            List<Point2D> sources = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> snap = new List<Point2D> { null, new Point2D(0.0005, 0) };

            List<Point2D> result = Geometry.Planar.Query.Snap(sources, snap, 1e-3);

            Assert.Equal(0.0005, result[0].X, 12);
        }

        // --- Geometry.Planar.Query.Snap(IEnumerable<Segment2D>, bool, double) -----------------------------

        [Fact]
        public void SnapSegments_TouchingEndpoints_AreAveraged()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(1.0005, 0), new Point2D(2, 0)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Snap(segments, true, 1e-3);

            Assert.Equal(2, result.Count);
            Assert.True(result[0][1].AlmostEquals(new Point2D(1.00025, 0), 1e-9));
            Assert.True(result[1][0].AlmostEquals(new Point2D(1.00025, 0), 1e-9));
            Assert.True(result[0][0].AlmostEquals(new Point2D(0, 0), 1e-9));
            Assert.True(result[1][1].AlmostEquals(new Point2D(2, 0), 1e-9));
        }

        [Fact]
        public void SnapSegments_EndpointsBeyondTolerance_Unchanged()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(1.005, 0), new Point2D(2, 0)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Snap(segments, true, 1e-3);

            Assert.Equal(2, result.Count);
            Assert.True(result[0][1].AlmostEquals(new Point2D(1, 0), 1e-12));
            Assert.True(result[1][0].AlmostEquals(new Point2D(1.005, 0), 1e-12));
        }

        [Fact]
        public void SnapSegments_ChainOfJoints_AveragesEachJoint()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(1.0005, 0), new Point2D(2, 0)),
                new Segment2D(new Point2D(2.0005, 0), new Point2D(3, 0)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Snap(segments, true, 1e-3);

            Assert.Equal(3, result.Count);
            Assert.True(result[0][1].AlmostEquals(new Point2D(1.00025, 0), 1e-9));
            Assert.True(result[1][0].AlmostEquals(new Point2D(1.00025, 0), 1e-9));
            Assert.True(result[1][1].AlmostEquals(new Point2D(2.00025, 0), 1e-9));
            Assert.True(result[2][0].AlmostEquals(new Point2D(2.00025, 0), 1e-9));
        }

        [Fact]
        public void SnapSegments_ShortSegments_AreRemoved()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(5, 0), new Point2D(5.0005, 0)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Snap(segments, true, 1e-3);

            Assert.Single(result);
            Assert.True(result[0][0].AlmostEquals(new Point2D(0, 0), 1e-12));
        }

        [Fact]
        public void SnapSegments_IdenticalSegments_BothKeptAndUnchanged()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Snap(segments, true, 1e-3);

            Assert.Equal(2, result.Count);
            Assert.True(result[0][0].AlmostEquals(new Point2D(0, 0), 1e-12));
            Assert.True(result[1][1].AlmostEquals(new Point2D(1, 0), 1e-12));
        }

        [Fact]
        public void SnapSegments_NaNTolerance_NoSnappingNoRemoval()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(1.0005, 0), new Point2D(2, 0)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Snap(segments, true, double.NaN);

            Assert.Equal(2, result.Count);
            Assert.True(result[0][1].AlmostEquals(new Point2D(1, 0), 1e-12));
            Assert.True(result[1][0].AlmostEquals(new Point2D(1.0005, 0), 1e-12));
        }

        [Fact]
        public void SnapSegments_IncludeIntersectionFalse_Averages()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(1.0005, 0), new Point2D(2, 0)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Snap(segments, false, 1e-3);

            Assert.Equal(2, result.Count);
            Assert.True(result[0][1].AlmostEquals(new Point2D(1.00025, 0), 1e-9));
        }

        // --- Modify.RemoveAlmostSimilar<T> --------------------------------------------------

        [Fact]
        public void RemoveAlmostSimilarGeneric_ExactDuplicates_KeepsFirstInstance()
        {
            Segment2D first = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
            Segment2D duplicate = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
            List<Segment2D> segments = new List<Segment2D> { first, duplicate };

            Geometry.Planar.Modify.RemoveAlmostSimilar(segments, 1e-3);

            Assert.Single(segments);
            Assert.Same(first, segments[0]);
        }

        [Fact]
        public void RemoveAlmostSimilarGeneric_NearDuplicateWithinTolerance_KeepsFirst()
        {
            Segment2D first = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
            Segment2D near = new Segment2D(new Point2D(0.0005, 0), new Point2D(1.0005, 0));
            List<Segment2D> segments = new List<Segment2D> { first, near };

            Geometry.Planar.Modify.RemoveAlmostSimilar(segments, 1e-3);

            Assert.Single(segments);
            Assert.Same(first, segments[0]);
        }

        [Fact]
        public void RemoveAlmostSimilarGeneric_ImmediatelyBeyondTolerance_KeepsBoth()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(0, 0.01), new Point2D(1, 0.01)),
            };

            Geometry.Planar.Modify.RemoveAlmostSimilar(segments, 1e-3);

            Assert.Equal(2, segments.Count);
        }

        [Fact]
        public void RemoveAlmostSimilarGeneric_ReversedSegment_IsSimilar()
        {
            Segment2D first = new Segment2D(new Point2D(0, 0), new Point2D(1, 0));
            Segment2D reversed = new Segment2D(new Point2D(1, 0), new Point2D(0, 0));
            List<Segment2D> segments = new List<Segment2D> { first, reversed };

            Geometry.Planar.Modify.RemoveAlmostSimilar(segments, 1e-3);

            Assert.Single(segments);
            Assert.Same(first, segments[0]);
        }

        [Fact]
        public void RemoveAlmostSimilarGeneric_TranslatedRotatedAndDifferentLength_AreKept()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(10, 10), new Point2D(11, 10)),  // translated away
                new Segment2D(new Point2D(0, 0), new Point2D(0, 1)),      // rotated 90 degrees
                new Segment2D(new Point2D(0, 0), new Point2D(2, 0)),      // same line, different length
            };

            Geometry.Planar.Modify.RemoveAlmostSimilar(segments, 1e-3);

            Assert.Equal(4, segments.Count);
        }

        [Fact]
        public void RemoveAlmostSimilarGeneric_NegativeAndLargeCoordinates()
        {
            Segment2D first = new Segment2D(new Point2D(-1e9, -1e9), new Point2D(-1e9 + 1, -1e9));
            Segment2D near = new Segment2D(new Point2D(-1e9, -1e9), new Point2D(-1e9 + 1, -1e9));
            List<Segment2D> segments = new List<Segment2D> { first, near };

            Geometry.Planar.Modify.RemoveAlmostSimilar(segments, 1e-3);

            Assert.Single(segments);
            Assert.Same(first, segments[0]);
        }

        [Fact]
        public void RemoveAlmostSimilarGeneric_ToleranceBoundaries()
        {
            // Segment2D.On is a strict `Distance(point) < tolerance`, so an offset exactly at
            // tolerance is NOT similar and an offset just below tolerance is.
            List<Segment2D> below = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(0, 1e-3 - 1e-9), new Point2D(1, 1e-3 - 1e-9)),
            };
            Geometry.Planar.Modify.RemoveAlmostSimilar(below, 1e-3);
            Assert.Single(below);

            List<Segment2D> at = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(0, 1e-3), new Point2D(1, 1e-3)),
            };
            Geometry.Planar.Modify.RemoveAlmostSimilar(at, 1e-3);
            Assert.Equal(2, at.Count);

            List<Segment2D> above = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(0, 1e-3 + 1e-9), new Point2D(1, 1e-3 + 1e-9)),
            };
            Geometry.Planar.Modify.RemoveAlmostSimilar(above, 1e-3);
            Assert.Equal(2, above.Count);
        }

        [Fact]
        public void RemoveAlmostSimilarGeneric_DegenerateTolerances()
        {
            Func<List<Segment2D>> create = () => new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(5, 5), new Point2D(6, 5)),
            };

            // Zero: On is strict (`< tolerance`), so not even coincident geometry is similar.
            List<Segment2D> zero = create();
            Geometry.Planar.Modify.RemoveAlmostSimilar(zero, 0);
            Assert.Equal(3, zero.Count);

            // Negative: nothing is similar.
            List<Segment2D> negative = create();
            Geometry.Planar.Modify.RemoveAlmostSimilar(negative, -1e-3);
            Assert.Equal(3, negative.Count);

            // NaN: the exact predicate accepts nothing (On uses distance <= tolerance).
            List<Segment2D> nan = create();
            Geometry.Planar.Modify.RemoveAlmostSimilar(nan, double.NaN);
            Assert.Equal(3, nan.Count);

            // Infinite: everything is similar to the first.
            List<Segment2D> infinite = create();
            Geometry.Planar.Modify.RemoveAlmostSimilar(infinite, double.PositiveInfinity);
            Assert.Single(infinite);
        }

        [Fact]
        public void RemoveAlmostSimilarGeneric_PreservesOutputOrder()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),
                new Segment2D(new Point2D(0, 5), new Point2D(1, 5)),
                new Segment2D(new Point2D(0, 0), new Point2D(1, 0)),      // duplicate of first
                new Segment2D(new Point2D(0, 2), new Point2D(1, 2)),
                new Segment2D(new Point2D(0, 5), new Point2D(1, 5)),      // duplicate of second
            };

            Geometry.Planar.Modify.RemoveAlmostSimilar(segments, 1e-3);

            Assert.Equal(3, segments.Count);
            Assert.Equal(0, segments[0][0].Y);
            Assert.Equal(5, segments[1][0].Y);
            Assert.Equal(2, segments[2][0].Y);
        }

        // --- Geometry.Planar.Query.Split(IEnumerable<Segment2D>, double) ------------------------------------

        [Fact]
        public void SplitSegments_CrossingSegments_SplitIntoFour()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(2, 0)),
                new Segment2D(new Point2D(1, -1), new Point2D(1, 1)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Split(segments, Core.Tolerance.Distance);

            Assert.Equal(4, result.Count);
            Assert.Equal(4, result.Sum(x => x.GetLength()), 9);
        }

        [Fact]
        public void SplitSegments_TJunction_SplitsOnlyTheCrossedSegment()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(2, 0)),
                new Segment2D(new Point2D(1, 0), new Point2D(1, 1)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Split(segments, Core.Tolerance.Distance);

            Assert.Equal(3, result.Count);
            Assert.Equal(3, result.Sum(x => x.GetLength()), 9);
        }

        [Fact]
        public void SplitSegments_CollinearOverlap_SplitsWithoutDuplicatePiece()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(2, 0)),
                new Segment2D(new Point2D(1, 0), new Point2D(3, 0)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Split(segments, Core.Tolerance.Distance);

            Assert.Equal(3, result.Count);
            Assert.Equal(3, result.Sum(x => x.GetLength()), 9);
        }

        [Fact]
        public void SplitSegments_ExactDuplicates_KeepOne()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(2, 0)),
                new Segment2D(new Point2D(0, 0), new Point2D(2, 0)),
            };

            List<Segment2D> result = Geometry.Planar.Query.Split(segments, Core.Tolerance.Distance);

            Assert.Single(result);
        }

        // --- Query.MergeCoplanar (New, partitions) ------------------------------------------

        [Fact]
        public void MergeCoplanarPartitions_GridOfFloors_MergesToSingleSlab()
        {
            List<IPartition> partitions = QuadraticScanFixtures.CoplanarFloorPanels(9, 5, 5).ConvertAll(x => (IPartition)new AirPartition(x.GetFace3D()));

            List<IPartition> result = Analytical.Query.MergeCoplanar(partitions, 0.1, out List<IPartition> redundantPartitions, true, Core.Tolerance.MacroDistance, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(225, result[0].Face3D.GetArea(), 4);
        }

        [Fact]
        public void MergeCoplanarPartitions_SeparatedFloors_StayApart()
        {
            List<IPartition> partitions = new List<IPartition>
            {
                FloorPartition(0, 0, 0, 4),
                FloorPartition(100, 0, 0, 4),
            };

            List<IPartition> result = Analytical.Query.MergeCoplanar(partitions, 0.1, out List<IPartition> redundantPartitions, true, Core.Tolerance.MacroDistance, Core.Tolerance.Distance);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(32, result.Sum(x => x.Face3D.GetArea()), 4);
        }

        private static IPartition FloorPartition(double x, double y, double z, double size)
        {
            Face3D face3D = QuadraticScanFixtures.Quad(
                new Geometry.Spatial.Point3D(x, y, z),
                new Geometry.Spatial.Point3D(x + size, y, z),
                new Geometry.Spatial.Point3D(x + size, y + size, z),
                new Geometry.Spatial.Point3D(x, y + size, z));

            return new AirPartition(face3D);
        }
    }
}
