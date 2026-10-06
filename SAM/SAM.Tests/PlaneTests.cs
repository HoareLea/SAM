// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using Xunit;

namespace SAM.Tests
{
    public class PlaneTests
    {
        [Fact]
        public void DefaultConstructor_InitializesWorldXYDefaults()
        {
            Plane plane = new Plane();

            Assert.Equal(0, plane.Origin.X);
            Assert.Equal(0, plane.Origin.Y);
            Assert.Equal(0, plane.Origin.Z);

            Assert.Equal(0, plane.Normal.X);
            Assert.Equal(0, plane.Normal.Y);
            Assert.Equal(1, plane.Normal.Z);

            Assert.Equal(1, plane.AxisX.X);
            Assert.Equal(0, plane.AxisX.Y);
            Assert.Equal(0, plane.AxisX.Z);

            Assert.Equal(0, plane.AxisY.X);
            Assert.Equal(1, plane.AxisY.Y);
            Assert.Equal(0, plane.AxisY.Z);
        }

        [Fact]
        public void OriginAndNormalConstructor_CalculatesAxes()
        {
            Point3D origin = new Point3D(10, 20, 30);
            Vector3D normal = new Vector3D(0, 0, 5); // non-unit normal should be normalized

            Plane plane = new Plane(origin, normal);

            Assert.Equal(10, plane.Origin.X);
            Assert.Equal(20, plane.Origin.Y);
            Assert.Equal(30, plane.Origin.Z);

            Assert.Equal(0, plane.Normal.X);
            Assert.Equal(0, plane.Normal.Y);
            Assert.Equal(1, plane.Normal.Z);
        }

        [Fact]
        public void ThreePointsConstructor_CalculatesPlane()
        {
            Point3D p1 = new Point3D(0, 0, 5);
            Point3D p2 = new Point3D(10, 0, 5);
            Point3D p3 = new Point3D(0, 10, 5);

            Plane plane = new Plane(p1, p2, p3);

            Assert.True(plane.On(p1));
            Assert.True(plane.On(p2));
            Assert.True(plane.On(p3));
            Assert.Equal(0, plane.Normal.X, 6);
            Assert.Equal(0, plane.Normal.Y, 6);
            Assert.Equal(100, plane.Normal.Z, 6);
            Assert.Equal(1, plane.Normal.Unit.Z, 6);
        }

        [Fact]
        public void OriginAxisXAxisYConstructor_InitializesPlane()
        {
            Point3D origin = new Point3D(1, 2, 3);
            Vector3D axisX = new Vector3D(1, 0, 0);
            Vector3D axisY = new Vector3D(0, 1, 0);

            Plane plane = new Plane(origin, axisX, axisY);

            Assert.Equal(1, plane.Origin.X);
            Assert.Equal(2, plane.Origin.Y);
            Assert.Equal(3, plane.Origin.Z);
            Assert.Equal(0, plane.Normal.X);
            Assert.Equal(0, plane.Normal.Y);
            Assert.Equal(1, plane.Normal.Z);
        }

        [Fact]
        public void WorldStaticProperties_CorrectOrientation()
        {
            Plane xy = Plane.WorldXY;
            Plane yz = Plane.WorldYZ;
            Plane xz = Plane.WorldXZ;

            Assert.Equal(new Vector3D(0, 0, 1), xy.Normal);
            Assert.Equal(new Vector3D(1, 0, 0), yz.Normal);
            Assert.Equal(new Vector3D(0, 1, 0), xz.Normal);
        }

        [Fact]
        public void PlaneEquationFactors_A_B_C_D_K()
        {
            Point3D origin = new Point3D(2, 3, 4);
            Vector3D normal = new Vector3D(0, 0, 1);
            Plane plane = new Plane(origin, normal);

            Assert.Equal(0, plane.A);
            Assert.Equal(0, plane.B);
            Assert.Equal(1, plane.C);
            Assert.Equal(-4, plane.D);
            Assert.Equal(4, plane.K);

            // A*x + B*y + C*z + D = 0 for origin
            double val = plane.A * origin.X + plane.B * origin.Y + plane.C * origin.Z + plane.D;
            Assert.Equal(0, val, 6);
        }

        [Fact]
        public void DistanceToPoint_CalculatesCorrectDistances()
        {
            Plane plane = new Plane(new Point3D(0, 0, 10), Vector3D.WorldZ);

            Point3D pointOn = new Point3D(5, 5, 10);
            Point3D pointAbove = new Point3D(5, 5, 15);
            Point3D pointBelow = new Point3D(5, 5, 3);

            Assert.Equal(0, plane.Distance(pointOn), 6);
            Assert.Equal(5, plane.Distance(pointAbove), 6);
            Assert.Equal(7, plane.Distance(pointBelow), 6);
        }

