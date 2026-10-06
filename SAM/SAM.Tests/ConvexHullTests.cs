// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using SAM.Geometry.Planar;
using Xunit;

namespace SAM.Tests
{
    public class ConvexHullTests
    {
        [Fact]
        public void ConvexHull_NullInput_ReturnsNull()
        {
            IEnumerable<Point2D> points = null;
            Assert.Null(points.ConvexHull());
        }

        [Fact]
        public void ConvexHull_EmptyInput_ReturnsEmpty()
        {
            List<Point2D> points = new List<Point2D>();
            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Empty(hull);
        }

        [Fact]
        public void ConvexHull_SinglePoint_ReturnsSinglePoint()
        {
            List<Point2D> points = new List<Point2D> { new Point2D(5, 5) };
            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Single(hull);
            Assert.Equal(5, hull[0].X);
            Assert.Equal(5, hull[0].Y);
        }

        [Fact]
        public void ConvexHull_TwoDistinctPoints_ReturnsTwoPoints()
        {
            List<Point2D> points = new List<Point2D> { new Point2D(0, 0), new Point2D(10, 5) };
            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Equal(2, hull.Count);
        }

        [Fact]
        public void ConvexHull_TwoIdenticalPoints_ReturnsSinglePoint()
        {
            List<Point2D> points = new List<Point2D> { new Point2D(3, 3), new Point2D(3, 3) };
            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Single(hull);
            Assert.Equal(3, hull[0].X);
            Assert.Equal(3, hull[0].Y);
        }

        [Fact]
        public void ConvexHull_ThreeCollinearPoints_ReturnsEndpointsOnly()
        {
            List<Point2D> points = new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(5, 5), // Middle point
                new Point2D(10, 10)
            };

            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Equal(2, hull.Count);
            Assert.Equal(0, hull[0].X);
            Assert.Equal(0, hull[0].Y);
            Assert.Equal(10, hull[1].X);
            Assert.Equal(10, hull[1].Y);
        }

