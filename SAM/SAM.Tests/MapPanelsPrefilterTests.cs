// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    public class MapPanelsPrefilterTests
    {
        #region Test oracle — original exhaustive algorithm

        private static List<Panel> MapPanels_Exhaustive(AdjacencyCluster cluster, IEnumerable<Panel> panels,
            double minArea = Tolerance.MacroDistance, double maxDistance = Tolerance.MacroDistance)
        {
            if (cluster == null || panels == null)
                return null;

            List<Panel> panels_Cluster = cluster.GetPanels();
            if (panels_Cluster == null)
                return null;

            List<Panel> result = new List<Panel>();
            if (panels.Count() == 0 || panels_Cluster.Count == 0)
                return result;

            List<Tuple<Point3D, Panel>> tuples = new List<Tuple<Point3D, Panel>>();
            foreach (Panel p in panels_Cluster)
            {
                Point3D pt = p?.GetInternalPoint3D();
                if (pt == null) continue;
                tuples.Add(new Tuple<Point3D, Panel>(pt, p));
            }

            foreach (Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if (face3D == null) continue;

                List<Panel> matched = new List<Panel>();
                foreach (Tuple<Point3D, Panel> tuple in tuples)
                {
                    double d = face3D.Distance(tuple.Item1);
                    if (d <= maxDistance)
                        matched.Add(tuple.Item2);
                }

                if (matched == null || matched.Count == 0) continue;

                for (int i = 0; i < matched.Count; i++)
                {
                    Panel mp = matched[i];
                    mp = SAM.Analytical.Create.Panel(mp.Guid, panel, mp.GetFace3D(), null, true, minArea, maxDistance);
                    cluster.AddObject(mp);
                    result.Add(mp);
                }
            }

            return result;
        }

        #endregion

        #region Helpers

        private static Panel CreateRectPanel(double x, double z, double w, double h, PanelType type, string constructionName)
        {
            var p0 = new Point3D(x, 0, z);
            var p1 = new Point3D(x + w, 0, z);
            var p2 = new Point3D(x + w, 0, z + h);
            var p3 = new Point3D(x, 0, z + h);
            var face = new Face3D(new Polygon3D(new[] { p0, p1, p2, p3 }));
            return SAM.Analytical.Create.Panel(new Construction(constructionName), type, face);
        }

        private static Panel CreateInclinedPanel(double x, double z, double w, double h, double tiltDegrees, PanelType type, string constructionName)
        {
            double rad = tiltDegrees * System.Math.PI / 180.0;
            double dz = h * System.Math.Cos(rad);
            double dy = h * System.Math.Sin(rad);
            var p0 = new Point3D(x, 0, z);
            var p1 = new Point3D(x + w, 0, z);
            var p2 = new Point3D(x + w, dy, z + dz);
            var p3 = new Point3D(x, dy, z + dz);
            var face = new Face3D(new Polygon3D(new[] { p0, p1, p2, p3 }));
            return SAM.Analytical.Create.Panel(new Construction(constructionName), type, face);
        }

        private static AdjacencyCluster BuildSimpleCluster(IEnumerable<Panel> destinationPanels)
        {
            var cluster = new AdjacencyCluster();
            foreach (var p in destinationPanels)
                cluster.AddObject(p);
            return cluster;
        }

        private static void AssertResultsIdentical(List<Panel> exhaustive, List<Panel> optimized)
        {
            Assert.Equal(exhaustive?.Count ?? 0, optimized?.Count ?? 0);

            if (exhaustive == null || exhaustive.Count == 0)
                return;

            for (int i = 0; i < exhaustive.Count; i++)
            {
                Assert.Equal(exhaustive[i].Guid, optimized[i].Guid);
                Assert.Equal(exhaustive[i].PanelType, optimized[i].PanelType);
                var ef = exhaustive[i].GetFace3D();
                var of = optimized[i].GetFace3D();
                Assert.NotNull(ef);
                Assert.NotNull(of);
                Assert.Equal(exhaustive[i].HasApertures, optimized[i].HasApertures);
            }
        }

        private void FullEquivalenceTest(List<Panel> sourcePanels, List<Panel> destPanels,
            double minArea = 0.01, double maxDistance = 0.5)
        {
            var clusterEx = BuildSimpleCluster(destPanels);
            var clusterOpt = BuildSimpleCluster(destPanels);

            var exhaustive = MapPanels_Exhaustive(clusterEx, sourcePanels, minArea, maxDistance);
            var optimized = clusterOpt.MapPanels(sourcePanels, minArea, maxDistance);

            AssertResultsIdentical(exhaustive, optimized);

            // Verify cluster contents match
            Assert.Equal(clusterEx.GetPanels().Count, clusterOpt.GetPanels().Count);
            var exGuids = clusterEx.GetPanels().Select(x => x.Guid).OrderBy(g => g).ToList();
            var optGuids = clusterOpt.GetPanels().Select(x => x.Guid).OrderBy(g => g).ToList();
            Assert.Equal(exGuids, optGuids);
        }

        #endregion

        #region Correctness tests

        [Fact]
        public void ParallelRectPanels_ShouldMatch()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "A") };
            var dst = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "B") };
            FullEquivalenceTest(src, dst);
        }

        [Fact]
        public void RotatedPanels_ShouldMatch()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "A") };
            var dst = new List<Panel> { CreateRectPanel(5, 0, 3, 3, PanelType.Wall, "B") };
            FullEquivalenceTest(src, dst);
        }

        [Fact]
        public void InclinedPanels_ShouldMatch()
        {
            var src = new List<Panel> { CreateInclinedPanel(0, 0, 3, 4, 45, PanelType.Roof, "A") };
            var dst = new List<Panel> { CreateInclinedPanel(0, 0, 3, 4, 45, PanelType.Roof, "B") };
            FullEquivalenceTest(src, dst, 0.01, 0.5);
        }

        [Fact]
        public void VerticalPanels_ShouldMatch()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "A") };
            var dst = new List<Panel>
            {
                CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "B"),
                CreateRectPanel(10, 0, 3, 3, PanelType.Wall, "C")
            };
            FullEquivalenceTest(src, dst);
        }

        [Fact]
        public void ConcaveFaces_ShouldMatch()
        {
            // L-shaped face
            var pts = new Point3D[]
            {
                new(0, 0, 0), new(6, 0, 0), new(6, 0, 2), new(2, 0, 2),
                new(2, 0, 4), new(0, 0, 4)
            };
            var src = new List<Panel>
            {
                SAM.Analytical.Create.Panel(new Construction("L"), PanelType.Floor, new Face3D(new Polygon3D(pts)))
            };
            var dst = new List<Panel>
            {
                SAM.Analytical.Create.Panel(new Construction("D"), PanelType.Floor, new Face3D(new Polygon3D(pts)))
            };
            FullEquivalenceTest(src, dst);
        }

        [Fact]
        public void DestinationPointInsideFace_ShouldMatch()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 5, 5, PanelType.Floor, "A") };
            var dst = new List<Panel>
            {
                SAM.Analytical.Create.Panel(
                    new Construction("B"), PanelType.Floor,
                    new Face3D(new Polygon3D(new Point3D[]
                    {
                        new(1, 0, 1), new(2, 0, 1), new(2, 0, 2), new(1, 0, 2)
                    })))
            };
            FullEquivalenceTest(src, dst);
        }

        [Fact]
        public void PointAtExactMaxDistance_ShouldMatch()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 1, 1, PanelType.Floor, "A") };
            var dst = new List<Panel> { CreateRectPanel(0, 0, 1, 1, PanelType.Floor, "B") };
            double maxDist = 0.5;
            FullEquivalenceTest(src, dst, 0.01, maxDist);
        }

        [Fact]
        public void PointJustInsideMaxDistance_ShouldMatch()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 1, 1, PanelType.Floor, "A") };
            var dst = new List<Panel> { CreateRectPanel(0.49, 0, 1, 1, PanelType.Floor, "B") };
            FullEquivalenceTest(src, dst, 0.01, 0.5);
        }

        [Fact]
        public void PointJustOutsideMaxDistance_ShouldMatch()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 1, 1, PanelType.Floor, "A") };
            var dst = new List<Panel> { CreateRectPanel(2.0, 0, 1, 1, PanelType.Floor, "B") };
            FullEquivalenceTest(src, dst, 0.01, 0.5);
        }

        [Fact]
        public void PointInsideExpandedBox_OutsideExactDistance_ShouldMatch()
        {
            // Destination is diagonally above the source — within the expanded AABB (x+z within range)
            // but elevation (y) difference exceeds maxDistance
            var src = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Floor, "A") };
            var dst = new List<Panel> { CreateRectPanel(2, 0, 3, 3, PanelType.Floor, "B") };
            FullEquivalenceTest(src, dst, 0.01, 0.5);
        }

        [Fact]
        public void ZeroMaxDistance_ShouldMatch()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "A") };
            var dst = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "B") };
            FullEquivalenceTest(src, dst, 0.01, 0.0);
        }

        [Fact]
        public void EmptySource_ShouldReturnEmpty()
        {
            var src = new List<Panel>();
            var dst = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "B") };
            var cluster = BuildSimpleCluster(dst);
            var result = cluster.MapPanels(src);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void EmptyDestination_ShouldReturnNull()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "A") };
            var dst = new List<Panel>();
            var cluster = BuildSimpleCluster(dst);
            var result = cluster.MapPanels(src);
            Assert.Null(result);
        }

        [Fact]
        public void NullSource_ShouldReturnNull()
        {
            var dst = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "B") };
            var cluster = BuildSimpleCluster(dst);
            var result = cluster.MapPanels(null);
            Assert.Null(result);
        }

        [Fact]
        public void MultipleDestinationToSingleSource_ShouldMatch()
        {
            var src = new List<Panel> { CreateRectPanel(0, 0, 6, 6, PanelType.Floor, "Large") };
            var dst = new List<Panel>
            {
                CreateRectPanel(0, 0, 3, 3, PanelType.Floor, "A"),
                CreateRectPanel(3, 0, 3, 3, PanelType.Floor, "B"),
                CreateRectPanel(0, 3, 3, 3, PanelType.Floor, "C"),
                CreateRectPanel(3, 3, 3, 3, PanelType.Floor, "D"),
            };
            FullEquivalenceTest(src, dst);
        }

        [Fact]
        public void DenselyOverlappingPanels_ShouldMatch()
        {
            var src = new List<Panel>();
            var dst = new List<Panel>();
            for (int i = 0; i < 20; i++)
            {
                double x = i * 0.1;
                src.Add(CreateRectPanel(x, x, 3, 3, PanelType.Wall, $"S{i}"));
                dst.Add(CreateRectPanel(x + 0.05, x + 0.05, 3, 3, PanelType.Wall, $"D{i}"));
            }
            FullEquivalenceTest(src, dst, 0.01, 0.5);
        }

        [Fact]
        public void SpatiallySeparatedPanels_ShouldMatch()
        {
            var src = new List<Panel>();
            var dst = new List<Panel>();
            double spacing = 20.0;
            for (int i = 0; i < 30; i++)
            {
                double x = i * spacing;
                src.Add(CreateRectPanel(x, 0, 3, 3, PanelType.Wall, $"S{i}"));
                dst.Add(CreateRectPanel(x + 1, 0, 3, 3, PanelType.Wall, $"D{i}"));
            }
            FullEquivalenceTest(src, dst, 0.01, 0.5);
        }

        [Fact]
        public void NullPanelInSource_SkippedNotCrashed()
        {
            var src = new List<Panel> { null, CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "A") };
            var dst = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "B") };
            var cluster = BuildSimpleCluster(dst);
            var result = cluster.MapPanels(src);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void PanelWithoutFace3D_SkippedNotCrashed()
        {
            var panelNoGeometry = SAM.Analytical.Create.Panel(new Construction("NoGeo"), PanelType.Wall);
            var src = new List<Panel> { panelNoGeometry, CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "A") };
            var dst = new List<Panel> { CreateRectPanel(0, 0, 3, 3, PanelType.Wall, "B") };
            var cluster = BuildSimpleCluster(dst);
            var result = cluster.MapPanels(src);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        #endregion
    }
}