        [Fact]
        public void DistanceToSegment_HandlesIntersectionAndOffset()
        {
            Plane plane = new Plane(new Point3D(0, 0, 5), Vector3D.WorldZ);

            // Segment crossing plane Z=5
            Segment3D intersecting = new Segment3D(new Point3D(0, 0, 0), new Point3D(0, 0, 10));
            Assert.Equal(0, plane.Distance(intersecting));

            // Segment strictly above plane Z=5
            Segment3D above = new Segment3D(new Point3D(0, 0, 7), new Point3D(0, 0, 12));
            Assert.Equal(2, plane.Distance(above), 6);
        }

        [Fact]
        public void DistanceToPlane_ParallelVsIntersecting()
        {
            Plane p1 = new Plane(new Point3D(0, 0, 0), Vector3D.WorldZ);
            Plane p2 = new Plane(new Point3D(0, 0, 10), Vector3D.WorldZ);
            Plane p3 = new Plane(new Point3D(0, 0, 0), Vector3D.WorldX);

            Assert.Equal(10, p1.Distance(p2), 6);
            Assert.Equal(0, p1.Distance(p3), 6); // non-parallel planes intersect -> distance 0
        }

        [Fact]
        public void ClosestPointAndOn()
        {
            Plane plane = new Plane(new Point3D(0, 0, 5), Vector3D.WorldZ);
            Point3D p = new Point3D(3, 4, 12);

            Point3D closest = plane.Closest(p);

            Assert.Equal(3, closest.X);
            Assert.Equal(4, closest.Y);
            Assert.Equal(5, closest.Z);
            Assert.True(plane.On(closest));
            Assert.False(plane.On(p));
        }

        [Fact]
        public void ProjectVector_ProjectsOntoPlane()
        {
            Plane plane = new Plane(Point3D.Zero, Vector3D.WorldZ);
            Vector3D v = new Vector3D(3, 4, 5);

            Vector3D projected = plane.Project(v);

            Assert.Equal(3, projected.X);
            Assert.Equal(4, projected.Y);
            Assert.Equal(0, projected.Z);
        }

        [Fact]
        public void Transformation_FlipZ_FlipX_Move_Reverse()
        {
            Plane plane = new Plane(new Point3D(1, 2, 3), Vector3D.WorldZ);

            plane.Move(new Vector3D(5, 5, 5));
            Assert.Equal(6, plane.Origin.X);
            Assert.Equal(7, plane.Origin.Y);
            Assert.Equal(8, plane.Origin.Z);

            plane.FlipZ(flipX: true);
            Assert.Equal(new Vector3D(0, 0, -1), plane.Normal);

            Plane plane2 = new Plane(new Point3D(0, 0, 0), Vector3D.WorldZ);
            plane2.Reverse();
            Assert.Equal(new Vector3D(0, 0, -1), plane2.Normal);
            Assert.Equal(new Vector3D(0, -1, 0), plane2.AxisY);
        }

        [Fact]
        public void EqualityAndHashCode_WorkAsExpected()
        {
            Plane plane1 = new Plane(new Point3D(1, 2, 3), Vector3D.WorldZ);
            Plane plane2 = new Plane(new Point3D(1, 2, 3), Vector3D.WorldZ);
            Plane plane3 = new Plane(new Point3D(1, 2, 4), Vector3D.WorldZ);

            Assert.True(plane1 == plane2);
            Assert.True(plane1.Equals(plane2));
            Assert.False(plane1 == plane3);
            Assert.True(plane1 != plane3);
            Assert.Equal(plane1.GetHashCode(), plane2.GetHashCode());
        }

        [Fact]
        public void SpatialQueryExtensions_Above_Below_Between_Horizontal_Azimuth_IsValid()
        {
            Plane plane = new Plane(new Point3D(0, 0, 5), Vector3D.WorldZ);
            Point3D pointAbove = new Point3D(0, 0, 10);
            Point3D pointBelow = new Point3D(0, 0, 0);

            Assert.True(plane.Above(pointAbove));
            Assert.False(plane.Above(pointBelow));

            Assert.True(plane.Below(pointBelow));
            Assert.False(plane.Below(pointAbove));

            Plane planeLow = new Plane(new Point3D(0, 0, 0), Vector3D.WorldZ);
            Plane planeHigh = new Plane(new Point3D(0, 0, 10), Vector3D.WorldZ);
            Assert.True(planeLow.Between(planeHigh, plane.Origin));

            Assert.True(plane.Horizontal());
            Assert.True(plane.IsValid());

            Plane verticalPlane = new Plane(Point3D.Zero, Vector3D.WorldX);
            Assert.False(verticalPlane.Horizontal());
            Assert.Equal(0, plane.Azimuth(Vector3D.WorldY));
        }

