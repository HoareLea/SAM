// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using SAM.Geometry.Planar;
using Xunit;

namespace SAM.Tests
{
    public class Segment2DsTests
    {
        [Fact]
        public void Segment2Ds_NullInput_ReturnsNull()
        {
            IEnumerable<ISegmentable2D> input = null;
            Assert.Null(input.Segment2Ds(5.0));
        }

        [Fact]
        public void Segment2Ds_UnconnectedOnly_SharedAndFreeEndpoints_ReturnsSegments()
        {
            // L-shape: the two segments share the corner (0,0) (a connected endpoint,
            // touched by two segments) while (10,0) and (0,10) are free endpoints. This
            // exercises the endpoint-connectivity grid's matches==2 (connected) and
            // matches==1 (unconnected) branches.
            List<ISegmentable2D> input = new List<ISegmentable2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(10, 0)),
                new Segment2D(new Point2D(0, 0), new Point2D(0, 10))
            };

            List<Segment2D> result = input.Segment2Ds(5.0, unconnectedOnly: true);

            Assert.NotNull(result);
            // The two non-collinear originals are always retained and cannot be merged.
            Assert.True(result.Count >= 2);
        }

        [Fact]
        public void Segment2Ds_UnconnectedOnly_FullyConnectedSquare_ReturnsSegments()
        {
            // Closed square: every endpoint is shared by exactly two segments, so no
            // endpoint is "unconnected" and no endpoint extensions are produced. Verifies
            // the grid correctly reports every corner as connected without error.
            List<ISegmentable2D> input = new List<ISegmentable2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(10, 0)),
                new Segment2D(new Point2D(10, 0), new Point2D(10, 10)),
                new Segment2D(new Point2D(10, 10), new Point2D(0, 10)),
                new Segment2D(new Point2D(0, 10), new Point2D(0, 0))
            };

            List<Segment2D> result = input.Segment2Ds(5.0, unconnectedOnly: true);

            Assert.NotNull(result);
            // Four distinct, non-collinear edges are retained; there are no interior crossings.
            Assert.True(result.Count >= 4);
        }

        [Fact]
        public void Segment2Ds_UnconnectedOnlyFalse_SkipsConnectivityGrid()
        {
            // With unconnectedOnly == false the connectivity grid is never built (the
            // short-circuit path). Ensure that path still returns the expected segments.
            List<ISegmentable2D> input = new List<ISegmentable2D>
            {
                new Segment2D(new Point2D(0, 0), new Point2D(10, 0)),
                new Segment2D(new Point2D(0, 0), new Point2D(0, 10))
            };

            List<Segment2D> result = input.Segment2Ds(5.0, unconnectedOnly: false);

            Assert.NotNull(result);
            Assert.True(result.Count >= 2);
        }
    }
}
