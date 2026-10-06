// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Tests
{
    /// <summary>
    /// Segment layouts for <c>Query.Split(IEnumerable&lt;Segment2D&gt;)</c>, shared by the
    /// scaling benchmarks and the A/B equivalence tests.
    /// <para>
    /// The three shapes differ in how the <em>output</em> grows, which is what decides whether
    /// a broad phase over the pair sweep can help at all. Dense: intersections grow as n^2, so
    /// the sweep is output-bound and no filter can win. Sparse and clustered: intersections
    /// grow linearly, so all but O(n) of the n^2 pairs are dead weight and removing them is the
    /// whole cost. Everything is laid out on a fixed lattice - no randomness - so a run at 2n
    /// is a superset-shaped version of the run at n.
    /// </para>
    /// </summary>
    internal static class PlanarSplitFixtures
    {
        /// <summary>
        /// Half horizontal, half vertical segments spanning one grid: every horizontal segment
        /// crosses every vertical one, so the intersection count is (n/2)^2. Quadratic output.
        /// </summary>
        public static List<Segment2D> CrossingGrid(int count)
        {
            int half = count / 2;
            List<Segment2D> result = new List<Segment2D>();
            for (int i = 0; i < half; i++)
            {
                result.Add(new Segment2D(new Point2D(0, i * 1.0), new Point2D(half, i * 1.0)));
                result.Add(new Segment2D(new Point2D(i * 1.0, 0), new Point2D(i * 1.0, half)));
            }

            return result;
        }

        /// <summary>
        /// n/2 isolated crossing pairs on a lattice, each an X spanning 4 units inside a 10-unit
        /// cell, so a pair can never reach any other pair. One intersection per pair: linear
        /// output, quadratic pair count.
        /// </summary>
        public static List<Segment2D> SparseCrossings(int count, double size = 4, double spacing = 10)
        {
            int pairs = count / 2;
            int columns = (int)System.Math.Ceiling(System.Math.Sqrt(pairs));

            List<Segment2D> result = new List<Segment2D>();
            for (int i = 0; i < pairs; i++)
            {
                double x = (i % columns) * spacing;
                double y = (i / columns) * spacing;

                result.Add(new Segment2D(new Point2D(x, y), new Point2D(x + size, y + size)));
                result.Add(new Segment2D(new Point2D(x, y + size), new Point2D(x + size, y)));
            }

            return result;
        }

        /// <summary>
        /// Locally dense bundles - a small crossing grid of <paramref name="perCluster"/>
        /// segments - scattered far enough apart that no two bundles interact. Intersections per
        /// bundle are fixed, so the output is linear in n while every one of the many
        /// cross-bundle pairs is work the sweep has to shed.
        /// </summary>
        public static List<Segment2D> ClusteredCrossings(int count, int perCluster = 16, double spacing = 100)
        {
            int clusters = System.Math.Max(1, count / perCluster);
            int columns = (int)System.Math.Ceiling(System.Math.Sqrt(clusters));
            int half = System.Math.Max(1, perCluster / 2);

            List<Segment2D> result = new List<Segment2D>();
            for (int i = 0; i < clusters; i++)
            {
                double x = (i % columns) * spacing;
                double y = (i / columns) * spacing;

                for (int j = 0; j < half; j++)
                {
                    result.Add(new Segment2D(new Point2D(x, y + j), new Point2D(x + half, y + j)));
                    result.Add(new Segment2D(new Point2D(x + j, y), new Point2D(x + j, y + half)));
                }
            }

            return result;
        }
    }
}
