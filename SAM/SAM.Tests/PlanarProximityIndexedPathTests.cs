// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Correctness coverage for the <em>indexed</em> planar proximity paths.
    /// <para>
    /// <c>Query.Snap(segments)</c> and <c>Modify.RemoveAlmostSimilar&lt;T&gt;</c> both run the
    /// original exhaustive algorithm below 32 items and the grid-indexed one at or above it.
    /// The hand-written fixtures in PlanarProximityRegressionTests are all 3-5 items, so they
    /// characterise the fallback - which is the pre-existing code - and never reach the index.
    /// Every fixture here is built past that threshold on purpose, and every result is compared
    /// against an exhaustive reference implementation held in this file, so a divergence in the
    /// indexed path fails rather than merely running faster.
    /// </para>
    /// <para>
    /// The reference implementations are verbatim copies of the pre-index algorithms. They are
    /// duplicated here deliberately: comparing production against itself would prove nothing.
    /// </para>
    /// </summary>
    public class PlanarProximityIndexedPathTests
    {
        /// <summary>Item count at which production switches from the exhaustive scan to the grid.</summary>
        private const int IndexedThreshold = 32;

        // --- Query.Snap(segments): indexed vs exhaustive ---------------------------------------

        [Theory]
        [InlineData(1e-3, true)]
        [InlineData(1e-3, false)]
        [InlineData(1e-2, true)]
        [InlineData(1e-6, true)]
        [InlineData(0.0, true)]
        [InlineData(-1e-3, true)]
        [InlineData(double.NaN, true)]
        public void Snap_Segments_IndexedMatchesExhaustive(double tolerance, bool includeIntersection)
        {
            List<Segment2D> fixture = SnapFixture();
            Assert.True(fixture.Count > IndexedThreshold, $"fixture must cross the indexed threshold, was {fixture.Count}");

            List<Segment2D> indexed = Query.Snap(Clone(fixture), includeIntersection, tolerance);
            List<Segment2D> exhaustive = Snap_Exhaustive(Clone(fixture), includeIntersection, tolerance);

            AssertSegmentsIdentical(exhaustive, indexed);
        }

        /// <summary>
        /// The indexed path is order sensitive in the same way the exhaustive one is - the first
        /// endpoint visited seeds the average that every later endpoint is measured against - so
        /// permuting the input must move both implementations the same way, not neither.
        /// </summary>
        [Fact]
        public void Snap_Segments_IndexedMatchesExhaustive_UnderInputPermutation()
        {
            List<Segment2D> fixture = SnapFixture();

            for (int seed = 0; seed < 5; seed++)
            {
                List<Segment2D> permuted = Permute(fixture, seed);
                Assert.True(permuted.Count > IndexedThreshold);

                List<Segment2D> indexed = Query.Snap(Clone(permuted), true, 1e-3);
                List<Segment2D> exhaustive = Snap_Exhaustive(Clone(permuted), true, 1e-3);

                AssertSegmentsIdentical(exhaustive, indexed);
            }
        }

        /// <summary>
        /// Guards the guard: if the fixture were quietly reduced below the threshold, or the
        /// threshold raised, every comparison above would silently degrade to testing the
        /// fallback against itself. Snapping must also actually happen, or the comparison is
        /// vacuous.
        /// </summary>
        [Fact]
        public void Snap_Segments_FixtureReachesIndexedPathAndSnaps()
        {
            List<Segment2D> fixture = SnapFixture();

            Assert.True(fixture.Count >= 64, $"at least 64 segments required, was {fixture.Count}");

            List<Segment2D> result = Query.Snap(Clone(fixture), true, 1e-3);

            int moved = 0;
            for (int i = 0; i < result.Count && i < fixture.Count; i++)
            {
                if (result[i][0].Distance(fixture[i][0]) > 0 || result[i][1].Distance(fixture[i][1]) > 0)
                    moved++;
            }

            Assert.True(moved > 0, "the fixture must contain endpoints that actually snap");
            Assert.True(result.Count < fixture.Count, "the fixture must contain segments short enough to be removed");
        }

        // --- Modify.RemoveAlmostSimilar<T>: indexed vs exhaustive -------------------------------

        [Theory]
        [InlineData(1e-3)]
        [InlineData(1e-2)]
        [InlineData(1e-6)]
        [InlineData(0.0)]
        [InlineData(-1e-3)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void RemoveAlmostSimilar_IndexedMatchesExhaustive(double tolerance)
        {
            List<Segment2D> fixture = RemoveAlmostSimilarFixture();
            Assert.True(fixture.Count > IndexedThreshold, $"fixture must cross the indexed threshold, was {fixture.Count}");

            List<Segment2D> indexed = new List<Segment2D>(fixture);
            Modify.RemoveAlmostSimilar(indexed, tolerance);

            List<Segment2D> exhaustive = RemoveAlmostSimilar_Exhaustive(fixture, tolerance);

            AssertSameInstancesInOrder(exhaustive, indexed);
        }

        /// <summary>
        /// Same comparison over a polyline payload, so the generic path is not only exercised
        /// with the two-point degenerate case.
        /// </summary>
        [Fact]
        public void RemoveAlmostSimilar_Polylines_IndexedMatchesExhaustive()
        {
            List<Polyline2D> fixture = PolylineFixture();
            Assert.True(fixture.Count > IndexedThreshold, $"fixture must cross the indexed threshold, was {fixture.Count}");

            foreach (double tolerance in new double[] { 1e-3, 1e-6, 0.0, -1e-3, double.NaN })
            {
                List<Polyline2D> indexed = new List<Polyline2D>(fixture);
                Modify.RemoveAlmostSimilar(indexed, tolerance);

                List<Polyline2D> exhaustive = RemoveAlmostSimilar_Exhaustive(fixture, tolerance);

                AssertSameInstancesInOrder(exhaustive, indexed);
            }
        }

        [Fact]
        public void RemoveAlmostSimilar_FixtureReachesIndexedPathAndRemoves()
        {
            List<Segment2D> fixture = RemoveAlmostSimilarFixture();

            Assert.True(fixture.Count >= 64, $"at least 64 segmentables required, was {fixture.Count}");

            List<Segment2D> result = new List<Segment2D>(fixture);
            Modify.RemoveAlmostSimilar(result, 1e-3);

            Assert.True(result.Count < fixture.Count, "the fixture must contain removable duplicates");
            Assert.True(result.Count > IndexedThreshold, "the kept set must stay large enough to be worth indexing");
        }

        // --- Negative tolerance: repeated instances --------------------------------------------

        /// <summary>
        /// Query.AlmostSimilar short-circuits on reference equality <em>before</em> tolerance is
        /// consulted, so an instance is similar to itself at any tolerance - including a negative
        /// one, where nothing else is similar to anything. A geometric broad-phase filter cannot
        /// see that, so the indexed path has to preserve it separately.
        /// </summary>
        [Theory]
        [InlineData(-1e-3)]
        [InlineData(-1.0)]
        public void RemoveAlmostSimilar_NegativeTolerance_RepeatedInstance_IsRemoved(double tolerance)
        {
            Segment2D repeated = new Segment2D(new Point2D(3, 4), new Point2D(5, 7));

            List<Segment2D> fixture = PaddedWith(repeated, repeated, 40);
            Assert.True(fixture.Count > IndexedThreshold);

            List<Segment2D> indexed = new List<Segment2D>(fixture);
            Modify.RemoveAlmostSimilar(indexed, tolerance);

            List<Segment2D> exhaustive = RemoveAlmostSimilar_Exhaustive(fixture, tolerance);

            AssertSameInstancesInOrder(exhaustive, indexed);
            Assert.Equal(1, indexed.Count(x => ReferenceEquals(x, repeated)));
        }

        /// <summary>
        /// The sibling case, which must NOT change: two distinct instances holding identical
        /// values are not reference equal, so at negative tolerance both survive.
        /// </summary>
        [Theory]
        [InlineData(-1e-3)]
        [InlineData(-1.0)]
        public void RemoveAlmostSimilar_NegativeTolerance_DistinctValueIdentical_BothKept(double tolerance)
        {
            Segment2D first = new Segment2D(new Point2D(3, 4), new Point2D(5, 7));
            Segment2D second = new Segment2D(new Point2D(3, 4), new Point2D(5, 7));

            List<Segment2D> fixture = PaddedWith(first, second, 40);

            List<Segment2D> indexed = new List<Segment2D>(fixture);
            Modify.RemoveAlmostSimilar(indexed, tolerance);

            List<Segment2D> exhaustive = RemoveAlmostSimilar_Exhaustive(fixture, tolerance);

            AssertSameInstancesInOrder(exhaustive, indexed);
            Assert.Contains(indexed, x => ReferenceEquals(x, first));
            Assert.Contains(indexed, x => ReferenceEquals(x, second));
        }

        /// <summary>
        /// The repeated instance must be removed at every tolerance, and the surviving one must
        /// be the first occurrence - the reference fast path must not disturb which copy is kept.
        /// </summary>
        [Fact]
        public void RemoveAlmostSimilar_RepeatedInstance_FirstOccurrenceKept_AcrossTolerances()
        {
            Segment2D repeated = new Segment2D(new Point2D(3, 4), new Point2D(5, 7));

            foreach (double tolerance in new double[] { 1e-3, 0.0, -1e-3, double.NaN })
            {
                List<Segment2D> fixture = PaddedWith(repeated, repeated, 40);
                int index_First = fixture.FindIndex(x => ReferenceEquals(x, repeated));

                List<Segment2D> indexed = new List<Segment2D>(fixture);
                Modify.RemoveAlmostSimilar(indexed, tolerance);

                List<Segment2D> exhaustive = RemoveAlmostSimilar_Exhaustive(fixture, tolerance);

                AssertSameInstancesInOrder(exhaustive, indexed);
                Assert.Equal(1, indexed.Count(x => ReferenceEquals(x, repeated)));

                // Kept copy sits where the first occurrence was, relative to the padding.
                int index_Kept = indexed.FindIndex(x => ReferenceEquals(x, repeated));
                Assert.Equal(exhaustive.FindIndex(x => ReferenceEquals(x, repeated)), index_Kept);
                Assert.True(index_Kept <= index_First);
            }
        }

        /// <summary>
        /// Query.Split runs the same AlmostSimilar dedup over its kept set, so the same
        /// reference-identity rule has to survive there.
        /// </summary>
        [Theory]
        [InlineData(-1e-3)]
        [InlineData(-1.0)]
        public void Split_NegativeTolerance_RepeatedInstance_MatchesExhaustive(double tolerance)
        {
            Segment2D repeated = new Segment2D(new Point2D(3, 4), new Point2D(5, 7));

            List<Segment2D> fixture = PaddedWith(repeated, repeated, 40);

            List<Segment2D> indexed = Query.Split(fixture, tolerance);
            List<Segment2D> exhaustive = Split_ExhaustiveDedupCount(fixture, tolerance);

            Assert.Equal(exhaustive.Count, indexed.Count);
        }

        // --- Extreme finite coordinates: BoundingBox2DGrid --------------------------------------

        /// <summary>
        /// A finite coordinate whose quantised cell index leaves long range cannot be placed in
        /// a cell. It must be held aside and offered to every query, never dropped: the exact
        /// predicate, not the grid, decides equivalence.
        /// </summary>
        [Fact]
        public void BoundingBox2DGrid_ExtremeFiniteBox_RemainsDiscoverable()
        {
            foreach (double origin in new double[] { 1e13, -1e13, 1e17, -1e17, 1e300, -1e300 })
            {
                object grid = CreateBoundingBox2DGrid(1e-6, 0);

                BoundingBox2D box_Ordinary = Box(0, 0, 1, 1);
                BoundingBox2D box_Extreme = Box(origin, origin, origin + 1, origin + 1);

                Assert.Equal(0, GridAdd(grid, box_Ordinary));
                Assert.Equal(1, GridAdd(grid, box_Extreme));

                // Queried at its own position...
                Assert.Contains(1, GridCandidates(grid, box_Extreme));

                // ...and offered to unrelated queries, because it could not be placed.
                Assert.Contains(1, GridCandidates(grid, box_Ordinary));

                // The ordinary box is still filtered normally.
                Assert.Contains(0, GridCandidates(grid, box_Ordinary));
            }
        }

        [Fact]
        public void BoundingBox2DGrid_ExtremeFiniteQuery_ReturnsCompleteSuperset()
        {
            object grid = CreateBoundingBox2DGrid(1e-6, 0);

            // Boxes smaller than the tolerance keep the cell size pinned at the 1e-6 floor -
            // the rehash target equals the current size, so it never fires - which is what puts
            // a 1e13 query outside the representable index range.
            for (int i = 0; i < 40; i++)
                GridAdd(grid, Box(i, 0, i + 1e-9, 1e-9));

            List<int> candidates = GridCandidates(grid, Box(1e13, 1e13, 1e13 + 1, 1e13 + 1));

            Assert.Equal(40, candidates.Count);
            for (int i = 0; i < 40; i++)
                Assert.Equal(i, candidates[i]);
        }

        /// <summary>
        /// Candidates arrive in ascending insertion order. A placed box occupies every cell it
        /// covers, so a multi-cell query legitimately sees it more than once - the contract is
        /// ordering, not distinctness, and the caller re-tests the exact predicate anyway.
        /// </summary>
        [Fact]
        public void BoundingBox2DGrid_Candidates_AreAscending_AndOfferEveryAsideEntry()
        {
            object grid = CreateBoundingBox2DGrid(1e-6, 0);

            // Interleave placeable and non-placeable boxes so the aside set and the cell set
            // both contribute to the same query.
            for (int i = 0; i < 24; i++)
            {
                GridAdd(grid, Box(i, 0, i + 1e-9, 1e-9));
                GridAdd(grid, Box(1e13 + i, 0, 1e13 + i + 1e-9, 1e-9));
            }

            List<int> candidates = GridCandidates(grid, Box(3, 0, 3 + 1e-9, 1e-9));

            for (int i = 1; i < candidates.Count; i++)
                Assert.True(candidates[i] >= candidates[i - 1], "candidates must be in ascending insertion order");

            // Every non-placeable box is offered; the placeable ones are filtered.
            for (int i = 0; i < 24; i++)
                Assert.Contains(2 * i + 1, candidates);

            List<int> distinct = candidates.Distinct().ToList();
            Assert.True(distinct.Count < 48, "placeable boxes must still be filtered");
            Assert.Contains(6, distinct);
        }

        /// <summary>
        /// End to end: extreme but finite coordinates must not make a duplicate invisible to
        /// RemoveAlmostSimilar. Tiny segments keep the cell size at the tolerance floor, which
        /// is what puts the quantised indexes out of range.
        /// </summary>
        [Fact]
        public void RemoveAlmostSimilar_ExtremeFiniteCoordinates_MatchesExhaustive()
        {
            List<Segment2D> fixture = new List<Segment2D>();

            // 40 tiny, well separated segments: median extent below tolerance, so cellSize
            // stays at 1e-6 and a 1e14 coordinate quantises past long range.
            for (int i = 0; i < 40; i++)
                fixture.Add(new Segment2D(new Point2D(i, 0), new Point2D(i + 1e-9, 0)));

            Segment2D extreme = new Segment2D(new Point2D(1e14, 1e14), new Point2D(1e14 + 1e-9, 1e14));
            fixture.Add(extreme);
            fixture.Add(new Segment2D(new Point2D(1e14, 1e14), new Point2D(1e14 + 1e-9, 1e14)));   // value duplicate
            fixture.Add(extreme);                                                                  // repeated instance

            Assert.True(fixture.Count > IndexedThreshold);

            foreach (double tolerance in new double[] { 1e-6, 1e-3, 0.0, -1e-3 })
            {
                List<Segment2D> indexed = new List<Segment2D>(fixture);
                Modify.RemoveAlmostSimilar(indexed, tolerance);

                List<Segment2D> exhaustive = RemoveAlmostSimilar_Exhaustive(fixture, tolerance);

                AssertSameInstancesInOrder(exhaustive, indexed);
            }
        }

        // --- Extreme finite coordinates: Point2DGrid --------------------------------------------

        [Fact]
        public void Point2DGrid_ExtremeFinitePoint_RemainsDiscoverable()
        {
            foreach (double origin in new double[] { 1e13, -1e13, 1e17, -1e17, 1e300, -1e300 })
            {
                object grid = CreatePoint2DGrid(1e-6);

                Point2D point_Ordinary = new Point2D(0.5, 0.5);
                Point2D point_Extreme = new Point2D(origin, origin);

                Assert.Equal(0, GridAdd(grid, point_Ordinary));
                Assert.Equal(1, GridAdd(grid, point_Extreme));

                Assert.Contains(1, GridCandidates(grid, point_Extreme));
                Assert.Contains(1, GridCandidates(grid, point_Ordinary));
                Assert.Contains(0, GridCandidates(grid, point_Ordinary));
            }
        }

        [Fact]
        public void Point2DGrid_Replace_KeepsExtremePointDiscoverable()
        {
            object grid = CreatePoint2DGrid(1e-6);

            GridAdd(grid, new Point2D(0.5, 0.5));
            GridAdd(grid, new Point2D(1e17, 1e17));

            // Extreme -> ordinary.
            GridReplace(grid, 1, new Point2D(0.5, 0.5));
            Assert.Contains(1, GridCandidates(grid, new Point2D(0.5, 0.5)));

            // Ordinary -> extreme.
            GridReplace(grid, 0, new Point2D(-1e17, -1e17));
            Assert.Contains(0, GridCandidates(grid, new Point2D(0.5, 0.5)));
            Assert.Contains(0, GridCandidates(grid, new Point2D(-1e17, -1e17)));
        }

        /// <summary>
        /// End to end through Query.Split, whose point de-duplication is the Point2DGrid's only
        /// production caller. A coincident endpoint at an extreme coordinate must resolve to the
        /// stored instance, exactly as the list scan it replaced did.
        /// </summary>
        [Fact]
        public void Split_ExtremeFiniteCoordinates_MatchesOrdinaryCoordinates()
        {
            // The same topology translated to an extreme origin must de-duplicate endpoints the
            // same way. Comparing against the ordinary-origin run rather than asserting an
            // absolute instance count keeps the test tied to observed behaviour: whatever Split
            // does at the origin is what it has to do far from it.
            //
            // The origins are powers of two above 9.2e12 - the point at which a coordinate
            // divided by the 1e-6 cell size leaves long range. Powers of two keep the
            // translation exact, so a difference in the signature is the index losing a point,
            // not the fixture losing precision.
            Tuple<int, int> reference = SplitEndpointSignature(0);

            foreach (double origin in new double[] { 70368744177664.0, -70368744177664.0, 140737488355328.0 })
            {
                Tuple<int, int> actual = SplitEndpointSignature(origin);

                Assert.Equal(reference.Item1, actual.Item1);
                Assert.Equal(reference.Item2, actual.Item2);
            }
        }

        /// <summary>
        /// Splits a fixed topology translated to <paramref name="origin"/> and returns
        /// (piece count, distinct endpoint instance count). The second number is the one the
        /// point de-duplication controls: coincident endpoints that resolve to one stored
        /// instance lower it, and endpoints that escape the index inflate it.
        /// </summary>
        private static Tuple<int, int> SplitEndpointSignature(double origin)
        {
            List<Segment2D> segment2Ds = new List<Segment2D>();
            for (int i = 0; i < 40; i++)
                segment2Ds.Add(new Segment2D(new Point2D(origin + i, origin), new Point2D(origin + i + 0.5, origin)));

            // Two segments meeting at one coincident endpoint, written as separate instances.
            segment2Ds.Add(new Segment2D(new Point2D(origin, origin + 10), new Point2D(origin + 1, origin + 10)));
            segment2Ds.Add(new Segment2D(new Point2D(origin + 1, origin + 10), new Point2D(origin + 2, origin + 10)));

            List<Segment2D> result = Query.Split(segment2Ds, 1e-6);

            HashSet<Point2D> instances = new HashSet<Point2D>(new ReferenceComparer());
            foreach (Segment2D segment2D in result)
            {
                instances.Add(segment2D[0]);
                instances.Add(segment2D[1]);
            }

            return new Tuple<int, int>(result.Count, instances.Count);
        }

        private sealed class ReferenceComparer : IEqualityComparer<Point2D>
        {
            public bool Equals(Point2D point2D_1, Point2D point2D_2)
            {
                return ReferenceEquals(point2D_1, point2D_2);
            }

            public int GetHashCode(Point2D point2D)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(point2D);
            }
        }

        // --- Exhaustive reference implementations ----------------------------------------------

        /// <summary>
        /// Verbatim pre-index Query.Snap(IEnumerable&lt;Segment2D&gt;, bool, double).
        /// </summary>
        private static List<Segment2D> Snap_Exhaustive(IEnumerable<Segment2D> segment2Ds, bool includeIntersection, double tolerance)
        {
            if (segment2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null)
                {
                    result.Add(null);
                    continue;
                }
                result.Add(new Segment2D(segment2D));
            }

            HashSet<Point2D> point2Ds_Unique = new HashSet<Point2D>();
            for (int i = 0; i < result.Count; i++)
            {
                List<Point2D> point2Ds_Segment2D = result[i].GetPoints();
                foreach (Point2D point2D_Segment2D in point2Ds_Segment2D)
                {
                    if (point2Ds_Unique.Contains(point2D_Segment2D))
                        continue;

                    point2Ds_Unique.Add(point2D_Segment2D);

                    Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
                    List<Point2D> point2Ds = new List<Point2D>();
                    for (int j = 0; j < result.Count; j++)
                    {
                        List<int> indexes = new List<int>();

                        if (result[j][0].Distance(point2D_Segment2D) <= tolerance)
                        {
                            indexes.Add(0);
                            point2Ds.Add(result[j][0]);
                        }

                        if (result[j][1].Distance(point2D_Segment2D) <= tolerance)
                        {
                            indexes.Add(1);
                            point2Ds.Add(result[j][1]);
                        }

                        if (indexes.Count == 0)
                            continue;

                        dictionary[j] = indexes;
                    }

                    Point2D point2D_New = null;
                    if (point2Ds.Count == 1)
                    {
                        point2D_New = point2Ds[0];
                    }
                    else if (point2Ds.Count == 2 && includeIntersection)
                    {
                        IEnumerable<int> indexes = dictionary.Keys;
                        if (indexes.Count() == 2)
                            point2D_New = result[indexes.ElementAt(0)].Intersection(result[indexes.ElementAt(1)], false, tolerance);
                    }

                    if (point2D_New == null)
                        point2D_New = Query.Average(point2Ds);

                    foreach (KeyValuePair<int, List<int>> keyValuePair in dictionary)
                    {
                        if (keyValuePair.Value.Contains(0))
                            result[keyValuePair.Key] = new Segment2D(point2D_New, result[keyValuePair.Key][1]);

                        if (keyValuePair.Value.Contains(1))
                            result[keyValuePair.Key] = new Segment2D(result[keyValuePair.Key][0], point2D_New);
                    }
                }
            }

            result.RemoveAll(x => x.GetLength() <= tolerance);
            return result;
        }

        /// <summary>
        /// Verbatim pre-index Modify.RemoveAlmostSimilar&lt;T&gt;.
        /// </summary>
        private static List<T> RemoveAlmostSimilar_Exhaustive<T>(IEnumerable<T> segmentable2Ds, double tolerance) where T : ISegmentable2D
        {
            List<T> result = new List<T>();
            foreach (T segmentable2D in segmentable2Ds)
                if (result.Find(x => Query.AlmostSimilar(x, segmentable2D, tolerance)) == null)
                    result.Add(segmentable2D);

            return result;
        }

        /// <summary>
        /// The kept set Query.Split's AlmostSimilar dedup would produce on inputs that never
        /// intersect, which is the only part of Split the reference-identity rule can change.
        /// </summary>
        private static List<Segment2D> Split_ExhaustiveDedupCount(IEnumerable<Segment2D> segment2Ds, double tolerance)
        {
            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null || segment2D.GetLength() < tolerance)
                    continue;

                if (result.Find(x => x.AlmostSimilar(segment2D, tolerance)) != null)
                    continue;

                result.Add(segment2D);
            }

            return result;
        }

        // --- Fixtures ---------------------------------------------------------------------------

        /// <summary>
        /// 73 segments spanning every category the indexed Snap has to handle: isolated
        /// segments, exactly and almost coincident endpoints, shared Point2D instances,
        /// reversed pairs, negative and large coordinates, endpoints placed exactly at and
        /// immediately either side of the nominal tolerance, and joints where three or more
        /// endpoints meet so the averaging order matters.
        /// </summary>
        private static List<Segment2D> SnapFixture()
        {
            const double t = 1e-3;
            List<Segment2D> result = new List<Segment2D>();

            // Isolated: far apart, nothing within tolerance. (16)
            for (int i = 0; i < 16; i++)
                result.Add(new Segment2D(new Point2D(10 * i, 0), new Point2D(10 * i + 4, 0)));

            // Exactly coincident endpoints. (8 -> 4 joints)
            for (int i = 0; i < 4; i++)
            {
                double x = 1000 + 10 * i;
                result.Add(new Segment2D(new Point2D(x, 0), new Point2D(x + 2, 0)));
                result.Add(new Segment2D(new Point2D(x + 2, 0), new Point2D(x + 4, 0)));
            }

            // Endpoint gaps exactly at tolerance, and immediately either side of it. (12)
            double[] gaps = new double[] { t, t * (1 - 1e-12), t * (1 + 1e-12) };
            for (int i = 0; i < gaps.Length; i++)
            {
                for (int k = 0; k < 2; k++)
                {
                    double x = 2000 + 10 * (2 * i + k);
                    result.Add(new Segment2D(new Point2D(x, 0), new Point2D(x + 2, 0)));
                    result.Add(new Segment2D(new Point2D(x + 2 + gaps[i], 0), new Point2D(x + 4, 0)));
                }
            }

            // Shared Point2D instances: the same object on two segments. (8)
            for (int i = 0; i < 4; i++)
            {
                Point2D shared = new Point2D(3000 + 10 * i, 0);
                result.Add(new Segment2D(new Point2D(3000 + 10 * i - 2, 0), shared));
                result.Add(new Segment2D(shared, new Point2D(3000 + 10 * i + 2, 0)));
            }

            // Reversed duplicates. (8)
            for (int i = 0; i < 4; i++)
            {
                double x = 4000 + 10 * i;
                result.Add(new Segment2D(new Point2D(x, 0), new Point2D(x + 3, 0)));
                result.Add(new Segment2D(new Point2D(x + 3, 0), new Point2D(x, 0)));
            }

            // Negative coordinates, with joints. (8)
            for (int i = 0; i < 4; i++)
            {
                double x = -1000 - 10 * i;
                result.Add(new Segment2D(new Point2D(x, -5), new Point2D(x - 2, -5)));
                result.Add(new Segment2D(new Point2D(x - 2 + t / 2, -5), new Point2D(x - 4, -5)));
            }

            // Large coordinates. (6)
            for (int i = 0; i < 3; i++)
            {
                double x = 1e6 + 10 * i;
                result.Add(new Segment2D(new Point2D(x, 1e6), new Point2D(x + 2, 1e6)));
                result.Add(new Segment2D(new Point2D(x + 2 + t / 2, 1e6), new Point2D(x + 4, 1e6)));
            }

            // Three-way and four-way joints: averaging depends on visit order. (5)
            Point2D hub = new Point2D(5000, 0);
            result.Add(new Segment2D(new Point2D(4996, 0), hub));
            result.Add(new Segment2D(new Point2D(5000 + t / 3, 0), new Point2D(5004, 0)));
            result.Add(new Segment2D(new Point2D(5000, t / 3), new Point2D(5000, 4)));
            result.Add(new Segment2D(new Point2D(5000 - t / 3, -t / 3), new Point2D(4996, -4)));
            result.Add(new Segment2D(new Point2D(5000 + t / 4, t / 4), new Point2D(5004, 4)));

            // Segments short enough to be dropped by the trailing RemoveAll. (2)
            result.Add(new Segment2D(new Point2D(6000, 0), new Point2D(6000 + t / 10, 0)));
            result.Add(new Segment2D(new Point2D(-6000, 0), new Point2D(-6000 + t / 10, 0)));

            return result;
        }

        /// <summary>
        /// 70 segments spanning every category the indexed RemoveAlmostSimilar has to handle.
        /// </summary>
        private static List<Segment2D> RemoveAlmostSimilarFixture()
        {
            const double t = 1e-3;
            List<Segment2D> result = new List<Segment2D>();

            // Far-apart padding that must never interact. (32)
            for (int i = 0; i < 32; i++)
                result.Add(new Segment2D(new Point2D(100 * i, 0), new Point2D(100 * i + 1, 0)));

            // Exact value duplicates, distinct instances. (8)
            for (int i = 0; i < 4; i++)
            {
                result.Add(new Segment2D(new Point2D(i, 50), new Point2D(i + 1, 50)));
                result.Add(new Segment2D(new Point2D(i, 50), new Point2D(i + 1, 50)));
            }

            // Repeated instances: the same object twice. (6)
            for (int i = 0; i < 3; i++)
            {
                Segment2D repeated = new Segment2D(new Point2D(i, 60), new Point2D(i + 1, 60));
                result.Add(repeated);
                result.Add(repeated);
            }

            // Almost similar, inside tolerance. (6)
            for (int i = 0; i < 3; i++)
            {
                result.Add(new Segment2D(new Point2D(i, 70), new Point2D(i + 1, 70)));
                result.Add(new Segment2D(new Point2D(i + t / 4, 70), new Point2D(i + 1 - t / 4, 70)));
            }

            // Immediately beyond tolerance. (6)
            for (int i = 0; i < 3; i++)
            {
                result.Add(new Segment2D(new Point2D(i, 80), new Point2D(i + 1, 80)));
                result.Add(new Segment2D(new Point2D(i + t * 2, 80), new Point2D(i + 1 + t * 2, 80)));
            }

            // Reversed geometry: similar under the bidirectional predicate. (6)
            for (int i = 0; i < 3; i++)
            {
                result.Add(new Segment2D(new Point2D(i, 90), new Point2D(i + 1, 90)));
                result.Add(new Segment2D(new Point2D(i + 1, 90), new Point2D(i, 90)));
            }

            // Negative coordinates. (4)
            for (int i = 0; i < 2; i++)
            {
                result.Add(new Segment2D(new Point2D(-i - 1, -100), new Point2D(-i, -100)));
                result.Add(new Segment2D(new Point2D(-i - 1, -100), new Point2D(-i, -100)));
            }

            // Large coordinates. (2)
            result.Add(new Segment2D(new Point2D(1e6, 1e6), new Point2D(1e6 + 1, 1e6)));
            result.Add(new Segment2D(new Point2D(1e6, 1e6), new Point2D(1e6 + 1, 1e6)));

            return result;
        }

        /// <summary>
        /// 52 polylines: far-apart padding, value duplicates, repeated instances and
        /// almost-similar geometry. Keeps the generic path from being exercised only with the
        /// two-point degenerate case.
        /// </summary>
        private static List<Polyline2D> PolylineFixture()
        {
            const double t = 1e-3;
            List<Polyline2D> result = new List<Polyline2D>();

            for (int i = 0; i < 32; i++)
                result.Add(new Polyline2D(new List<Point2D> { new Point2D(100 * i, 0), new Point2D(100 * i + 1, 0), new Point2D(100 * i + 1, 1) }, false));

            for (int i = 0; i < 4; i++)
            {
                List<Point2D> points = new List<Point2D> { new Point2D(i, 50), new Point2D(i + 1, 50), new Point2D(i + 1, 51) };
                result.Add(new Polyline2D(points, false));
                result.Add(new Polyline2D(new List<Point2D>(points.ConvertAll(x => new Point2D(x))), false));
            }

            for (int i = 0; i < 3; i++)
            {
                Polyline2D repeated = new Polyline2D(new List<Point2D> { new Point2D(i, 60), new Point2D(i + 1, 60), new Point2D(i + 1, 61) }, false);
                result.Add(repeated);
                result.Add(repeated);
            }

            for (int i = 0; i < 3; i++)
            {
                result.Add(new Polyline2D(new List<Point2D> { new Point2D(i, 70), new Point2D(i + 1, 70), new Point2D(i + 1, 71) }, false));
                result.Add(new Polyline2D(new List<Point2D> { new Point2D(i + t / 4, 70), new Point2D(i + 1, 70), new Point2D(i + 1, 71 - t / 4) }, false));
            }

            return result;
        }

        /// <summary>
        /// Places <paramref name="first"/> and <paramref name="second"/> among far-apart padding
        /// that cannot interact with them or each other, at a total size past the indexed
        /// threshold.
        /// </summary>
        private static List<Segment2D> PaddedWith(Segment2D first, Segment2D second, int padding)
        {
            List<Segment2D> result = new List<Segment2D>();

            for (int i = 0; i < padding / 2; i++)
                result.Add(new Segment2D(new Point2D(100 * i + 1000, 0), new Point2D(100 * i + 1001, 0)));

            result.Add(first);

            for (int i = padding / 2; i < padding; i++)
                result.Add(new Segment2D(new Point2D(100 * i + 1000, 0), new Point2D(100 * i + 1001, 0)));

            result.Add(second);

            return result;
        }

        private static List<Segment2D> Clone(List<Segment2D> segment2Ds)
        {
            return segment2Ds.ConvertAll(x => new Segment2D(x));
        }

        private static List<Segment2D> Permute(List<Segment2D> segment2Ds, int seed)
        {
            List<Segment2D> result = new List<Segment2D>(segment2Ds);
            Random random = new Random(seed * 7919 + 13);
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Segment2D temp = result[i];
                result[i] = result[j];
                result[j] = temp;
            }

            return result;
        }

        private static BoundingBox2D Box(double minX, double minY, double maxX, double maxY)
        {
            return new BoundingBox2D(new Point2D(minX, minY), new Point2D(maxX, maxY));
        }

        // --- Assertions ---------------------------------------------------------------------------

        private static void AssertSegmentsIdentical(List<Segment2D> expected, List<Segment2D> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i][0].X, actual[i][0].X);
                Assert.Equal(expected[i][0].Y, actual[i][0].Y);
                Assert.Equal(expected[i][1].X, actual[i][1].X);
                Assert.Equal(expected[i][1].Y, actual[i][1].Y);
            }
        }

        private static void AssertSameInstancesInOrder<T>(List<T> expected, List<T> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
                Assert.Same(expected[i], actual[i]);
        }

        // --- Reflection accessors for the internal grids -------------------------------------------

        private static Type GeometryType(string name)
        {
            Type type = typeof(Point2D).Assembly.GetType("SAM.Geometry.Planar." + name);
            Assert.NotNull(type);
            return type;
        }

        private static object CreateBoundingBox2DGrid(double tolerance, double cellSizeHint)
        {
            return Activator.CreateInstance(GeometryType("BoundingBox2DGrid"), new object[] { tolerance, cellSizeHint });
        }

        private static object CreatePoint2DGrid(double tolerance)
        {
            return Activator.CreateInstance(GeometryType("Point2DGrid"), new object[] { tolerance });
        }

        private static int GridAdd(object grid, object item)
        {
            return (int)grid.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public).Invoke(grid, new object[] { item });
        }

        private static void GridReplace(object grid, int index, Point2D point2D)
        {
            grid.GetType().GetMethod("Replace", BindingFlags.Instance | BindingFlags.Public).Invoke(grid, new object[] { index, point2D });
        }

        private static List<int> GridCandidates(object grid, object query)
        {
            return (List<int>)grid.GetType().GetMethod("Candidates", BindingFlags.Instance | BindingFlags.Public).Invoke(grid, new object[] { query });
        }
    }
}
