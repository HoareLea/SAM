// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;
using SAM.Math;
using Xunit;
using SAM.Core;

namespace SAM.Tests
{
    public class BoundingBox3DTests
    {
        [Fact]
        public void Constructor_WithOffset_ShouldNotBeFlatZ()
        {
            Point3D center = new Point3D(10, 20, 30);
            double offset = 5;
            BoundingBox3D bbox = new BoundingBox3D(center, offset);

            Assert.Equal(5, bbox.MinX);
            Assert.Equal(15, bbox.MaxX);
            Assert.Equal(15, bbox.MinY);
            Assert.Equal(25, bbox.MaxY);
            Assert.Equal(25, bbox.MinZ);
            Assert.Equal(35, bbox.MaxZ);

            Assert.Equal(10, bbox.Width);
            Assert.Equal(10, bbox.Depth);
            Assert.Equal(10, bbox.Height);
        }

        [Fact]
        public void IsValid_WithNaNCoordinates_ShouldReturnFalse()
        {
            // Vector3D NaN checks
            Assert.False(new Vector3D(double.NaN, 0, 0).IsValid());
            Assert.False(new Vector3D(0, double.NaN, 0).IsValid());
            Assert.False(new Vector3D(0, 0, double.NaN).IsValid());

            // Point3D NaN checks
            Assert.False(new Point3D(double.NaN, 0, 0).IsValid());
            Assert.False(new Point3D(0, double.NaN, 0).IsValid());
            Assert.False(new Point3D(0, 0, double.NaN).IsValid());

            // BoundingBox3D NaN checks
            BoundingBox3D validBox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
            Assert.True(validBox.IsValid());

            BoundingBox3D invalidBoxX = new BoundingBox3D(new Point3D(double.NaN, 0, 0), new Point3D(1, 1, 1));
            Assert.False(invalidBoxX.IsValid());

            BoundingBox3D invalidBoxY = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, double.NaN, 1));
            Assert.False(invalidBoxY.IsValid());

