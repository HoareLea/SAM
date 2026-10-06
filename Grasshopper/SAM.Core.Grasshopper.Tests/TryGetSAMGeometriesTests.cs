// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using NUnit.Framework;

namespace SAM.Core.Grasshopper.Tests
{
    [TestFixture]
    public class TryGetSAMGeometriesTests
    {
        [Test]
        public void Face3D_FromDirectAndWrapped()
        {
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(10, 0, 0);
            Point3D p3 = new Point3D(10, 10, 0);
            Point3D p4 = new Point3D(0, 10, 0);
            Polygon3D poly = new Polygon3D(new[] { p1, p2, p3, p4 });
            Face3D face = new Face3D(poly);

            // Direct object
            GH_ObjectWrapper wrapperDirect = new GH_ObjectWrapper(face);
            Assert.True(wrapperDirect.TryGetSAMGeometries(out List<Face3D> facesDirect));
            Assert.Single(facesDirect);
            Assert.Same(face, facesDirect[0]);

            // GooSAMGeometry
            GooSAMGeometry goo = new GooSAMGeometry(face);
            GH_ObjectWrapper wrapperGoo = new GH_ObjectWrapper(goo);
            Assert.True(wrapperGoo.TryGetSAMGeometries(out List<Face3D> facesGoo));
            Assert.Single(facesGoo);
            Assert.Same(face, facesGoo[0]);
        }

        [Test]
        public void Face3D_FromShellAndClosedPolyline()
        {
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(10, 0, 0);
            Point3D p3 = new Point3D(10, 10, 0);
            Polygon3D poly = new Polygon3D(new[] { p1, p2, p3 });
            Face3D face1 = new Face3D(poly);
            Face3D face2 = new Face3D(poly);
            Shell shell = new Shell(new List<Face3D> { face1, face2 });

            GH_ObjectWrapper shellWrapper = new GH_ObjectWrapper(shell);
            Assert.True(shellWrapper.TryGetSAMGeometries(out List<Face3D> facesFromShell));
            Assert.Equal(2, facesFromShell.Count);

            // From closed Polyline3D
            Polyline3D closedPolyline = new Polyline3D(new[] { p1, p2, p3, p1 }, true);
            GH_ObjectWrapper polylineWrapper = new GH_ObjectWrapper(closedPolyline);
            Assert.True(polylineWrapper.TryGetSAMGeometries(out List<Face3D> facesFromPolyline));
            Assert.Single(facesFromPolyline);
        }

        [Test]
        public void Polyline3D_FromFace3D_NoInvalidCastException()
        {
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(10, 0, 0);
            Point3D p3 = new Point3D(10, 10, 0);
            Polygon3D poly = new Polygon3D(new[] { p1, p2, p3 });
            Face3D face = new Face3D(poly);

            GH_ObjectWrapper faceWrapper = new GH_ObjectWrapper(face);
            Assert.True(faceWrapper.TryGetSAMGeometries(out List<Polyline3D> polylines));
            Assert.NotEmpty(polylines);
            Assert.NotNull(polylines[0]);
        }

        [Test]
        public void Segment3D_FromFace3DAndPolyline3D()
        {
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(10, 0, 0);
            Point3D p3 = new Point3D(10, 10, 0);
            Polygon3D poly = new Polygon3D(new[] { p1, p2, p3 });
            Face3D face = new Face3D(poly);

            GH_ObjectWrapper faceWrapper = new GH_ObjectWrapper(face);
            Assert.True(faceWrapper.TryGetSAMGeometries(out List<Segment3D> segments));
            Assert.Equal(3, segments.Count);
        }

        [Test]
        public void Shell_FromMultiFaceCollection()
        {
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(10, 0, 0);
            Point3D p3 = new Point3D(10, 10, 0);
            Polygon3D poly = new Polygon3D(new[] { p1, p2, p3 });
            Face3D face1 = new Face3D(poly);
            Face3D face2 = new Face3D(poly);
            List<Face3D> faceList = new List<Face3D> { face1, face2 };

            GH_ObjectWrapper collectionWrapper = new GH_ObjectWrapper(faceList);
            Assert.True(collectionWrapper.TryGetSAMGeometries(out List<Shell> shells));
            Assert.Single(shells);
            Assert.Equal(2, shells[0].Face3Ds.Count);
        }

        [Test]
        public void PlaneAndPoint3D_FromFace3D()
        {
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(10, 0, 0);
            Point3D p3 = new Point3D(10, 10, 0);
            Polygon3D poly = new Polygon3D(new[] { p1, p2, p3 });
            Face3D face = new Face3D(poly);

            GH_ObjectWrapper faceWrapper = new GH_ObjectWrapper(face);
            Assert.True(faceWrapper.TryGetSAMGeometries(out List<SAM.Geometry.Spatial.Plane> planes));
            Assert.Single(planes);

            SAM.Geometry.Spatial.Plane plane = planes[0];
            GH_ObjectWrapper planeWrapper = new GH_ObjectWrapper(plane);
            Assert.True(planeWrapper.TryGetSAMGeometries(out List<Point3D> points));
            Assert.Single(points);
        }

        [Test]
        public void CollectionWithFirstInvalidElement_DoesNotAbortEarly()
        {
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(10, 0, 0);
            Point3D p3 = new Point3D(10, 10, 0);
            Polygon3D poly = new Polygon3D(new[] { p1, p2, p3 });
            Face3D face = new Face3D(poly);

            List<GH_ObjectWrapper> wrappers = new List<GH_ObjectWrapper>
            {
                new GH_ObjectWrapper("invalid_string_element"),
                new GH_ObjectWrapper(face)
            };

            Assert.True(wrappers.TryGetSAMGeometries(out List<Face3D> faces));
            Assert.Single(faces);
            Assert.Same(face, faces[0]);
        }

        [Test]
        public void NullAndEmptyGuards()
        {
            GH_ObjectWrapper nullWrapper = null;
            Assert.False(nullWrapper.TryGetSAMGeometries(out List<Face3D> faces1));
            Assert.Null(faces1);

            GH_ObjectWrapper emptyWrapper = new GH_ObjectWrapper(null);
            Assert.False(emptyWrapper.TryGetSAMGeometries(out List<Face3D> faces2));
            Assert.Null(faces2);

            List<GH_ObjectWrapper> nullList = null;
            Assert.False(nullList.TryGetSAMGeometries(out List<Face3D> faces3));
            Assert.Null(faces3);
        }
    }
}
