// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using Xunit;

namespace SAM.Tests
{
    // Regression tests for PlanarBoundary3D.Coplanar(Plane, double). The
    // second overload used to shadow the "plane" field with its "plane"
    // parameter and compare that parameter against itself
    // (return plane.Coplanar(plane, tolerance)), so it always returned true
    // regardless of the instance's own plane. Query.IsValid (the guard used
    // by Panel.AddAperture / AddApertures) relies on this method as a cheap
    // coplanarity check before the more expensive Face3D.Inside containment
    // test, so the bug silently disabled that guard everywhere.
    public class PlanarBoundary3DTests
    {
        private static PlanarBoundary3D UnitSquareBoundary(Plane plane)
        {
            Face3D face3D = new Face3D(plane, new Rectangle2D(1, 1));
            return new PlanarBoundary3D(face3D);
        }

        [Fact]
        public void Coplanar_SamePlane_ReturnsTrue()
        {
            Plane plane = new Plane(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));

            PlanarBoundary3D planarBoundary3D_1 = UnitSquareBoundary(plane);
            PlanarBoundary3D planarBoundary3D_2 = UnitSquareBoundary(plane);

            Assert.True(planarBoundary3D_1.Coplanar(planarBoundary3D_2));
            Assert.True(planarBoundary3D_1.Coplanar(plane));
        }

        [Fact]
        public void Coplanar_ParallelOffsetPlanes_ReturnsTrue()
        {
            // Plane.Coplanar (SAM.Geometry.Spatial.Plane.Coplanar) only compares
            // normal direction and ignores the planes' offset/origin, so two
            // parallel planes at different heights are still "coplanar" by that
            // definition. PlanarBoundary3D.Coplanar must match that semantic
            // rather than introduce its own offset check.
            Plane plane_1 = new Plane(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
            Plane plane_2 = new Plane(new Point3D(0, 0, 5), new Vector3D(0, 0, 1));

            PlanarBoundary3D planarBoundary3D_1 = UnitSquareBoundary(plane_1);
            PlanarBoundary3D planarBoundary3D_2 = UnitSquareBoundary(plane_2);

            Assert.True(planarBoundary3D_1.Coplanar(planarBoundary3D_2));
            Assert.True(planarBoundary3D_1.Coplanar(plane_2));
        }

        [Fact]
        public void Coplanar_PerpendicularPlanes_ReturnsFalse()
        {
            Plane plane_1 = new Plane(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
            Plane plane_2 = new Plane(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));

            PlanarBoundary3D planarBoundary3D_1 = UnitSquareBoundary(plane_1);
            PlanarBoundary3D planarBoundary3D_2 = UnitSquareBoundary(plane_2);

            Assert.False(planarBoundary3D_1.Coplanar(planarBoundary3D_2));
            Assert.False(planarBoundary3D_1.Coplanar(plane_2));
        }
    }
}
