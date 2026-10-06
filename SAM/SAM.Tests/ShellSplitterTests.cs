// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Core;
using SAM.Geometry.Spatial;
using Xunit;

namespace SAM.Tests
{
    public class ShellSplitterTests
    {
        [Fact]
        public void Constructor_DefaultAndProperties_InitializedCorrectly()
        {
            ShellSplitter splitter = new ShellSplitter();

            Assert.Equal(Core.Tolerance.Distance, splitter.Tolerance_Distance);
            Assert.Equal(Core.Tolerance.Angle, splitter.Tolerance_Angle);
            Assert.Equal(Core.Tolerance.MacroDistance, splitter.Tolerance_Snap);
        }

        [Fact]
        public void Constructor_Parameterized_FiltersNullElements()
        {
            Shell shell = CreateBoxShell(0, 0, 0, 10, 10, 10);
            Face3D face3D = CreateCuttingFace(
                new Point3D(-5, -5, 5),
                new Point3D(15, -5, 5),
                new Point3D(15, 15, 5),
                new Point3D(-5, 15, 5)
            );

            List<Shell> shells = new List<Shell>() { shell, null };
            List<Face3D> face3Ds = new List<Face3D>() { null, face3D };

            ShellSplitter splitter = new ShellSplitter(shells, face3Ds);

            List<Shell> result = splitter.Split();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Add_ValidAndNull_ReturnsExpectedBool()
        {
            ShellSplitter splitter = new ShellSplitter();

            Assert.False(splitter.Add((Shell)null));
            Assert.False(splitter.Add((Face3D)null));

            Shell shell = CreateBoxShell(0, 0, 0, 10, 10, 10);
            Face3D face = CreateCuttingFace(
                new Point3D(-5, -5, 5),
                new Point3D(15, -5, 5),
                new Point3D(15, 15, 5),
                new Point3D(-5, 15, 5)
            );

            Assert.True(splitter.Add(shell));
            Assert.True(splitter.Add(face));
        }

        [Fact]
        public void Split_NullCollections_ReturnsNull()
        {
            ShellSplitter splitter = new ShellSplitter();
            Assert.Null(splitter.Split());
        }

        [Fact]
        public void Split_SingleBoxShell_HorizontalCut_SplitsIntoTwoShells()
        {
            Shell boxShell = CreateBoxShell(0, 0, 0, 10, 10, 10);
            Face3D cutFace = CreateCuttingFace(
                new Point3D(-5, -5, 5),
                new Point3D(15, -5, 5),
                new Point3D(15, 15, 5),
                new Point3D(-5, 15, 5)
            );

            ShellSplitter splitter = new ShellSplitter();
            splitter.Add(boxShell);
            splitter.Add(cutFace);

            List<Shell> subShells = splitter.Split();

            Assert.NotNull(subShells);
            Assert.Equal(2, subShells.Count);

            double originalVolume = boxShell.Volume();
            double totalSubVolume = subShells.Sum(x => x.Volume());

            Assert.True(System.Math.Abs(originalVolume - totalSubVolume) < Core.Tolerance.MacroDistance,
                $"Original volume {originalVolume} does not match split volume sum {totalSubVolume}");
        }

        [Fact]
        public void Split_SingleBoxShell_CrossCut_SplitsIntoFourShells()
        {
            Shell boxShell = CreateBoxShell(0, 0, 0, 10, 10, 10);

            // Horizontal cut at Z = 5
            Face3D cutZ = CreateCuttingFace(
                new Point3D(-5, -5, 5),
                new Point3D(15, -5, 5),
                new Point3D(15, 15, 5),
                new Point3D(-5, 15, 5)
            );

            // Vertical cut at X = 5
            Face3D cutX = CreateCuttingFace(
                new Point3D(5, -5, -5),
                new Point3D(5, 15, -5),
                new Point3D(5, 15, 15),
                new Point3D(5, -5, 15)
            );

            ShellSplitter splitter = new ShellSplitter();
            splitter.Add(boxShell);
            splitter.Add(cutZ);
            splitter.Add(cutX);

            List<Shell> subShells = splitter.Split();

            Assert.NotNull(subShells);
            Assert.Equal(4, subShells.Count);

            double originalVolume = boxShell.Volume();
            double totalSubVolume = subShells.Sum(x => x.Volume());

            Assert.True(System.Math.Abs(originalVolume - totalSubVolume) < Core.Tolerance.MacroDistance,
                $"Original volume {originalVolume} does not match split volume sum {totalSubVolume}");
        }

        [Fact]
        public void Split_NonIntersectingCut_ReturnsOriginalShell()
        {
            Shell boxShell = CreateBoxShell(0, 0, 0, 10, 10, 10);

            // Cutting face way outside at Z = 100
            Face3D cutFaceFar = CreateCuttingFace(
                new Point3D(-5, -5, 100),
                new Point3D(15, -5, 100),
                new Point3D(15, 15, 100),
                new Point3D(-5, 15, 100)
            );

            ShellSplitter splitter = new ShellSplitter();
            splitter.Add(boxShell);
            splitter.Add(cutFaceFar);

            List<Shell> subShells = splitter.Split();

            Assert.NotNull(subShells);
            Assert.Single(subShells);

            double originalVolume = boxShell.Volume();
            double resultVolume = subShells[0].Volume();

            Assert.True(System.Math.Abs(originalVolume - resultVolume) < Core.Tolerance.MacroDistance);
        }

        [Fact]
        public void Split_MultipleShells_SplitsBothConcurrently()
        {
            Shell boxA = CreateBoxShell(0, 0, 0, 10, 10, 10);
            Shell boxB = CreateBoxShell(20, 0, 0, 10, 10, 10);

            // Cutting face spanning Z = 5 across both boxes
            Face3D cutZ = CreateCuttingFace(
                new Point3D(-5, -5, 5),
                new Point3D(35, -5, 5),
                new Point3D(35, 15, 5),
                new Point3D(-5, 15, 5)
            );

            ShellSplitter splitter = new ShellSplitter();
            splitter.Add(boxA);
            splitter.Add(boxB);
            splitter.Add(cutZ);

            List<Shell> subShells = splitter.Split();

            Assert.NotNull(subShells);
            Assert.Equal(4, subShells.Count);

            double expectedTotalVolume = boxA.Volume() + boxB.Volume();
            double actualTotalVolume = subShells.Sum(x => x.Volume());

            Assert.True(System.Math.Abs(expectedTotalVolume - actualTotalVolume) < Core.Tolerance.MacroDistance);
        }

        [Fact]
        public void Extension_Split_ShellAndFace3Ds_ExtensionMethodCallSucceeds()
        {
            Shell boxShell = CreateBoxShell(0, 0, 0, 10, 10, 10);
            Face3D cutFace = CreateCuttingFace(
                new Point3D(-5, -5, 5),
                new Point3D(15, -5, 5),
                new Point3D(15, 15, 5),
                new Point3D(-5, 15, 5)
            );

            List<Shell> subShells = boxShell.Split(new Face3D[] { cutFace });

            Assert.NotNull(subShells);
            Assert.Equal(2, subShells.Count);
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

        private static Face3D CreateCuttingFace(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            return new Face3D(new Polygon3D(new Point3D[] { p0, p1, p2, p3 }));
        }
    }
}
