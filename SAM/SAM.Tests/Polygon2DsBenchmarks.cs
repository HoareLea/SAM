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
    public class Polygon2DsBenchmarks
    {
        private readonly ITestOutputHelper _output;

        public Polygon2DsBenchmarks(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Benchmark_Polygon2Ds_Performance()
        {
            int[] counts = new[] { 10, 50, 200 };

            _output.WriteLine("--- Polygon2Ds Optimization Benchmark ---");

            foreach (int n in counts)
            {
                List<ISegmentable2D> polylines = new List<ISegmentable2D>(n);
                int cols = (int)System.Math.Ceiling(System.Math.Sqrt(n));
                for (int i = 0; i < n; i++)
                {
                    int r = i / cols;
                    int c = i % cols;
                    double x = c * 15.0;
                    double y = r * 15.0;

                    Polyline2D poly = new Polyline2D(new List<Point2D>
                    {
                        new Point2D(x, y),
                        new Point2D(x + 10, y),
                        new Point2D(x + 10, y + 10),
                        new Point2D(x, y + 10),
                        new Point2D(x, y)
                    });
                    polylines.Add(poly);
                }

                // Warmup
                for (int i = 0; i < 3; i++)
                {
                    polylines.Polygon2Ds();
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long startAlloc = GC.GetAllocatedBytesForCurrentThread();
                Stopwatch sw = Stopwatch.StartNew();
                int iterations = 10;

                List<Polygon2D> result = null;
                for (int i = 0; i < iterations; i++)
                {
                    result = polylines.Polygon2Ds();
                }

                sw.Stop();
                long endAlloc = GC.GetAllocatedBytesForCurrentThread();

                double avgTimeMs = sw.Elapsed.TotalMilliseconds / iterations;
                double avgAllocKb = (double)(endAlloc - startAlloc) / (1024.0 * iterations);

                _output.WriteLine($"Grid N={n,4}: Avg Time = {avgTimeMs,7:F4} ms | Avg Alloc = {avgAllocKb,7:F2} KB | Polygons Created = {result?.Count}");
            }
        }
    }
}
