// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;
using SAM.Math;
using Xunit;

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
            Assert.False(Query.IsValid(new Point3D(double.NaN, 0, 0)));
            Assert.False(Query.IsValid(new Point3D(0, double.NaN, 0)));
            Assert.False(Query.IsValid(new Point3D(0, 0, double.NaN)));

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
            Matrix4D rotation = new Matrix4D(
                cos, -sin, 0, 0,
                sin, cos, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

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
    }
}