            BoundingBox3D invalidBoxZ = new BoundingBox3D(new Point3D(0, 0, double.NaN), new Point3D(1, 1, 1));
            Assert.False(invalidBoxZ.IsValid());
        }

        [Fact]
        public void GetSegments_ShouldReturnExactly12Segments()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 2, 3));
            List<Segment3D> segments = bbox.GetSegments();

            Assert.Equal(12, segments.Count);
        }

        [Fact]
        public void GetPoints_ShouldReturnActual8Corners()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 2, 3));
            List<Point3D> points = bbox.GetPoints();

            Assert.Equal(8, points.Count);

            // Verify all points are within [0,1], [0,2], [0,3] bounds
            foreach (Point3D pt in points)
            {
                Assert.True(pt.X >= 0 && pt.X <= 1);
                Assert.True(pt.Y >= 0 && pt.Y <= 2);
                Assert.True(pt.Z >= 0 && pt.Z <= 3);
            }

            // Verify top corners specifically do not go to x=2, y=4 due to previous offset multiplication bug
            Assert.Contains(points, pt => pt.X == 1 && pt.Y == 2 && pt.Z == 3); // Max corner
            Assert.Contains(points, pt => pt.X == 0 && pt.Y == 0 && pt.Z == 0); // Min corner
            Assert.Contains(points, pt => pt.X == 0 && pt.Y == 2 && pt.Z == 3);
            Assert.Contains(points, pt => pt.X == 1 && pt.Y == 0 && pt.Z == 3);
        }

        [Fact]
        public void Transform_WithRotation_ShouldResultInAxesAlignedAABB()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));

            // Rotate around Z axis by 45 degrees
            double angle = System.Math.PI / 4;
            double cos = System.Math.Cos(angle);
            double sin = System.Math.Sin(angle);

            // Matrix4D setup for 45 deg Z rotation
            Matrix4D rotation = new Matrix4D();
            rotation[0, 0] = cos;
            rotation[0, 1] = -sin;
            rotation[1, 0] = sin;
            rotation[1, 1] = cos;
            rotation[2, 2] = 1;
            rotation[3, 3] = 1;

            BoundingBox3D transformedBox = bbox.Transform(rotation);

            // The original corners of (0,0,0) and (1,1,1) transform to:
            // (0,0,0) -> (0,0,0)
            // (1,0,0) -> (cos, sin, 0) ~ (0.707, 0.707, 0)
            // (0,1,0) -> (-sin, cos, 0) ~ (-0.707, 0.707, 0)
            // (1,1,0) -> (cos - sin, sin + cos, 0) ~ (0, 1.414, 0)
            // Maximum coordinate in Y is sin + cos = 1.414. Minimum coordinate in X is -sin = -0.707.
            // Under old implementation, it only transformed min(0,0,0) and max(1,1,1), getting bounds [-0.707, 0] in X which cuts off (1,0,0)!
            // Under new correct implementation, transformed box bounds should contain all transformed corners.

            Assert.True(transformedBox.MinX <= -0.7);
            Assert.True(transformedBox.MaxX >= 0.7);
            Assert.True(transformedBox.MinY >= 0.0);
            Assert.True(transformedBox.MaxY >= 1.4);
            Assert.Equal(0.0, transformedBox.MinZ);
            Assert.Equal(1.0, transformedBox.MaxZ);
        }

        [Fact]
        public void Point3Ds_WithInvalidOffset_ShouldThrowException()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));

            Assert.Throws<ArgumentOutOfRangeException>(() => bbox.Point3Ds(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => bbox.Point3Ds(-1));
        }

        [Fact]
        public void GetHashCode_DifferentObjects_ShouldBeConsistent()
        {
            BoundingBox3D bbox1 = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 2, 3));
            BoundingBox3D bbox2 = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 2, 3));
            BoundingBox3D bbox3 = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 20, 30));

            Assert.Equal(bbox1.GetHashCode(), bbox2.GetHashCode());
            Assert.NotEqual(bbox1.GetHashCode(), bbox3.GetHashCode());
        }

        [Fact]
        public void Intersect_SegmentCrossingBounds_ShouldCheckMultiplePlanes()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            // Segment crossing through the right face (X = 10)
            Segment3D segment = new Segment3D(new Point3D(5, 5, 5), new Point3D(15, 5, 5));

            Assert.True(bbox.Intersect(segment));
        }

        [Fact]
        public void Intersect_SegmentBothEndpointsOutsideCrossingBox_ShouldReturnTrue()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            // Segment from (-5, 5, 5) to (15, 5, 5) completely crosses the box
            Segment3D segment = new Segment3D(new Point3D(-5, 5, 5), new Point3D(15, 5, 5));

            Assert.True(bbox.Intersect(segment));
        }

        [Fact]
        public void Intersect_SegmentBboxOverlapsButSegmentMisses_ShouldReturnFalse()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            // Segment from (12, 9, 5) to (9, 12, 5) has bbox [9,12]x[9,12]x[5,5], which overlaps the box,
            // but the segment itself misses it entirely (closest point is (10, 11, 5) which has Y > 10).
            Segment3D segment = new Segment3D(new Point3D(12, 9, 5), new Point3D(9, 12, 5));

            Assert.False(bbox.Intersect(segment));
        }

        [Fact]
        public void Range_WithValidAndInvalidIndices_ShouldBehaveCorrectly()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(1, 2, 3), new Point3D(10, 20, 30));

            // Valid indices
            Range<double> rangeX = bbox.Range(0);
            Assert.Equal(1, rangeX.Min);
            Assert.Equal(10, rangeX.Max);

            Range<double> rangeY = bbox.Range(1);
            Assert.Equal(2, rangeY.Min);
            Assert.Equal(20, rangeY.Max);

            Range<double> rangeZ = bbox.Range(2);
            Assert.Equal(3, rangeZ.Min);
            Assert.Equal(30, rangeZ.Max);

            // Invalid indices
            Assert.Throws<IndexOutOfRangeException>(() => bbox.Range(-1));
            Assert.Throws<IndexOutOfRangeException>(() => bbox.Range(3));
        }

        [Fact]
        public void GetArea_ShouldReturnTotal3DSurfaceArea()
        {
            // Box of Width = 2 (X: 0->2), Depth = 3 (Y: 0->3), Height = 4 (Z: 0->4)
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 3, 4));

            // Surface Area = 2 * (W*D + W*H + D*H) = 2 * (6 + 8 + 12) = 52
            Assert.Equal(52.0, bbox.GetArea());
        }

        [Fact]
        public void Constructor_IEnumerableBoundingBox3D_ShouldHandleNullAndMultipleBoxes()
        {
            BoundingBox3D box1 = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(2, 2, 2));
            BoundingBox3D box2 = new BoundingBox3D(new Point3D(5, 5, 5), new Point3D(10, 10, 10));

            BoundingBox3D combined = new BoundingBox3D(new List<BoundingBox3D> { box1, box2 });

            Assert.Equal(0, combined.MinX);
            Assert.Equal(0, combined.MinY);
            Assert.Equal(0, combined.MinZ);
            Assert.Equal(10, combined.MaxX);
            Assert.Equal(10, combined.MaxY);
            Assert.Equal(10, combined.MaxZ);

            BoundingBox3D nullListComb = new BoundingBox3D((IEnumerable<BoundingBox3D>)null);
            Assert.True(double.IsNaN(nullListComb.MinX));
        }

        [Fact]
        public void Constructor_IEnumerablePoint3D_ShouldHandleNullAndEmpty()
        {
            BoundingBox3D nullBox = new BoundingBox3D((IEnumerable<Point3D>)null);
            Assert.True(double.IsNaN(nullBox.MinX));

            BoundingBox3D emptyBox = new BoundingBox3D(new List<Point3D>());
            Assert.True(double.IsNaN(emptyBox.MinX));
        }

        [Fact]
        public void PlaneIntersect_ShouldDetectPlaneBoundingBoxIntersection()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            // Plane passing through (5, 5, 5) with normal (0, 0, 1) -> Z = 5
            Plane planeZ5 = new Plane(new Point3D(5, 5, 5), Vector3D.WorldZ);
            Assert.True(planeZ5.Intersect(bbox));

            // Plane far away Z = 20
            Plane planeZ20 = new Plane(new Point3D(0, 0, 20), Vector3D.WorldZ);
            Assert.False(planeZ20.Intersect(bbox));
        }

        [Fact]
        public void PlaneAbove_ShouldCheckIfAllBoxCornersAreAbovePlane()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(10, 10, 10), new Point3D(20, 20, 20));

            // Plane at Z = 0 with normal (0, 0, 1) -> bbox is completely above plane Z=0
            Plane planeZ0 = new Plane(new Point3D(0, 0, 0), Vector3D.WorldZ);
            Assert.True(planeZ0.Above(bbox, 0));

            // Plane at Z = 15 -> bbox is bisected by plane, so not all corners are above
            Plane planeZ15 = new Plane(new Point3D(0, 0, 15), Vector3D.WorldZ);
            Assert.False(planeZ15.Above(bbox, 0));
        }

        [Fact]
        public void QueryOn_PointOnEdge_ShouldReturnTrue()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));

            // Point (5, 0, 0) lies on the bottom-front edge (Y=0, Z=0)
            Point3D ptOnEdge = new Point3D(5, 0, 0);
            Assert.True(bbox.On(ptOnEdge));

            // Point (5, 5, 5) lies inside box interior, not on edge
            Point3D ptInterior = new Point3D(5, 5, 5);
            Assert.False(bbox.On(ptInterior));
        }

        [Fact]
        public void QueryExternal_ShouldFilterEnclosedBoxes()
        {
            BoundingBox3D innerBox = new BoundingBox3D(new Point3D(2, 2, 2), new Point3D(4, 4, 4));
            BoundingBox3D outerBox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(10, 10, 10));
            BoundingBox3D separateBox = new BoundingBox3D(new Point3D(20, 20, 20), new Point3D(30, 30, 30));

            List<BoundingBox3D> boxes = new List<BoundingBox3D> { innerBox, outerBox, separateBox };
            List<BoundingBox3D> external = SAM.Geometry.Spatial.Query.External(boxes);

            Assert.Equal(2, external.Count);
            Assert.Contains(outerBox, external);
            Assert.Contains(separateBox, external);
            Assert.DoesNotContain(innerBox, external);
        }

        [Fact]
        public void Include_SinglePass_ShouldExpandBounds()
        {
            BoundingBox3D bbox = new BoundingBox3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
            List<Point3D> points = new List<Point3D>
            {
                new Point3D(-5, 0, 0),
                new Point3D(0, 15, 0),
                new Point3D(0, 0, 25)
            };

            Assert.True(bbox.Include(points));
            Assert.Equal(-5, bbox.MinX);
            Assert.Equal(15, bbox.MaxY);
            Assert.Equal(25, bbox.MaxZ);
        }
    }
}
