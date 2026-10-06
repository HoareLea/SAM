// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Spatial;
using Xunit;

namespace SAM.Tests
{
    public class ShellIsClosedTests
    {
        [Fact]
        public void IsClosed_ExactBox_ReturnsTrue()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);

            Assert.True(shell.IsClosed(Core.Tolerance.Distance));
        }

        [Fact]
        public void IsClosed_OpenBoxMissingFace_ReturnsFalse()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;
            face3Ds.RemoveAt(0);

            Assert.False(new Shell(face3Ds).IsClosed(Core.Tolerance.Distance));
        }

        [Fact]
        public void IsClosed_ShellWithTwoFaces_ReturnsFalse()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;

            Assert.False(new Shell(face3Ds.Take(2)).IsClosed(Core.Tolerance.Distance));
        }

        [Fact]
        public void IsClosed_SplitWallTJunction_ReturnsTrue()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 2, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;

            int index = face3Ds.FindIndex(x =>
            {
                BoundingBox3D boundingBox3D = x.GetBoundingBox();
                return boundingBox3D != null && System.Math.Abs(boundingBox3D.Min.Y) < 1e-9 && System.Math.Abs(boundingBox3D.Max.Y) < 1e-9;
            });

            face3Ds.RemoveAt(index);
            face3Ds.Add(CreateQuad(new Point3D(0, 0, 0), new Point3D(0, 0, 1), new Point3D(1, 0, 1), new Point3D(1, 0, 0)));
            face3Ds.Add(CreateQuad(new Point3D(1, 0, 0), new Point3D(1, 0, 1), new Point3D(2, 0, 1), new Point3D(2, 0, 0)));

            Assert.True(new Shell(face3Ds).IsClosed(Core.Tolerance.Distance));
        }

        [Fact]
        public void IsClosed_MidEdgeTJunction_ReturnsTrue()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;

            int index = face3Ds.FindIndex(x =>
            {
                BoundingBox3D boundingBox3D = x.GetBoundingBox();
                return boundingBox3D != null && System.Math.Abs(boundingBox3D.Min.Y) < 1e-9 && System.Math.Abs(boundingBox3D.Max.Y) < 1e-9;
            });

            face3Ds.RemoveAt(index);
            face3Ds.Add(CreateQuad(new Point3D(0, 0, 0), new Point3D(0, 0, 1), new Point3D(0.5, 0, 1), new Point3D(0.5, 0, 0)));
            face3Ds.Add(CreateQuad(new Point3D(0.5, 0, 0), new Point3D(0.5, 0, 1), new Point3D(1, 0, 1), new Point3D(1, 0, 0)));

            Assert.True(new Shell(face3Ds).IsClosed(Core.Tolerance.Distance));
        }

        [Fact]
        public void IsClosed_GapAboveTolerance_ReturnsFalse()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;
            face3Ds[0] = (Face3D)face3Ds[0].GetMoved(new Vector3D(0, 0, 5e-4));

            Assert.False(new Shell(face3Ds).IsClosed(1e-6));
        }

        [Fact]
        public void IsClosed_GapWithinTolerance_ReturnsTrue()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;
            face3Ds[0] = (Face3D)face3Ds[0].GetMoved(new Vector3D(0, 0, 5e-4));

            Assert.True(new Shell(face3Ds).IsClosed(1e-3));
        }

        [Fact]
        public void IsClosed_GlazedHole_ReturnsTrue()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;

            int index = face3Ds.FindIndex(x =>
            {
                BoundingBox3D boundingBox3D = x.GetBoundingBox();
                return boundingBox3D != null && System.Math.Abs(boundingBox3D.Min.Z - 1) < 1e-9 && System.Math.Abs(boundingBox3D.Max.Z - 1) < 1e-9;
            });

            Face3D top = face3Ds[index];
            Polygon3D hole = new Polygon3D(new Point3D[]
            {
                new Point3D(0.4, 0.4, 1),
                new Point3D(0.4, 0.6, 1),
                new Point3D(0.6, 0.6, 1),
                new Point3D(0.6, 0.4, 1),
            });

            face3Ds[index] = Face3D.Create(new List<IClosedPlanar3D> { top.GetExternalEdge3D(), hole });
            face3Ds.Add(new Face3D(hole));

            Assert.True(new Shell(face3Ds).IsClosed(Core.Tolerance.Distance));
        }

        [Fact]
        public void IsClosed_OpenHole_ReturnsFalse()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;

            int index = face3Ds.FindIndex(x =>
            {
                BoundingBox3D boundingBox3D = x.GetBoundingBox();
                return boundingBox3D != null && System.Math.Abs(boundingBox3D.Min.Z - 1) < 1e-9 && System.Math.Abs(boundingBox3D.Max.Z - 1) < 1e-9;
            });

            Face3D top = face3Ds[index];
            Polygon3D hole = new Polygon3D(new Point3D[]
            {
                new Point3D(0.4, 0.4, 1),
                new Point3D(0.4, 0.6, 1),
                new Point3D(0.6, 0.6, 1),
                new Point3D(0.6, 0.4, 1),
            });

            face3Ds[index] = Face3D.Create(new List<IClosedPlanar3D> { top.GetExternalEdge3D(), hole });

            Assert.False(new Shell(face3Ds).IsClosed(Core.Tolerance.Distance));
        }

        [Fact]
        public void IsClosed_NonManifoldFin_ReturnsFalse()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;
            face3Ds.Add(CreateQuad(new Point3D(0, 0, 1), new Point3D(1, 0, 1), new Point3D(1, 0, 2), new Point3D(0, 0, 2)));

            Assert.False(new Shell(face3Ds).IsClosed(Core.Tolerance.Distance));
        }

        [Fact]
        public void NakedSegment3Ds_OpenBox_ReturnsBoundaryEdges()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;
            face3Ds.RemoveAt(0);

            List<Segment3D> segment3Ds = new Shell(face3Ds).NakedSegment3Ds(int.MaxValue, Core.Tolerance.Distance);

            Assert.NotNull(segment3Ds);
            Assert.Equal(4, segment3Ds.Count);
            Assert.All(segment3Ds, x => Assert.True(System.Math.Abs(x.GetLength() - 1) < Core.Tolerance.Distance));
        }

        [Fact]
        public void NakedSegment3Ds_PartialCoverage_ReturnsUncoveredPiece()
        {
            Face3D face3D_Base = CreateQuad(new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(1, 1, 0), new Point3D(0, 1, 0));
            Face3D face3D_Wall = CreateQuad(new Point3D(0, 0, 0), new Point3D(0.5, 0, 0), new Point3D(0.5, 0, 1), new Point3D(0, 0, 1));

            Shell shell = new Shell(new List<Face3D> { face3D_Base, face3D_Wall });
            List<Segment3D> segment3Ds = shell.NakedSegment3Ds(int.MaxValue, Core.Tolerance.Distance);

            Assert.NotNull(segment3Ds);

            Segment3D uncovered = segment3Ds.FirstOrDefault(x => System.Math.Abs(x.Mid().Y) < 1e-9 && System.Math.Abs(x.Mid().Z) < 1e-9);
            Assert.NotNull(uncovered);
            Assert.True(System.Math.Abs(uncovered.GetLength() - 0.5) < 2e-6);
            Assert.True(System.Math.Abs(uncovered.Mid().X - 0.75) < 2e-6);
        }

        [Fact]
        public void NakedSegment3Ds_MaxCount_RespectsLimit()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;
            face3Ds.RemoveAt(0);

            List<Segment3D> segment3Ds = new Shell(face3Ds).NakedSegment3Ds(2, Core.Tolerance.Distance);

            Assert.NotNull(segment3Ds);
            Assert.Equal(2, segment3Ds.Count);
        }

        [Fact]
        public void NakedSegment3Ds_ExactBox_ReturnsEmpty()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);

            List<Segment3D> segment3Ds = shell.NakedSegment3Ds(int.MaxValue, Core.Tolerance.Distance);

            Assert.NotNull(segment3Ds);
            Assert.Empty(segment3Ds);
        }

        [Fact]
        public void IsClosed_CornerOffsetAxialAndPerpendicularAboveTolerance_ReturnsFalse()
        {
            // Front wall moved by (0.9e-6 axial, 0.9e-6 perpendicular): the Euclidean
            // corner gap is sqrt(0.9^2 + 0.9^2) e-6 = 1.27e-6 > 1e-6, so the shell
            // must report open even though each component is below tolerance.
            Shell shell = CreateCornerOffsetBoxShell(0.9e-6, 0.9e-6);

            Assert.False(shell.IsClosed(1e-6));
        }

        [Fact]
        public void IsClosed_CornerOffsetAxialAndPerpendicularWithinTolerance_ReturnsTrue()
        {
            // Same construction at (0.6e-6, 0.6e-6): Euclidean corner gap 0.85e-6 < 1e-6.
            Shell shell = CreateCornerOffsetBoxShell(0.6e-6, 0.6e-6);

            Assert.True(shell.IsClosed(1e-6));
        }

        [Fact]
        public void IsClosed_PerpendicularOffsetAboveTolerance_ReturnsFalse()
        {
            Shell shell = CreateCornerOffsetBoxShell(0, 1.5e-6);

            Assert.False(shell.IsClosed(1e-6));
        }

        [Fact]
        public void IsClosed_PerpendicularOffsetWithinTolerance_ReturnsTrue()
        {
            Shell shell = CreateCornerOffsetBoxShell(0, 0.5e-6);

            Assert.True(shell.IsClosed(1e-6));
        }

        [Fact]
        public void IsClosed_PerpendicularOffsetExactlyTolerance_ReturnsTrue()
        {
            Shell shell = CreateCornerOffsetBoxShell(0, 1e-6);

            Assert.True(shell.IsClosed(1e-6));
        }

        [Fact]
        public void IsClosed_AxialOffsetExactlyTolerance_ReturnsTrue()
        {
            Shell shell = CreateCornerOffsetBoxShell(1e-6, 0);

            Assert.True(shell.IsClosed(1e-6));
        }

        [Fact]
        public void IsClosed_MidEdgeGapBeyondCombinedTolerance_ReturnsFalse()
        {
            // The two front-wall quads cover the long bottom edge of the single bottom
            // face, but their bottom edges are separated by 2.1 x tolerance: after
            // capsule expansion a ~0.1 x tolerance region stays farther than tolerance
            // from every neighbouring edge and must report open.
            Shell shell = CreateMidEdgeGapBoxShell(2.1e-3);

            Assert.False(shell.IsClosed(1e-3));
        }

        [Fact]
        public void NakedSegment3Ds_MidEdgeGapBeyondCombinedTolerance_ReportsSubTolerancePiece()
        {
            Shell shell = CreateMidEdgeGapBoxShell(2.1e-3);

            List<Segment3D> segment3Ds = shell.NakedSegment3Ds(int.MaxValue, 1e-3);

            Assert.NotNull(segment3Ds);

            Segment3D piece = segment3Ds.FirstOrDefault(x => x.GetLength() < 1e-3 && System.Math.Abs(x.Mid().Y) < 1e-9 && System.Math.Abs(x.Mid().Z) < 1e-9);
            Assert.NotNull(piece);
            Assert.True(System.Math.Abs(piece.GetLength() - 0.1e-3) < 2e-5);
        }

        [Fact]
        public void IsClosed_LongDiagonalEdgesAmongShortEdges_ReturnsTrueWithinTimeBound()
        {
            // 200 x 5 x 5 box with its long axis along (1,1,1): the four ~200 long
            // diagonal edges span thousands of uniform-grid cells per axis among the
            // short 5 long edges (median cell size), exercising the max-cells guard.
            // Without the guard, insertion would create millions of cell entries.
            Shell shell = CreateLongDiagonalBoxShell(200, 5);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool result = shell.IsClosed(Core.Tolerance.Distance);
            stopwatch.Stop();

            Assert.True(result);
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, $"Expected bounded runtime, took {stopwatch.ElapsedMilliseconds} ms");
        }

        private static Shell CreateCornerOffsetBoxShell(double axial, double perpendicular)
        {
            Shell shell = CreateBoxShell(0, 0, 0, 1, 1, 1);
            List<Face3D> face3Ds = shell.Face3Ds;

            int index = face3Ds.FindIndex(x =>
            {
                BoundingBox3D boundingBox3D = x.GetBoundingBox();
                return boundingBox3D != null && System.Math.Abs(boundingBox3D.Min.Y) < 1e-9 && System.Math.Abs(boundingBox3D.Max.Y) < 1e-9;
            });

            face3Ds[index] = (Face3D)face3Ds[index].GetMoved(new Vector3D(axial, 0, -perpendicular));
            return new Shell(face3Ds);
        }

        private static Shell CreateMidEdgeGapBoxShell(double separation)
        {
            return new Shell(new List<Face3D>()
            {
                CreateQuad(new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 1, 0), new Point3D(0, 1, 0)),
                CreateQuad(new Point3D(0, 0, 1), new Point3D(0, 1, 1), new Point3D(10, 1, 1), new Point3D(10, 0, 1)),
                CreateQuad(new Point3D(0, 0, 0), new Point3D(0, 0, 1), new Point3D(5, 0, 1), new Point3D(5, 0, 0)),
                CreateQuad(new Point3D(5 + separation, 0, 0), new Point3D(5 + separation, 0, 1), new Point3D(10, 0, 1), new Point3D(10, 0, 0)),
                CreateQuad(new Point3D(10, 1, 0), new Point3D(10, 1, 1), new Point3D(0, 1, 1), new Point3D(0, 1, 0)),
                CreateQuad(new Point3D(0, 1, 0), new Point3D(0, 1, 1), new Point3D(0, 0, 1), new Point3D(0, 0, 0)),
                CreateQuad(new Point3D(10, 0, 0), new Point3D(10, 0, 1), new Point3D(10, 1, 1), new Point3D(10, 1, 0)),
            });
        }

        private static Shell CreateLongDiagonalBoxShell(double length, double side)
        {
            Vector3D u = new Vector3D(1, 1, 1).GetNormalized();
            Vector3D v = new Vector3D(1, -1, 0).GetNormalized();
            Vector3D w = u.CrossProduct(v).GetNormalized();

            Point3D Point(double a, double b, double c)
            {
                return new Point3D(
                    u.X * a + v.X * b + w.X * c,
                    u.Y * a + v.Y * b + w.Y * c,
                    u.Z * a + v.Z * b + w.Z * c);
            }

            return new Shell(new List<Face3D>()
            {
                CreateQuad(Point(0, 0, 0), Point(length, 0, 0), Point(length, side, 0), Point(0, side, 0)),
                CreateQuad(Point(0, 0, side), Point(0, side, side), Point(length, side, side), Point(length, 0, side)),
                CreateQuad(Point(0, 0, 0), Point(0, 0, side), Point(length, 0, side), Point(length, 0, 0)),
                CreateQuad(Point(length, 0, 0), Point(length, 0, side), Point(length, side, side), Point(length, side, 0)),
                CreateQuad(Point(length, side, 0), Point(length, side, side), Point(0, side, side), Point(0, side, 0)),
                CreateQuad(Point(0, side, 0), Point(0, side, side), Point(0, 0, side), Point(0, 0, 0)),
            });
        }

        private static Shell CreateBoxShell(double x0, double y0, double z0, double width, double depth, double height)
        {
            double x1 = x0 + width;
            double y1 = y0 + depth;
            double z1 = z0 + height;

            Point3D p000 = new Point3D(x0, y0, z0);
            Point3D p100 = new Point3D(x1, y0, z0);
            Point3D p110 = new Point3D(x1, y1, z0);
            Point3D p010 = new Point3D(x0, y1, z0);

            Point3D p001 = new Point3D(x0, y0, z1);
            Point3D p101 = new Point3D(x1, y0, z1);
            Point3D p111 = new Point3D(x1, y1, z1);
            Point3D p011 = new Point3D(x0, y1, z1);

            List<Face3D> faces = new List<Face3D>()
            {
                new Face3D(new Polygon3D(new Point3D[] { p000, p100, p110, p010 })),
                new Face3D(new Polygon3D(new Point3D[] { p001, p011, p111, p101 })),
                new Face3D(new Polygon3D(new Point3D[] { p000, p001, p101, p100 })),
                new Face3D(new Polygon3D(new Point3D[] { p010, p110, p111, p011 })),
                new Face3D(new Polygon3D(new Point3D[] { p000, p010, p011, p001 })),
                new Face3D(new Polygon3D(new Point3D[] { p100, p101, p111, p110 })),
            };

            return new Shell(faces);
        }

        private static Face3D CreateQuad(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            return new Face3D(new Polygon3D(new Point3D[] { p0, p1, p2, p3 }));
        }
    }
}
