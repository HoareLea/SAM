// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using SAM.Geometry.Planar;
using Xunit;

namespace SAM.Tests
{
    public class Polygon2DsTests
    {
        [Fact]
        public void Polygon2Ds_NullInput_ReturnsNull()
        {
            IEnumerable<ISegmentable2D> input = null;
            Assert.Null(input.Polygon2Ds());
        }

        [Fact]
        public void Polygon2Ds_EmptyInput_ReturnsEmptyList()
        {
            List<ISegmentable2D> input = new List<ISegmentable2D>();
            List<Polygon2D> result = input.Polygon2Ds();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Polygon2Ds_SingleClosedPolyline_ReturnsOnePolygon()
        {
            Polyline2D polyline = new Polyline2D(new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10),
                new Point2D(0, 0)
            });

            List<Polygon2D> result = new List<ISegmentable2D> { polyline }.Polygon2Ds();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(System.Math.Abs(result[0].GetArea() - 100.0) < Core.Tolerance.Distance);
        }

        [Fact]
        public void Polygon2Ds_DuplicatePolylines_DeduplicatesToSingle()
        {
            Polyline2D polyline = new Polyline2D(new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10),
                new Point2D(0, 0)
            });

            List<Polygon2D> result = new List<ISegmentable2D> { polyline, polyline }.Polygon2Ds();
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void Polygon2Ds_MultipleDisjointPolygons_ReturnsAllPolygons()
        {
            Polyline2D square1 = new Polyline2D(new List<Point2D>
            {
                new Point2D(0, 0), new Point2D(5, 0), new Point2D(5, 5), new Point2D(0, 5), new Point2D(0, 0)
            });

            Polyline2D square2 = new Polyline2D(new List<Point2D>
            {
                new Point2D(20, 20), new Point2D(25, 20), new Point2D(25, 25), new Point2D(20, 25), new Point2D(20, 20)
            });

            List<Polygon2D> result = new List<ISegmentable2D> { square1, square2 }.Polygon2Ds();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Polygon2Ds_PolygonWithInternalHole_SubtractionsHandled()
        {
            // Outer square 20x20
            Polyline2D outer = new Polyline2D(new List<Point2D>
            {
                new Point2D(0, 0), new Point2D(20, 0), new Point2D(20, 20), new Point2D(0, 20), new Point2D(0, 0)
            });

            // Inner hole square 6x6 inside
            Polyline2D inner = new Polyline2D(new List<Point2D>
            {
                new Point2D(7, 7), new Point2D(13, 7), new Point2D(13, 13), new Point2D(7, 13), new Point2D(7, 7)
            });

            List<Polygon2D> result = new List<ISegmentable2D> { outer, inner }.Polygon2Ds();
            Assert.NotNull(result);
            Assert.True(result.Count >= 1);
        }

        [Fact]
        public void Polygon2Ds_SplitOption_SplitsAndCreatesPolygons()
        {
            // Two intersecting polylines creating a figure 8 or grid
            Polyline2D poly1 = new Polyline2D(new List<Point2D>
            {
                new Point2D(0, 0), new Point2D(10, 0), new Point2D(10, 10), new Point2D(0, 10), new Point2D(0, 0)
            });
            Polyline2D poly2 = new Polyline2D(new List<Point2D>
            {
                new Point2D(10, 0), new Point2D(20, 0), new Point2D(20, 10), new Point2D(10, 10), new Point2D(10, 0)
            });

            List<Polygon2D> result = new List<ISegmentable2D> { poly1, poly2 }.Polygon2Ds(split: true);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
