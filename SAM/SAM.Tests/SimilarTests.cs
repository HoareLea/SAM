// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using SAM.Geometry.Planar;
using Xunit;

namespace SAM.Tests
{
    // Covers Query.Similar for closed regions and faces. These queries drive
    // deduplication in Create.Polygon2Ds, so their correctness is load-bearing
    // across the wider SAM stack. The Face2D tests in particular lock down the
    // fixed self-comparison bug (internalEdge2Ds_2 must come from face_2, not face_1).
    public class SimilarTests
    {
        private static Polygon2D Square(double x, double y, double size)
        {
            return new Polygon2D(new List<Point2D>
            {
                new Point2D(x, y),
                new Point2D(x + size, y),
                new Point2D(x + size, y + size),
                new Point2D(x, y + size)
            });
        }

        [Fact]
        public void Similar_IdenticalPolygons_ReturnsTrue()
        {
            Polygon2D polygon2D_1 = Square(0, 0, 10);
            Polygon2D polygon2D_2 = Square(0, 0, 10);
            Assert.True(polygon2D_1.Similar(polygon2D_2));
        }

        [Fact]
        public void Similar_SameShapeDifferentVertexOrder_ReturnsTrue()
        {
            // The vertex-on-edge test is order-independent, so the same square described
            // starting from a different corner must still be reported as similar.
            Polygon2D polygon2D_1 = new Polygon2D(new List<Point2D>
            {
                new Point2D(0, 0),
                new Point2D(10, 0),
                new Point2D(10, 10),
                new Point2D(0, 10)
            });
            Polygon2D polygon2D_2 = new Polygon2D(new List<Point2D>
            {
                new Point2D(10, 10),
                new Point2D(0, 10),
                new Point2D(0, 0),
                new Point2D(10, 0)
            });
            Assert.True(polygon2D_1.Similar(polygon2D_2));
        }

        [Fact]
        public void Similar_DifferentArea_ReturnsFalse()
        {
            // Area fast-reject: 100 vs 25 is well beyond tolerance.
            Polygon2D polygon2D_1 = Square(0, 0, 10);
            Polygon2D polygon2D_2 = Square(0, 0, 5);
            Assert.False(polygon2D_1.Similar(polygon2D_2));
        }

        [Fact]
        public void Similar_SameAreaDifferentLocation_ReturnsFalse()
        {
            // Equal area but disjoint bounding boxes: exercises the bounding-box fast-reject
            // and confirms it does not mistake congruent-but-displaced shapes for similar.
            Polygon2D polygon2D_1 = Square(0, 0, 10);
            Polygon2D polygon2D_2 = Square(100, 100, 10);
            Assert.False(polygon2D_1.Similar(polygon2D_2));
        }

        [Fact]
        public void Similar_Faces_IdenticalExternalNoHoles_ReturnsTrue()
        {
            Face2D face2D_1 = Square(0, 0, 10);
            Face2D face2D_2 = Square(0, 0, 10);
            Assert.True(Query.Similar(face2D_1, face2D_2));
        }

        [Fact]
        public void Similar_Faces_SameHoles_ReturnsTrue()
        {
            Face2D face2D_1 = Square(0, 0, 10).Face2D(new List<IClosed2D> { Square(2, 2, 2) });
            Face2D face2D_2 = Square(0, 0, 10).Face2D(new List<IClosed2D> { Square(2, 2, 2) });
            Assert.True(Query.Similar(face2D_1, face2D_2));
        }

        [Fact]
        public void Similar_Faces_DifferentHoles_ReturnsFalse()
        {
            // Regression guard for the fixed self-comparison bug: the external edges are
            // identical and both faces have exactly one hole, but the holes are in different
            // places. The previous code compared face_1's internal edges against themselves
            // and wrongly returned true; the corrected code compares face_1 against face_2.
            Face2D face2D_1 = Square(0, 0, 10).Face2D(new List<IClosed2D> { Square(2, 2, 2) });
            Face2D face2D_2 = Square(0, 0, 10).Face2D(new List<IClosed2D> { Square(6, 6, 2) });
            Assert.False(Query.Similar(face2D_1, face2D_2));
        }

        [Fact]
        public void Similar_Faces_DifferentHoleCount_ReturnsFalse()
        {
            Face2D face2D_1 = Square(0, 0, 10).Face2D(new List<IClosed2D> { Square(2, 2, 2) });
            Face2D face2D_2 = Square(0, 0, 10);
            Assert.False(Query.Similar(face2D_1, face2D_2));
        }
    }
}