        [Fact]
        public void ConvexHull_ThreeNonCollinearPoints_ReturnsThreePoints()
        {
            List<Point2D> points = new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(5, 10)
            };

            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Equal(3, hull.Count);
        }

        [Fact]
        public void ConvexHull_SquareWithInteriorPoints_ReturnsFourCornerPoints()
        {
            List<Point2D> points = new List<Point2D>
            {
                // Corners
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10),
                // Interior points
                new Point2D(5, 5),
                new Point2D(2, 3),
                new Point2D(8, 2),
                new Point2D(3, 7),
                new Point2D(9, 9)
            };

            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Equal(4, hull.Count);

            HashSet<(double, double)> expectedCorners = new HashSet<(double, double)>
            {
                (0, 0), (10, 0), (10, 10), (0, 10)
            };

            foreach (Point2D p in hull)
            {
                Assert.Contains((p.X, p.Y), expectedCorners);
            }
        }

        [Fact]
        public void ConvexHull_LargePointCloud_FiltersInteriorPointsCorrectly()
        {
            Random rng = new Random(42);
            int n = 10000;
            List<Point2D> points = new List<Point2D>(n + 4)
            {
                // Known extreme corners
                new Point2D(0, 0),
                new Point2D(1000, 0),
                new Point2D(1000, 1000),
                new Point2D(0, 1000)
            };

            for (int i = 0; i < n; i++)
            {
                double x = 1.0 + rng.NextDouble() * 998.0;
                double y = 1.0 + rng.NextDouble() * 998.0;
                points.Add(new Point2D(x, y));
            }

            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.True(hull.Count >= 4);

            HashSet<(double, double)> cornerSet = new HashSet<(double, double)>
            {
                (0, 0), (1000, 0), (1000, 1000), (0, 1000)
            };

            int cornersFound = 0;
            foreach (Point2D p in hull)
            {
                if (cornerSet.Contains((p.X, p.Y))) cornersFound++;
            }
            Assert.Equal(4, cornersFound);
        }

        [Fact]
        public void ConvexHull_DenseGrid_ReturnsExactlyFourCorners()
        {
            // Filled square grid (well above the N >= 16 Akl-Toussaint threshold).
            // Every edge is dense with collinear points, so the hull must reduce to
            // exactly the four corners once interior and collinear points are removed.
            List<Point2D> points = new List<Point2D>();
            for (int x = 0; x <= 20; x++)
            {
                for (int y = 0; y <= 20; y++)
                {
                    points.Add(new Point2D(x, y));
                }
            }

            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Equal(4, hull.Count);

            HashSet<(double, double)> hullSet = new HashSet<(double, double)>();
            foreach (Point2D p in hull)
            {
                hullSet.Add((p.X, p.Y));
            }
            Assert.Contains((0.0, 0.0), hullSet);
            Assert.Contains((20.0, 0.0), hullSet);
            Assert.Contains((20.0, 20.0), hullSet);
            Assert.Contains((0.0, 20.0), hullSet);
        }

        [Fact]
        public void ConvexHull_TriangularExtremeCloud_KeepsTriangleVertices()
        {
            // Extreme points collapse to a triangle (qCount == 3 filter branch). All
            // added points are strictly inside the triangle and must be discarded,
            // leaving exactly the three extreme vertices.
            List<Point2D> points = new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(100, 0),
                new Point2D(50, 100)
            };

            Random rng = new Random(7);
            while (points.Count < 200)
            {
                double a = rng.NextDouble();
                double b = rng.NextDouble();
                if (a + b >= 1.0) continue; // reject points outside the triangle
                double x = a * 100 + b * 50;
                double y = b * 100;
                points.Add(new Point2D(x, y));
            }

            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Equal(3, hull.Count);

            HashSet<(double, double)> hullSet = new HashSet<(double, double)>();
            foreach (Point2D p in hull)
            {
                hullSet.Add((p.X, p.Y));
            }
            Assert.Contains((0.0, 0.0), hullSet);
            Assert.Contains((100.0, 0.0), hullSet);
            Assert.Contains((50.0, 100.0), hullSet);
        }

        [Fact]
        public void ConvexHull_CollinearEdgePoints_BelowFilterThreshold_ReturnsCornersOnly()
        {
            // 8 points (< 16, so the Akl-Toussaint pre-filter is skipped and the general
            // Monotone Chain path runs): four square corners plus a collinear midpoint on
            // each edge. The midpoints must be dropped, leaving exactly the four corners.
            List<Point2D> points = new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10),
                new Point2D(5, 0),
                new Point2D(10, 5),
                new Point2D(5, 10),
                new Point2D(0, 5)
            };

            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Equal(4, hull.Count);

            HashSet<(double, double)> hullSet = new HashSet<(double, double)>();
            foreach (Point2D p in hull)
            {
                hullSet.Add((p.X, p.Y));
            }
            Assert.Contains((0.0, 0.0), hullSet);
            Assert.Contains((10.0, 0.0), hullSet);
            Assert.Contains((10.0, 10.0), hullSet);
            Assert.Contains((0.0, 10.0), hullSet);
        }

        [Fact]
        public void ConvexHull_Segment2DOverload_NullInput_ReturnsNull()
        {
            IEnumerable<Segment2D> segments = null;
            Assert.Null(segments.ConvexHull());
        }

        [Fact]
        public void ConvexHull_ISegmentable2DOverload_NullInput_ReturnsNull()
        {
            ISegmentable2D segmentable = null;
            Assert.Null(segmentable.ConvexHull());
        }

        [Fact]
        public void ConvexHull_Segment2DOverload_ReturnsCorrectHull()
        {
            List<Segment2D> segments = new List<Segment2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(10, 0)),
                new Segment2D(new Point2D(10, 0), new Point2D(10, 10)),
                new Segment2D(new Point2D(10, 10), new Point2D(0, 10)),
                new Segment2D(new Point2D(0, 10), new Point2D(0, 0))
            };

            List<Point2D> hull = Query.ConvexHull(segments);
            Assert.NotNull(hull);
            Assert.Equal(4, hull.Count);
        }

        [Fact]
        public void ConvexHull_ISegmentable2DOverload_ReturnsCorrectHull()
        {
            Polygon2D polygon = new Polygon2D(new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 20),
                new Point2D(0, 20)
            });

            ISegmentable2D segmentable = polygon;
            List<Point2D> hull = Query.ConvexHull(segmentable);
            Assert.NotNull(hull);
            Assert.Equal(4, hull.Count);

            HashSet<(double, double)> expectedCorners = new HashSet<(double, double)>
            {
                (0, 0), (10, 0), (10, 20), (0, 20)
            };

            foreach (Point2D p in hull)
            {
                Assert.Contains((p.X, p.Y), expectedCorners);
            }
        }

        [Fact]
        public void ConvexHull_TriangleBoundingPointCloud_FiltersInteriorPointsCorrectly()
        {
            // Point set where minX and minY coincide at (0,0), creating a qCount == 3 extreme triangle
            List<Point2D> points = new List<Point2D>(20)
            {
                new Point2D(0, 0),    // minX and minY
                new Point2D(100, 0),  // maxX
                new Point2D(0, 100),  // maxY
            };

            // Add interior points strictly inside the triangle
            for (int i = 1; i <= 17; i++)
            {
                points.Add(new Point2D(i * 2, i * 2));
            }

            List<Point2D> hull = points.ConvexHull();
            Assert.NotNull(hull);
            Assert.Equal(3, hull.Count);
            HashSet<(double, double)> expectedCorners = new HashSet<(double, double)>
            {
                (0, 0), (100, 0), (0, 100)
            };
            foreach (Point2D p in hull)
            {
                Assert.Contains((p.X, p.Y), expectedCorners);
            }
        }
    }
}
