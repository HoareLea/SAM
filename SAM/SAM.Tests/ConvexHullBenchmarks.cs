// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SAM.Geometry.Planar;
using Xunit;
using Xunit.Abstractions;

namespace SAM.Tests
{
    [Trait("Category", "Benchmark")]
    public class ConvexHullBenchmarks
    {
        private readonly ITestOutputHelper _output;

        public ConvexHullBenchmarks(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Benchmark_ConvexHull_Performance()
        {
            int[] sizes = new[] { 100, 1000, 10000, 100000 };
            Random rng = new Random(42);

            _output.WriteLine("--- ConvexHull Optimization Benchmark ---");

            foreach (int n in sizes)
            {
                List<Point2D> points = new List<Point2D>(n);
                for (int i = 0; i < n; i++)
                {
                    points.Add(new Point2D(rng.NextDouble() * 1000.0, rng.NextDouble() * 1000.0));
                }

                // Warmup
                for (int i = 0; i < 5; i++)
                {
                    points.ConvexHull();
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long startAlloc = GC.GetAllocatedBytesForCurrentThread();
                Stopwatch sw = Stopwatch.StartNew();
                int iterations = n >= 100000 ? 10 : 100;

                List<Point2D> hull = null;
                for (int i = 0; i < iterations; i++)
                {
                    hull = points.ConvexHull();
                }

                sw.Stop();
                long endAlloc = GC.GetAllocatedBytesForCurrentThread();

                double avgTimeMs = sw.Elapsed.TotalMilliseconds / iterations;
                double avgAllocKb = (double)(endAlloc - startAlloc) / (1024.0 * iterations);

                _output.WriteLine($"N={n,6}: Avg Time = {avgTimeMs,7:F4} ms | Avg Alloc = {avgAllocKb,7:F2} KB | Hull Vertices = {hull?.Count}");
            }
        }
    }
}
