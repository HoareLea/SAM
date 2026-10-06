// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// A/B coverage for the broad phase over the segment-pair intersection sweep in
    /// <c>Query.Split(IEnumerable&lt;Segment2D&gt;)</c>.
    /// <para>
    /// The sweep used to compare every pair of segments, guarded only by an InRange test on
    /// their bounding boxes. It is now driven by a BoundingBox2DGrid that offers a superset of
    /// the boxes that test can accept; InRange, and then the exact predicates (On, Intersection,
    /// AlmostSimilar), still decide every pair. That makes the broad phase invisible by
    /// construction - which is only worth anything if it is actually checked, so every fixture
    /// here is built past the 32-segment threshold at which production switches the index on,
    /// and every result is compared against a fully exhaustive implementation held verbatim in
    /// this file. Comparing production against itself would prove nothing.
    /// </para>
    /// <para>
    /// The reference is deliberately older than the PR base rather than equal to it: it is Split
    /// as it stood before <em>any</em> of the indexing work on this branch, so it is exhaustive
    /// in the point de-duplication and the result de-duplication as well as in the pair sweep.
    /// The broad phase is what these tests are about, but holding the whole method to the
    /// pre-index behaviour re-checks the two indexed paths that landed ahead of it at the same
    /// time, at no extra cost.
    /// </para>
    /// <para>
    /// The comparison is exact: piece count, piece coordinates, piece order, and the pattern of
    /// Point2D instances shared between pieces. That last one is the sharpest of the four - it
    /// is what the accumulation order into <c>point2DsList[i]</c> controls, so a broad phase
    /// that offered candidates out of order, or twice, would move it even where the coordinates
    /// happened to survive.
    /// </para>
    /// </summary>
    public class PlanarSplitBroadPhaseTests
    {
        /// <summary>Segment count at which Split switches from the exhaustive sweep to the grid.</summary>
        private const int IndexedThreshold = 32;

        /// <summary>
        /// Tolerances every fixture is run at. The negative and NaN entries are not decoration:
        /// a negative tolerance shrinks the inserted boxes instead of inflating them, which
        /// collapses the span of any axis-aligned segment and sends it to the grid's aside set,
        /// and a NaN tolerance makes InRange accept everything and the grid fall back to its
        /// whole index. Both have to reproduce the exhaustive sweep exactly.
        /// </summary>
        private static readonly double[] Tolerances = new double[]
        {
            Core.Tolerance.Distance, 1e-3, 1e-2, 1e-6, 0.0, -1e-3, -1.0, double.NaN,
        };

        // --- Benchmark shapes: indexed vs exhaustive -------------------------------------------

        /// <summary>
        /// The sparse shape the broad phase exists for: linear intersection count, quadratic
        /// pair count. If the grid ever dropped a pair that intersects, this is where it shows.
        /// </summary>
        [Fact]
        public void Split_SparseCrossings_IndexedMatchesExhaustive()
        {
            AssertMatchesExhaustive(PlanarSplitFixtures.SparseCrossings(64));
        }

        /// <summary>
        /// Locally dense bundles: within a bundle every pair is a real hit, between bundles none
        /// is. The grid has to shed only the second kind.
        /// </summary>
        [Fact]
        public void Split_ClusteredCrossings_IndexedMatchesExhaustive()
        {
            AssertMatchesExhaustive(PlanarSplitFixtures.ClusteredCrossings(64));
        }

        /// <summary>
        /// The dense control. Every pair intersects, so the grid must offer effectively
        /// everything - the case where a broad phase can only do harm.
        /// </summary>
        [Fact]
        public void Split_DenseCrossingGrid_IndexedMatchesExhaustive()
        {
            AssertMatchesExhaustive(PlanarSplitFixtures.CrossingGrid(40));
        }

        // --- Mixed fixture: indexed vs exhaustive ----------------------------------------------

        /// <summary>
        /// One fixture spanning every category the sweep has to get right at once, so the
        /// categories interact rather than being checked in isolation.
        /// </summary>
        [Fact]
        public void Split_MixedFixture_IndexedMatchesExhaustive()
        {
            AssertMatchesExhaustive(MixedFixture());
        }

        /// <summary>
        /// Split's output depends on input order - the sweep visits pairs in index order and
        /// <c>point2DsList[i]</c> accumulates in it - so permuting the input must move both
        /// implementations the same way, not neither. A broad phase that quietly reordered
        /// candidates would pass a single fixed ordering and fail here.
        /// </summary>
        [Fact]
        public void Split_MixedFixture_IndexedMatchesExhaustive_UnderInputPermutation()
        {
            List<Segment2D> fixture = MixedFixture();

            for (int seed = 0; seed < 5; seed++)
            {
                AssertMatchesExhaustive(Permute(fixture, seed));
            }
        }

        // --- Grid fallbacks ---------------------------------------------------------------------

        /// <summary>
        /// A segment far longer than the median spans more cells than the grid will register,
        /// so it is held aside and offered to every query. It crosses everything, so if the
        /// aside set were ever skipped the result would collapse - this fixture cannot pass by
        /// accident.
        /// </summary>
        [Fact]
        public void Split_SpanningSegmentBeyondCellBudget_IndexedMatchesExhaustive()
        {
            List<Segment2D> fixture = new List<Segment2D>();

            // 48 unit segments on a wide lattice: the median extent, and so the cell size, is 1.
            for (int i = 0; i < 48; i++)
                fixture.Add(new Segment2D(new Point2D(100 * i, 0), new Point2D(100 * i + 1, 1)));

            // Two segments spanning the whole lattice diagonally. Their boxes cover roughly
            // 4700 x 4700 cells, far past the per-item cell budget, so they cannot be placed.
            fixture.Add(new Segment2D(new Point2D(-10, -10), new Point2D(4800, 4800)));
            fixture.Add(new Segment2D(new Point2D(-10, 4800), new Point2D(4800, -10)));

            Assert.True(fixture.Count > IndexedThreshold);

            AssertMatchesExhaustive(fixture);
        }

        /// <summary>
        /// Non-finite coordinates cannot be quantised at all. The exhaustive sweep put them
        /// through the exact predicates like anything else and so must the indexed one, however
        /// little sense the geometry makes.
        /// </summary>
        [Fact]
        public void Split_NonFiniteCoordinates_IndexedMatchesExhaustive()
        {
            List<Segment2D> fixture = new List<Segment2D>();

            for (int i = 0; i < 40; i++)
                fixture.Add(new Segment2D(new Point2D(i, 0), new Point2D(i + 1, 1)));

            fixture.Add(new Segment2D(new Point2D(double.NaN, 0), new Point2D(1, 1)));
            fixture.Add(new Segment2D(new Point2D(double.PositiveInfinity, 0), new Point2D(1, 1)));
            fixture.Add(new Segment2D(new Point2D(0, double.NegativeInfinity), new Point2D(1, 1)));

            Assert.True(fixture.Count > IndexedThreshold);

            AssertMatchesExhaustive(fixture);
        }

        /// <summary>
        /// Extreme but finite coordinates alongside ordinary ones. The tiny segments hold the
        /// cell size near the tolerance floor, which is what pushes the far-away boxes out of
        /// representable cell-index range and into the aside set.
        /// </summary>
        [Fact]
        public void Split_ExtremeFiniteCoordinates_IndexedMatchesExhaustive()
        {
            List<Segment2D> fixture = new List<Segment2D>();

            for (int i = 0; i < 40; i++)
                fixture.Add(new Segment2D(new Point2D(i * 1e-4, 0), new Point2D(i * 1e-4 + 4e-5, 4e-5)));

            // Crossing pair far enough out that its cell indexes leave long range.
            fixture.Add(new Segment2D(new Point2D(1e15, 1e15), new Point2D(1e15 + 1, 1e15 + 1)));
            fixture.Add(new Segment2D(new Point2D(1e15, 1e15 + 1), new Point2D(1e15 + 1, 1e15)));

            Assert.True(fixture.Count > IndexedThreshold);

            AssertMatchesExhaustive(fixture);
        }

        // --- Threshold guard ---------------------------------------------------------------------

        /// <summary>
        /// Guards the guard. Every comparison above is only worth running if the fixture reaches
        /// the indexed path and the sweep actually splits something; if the threshold moved, or a
        /// fixture shrank, they would silently degrade to testing the fallback against itself.
        /// </summary>
        [Fact]
        public void Split_FixturesReachIndexedPathAndSplit()
        {
            foreach (List<Segment2D> fixture in new List<Segment2D>[]
            {
                PlanarSplitFixtures.SparseCrossings(64),
                PlanarSplitFixtures.ClusteredCrossings(64),
                PlanarSplitFixtures.CrossingGrid(40),
                MixedFixture(),
            })
            {
                Assert.True(fixture.Count > IndexedThreshold, $"fixture must cross the indexed threshold, was {fixture.Count}");

                List<Segment2D> result = Query.Split(fixture, Core.Tolerance.Distance);
                Assert.True(result.Count > fixture.Count, "the fixture must contain segments that actually split");
            }
        }

        // --- Exhaustive reference implementation ---------------------------------------------------

        /// <summary>
        /// Verbatim <c>Query.Split(IEnumerable&lt;Segment2D&gt;, double)</c> as it stood before any
        /// indexing work on this branch: the full pairwise sweep with no broad phase, the linear
        /// point de-duplication scans and the linear result scans.
        /// </summary>
        private static List<Segment2D> Split_Exhaustive(IEnumerable<Segment2D> segment2Ds, double tolerance)
        {
            if (segment2Ds == null)
                return null;

            List<Tuple<BoundingBox2D, Segment2D>> tuples = new List<Tuple<BoundingBox2D, Segment2D>>();
            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null || segment2D.GetLength() < tolerance)
                {
                    continue;
                }

                tuples.Add(new Tuple<BoundingBox2D, Segment2D>(segment2D.GetBoundingBox(), segment2D));
                Modify.Add(point2Ds, segment2D[0], tolerance);
                Modify.Add(point2Ds, segment2D[1], tolerance);
            }

            int count = tuples.Count;

            List<List<Point2D>> point2DsList = new List<List<Point2D>>();
            for (int i = 0; i < count; i++)
                point2DsList.Add(null);

            for (int i = 0; i < count - 1; i++)
            {
                BoundingBox2D boundingBox2D_1 = tuples[i].Item1;
                Segment2D segment2D_1 = tuples[i].Item2;

                for (int j = i + 1; j < count; j++)
                {
                    BoundingBox2D boundingBox2D_2 = tuples[j].Item1;
                    if (!boundingBox2D_1.InRange(boundingBox2D_2, tolerance))
                    {
                        continue;
                    }

                    Segment2D segment2D_2 = tuples[j].Item2;
                    if (segment2D_1.AlmostSimilar(segment2D_2, tolerance))
                    {
                        continue;
                    }

                    Point2D point2D_Closest1;
                    Point2D point2D_Closest2;

                    List<Point2D> point2Ds_Intersection = new List<Point2D>();

                    if (segment2D_1.On(segment2D_2[0], tolerance))
                        point2Ds_Intersection.Add(segment2D_2[0]);

                    if (segment2D_2.On(segment2D_1[0], tolerance))
                        point2Ds_Intersection.Add(segment2D_1[0]);

                    if (segment2D_1.On(segment2D_2[1], tolerance))
                        point2Ds_Intersection.Add(segment2D_2[1]);

                    if (segment2D_2.On(segment2D_1[1], tolerance))
                        point2Ds_Intersection.Add(segment2D_1[1]);

                    if (point2Ds_Intersection.Count == 0)
                    {
                        Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, out point2D_Closest1, out point2D_Closest2, tolerance);
                        if (point2D_Intersection == null || point2D_Intersection.IsNaN())
                            continue;

                        if (point2D_Closest1 != null && point2D_Closest2 != null)
                            if (point2D_Closest1.Distance(point2D_Closest2) > tolerance)
                                continue;

                        point2Ds_Intersection.Add(point2D_Intersection);
                    }

                    if (point2Ds_Intersection == null || point2Ds_Intersection.Count == 0)
                    {
                        continue;
                    }

                    foreach (Point2D point2D_Intersection in point2Ds_Intersection)
                    {
                        Point2D point2D_Intersection_Temp = point2Ds.Find(x => point2D_Intersection.AlmostEquals(x, tolerance));
                        if (point2D_Intersection_Temp == null)
                        {
                            point2D_Intersection_Temp = point2D_Intersection;
                            Modify.Add(point2Ds, point2D_Intersection_Temp, tolerance);
                        }

                        if (point2D_Intersection_Temp.Distance(segment2D_1.Start) > tolerance && point2D_Intersection_Temp.Distance(segment2D_1.End) > tolerance)
                        {
                            if (point2DsList[i] == null)
                            {
                                point2DsList[i] = new List<Point2D>();
                            }

                            Modify.Add(point2DsList[i], point2D_Intersection_Temp, tolerance);
                        }

                        if (point2D_Intersection_Temp.Distance(segment2D_2.Start) > tolerance && point2D_Intersection_Temp.Distance(segment2D_2.End) > tolerance)
                        {
                            if (point2DsList[j] == null)
                            {
                                point2DsList[j] = new List<Point2D>();
                            }

                            Modify.Add(point2DsList[j], point2D_Intersection_Temp, tolerance);
                        }
                    }
                }
            }

            List<Segment2D> result = new List<Segment2D>();
            for (int i = 0; i < count; i++)
            {
                Segment2D segment2D_Temp = tuples[i].Item2;
                if (result.Find(x => x.AlmostSimilar(segment2D_Temp, tolerance)) != null)
                    continue;

                List<Point2D> point2Ds_Temp = point2DsList[i];
                if (point2Ds_Temp == null || point2Ds_Temp.Count == 0)
                {
                    result.Add(segment2D_Temp);
                    continue;
                }

                Modify.Add(point2Ds_Temp, segment2D_Temp[0], tolerance);
                Modify.Add(point2Ds_Temp, segment2D_Temp[1], tolerance);

                Modify.SortByDistance(point2Ds_Temp, segment2D_Temp[0]);

                for (int j = 0; j < point2Ds_Temp.Count - 1; j++)
                {
                    Point2D point2D_1 = point2Ds_Temp[j];
                    Point2D point2D_2 = point2Ds_Temp[j + 1];

                    Segment2D segment2D = result.Find(x => (x[0].AlmostEquals(point2D_1, tolerance) && x[1].AlmostEquals(point2D_2, tolerance)) || (x[1].AlmostEquals(point2D_1, tolerance) && x[0].AlmostEquals(point2D_2, tolerance)));
                    if (segment2D != null)
                        continue;

                    result.Add(new Segment2D(point2D_1, point2D_2));
                }
            }

            return result;
        }

        // --- Fixtures -----------------------------------------------------------------------------

        /// <summary>
        /// 66 segments spanning every category the sweep has to handle: isolated segments that
        /// must never pair, clean crossings, T-junctions where an endpoint lands exactly on
        /// another segment, collinear overlaps that the On tests pick up rather than
        /// Intersection, exact and reversed duplicates that AlmostSimilar rejects, shared
        /// Point2D instances, near misses placed exactly at and immediately either side of the
        /// nominal tolerance, negative coordinates, and large coordinates.
        /// </summary>
        private static List<Segment2D> MixedFixture()
        {
            const double t = 1e-3;
            List<Segment2D> result = new List<Segment2D>();

            // Isolated: far apart, no pair ever in range. (20)
            for (int i = 0; i < 20; i++)
                result.Add(new Segment2D(new Point2D(50 * i, 0), new Point2D(50 * i + 4, 0)));

            // Clean crossings: one interior intersection each. (8)
            for (int i = 0; i < 4; i++)
            {
                double x = 1000 + 20 * i;
                result.Add(new Segment2D(new Point2D(x, 0), new Point2D(x + 8, 8)));
                result.Add(new Segment2D(new Point2D(x, 8), new Point2D(x + 8, 0)));
            }

            // T-junctions: an endpoint exactly on another segment's interior. (8)
            for (int i = 0; i < 4; i++)
            {
                double x = 2000 + 20 * i;
                result.Add(new Segment2D(new Point2D(x, 0), new Point2D(x + 8, 0)));
                result.Add(new Segment2D(new Point2D(x + 4, 0), new Point2D(x + 4, 6)));
            }

            // Collinear overlaps: both endpoints of one lie on the other. (6)
            for (int i = 0; i < 3; i++)
            {
                double x = 3000 + 20 * i;
                result.Add(new Segment2D(new Point2D(x, 0), new Point2D(x + 10, 0)));
                result.Add(new Segment2D(new Point2D(x + 3, 0), new Point2D(x + 7, 0)));
            }

            // Exact duplicates and reversed duplicates: AlmostSimilar rejects the pair. (8)
            for (int i = 0; i < 2; i++)
            {
                double x = 4000 + 20 * i;
                result.Add(new Segment2D(new Point2D(x, 0), new Point2D(x + 5, 0)));
                result.Add(new Segment2D(new Point2D(x, 0), new Point2D(x + 5, 0)));
                result.Add(new Segment2D(new Point2D(x, 3), new Point2D(x + 5, 3)));
                result.Add(new Segment2D(new Point2D(x + 5, 3), new Point2D(x, 3)));
            }

            // Shared Point2D instances: the same object on two segments. (6)
            for (int i = 0; i < 3; i++)
            {
                Point2D shared = new Point2D(5000 + 20 * i, 0);
                result.Add(new Segment2D(new Point2D(5000 + 20 * i - 4, 0), shared));
                result.Add(new Segment2D(shared, new Point2D(5000 + 20 * i, 4)));
            }

            // Near misses exactly at the tolerance, and immediately either side of it. (6)
            double[] gaps = new double[] { t, t * (1 - 1e-12), t * (1 + 1e-12) };
            for (int i = 0; i < gaps.Length; i++)
            {
                double x = 6000 + 20 * i;
                result.Add(new Segment2D(new Point2D(x, 0), new Point2D(x + 6, 0)));
                result.Add(new Segment2D(new Point2D(x + 3, gaps[i]), new Point2D(x + 3, 6)));
            }

            // Negative coordinates, crossing. (2)
            result.Add(new Segment2D(new Point2D(-100, -100), new Point2D(-92, -92)));
            result.Add(new Segment2D(new Point2D(-100, -92), new Point2D(-92, -100)));

            // Large coordinates, crossing. (2)
            result.Add(new Segment2D(new Point2D(1e6, 1e6), new Point2D(1e6 + 8, 1e6 + 8)));
            result.Add(new Segment2D(new Point2D(1e6, 1e6 + 8), new Point2D(1e6 + 8, 1e6)));

            return result;
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

        // --- Assertions ---------------------------------------------------------------------------

        private static void AssertMatchesExhaustive(List<Segment2D> fixture)
        {
            Assert.True(fixture.Count > IndexedThreshold, $"fixture must cross the indexed threshold, was {fixture.Count}");

            foreach (double tolerance in Tolerances)
            {
                Exception exception_Indexed;
                Exception exception_Exhaustive;

                List<Segment2D> indexed = Outcome(() => Query.Split(fixture, tolerance), out exception_Indexed);
                List<Segment2D> exhaustive = Outcome(() => Split_Exhaustive(fixture, tolerance), out exception_Exhaustive);

                // Throwing is an outcome the two implementations have to share. A NaN tolerance
                // reaches Core.Query.Round, which guards the value it rounds against NaN but not
                // the tolerance it rounds to, and (decimal)double.NaN overflows. That predates the
                // index; what matters here is that the sweep still gets far enough to hit it.
                Assert.True(exception_Exhaustive?.GetType() == exception_Indexed?.GetType(),
                    $"outcome differs at tolerance {tolerance}: expected {Text(exception_Exhaustive)}, was {Text(exception_Indexed)}");

                if (exception_Exhaustive != null)
                {
                    continue;
                }

                AssertSplitIdentical(exhaustive, indexed, tolerance);
            }
        }

        private static List<Segment2D> Outcome(Func<List<Segment2D>> func, out Exception exception)
        {
            exception = null;

            try
            {
                return func();
            }
            catch (Exception exception_Temp)
            {
                exception = exception_Temp;
                return null;
            }
        }

        private static void AssertSplitIdentical(List<Segment2D> expected, List<Segment2D> actual, double tolerance)
        {
            Assert.True(expected.Count == actual.Count, $"piece count differs at tolerance {tolerance}: {expected.Count} vs {actual.Count}");

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.True(Same(expected[i][0], actual[i][0]) && Same(expected[i][1], actual[i][1]),
                    $"piece {i} differs at tolerance {tolerance}: expected {Text(expected[i])}, was {Text(actual[i])}");
            }

            // Which output endpoints are the same Point2D object. The two implementations build
            // their own instances, so the instances cannot be compared across them - the pattern
            // of sharing can, and it is exactly what the de-duplication and the order pieces were
            // accumulated in decide.
            Assert.Equal(SharingPattern(expected), SharingPattern(actual));
        }

        /// <summary>
        /// For every endpoint of every piece in order, the position of the first endpoint that is
        /// the same Point2D instance. Two runs that de-duplicated identically produce the same
        /// sequence whatever objects they allocated.
        /// </summary>
        private static List<int> SharingPattern(List<Segment2D> segment2Ds)
        {
            List<Point2D> seen = new List<Point2D>();
            List<int> result = new List<int>();

            foreach (Segment2D segment2D in segment2Ds)
            {
                for (int i = 0; i < 2; i++)
                {
                    Point2D point2D = segment2D[i];

                    int index = seen.FindIndex(x => ReferenceEquals(x, point2D));
                    if (index < 0)
                    {
                        index = seen.Count;
                        seen.Add(point2D);
                    }

                    result.Add(index);
                }
            }

            return result;
        }

        private static bool Same(Point2D point2D_1, Point2D point2D_2)
        {
            return point2D_1.X.Equals(point2D_2.X) && point2D_1.Y.Equals(point2D_2.Y);
        }

        private static string Text(Segment2D segment2D)
        {
            return $"({segment2D[0].X}, {segment2D[0].Y})-({segment2D[1].X}, {segment2D[1].Y})";
        }

        private static string Text(Exception exception)
        {
            return exception == null ? "a result" : exception.GetType().Name;
        }
    }
}
