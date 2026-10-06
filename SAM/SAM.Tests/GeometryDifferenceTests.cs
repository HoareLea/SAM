// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Planar;
using Xunit;

namespace SAM.Tests
{
    public class GeometryDifferenceTests
    {
        // Regression test for a bug where the Parallel.For branch of
        // Difference(this IEnumerable<Polygon2D>, IEnumerable<Polygon2D>, double) used
        // polygon2Ds_1.ElementAt(0) instead of the loop index i, so every parallel
        // iteration differenced the *first* polygon against polygon2Ds_2 instead of the
        // i-th polygon. The parallel branch only engages once count >= 10 (or
        // polygon2Ds_2.Count() >= 10), so this test builds a list of 12 distinct,
        // non-overlapping polygons (well above the threshold) and checks that the
        // per-polygon ("per-slot") results line up with polygon2Ds_1[i], not
        // polygon2Ds_1[0].
        [Fact]
        public void Difference_Polygon2DList_ParallelBranch_UsesPerIndexPolygon()
        {
            double tolerance = SAM.Core.Tolerance.MicroDistance;

            const int count = 12; // >= parallel-branch threshold (10)
            const double gap = 10.0;

            // 12 distinct, non-overlapping polygons at different positions/shapes.
            List<Polygon2D> polygon2Ds_1 = new List<Polygon2D>();
            for (int i = 0; i < count; i++)
            {
                double x0 = i * gap;
                double width = 2.0 + (i % 3); // vary shape slightly across indices
                polygon2Ds_1.Add(Rectangle(x0, 0, width, 2.0));
            }

            // polygon2Ds_2 fully removes polygon2Ds_1[3] and removes the left half of polygon2Ds_1[7].
            // polygon2Ds_1[0] (used by the buggy code for every iteration) is left completely untouched,
            // so a bug that always differences against polygon2Ds_1[0] is easy to detect.
            List<Polygon2D> polygon2Ds_2 = new List<Polygon2D>()
            {
                Rectangle(3 * gap, 0, 2.0 + (3 % 3), 2.0), // exact match with polygon2Ds_1[3]
                Rectangle(7 * gap, 0, (2.0 + (7 % 3)) / 2.0, 2.0), // left half of polygon2Ds_1[7]
            };

            // Oracle: per-index expected result using the single-polygon overload, which is
            // unaffected by the bug (shared by both the count==1 and count<10 sequential branches).
            List<List<Polygon2D>> expectedPerIndex = new List<List<Polygon2D>>();
            for (int i = 0; i < count; i++)
            {
                List<Polygon2D> difference = polygon2Ds_1[i]?.Difference(polygon2Ds_2, tolerance);
                expectedPerIndex.Add(difference ?? new List<Polygon2D>());
            }

            // Sanity-check the oracle/test construction itself before trusting it as ground truth.
            Assert.Empty(expectedPerIndex[3]); // fully removed
            Assert.NotEmpty(expectedPerIndex[7]); // partially removed
            Assert.True(expectedPerIndex[7].Sum(x => x.GetArea()) < polygon2Ds_1[7].GetArea() - tolerance);
            for (int i = 0; i < count; i++)
            {
                if (i == 3 || i == 7)
                {
                    continue;
                }

                // untouched indices should come back unchanged (single polygon, same area)
                Assert.Single(expectedPerIndex[i]);
                Assert.True(System.Math.Abs(expectedPerIndex[i][0].GetArea() - polygon2Ds_1[i].GetArea()) < tolerance);
            }

            // Actual: goes through the count >= 10 Parallel.For branch under test.
            List<Polygon2D> actual = polygon2Ds_1.Difference(polygon2Ds_2, tolerance);

            Assert.NotNull(actual);

            int expectedTotalCount = expectedPerIndex.Sum(x => x.Count);
            Assert.Equal(expectedTotalCount, actual.Count);

            double expectedTotalArea = expectedPerIndex.SelectMany(x => x).Sum(x => x.GetArea());
            double actualTotalArea = actual.Sum(x => x.GetArea());
            Assert.True(System.Math.Abs(expectedTotalArea - actualTotalArea) < 1e-6, $"Expected total area {expectedTotalArea}, actual {actualTotalArea}");

            // Per-index ("per-slot") verification: bucket the flattened actual results by which
            // original polygon's x-range their centroid falls into, and compare the bucketed area
            // against the oracle's per-index area. With the ElementAt(0) bug, every iteration
            // differences polygon2Ds_1[0] (untouched by polygon2Ds_2), so every result polygon would
            // land in slot 0 and every other slot would come back empty - this loop catches that.
            for (int i = 0; i < count; i++)
            {
                double width = 2.0 + (i % 3);
                double xMin = i * gap - tolerance;
                double xMax = i * gap + width + tolerance;

                double actualAreaForSlot = actual
                    .Where(x =>
                    {
                        double centroidX = x.GetCentroid().X;
                        return centroidX >= xMin && centroidX <= xMax;
                    })
                    .Sum(x => x.GetArea());

                double expectedAreaForSlot = expectedPerIndex[i].Sum(x => x.GetArea());

                Assert.True(System.Math.Abs(actualAreaForSlot - expectedAreaForSlot) < 1e-6, $"Mismatch at index {i}: expected area {expectedAreaForSlot}, actual {actualAreaForSlot}");
            }
        }

        private static Polygon2D Rectangle(double x, double y, double width, double height)
        {
            List<Point2D> point2Ds = new List<Point2D>()
            {
                new Point2D(x, y),
                new Point2D(x + width, y),
                new Point2D(x + width, y + height),
                new Point2D(x, y + height),
            };

            return new Polygon2D(point2Ds);
        }
    }
}