        [Fact]
        public void SpatialCreateExtensions_FactoryMethods()
        {
            Plane pElev = SAM.Geometry.Spatial.Create.Plane(12.5);
            Assert.Equal(12.5, pElev.Origin.Z);
            Assert.Equal(Vector3D.WorldZ, pElev.Normal);

            Plane pDim0 = SAM.Geometry.Spatial.Create.Plane(3.0, 0);
            Assert.Equal(3.0, pDim0.Origin.X);
            Assert.Equal(Vector3D.WorldX, pDim0.Normal);

            Plane pDim1 = SAM.Geometry.Spatial.Create.Plane(4.0, 1);
            Assert.Equal(4.0, pDim1.Origin.Y);
            Assert.Equal(Vector3D.WorldY, pDim1.Normal);

            Plane pDim2 = SAM.Geometry.Spatial.Create.Plane(5.0, 2);
            Assert.Equal(5.0, pDim2.Origin.Z);
            Assert.Equal(Vector3D.WorldZ, pDim2.Normal);

            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(3, 0, 0);
            Point3D p3 = new Point3D(0, 3, 0);
            Plane p3Points = SAM.Geometry.Spatial.Create.Plane(p1, p2, p3);
            Assert.NotNull(p3Points);
            Assert.Equal(1, p3Points.Origin.X);
            Assert.Equal(1, p3Points.Origin.Y);
            Assert.Equal(0, p3Points.Origin.Z);
        }

        [Fact]
        public void Point2D_Point3D_Conversion_RoundTrip()
        {
            Plane plane = new Plane(new Point3D(10, 20, 30), Vector3D.WorldZ);

            Point2D p2d = new Point2D(5, -7);
            Point3D p3d = plane.Convert(p2d);

            Assert.Equal(15, p3d.X);
            Assert.Equal(13, p3d.Y);
            Assert.Equal(30, p3d.Z);

            Point2D p2dRoundTrip = plane.Convert(p3d);
            Assert.Equal(p2d.X, p2dRoundTrip.X, 6);
            Assert.Equal(p2d.Y, p2dRoundTrip.Y, 6);
        }

        [Fact]
        public void Vector2D_Vector3D_Conversion_RoundTrip()
        {
            Plane plane = new Plane(new Point3D(0, 0, 0), Vector3D.WorldZ);

            Vector2D v2d = new Vector2D(3, 4);
            Vector3D v3d = plane.Convert(v2d);

            Assert.Equal(3, v3d.X);
            Assert.Equal(4, v3d.Y);
            Assert.Equal(0, v3d.Z);

            Vector2D v2dRoundTrip = plane.Convert(v3d);
            Assert.Equal(v2d.X, v2dRoundTrip.X, 6);
            Assert.Equal(v2d.Y, v2dRoundTrip.Y, 6);
        }

        [Fact]
        public void BatchPointConversion_RoundTrip()
        {
            Plane plane = new Plane(new Point3D(1, 2, 3), Vector3D.WorldZ);
            List<Point3D> points3D = new List<Point3D>
            {
                new Point3D(1, 2, 3),
                new Point3D(5, 7, 3),
                new Point3D(-2, 10, 3)
            };

            List<Point2D> points2D = plane.Convert(points3D);
            Assert.Equal(3, points2D.Count);

            List<Point3D> points3DRoundTrip = plane.Convert(points2D);
            Assert.Equal(3, points3DRoundTrip.Count);

            for (int i = 0; i < points3D.Count; i++)
            {
                Assert.Equal(points3D[i].X, points3DRoundTrip[i].X, 6);
                Assert.Equal(points3D[i].Y, points3DRoundTrip[i].Y, 6);
                Assert.Equal(points3D[i].Z, points3DRoundTrip[i].Z, 6);
            }
        }

        [Fact]
        public void JsonSerialization_RoundTrip()
        {
            Plane plane = new Plane(new Point3D(1.5, 2.5, 3.5), new Vector3D(0, 0, 1));
            JsonObject jsonObject = plane.ToJsonObject();
            Assert.NotNull(jsonObject);

            Plane roundTripped = new Plane(jsonObject);
            Assert.Equal(plane.Origin, roundTripped.Origin);
            Assert.Equal(plane.Normal, roundTripped.Normal);
            Assert.Equal(plane.AxisY, roundTripped.AxisY);
        }
    }
}
