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
    /// Tests for the separated-set invariant that makes snap candidate filtering exact in the
    /// panel merging workflows: the snap set is de-duplicated with the same tolerance Snap
    /// uses, so retained points are pairwise farther apart than tolerance and the greedy Snap
    /// can make at most one hop per source vertex. The generic Snap hazard (multi-hop walks on
    /// non-separated sets) is documented separately in QuadraticScanRegressionTests.
    /// The grid helper is private; it is exercised by reflection rather than by weakening
    /// production encapsulation.
    /// </summary>
    public class SnapGridInvariantTests
    {
        // --- Invariant -----------------------------------------------------------------------

        [Fact]
        public void Point2DGrid_Deduplication_MatchesLegacyAddAndSeparates()
        {
            List<Point2D> all = new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(0, 0),                    // exact duplicate
                new Point2D(0.0005, 0),               // within tolerance of (0,0)
                new Point2D(0.001, 0),                // exactly at tolerance of (0,0)
                new Point2D(-0.0015, 0),              // beyond tolerance of all previous
                new Point2D(1e6, -1e6),
                new Point2D(1e6 + 0.0004, -1e6),
                null,
            };

            double tolerance = 1e-3;

            List<Point2D> legacy = new List<Point2D>();
            foreach (Point2D point2D in all)
                Modify.Add(legacy, point2D, tolerance);

            List<Point2D> retained = GridPoints(CreateGrid(all, tolerance));

            // Same members, same order, same instances as the legacy rule.
            Assert.Equal(legacy.Count, retained.Count);
            for (int i = 0; i < legacy.Count; i++)
                Assert.Same(legacy[i], retained[i]);

            // Pairwise strictly farther apart than tolerance (the separated-set invariant).
            for (int i = 0; i < retained.Count; i++)
                for (int j = i + 1; j < retained.Count; j++)
                    Assert.True(retained[i].Distance(retained[j]) > tolerance, $"points {i} and {j} violate separation");
        }

        [Fact]
        public void Point2DGrid_Candidates_PreserveInsertionOrder()
        {
            List<Point2D> all = SeparatedSet(new Random(3), 50, 0, 0, 5.0, 1e-3);
            object grid = CreateGrid(all, 1e-3);
            List<Point2D> retained = GridPoints(grid);

            // A box over the whole cloud is a proportionate scan, so this is the filtering
            // path rather than the fallback: it walks the cells and still returns every
            // retained point, in insertion order.
            List<Point2D> candidates = GridCandidates(grid, new BoundingBox2D(new Point2D(0, 0), new Point2D(5, 5)));

            Assert.Equal(retained.Count, candidates.Count);
            for (int i = 0; i < retained.Count; i++)
                Assert.Same(retained[i], candidates[i]);

            // The same grid genuinely filters when the query is narrow - the returned set is
            // a strict subsequence, so filtering never reorders what survives.
            List<Point2D> candidates_Narrow = GridCandidates(grid, new BoundingBox2D(new Point2D(0, 0), new Point2D(0.5, 0.5)));

            Assert.NotEmpty(candidates_Narrow);
            Assert.True(candidates_Narrow.Count < retained.Count, "a narrow query must not return the complete retained set");
            AssertSubsequence(retained, candidates_Narrow);
        }

        [Fact]
        public void Point2DGrid_Candidates_IncludeToleranceBoundaryPoints()
        {
            double tolerance = 0.1;
            List<Point2D> all = new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(0.15, 0),                 // pairwise separated (> 0.1)
                new Point2D(1.1, 0),                  // exactly bbox.Max.X + tolerance
                new Point2D(1.1001, 0),               // just outside bbox + tolerance
                new Point2D(5, 5),
            };

            object grid = CreateGrid(all, tolerance);
            List<Point2D> retained = GridPoints(grid);
            List<Point2D> candidates = GridCandidates(grid, new BoundingBox2D(new Point2D(0, 0), new Point2D(1, 1)));

            Assert.Contains(candidates, x => x.X == 1.1);
            Assert.Contains(candidates, x => x.X == 0.15);

            // ...and the query really did filter rather than hand back everything: the far
            // point is retained by de-duplication but excluded from this box.
            Assert.Contains(retained, x => x.X == 5);
            Assert.DoesNotContain(candidates, x => x.X == 5);
            Assert.True(candidates.Count < retained.Count, "the query must exclude the far point");
        }

        // --- Bounded scanning -----------------------------------------------------------------

        [Fact]
        public void Point2DGrid_Candidates_DisproportionateScan_FallsBackToCompleteSet()
        {
            double tolerance = 1e-3;

            // An ordinary finite query box. At the minimum cell size a grid holding almost
            // nothing quantises this to ~4e18 cells - representable, so no range guard catches
            // it, and one dictionary probe each. Every case below must return promptly.
            BoundingBox2D boundingBox2D = new BoundingBox2D(new Point2D(-1e6, -1e6), new Point2D(1e6, 1e6));

            // Empty grid.
            object grid_Empty = CreateGrid(new List<Point2D>(), tolerance);
            Assert.Empty(GridCandidates(grid_Empty, boundingBox2D));

            // Single point: the complete retained set, same instance.
            object grid_Single = CreateGrid(new List<Point2D> { new Point2D(0.25, -0.5) }, tolerance);
            List<Point2D> retained_Single = GridPoints(grid_Single);
            List<Point2D> candidates_Single = GridCandidates(grid_Single, boundingBox2D);

            Assert.Single(candidates_Single);
            Assert.Same(retained_Single[0], candidates_Single[0]);

            // Several points, still far too few to justify the scan: the fallback set is
            // complete and in insertion order.
            List<Point2D> all = SeparatedSet(new Random(11), 40, 0, 0, 2.0, tolerance);
            object grid = CreateGrid(all, tolerance);
            List<Point2D> retained = GridPoints(grid);
            List<Point2D> candidates = GridCandidates(grid, boundingBox2D);

            Assert.Equal(retained.Count, candidates.Count);
            for (int i = 0; i < retained.Count; i++)
                Assert.Same(retained[i], candidates[i]);

            // Same grid, proportionate query: filtering is still in force, so the budget guard
            // has not simply disabled the index.
            List<Point2D> candidates_Proportionate = GridCandidates(grid, new BoundingBox2D(new Point2D(0, 0), new Point2D(0.4, 0.4)));

            Assert.True(candidates_Proportionate.Count < retained.Count, "a proportionate query must still filter");
            AssertSubsequence(retained, candidates_Proportionate);
        }

        [Fact]
        public void Point2DGrid_Candidates_ExtremeButFiniteBounds_FallBackWithoutOverflow()
        {
            double tolerance = 1e-3;
            List<Point2D> all = new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(1, 0),
                new Point2D(0, 1),
            };

            object grid = CreateGrid(all, tolerance);
            List<Point2D> retained = GridPoints(grid);

            // Finite bounds spanning only a few cells, but quantised to indexes far outside
            // long range. The cell budget cannot catch this - the span is three - so only the
            // index-magnitude guard stands between the loop and an unspecified double-to-long
            // conversion whose result varies by runtime and architecture.
            foreach (double origin in new double[] { 1e300, -1e300 })
            {
                BoundingBox2D boundingBox2D = new BoundingBox2D(new Point2D(origin, origin), new Point2D(origin + 1, origin + 1));
                List<Point2D> candidates = GridCandidates(grid, boundingBox2D);

                Assert.Equal(retained.Count, candidates.Count);
                for (int i = 0; i < retained.Count; i++)
                    Assert.Same(retained[i], candidates[i]);
            }
        }

        // --- Snap semantics -------------------------------------------------------------------

        [Fact]
        public void Snap_SeparatedCandidates_KeepsFirstAcceptedNotNearest()
        {
            double tolerance = 0.1;

            // Both candidates are within tolerance of the source, and they are 0.15 apart -
            // more than tolerance - so this is a legitimate separated set and de-duplication
            // keeps both.
            List<Point2D> all = new List<Point2D>
            {
                new Point2D(0.09, 0),
                new Point2D(-0.06, 0),
            };

            object grid = CreateGrid(all, tolerance);
            List<Point2D> retained = GridPoints(grid);
            Assert.Equal(2, retained.Count);

            List<Point2D> source = new List<Point2D> { new Point2D(0, 0) };
            List<Point2D> filtered = GridCandidates(grid, new BoundingBox2D(new Point2D(0, 0), new Point2D(0, 0)));

            List<Point2D> result_Full = Query.Snap(source, retained, tolerance);
            List<Point2D> result_Filtered = Query.Snap(source, filtered, tolerance);

            // (0.09, 0) is accepted first; (-0.06, 0) is then measured against the moved point,
            // 0.15 away, and rejected. So the first accepted candidate stands even though
            // (-0.06, 0) is the nearer of the two to the original source. Collection order plus
            // strict distance improvement decides this, not geometric nearness - pinned here
            // because a candidate filter that reordered the set would silently change it.
            Assert.Equal(0.09, result_Full[0].X, 12);
            Assert.Equal(0, result_Full[0].Y, 12);

            Assert.Equal(result_Full.Count, result_Filtered.Count);
            Assert.Equal(result_Full[0].X, result_Filtered[0].X);
            Assert.Equal(result_Full[0].Y, result_Filtered[0].Y);
        }

        [Fact]
        public void Snap_SeparatedSet_FullAndFilteredProduceIdenticalOutput()
        {
            Face2D face2D = Square(0, 0, 1);
            double tolerance = 0.1;
            List<Point2D> all = new List<Point2D>
            {
                new Point2D(0.05, 0),                 // within tolerance of vertex (0,0)
                new Point2D(0.2, 0),                  // separated from the first
                new Point2D(1.05, 0),                 // near vertex (1,0)
                new Point2D(-0.5, -0.5),
                new Point2D(3, 3),
            };

            object grid = CreateGrid(all, tolerance);
            List<Point2D> full = GridPoints(grid);
            List<Point2D> filtered = GridCandidates(grid, face2D.GetBoundingBox());

            Face2D result_Full = Query.Snap(face2D, full, tolerance);
            Face2D result_Filtered = Query.Snap(face2D, filtered, tolerance);

            AssertIdentical(result_Full, result_Filtered);
        }

        [Fact]
        public void Snap_SeparatedSet_DeterministicPropertySweep()
        {
            double[] tolerances = new double[] { 1e-3, 1e-6 };
            double[] origins = new double[] { 0, -1000, 1e6, -1e9 };

            int scenarios = 0;
            foreach (double tolerance in tolerances)
            {
                foreach (double origin in origins)
                {
                    for (int seed = 0; seed < 10; seed++)
                    {
                        Random random = new Random(seed * 977 + origin.GetHashCode() + tolerance.GetHashCode());

                        // Separated snap sets: scattered, clustered near the face, and aligned
                        // to tolerance multiples (cell boundaries).
                        List<Point2D> all = SeparatedSet(random, 30, origin, origin, 4.0, tolerance);
                        if (seed % 3 == 0)
                            all.AddRange(SeparatedSet(random, 15, origin + 0.5, origin + 0.5, 0.5, tolerance, all));
                        if (seed % 4 == 0)
                        {
                            double bx = origin + random.Next(5) * 10 * tolerance;
                            double by = origin + random.Next(5) * 10 * tolerance;
                            if (Far(all, bx, by, tolerance))
                                all.Add(new Point2D(bx, by));
                        }

                        Face2D face2D = Square(origin, origin, 1);
                        if (seed % 2 == 0)
                        {
                            // duplicate source vertices
                            List<Point2D> points = ((Polygon2D)face2D.ExternalEdge2D).Points;
                            points.Insert(1, new Point2D(points[0]));
                            face2D = new Face2D(new Polygon2D(points));
                        }

                        object grid = CreateGrid(all, tolerance);
                        List<Point2D> full = GridPoints(grid);
                        List<Point2D> filtered = GridCandidates(grid, face2D.GetBoundingBox());

                        Face2D result_Full = Query.Snap(face2D, full, tolerance);
                        Face2D result_Filtered = Query.Snap(face2D, filtered, tolerance);

                        AssertIdentical(result_Full, result_Filtered);
                        scenarios++;
                    }
                }
            }

            Assert.Equal(80, scenarios);
        }

        [Fact]
        public void Snap_SeparatedSet_ZeroAndNegativeTolerance_Identical()
        {
            foreach (double tolerance in new double[] { 0, -1e-3 })
            {
                Face2D face2D = Square(0, 0, 1);
                List<Point2D> all = new List<Point2D>
                {
                    new Point2D(0.05, 0),
                    new Point2D(0.2, 0),
                    new Point2D(0.05, 0),             // exact duplicate
                    new Point2D(-1, 0),
                };

                object grid = CreateGrid(all, tolerance);
                List<Point2D> full = GridPoints(grid);
                List<Point2D> filtered = GridCandidates(grid, face2D.GetBoundingBox());

                Face2D result_Full = Query.Snap(face2D, full, tolerance);
                Face2D result_Filtered = Query.Snap(face2D, filtered, tolerance);

                AssertIdentical(result_Full, result_Filtered);
            }
        }

        [Fact]
        public void Point2DGrid_DegenerateInputs_FallBackToCompleteSet()
        {
            List<Point2D> all = new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(0, 0),
                new Point2D(0.05, 0),
                new Point2D(1, 1),
            };

            BoundingBox2D boundingBox2D = new BoundingBox2D(new Point2D(0, 0), new Point2D(1, 1));

            // NaN tolerance: nothing is rejected at de-duplication, the set is not separated,
            // so every query must return the complete retained set.
            object grid_NaN = CreateGrid(all, double.NaN);
            List<Point2D> retained_NaN = GridPoints(grid_NaN);
            List<Point2D> candidates_NaN = GridCandidates(grid_NaN, boundingBox2D);
            Assert.Equal(retained_NaN.Count, candidates_NaN.Count);

            // Infinite tolerance.
            object grid_Inf = CreateGrid(all, double.PositiveInfinity);
            Assert.Equal(GridPoints(grid_Inf).Count, GridCandidates(grid_Inf, boundingBox2D).Count);

            // Null and non-finite query bounds.
            object grid = CreateGrid(all, 1e-3);
            Assert.Equal(GridPoints(grid).Count, GridCandidates(grid, null).Count);
            Assert.Equal(GridPoints(grid).Count, GridCandidates(grid, new BoundingBox2D(new Point2D(double.NaN, 0), new Point2D(1, 1))).Count);
            Assert.Equal(GridPoints(grid).Count, GridCandidates(grid, new BoundingBox2D(new Point2D(0, 0), new Point2D(double.PositiveInfinity, 1))).Count);
        }

        [Fact]
        public void Point2DGrid_AdversarialChain_DedupRemovesSecondHopPrecondition()
        {
            // Raw, non-separated set: the greedy Snap walks through the intermediate point to
            // the far point (documented generic-method hazard).
            Face2D face2D = Square(0, 0, 1);
            List<Point2D> chain = new List<Point2D> { new Point2D(-0.09, 0), new Point2D(-0.15, 0) };

            Face2D raw = Query.Snap(face2D, chain, 0.1);
            Assert.Contains(((Polygon2D)raw.ExternalEdge2D).Points, x => System.Math.Abs(x.X - (-0.15)) < 1e-9);

            // After de-duplication with the same tolerance the intermediate point survives
            // (it is first), the far point is rejected, and full vs filtered agree on the
            // single legitimate hop.
            object grid = CreateGrid(chain, 0.1);
            List<Point2D> retained = GridPoints(grid);
            Assert.Single(retained);
            Assert.Equal(-0.09, retained[0].X, 12);

            Face2D result_Full = Query.Snap(face2D, retained, 0.1);
            Face2D result_Filtered = Query.Snap(face2D, GridCandidates(grid, face2D.GetBoundingBox()), 0.1);
            AssertIdentical(result_Full, result_Filtered);
            Assert.Contains(((Polygon2D)result_Full.ExternalEdge2D).Points, x => System.Math.Abs(x.X - (-0.09)) < 1e-9);
        }

        // --- Helpers --------------------------------------------------------------------------

        private static Face2D Square(double x, double y, double size)
        {
            return new Face2D(new Polygon2D(new List<Point2D>
            {
                new Point2D(x, y),
                new Point2D(x + size, y),
                new Point2D(x + size, y + size),
                new Point2D(x, y + size),
            }));
        }

        /// <summary>
        /// Builds a pairwise tolerance-separated set by rejection sampling, optionally
        /// respecting an already populated set.
        /// </summary>
        private static List<Point2D> SeparatedSet(Random random, int count, double originX, double originY, double spread, double tolerance, List<Point2D> existing = null)
        {
            List<Point2D> result = existing ?? new List<Point2D>();
            List<Point2D> added = new List<Point2D>();

            int attempts = 0;
            while (added.Count < count && attempts < count * 200)
            {
                attempts++;
                double x = originX + random.NextDouble() * spread;
                double y = originY + random.NextDouble() * spread;
                if (!Far(result, x, y, tolerance))
                    continue;

                Point2D point2D = new Point2D(x, y);
                result.Add(point2D);
                added.Add(point2D);
            }

            return existing == null ? result : added;
        }

        private static bool Far(List<Point2D> points, double x, double y, double tolerance)
        {
            Point2D candidate = new Point2D(x, y);
            foreach (Point2D point2D in points)
                if (point2D.Distance(candidate) <= tolerance)
                    return false;

            return true;
        }

        /// <summary>
        /// Asserts that <paramref name="subset"/> is the retained list with members removed -
        /// same instances, same relative order, nothing invented.
        /// </summary>
        private static void AssertSubsequence(List<Point2D> retained, List<Point2D> subset)
        {
            int i = 0;
            foreach (Point2D point2D in subset)
            {
                while (i < retained.Count && !ReferenceEquals(retained[i], point2D))
                    i++;

                Assert.True(i < retained.Count, "candidates are not a subsequence of the retained set");
                i++;
            }
        }

        private static void AssertIdentical(Face2D face2D_1, Face2D face2D_2)
        {
            List<Point2D> points_1 = ((Polygon2D)face2D_1.ExternalEdge2D).Points;
            List<Point2D> points_2 = ((Polygon2D)face2D_2.ExternalEdge2D).Points;

            Assert.Equal(points_1.Count, points_2.Count);
            for (int i = 0; i < points_1.Count; i++)
            {
                Assert.Equal(points_1[i].X, points_2[i].X);
                Assert.Equal(points_1[i].Y, points_2[i].Y);
            }
        }

        private static object CreateGrid(List<Point2D> point2Ds_All, double tolerance)
        {
            Type type = GridType();
            return type.GetMethod("Create").Invoke(null, new object[] { point2Ds_All, tolerance });
        }

        private static List<Point2D> GridPoints(object grid)
        {
            return (List<Point2D>)grid.GetType().GetProperty("Points").GetValue(grid);
        }

        private static List<Point2D> GridCandidates(object grid, BoundingBox2D boundingBox2D)
        {
            return (List<Point2D>)grid.GetType().GetMethod("Candidates").Invoke(grid, new object[] { boundingBox2D });
        }

        private static Type GridType()
        {
            Type type = typeof(SAM.Analytical.Query).GetNestedType("Point2DGrid", BindingFlags.NonPublic);
            Assert.NotNull(type);
            return type;
        }
    }
}
