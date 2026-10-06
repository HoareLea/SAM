// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    [Trait("Category", "Benchmark")]
    public class MapPanelsBenchmarks
    {
        private readonly ITestOutputHelper _output;

        public MapPanelsBenchmarks(ITestOutputHelper output)
        {
            _output = output;
        }

        private static Panel CreateSquarePanel(double x, double z, double size, string name)
        {
            var p0 = new Point3D(x, 0, z);
            var p1 = new Point3D(x + size, 0, z);
            var p2 = new Point3D(x + size, 0, z + size);
            var p3 = new Point3D(x, 0, z + size);
            var face = new Face3D(new Polygon3D(new[] { p0, p1, p2, p3 }));
            return SAM.Analytical.Create.Panel(new Construction(name), PanelType.Wall, face);
        }

        private static (AdjacencyCluster cluster, List<Panel> sourcePanels) BuildModel(int count, double offset, double spacing)
        {
            var construction = new Construction("Bench");
            var cluster = new AdjacencyCluster();
            int cols = (int)System.Math.Ceiling(System.Math.Sqrt(count));
            for (int i = 0; i < count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                double x = col * spacing + offset;
                double z = row * spacing + offset;
                cluster.AddObject(CreateSquarePanel(x, z, 3.5, $"D{i}"));
            }

            var sourcePanels = new List<Panel>();
            for (int i = 0; i < count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                double x = col * spacing + 1.0;
                double z = row * spacing + 1.0;
                sourcePanels.Add(CreateSquarePanel(x, z, 3.5, $"S{i}"));
            }

            return (cluster, sourcePanels);
        }

        private static double RunBenchmark(AdjacencyCluster cluster, List<Panel> sourcePanels, int warmupRuns, int measuredRuns)
        {
            for (int i = 0; i < warmupRuns; i++)
            {
                var other = BuildModel(sourcePanels.Count, 0, 4.0).cluster;
                other.MapPanels(BuildModel(sourcePanels.Count, 1, 4.0).sourcePanels);
                other = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var times = new List<double>();
            for (int i = 0; i < measuredRuns; i++)
            {
                // Fresh cluster each run to avoid cumulative mutation
                var (freshCluster, _) = BuildModel(sourcePanels.Count, 0, 4.0);

                var sw = Stopwatch.StartNew();
                freshCluster.MapPanels(sourcePanels, 0.1, 0.5);
                sw.Stop();

                times.Add(sw.Elapsed.TotalMilliseconds);
            }

            // Return median
            times.Sort();
            return times[times.Count / 2];
        }

        [Fact]
        public void MapPanels_SpatiallySeparated_Optimized()
        {
            // Source and dest panels are offset by 1m — destination internal points are inside source face boxes
            // The expanded BB will include nearly all dest points within maxDistance (0.5m).
            // This is the "best case" for the prefilter — most candidates pass BB and proceed to exact check.
            foreach (int n in new[] { 100, 500, 1000, 2500, 5000 })
            {
                var (cluster, sourcePanels) = BuildModel(n, 0, 4.0);
                double ms = RunBenchmark(cluster, sourcePanels, 2, 3);
                _output.WriteLine($"  N={n,4}: {ms,8:F2} ms  (optimized)");
            }
        }

        [Fact]
        public void MapPanels_WorstCase_DenseNearby_Optimized()
        {
            // Very dense panels — every dest is close to every source.
            // BB prefilter adds near-zero filtering benefit, but adds BB computation cost.
            // This validates the prefilter isn't slower on worst-case inputs.
            foreach (int n in new[] { 100, 200, 400 })
            {
                var (cluster, sourcePanels) = BuildModel(n, 0, 0.5);
                double ms = RunBenchmark(cluster, sourcePanels, 2, 3);
                _output.WriteLine($"  N={n,4}: {ms,8:F2} ms  (optimized, dense)");
            }
        }
    }
}
