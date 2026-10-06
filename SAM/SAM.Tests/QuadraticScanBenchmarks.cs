// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    /// <summary>
    /// Scaling benchmarks for the pairwise scans identified by the quadratic-scan audit.
    /// Each benchmark runs at N, 2N and 4N so the observed growth exponent can be read
    /// directly from the output. They are assertions-light on purpose - the guards are
    /// generous so the suite stays green on slow machines, the value is the printed table.
    /// </summary>
    [Trait("Category", "Benchmark")]
    public class QuadraticScanBenchmarks
    {
        private readonly ITestOutputHelper _output;

        public QuadraticScanBenchmarks(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SplitFace3Ds_Shells_Scaling()
        {
            Report("Modify.SplitFace3Ds(List<Shell>)", new int[] { 1000, 2000, 4000 }, count =>
            {
                List<Shell> shells = QuadraticScanFixtures.DisjointBoxShells(count);
                Stopwatch stopwatch = Stopwatch.StartNew();
                shells.SplitFace3Ds(Core.Tolerance.MacroDistance, Core.Tolerance.Angle, Core.Tolerance.Distance);
                stopwatch.Stop();
                return stopwatch.Elapsed.TotalMilliseconds;
            });
        }

        [Fact]
        public void CreateAdjacencyCluster_ByShells_Scaling()
        {
            Report("Create.AdjacencyCluster(shells, spaces)", new int[] { 300, 600, 1200 }, count =>
            {
                List<Shell> shells = QuadraticScanFixtures.DisjointBoxShells(count);
                Stopwatch stopwatch = Stopwatch.StartNew();
                AdjacencyCluster adjacencyCluster = Analytical.Create.AdjacencyCluster(shells, null);
                stopwatch.Stop();
                Assert.NotNull(adjacencyCluster);
                return stopwatch.Elapsed.TotalMilliseconds;
            });
        }

        [Fact]
        public void MergeOverlapPanels_Vertical_Scaling()
        {
            Report("Query.MergeOverlapPanels(walls)", new int[] { 1000, 2000, 4000 }, count =>
            {
                List<Panel> panels = QuadraticScanFixtures.DisjointWallPanels(count);
                List<Panel> redundantPanels = new List<Panel>();
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<Panel> result = Analytical.Query.MergeOverlapPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.Distance);
                stopwatch.Stop();
                Assert.NotNull(result);
                return stopwatch.Elapsed.TotalMilliseconds;
            });
        }

        [Fact]
        public void MergeOverlapPanels_Horizontal_Scaling()
        {
            Report("Query.MergeOverlapPanels(floors)", new int[] { 500, 1000, 2000 }, count =>
            {
                List<Panel> panels = QuadraticScanFixtures.CoplanarFloorPanels(count);
                List<Panel> redundantPanels = new List<Panel>();
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<Panel> result = Analytical.Query.MergeOverlapPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.Distance);
                stopwatch.Stop();
                Assert.NotNull(result);
                return stopwatch.Elapsed.TotalMilliseconds;
            });
        }

        [Fact]
        public void MergeCoplanarPanels_Scaling()
        {
            Report("Query.MergeCoplanarPanels(floors)", new int[] { 500, 1000, 2000 }, count =>
            {
                List<Panel> panels = QuadraticScanFixtures.CoplanarFloorPanels(count);
                List<Panel> redundantPanels = new List<Panel>();
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<Panel> result = Analytical.Query.MergeCoplanarPanels(panels, 0.1, ref redundantPanels, false, Core.Tolerance.MacroDistance, Core.Tolerance.Distance);
                stopwatch.Stop();
                Assert.NotNull(result);
                return stopwatch.Elapsed.TotalMilliseconds;
            });
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
