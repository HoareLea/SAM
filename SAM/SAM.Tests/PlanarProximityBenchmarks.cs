// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    /// <summary>
    /// Scaling benchmarks for the planar proximity scans indexed by the planar-proximity
    /// branch. Each benchmark runs at N, 2N and 4N so the growth exponent can be read from
    /// the output. Guards are correctness-only; there are no wall-clock thresholds.
    /// </summary>
    [Trait("Category", "Benchmark")]
    public class PlanarProximityBenchmarks
    {
        private readonly ITestOutputHelper _output;

        public PlanarProximityBenchmarks(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Snap_Segments_Scaling()
        {
            Report("Query.Snap(segments, averaging)", new int[] { 1000, 2000, 4000 }, count =>
            {
                List<Segment2D> segments = ChainedSegments(count);
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<Segment2D> result = Geometry.Planar.Query.Snap(segments, true, 1e-3);
                stopwatch.Stop();
                Assert.NotNull(result);
                return stopwatch.Elapsed.TotalMilliseconds;
            });
        }

        [Fact]
        public void RemoveAlmostSimilar_Generic_Scaling()
        {
            Report("Modify.RemoveAlmostSimilar<T>", new int[] { 500, 1000, 2000 }, count =>
            {
                List<Segment2D> segments = NearDuplicateSegments(count);
                Stopwatch stopwatch = Stopwatch.StartNew();
                Geometry.Planar.Modify.RemoveAlmostSimilar(segments, 1e-3);
                stopwatch.Stop();
                Assert.NotEmpty(segments);
                return stopwatch.Elapsed.TotalMilliseconds;
            });
        }

        /// <summary>
        /// Dense case: every horizontal segment crosses every vertical one, so the number of
        /// intersections is itself quadratic in n. No broad phase can beat that - the pairs it
        /// would discard are exactly the pairs that do not exist here. Kept as the control: it
        /// shows the broad phase does not make the output-bound case worse.
        /// </summary>
        [Fact]
        public void Split_Segments_Dense_Scaling()
        {
            Report("Query.Split(segments, dense crossing grid)", new int[] { 300, 600, 1200 }, count =>
            {
                List<Segment2D> segments = PlanarSplitFixtures.CrossingGrid(count);
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<Segment2D> result = Geometry.Planar.Query.Split(segments, Core.Tolerance.Distance);
                stopwatch.Stop();
                Assert.NotNull(result);
                return stopwatch.Elapsed.TotalMilliseconds;
            });
        }

        /// <summary>
        /// Sparse case: n/2 isolated crossing pairs, so n/2 intersections - linear output from
        /// a quadratic number of pairs. The pair sweep is the only quadratic term left, so this
        /// is where a broad phase shows, and it is the shape real floor-plan input takes: many
        /// short walls, each meeting a handful of neighbours. The sizes are large enough for the
        /// quadratic term to dominate; below about a thousand segments the linear result-building
        /// work hides it and every column reads ~2x whether or not the sweep is indexed.
        /// </summary>
        [Fact]
        public void Split_Segments_Sparse_Scaling()
        {
            Report("Query.Split(segments, sparse crossings)", new int[] { 2000, 4000, 8000 }, count =>
            {
                List<Segment2D> segments = PlanarSplitFixtures.SparseCrossings(count);
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<Segment2D> result = Geometry.Planar.Query.Split(segments, Core.Tolerance.Distance);
                stopwatch.Stop();
                Assert.NotNull(result);
                return stopwatch.Elapsed.TotalMilliseconds;
            });
        }

        /// <summary>
        /// Clustered case: locally dense bundles that never reach each other. Output is still
        /// linear in n (a fixed intersection count per bundle), but unlike the sparse fixture
        /// each accepted pair does real geometric work, so the broad phase has to shed the
        /// cross-bundle pairs without disturbing the within-bundle ones.
        /// </summary>
        [Fact]
        public void Split_Segments_Clustered_Scaling()
        {
            Report("Query.Split(segments, clustered crossings)", new int[] { 1000, 2000, 4000 }, count =>
            {
                List<Segment2D> segments = PlanarSplitFixtures.ClusteredCrossings(count);
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<Segment2D> result = Geometry.Planar.Query.Split(segments, Core.Tolerance.Distance);
                stopwatch.Stop();
                Assert.NotNull(result);
                return stopwatch.Elapsed.TotalMilliseconds;
            });
        }

        // --- Fixtures -------------------------------------------------------------------

        /// <summary>Segment chains whose joints are just inside the snap tolerance, on a lattice.</summary>
        private static List<Segment2D> ChainedSegments(int count)
        {
            int columns = (int)System.Math.Ceiling(System.Math.Sqrt(count));
            List<Segment2D> result = new List<Segment2D>();
            for (int i = 0; i < count; i++)
            {
                double x = (i % columns) * 10;
                double y = (i / columns) * 10;
                result.Add(new Segment2D(new Point2D(x, y), new Point2D(x + 4, y)));
                result.Add(new Segment2D(new Point2D(x + 4.0005, y), new Point2D(x + 8, y)));
                i++;
            }

            return result;
        }

        /// <summary>About one in three segments is a near-duplicate of a lattice sibling.</summary>
        private static List<Segment2D> NearDuplicateSegments(int count)
        {
            int columns = (int)System.Math.Ceiling(System.Math.Sqrt(count));
            List<Segment2D> result = new List<Segment2D>();
            for (int i = 0; i < count; i++)
            {
                double x = (i % columns) * 5;
                double y = (i / columns) * 5;
                double offset = (i % 3 == 0) ? 0.0004 : 0;
                result.Add(new Segment2D(new Point2D(x + offset, y + offset), new Point2D(x + 4 + offset, y + offset)));
            }

            return result;
        }

        private void Report(string name, int[] counts, System.Func<int, double> run)
        {
            // Warm up so JIT and first-touch allocation do not land in the smallest sample.
            run(counts[0]);

            List<double> times = new List<double>();
            foreach (int count in counts)
            {
                times.Add(run(count));
            }

            _output.WriteLine(name);
            for (int i = 0; i < counts.Length; i++)
            {
                string growth = i == 0 ? "-" : (times[i] / times[i - 1]).ToString("0.00") + "x";
                _output.WriteLine(string.Format("  n={0,-6} {1,10:0.0} ms   growth {2}", counts[i], times[i], growth));
            }
        }
    }
}
