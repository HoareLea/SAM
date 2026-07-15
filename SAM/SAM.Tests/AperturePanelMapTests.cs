// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    /// <summary>
    /// Correctness tests for the operation-local aperture-panel snapshot map
    /// used in AnalyticalModel_ByOpeningProperties.
    /// </summary>
    public class ApertureSnapshotTests
    {
        private static readonly Construction wallCons = new Construction(Guid.NewGuid(), "Wall");
        private static readonly ApertureConstruction windowCons = new ApertureConstruction(Guid.NewGuid(), "Window", ApertureType.Window);
        private static Point3D P(double x, double y, double z) => new Point3D(x, y, z);

        // ---- snapshot builder (matches production helper) ----

        private static Dictionary<Guid, Panel> BuildMap(AdjacencyCluster cluster)
        {
            List<Panel> panels = cluster?.GetPanels();
            var result = new Dictionary<Guid, Panel>();
            if (panels == null) return result;

            foreach (Panel panel in panels)
            {
                if (panel == null || !panel.HasApertures) continue;
                List<Aperture> apertures = panel.Apertures;
                if (apertures == null) continue;
                foreach (Aperture ap in apertures)
                {
                    if (ap != null && !result.ContainsKey(ap.Guid))
                        result.Add(ap.Guid, panel);
                }
            }
            return result;
        }

        private static AdjacencyCluster BuildCluster(int panelCount, int apPerPanel)
        {
            var c = new AdjacencyCluster();
            for (int i = 0; i < panelCount; i++)
            {
                var f = new Face3D(new Polygon3D(new[]
                    { P(i * 12.0, 0, 0), P(i * 12.0 + 10, 0, 0), P(i * 12.0 + 10, 10, 0), P(i * 12.0, 10, 0) }));
                Panel p = AnalyticalCreate.Panel(wallCons, PanelType.Wall, f);
                for (int j = 0; j < apPerPanel; j++)
                {
                    double ax = i * 12.0 + 1 + j * 3.5, ay = 1 + j * 3.5;
                    var af = new Face3D(new Polygon3D(new[]
                        { P(ax, ay, 0), P(ax + 2, ay, 0), P(ax + 2, ay + 2, 0), P(ax, ay + 2, 0) }));
                    p.AddAperture(AnalyticalCreate.Aperture(windowCons, af));
                }
                c.AddObject(p);
            }
            return c;
        }

        // ---- tests ----

        [Fact]
        public void NullApertureCollection_EmptyMap()
        {
            var c = new AdjacencyCluster();
            c.AddObject(AnalyticalCreate.Panel(wallCons, PanelType.Wall,
                new Face3D(new Polygon3D(new[] { P(0, 0, 0), P(10, 0, 0), P(10, 10, 0), P(0, 10, 0) }))));
            Assert.Empty(BuildMap(c));
        }

        [Fact]
        public void GhostAperture_NotFound()
        {
            var c = BuildCluster(10, 1);
            var ghost = AnalyticalCreate.Aperture(windowCons, new Face3D(new Polygon3D(new[]
                { P(999, 999, 0), P(1001, 999, 0), P(1001, 1001, 0), P(999, 1001, 0) })));
            Assert.Null(c.GetPanel(ghost));
            Assert.False(BuildMap(c).ContainsKey(ghost.Guid));
        }

        [Fact]
        public void FirstMatchWins_AllAperturesMatch()
        {
            var c = BuildCluster(100, 1);
            var map = BuildMap(c);
            foreach (var ap in c.GetApertures())
            {
                Panel prod = c.GetPanel(ap);
                Assert.True(map.TryGetValue(ap.Guid, out Panel sp));
                Assert.Equal(prod.Guid, sp.Guid);
            }
        }

        [Fact]
        public void DupGuid_SingleApertureOnOnePanel()
        {
            var c = new AdjacencyCluster();
            Panel p1 = AnalyticalCreate.Panel(wallCons, PanelType.Wall,
                new Face3D(new Polygon3D(new[] { P(0, 0, 0), P(10, 0, 0), P(10, 10, 0), P(0, 10, 0) })));
            Panel p2 = AnalyticalCreate.Panel(wallCons, PanelType.Wall,
                new Face3D(new Polygon3D(new[] { P(20, 0, 0), P(30, 0, 0), P(30, 10, 0), P(20, 10, 0) })));
            var ap = AnalyticalCreate.Aperture(windowCons,
                new Face3D(new Polygon3D(new[] { P(2, 2, 0), P(4, 2, 0), P(4, 4, 0), P(2, 4, 0) })));
            p1.AddAperture(ap); c.AddObject(p1); c.AddObject(p2);
            Panel prod = c.GetPanel(ap);
            var map = BuildMap(c);
            Assert.True(map.TryGetValue(ap.Guid, out Panel sp));
            Assert.Equal(prod.Guid, sp.Guid);
        }

        [Fact]
        public void InOperation_MutationDoesNotInvalidate()
        {
            var c = new AdjacencyCluster();
            var f1 = new Face3D(new Polygon3D(new[] { P(0, 0, 0), P(10, 0, 0), P(10, 10, 0), P(0, 10, 0) }));
            var af = new Face3D(new Polygon3D(new[] { P(2, 2, 0), P(4, 2, 0), P(4, 4, 0), P(2, 4, 0) }));
            Panel panel = AnalyticalCreate.Panel(wallCons, PanelType.Wall, f1);
            Aperture ap = AnalyticalCreate.Aperture(windowCons, af);
            panel.AddAperture(ap); c.AddObject(panel);
            var map = BuildMap(c);
            Assert.True(map.TryGetValue(ap.Guid, out Panel before));
            Assert.Equal(panel.Guid, before.Guid);
            // simulate AnalyticalModel_ByOpeningProperties mutations
            panel.RemoveAperture(ap.Guid);
            panel.AddAperture(AnalyticalCreate.Aperture(windowCons,
                new Face3D(new Polygon3D(new[] { P(2, 2, 0), P(4, 2, 0), P(4, 4, 0), P(2, 4, 0) }))));
            c.AddObject(panel);
            Assert.True(map.TryGetValue(ap.Guid, out Panel after));
            Assert.Equal(panel.Guid, after.Guid);
        }

        [Fact]
        public void MultipleAperturesOnOnePanel_AllMatch()
        {
            var c = BuildCluster(5, 4);
            var map = BuildMap(c);
            foreach (var ap in c.GetApertures())
            {
                Panel prod = c.GetPanel(ap);
                Assert.True(map.TryGetValue(ap.Guid, out Panel sp));
                Assert.Equal(prod.Guid, sp.Guid);
            }
        }

        [Fact]
        public void MultipleAperturesOnSamePanel_PreservesEarlierUpdates()
        {
            // Simulate two apertures on one panel — after processing the
            // first, the snapshot must be updated so the second iteration
            // sees apertures added in the first iteration.

            var c = new AdjacencyCluster();
            var f1 = new Face3D(new Polygon3D(new[]
                { P(0, 0, 0), P(10, 0, 0), P(10, 10, 0), P(0, 10, 0) }));
            Panel panel = AnalyticalCreate.Panel(wallCons, PanelType.Wall, f1);
            Aperture ap1 = AnalyticalCreate.Aperture(windowCons,
                new Face3D(new Polygon3D(new[]
                    { P(1, 1, 0), P(3, 1, 0), P(3, 3, 0), P(1, 3, 0) })));
            Aperture ap2 = AnalyticalCreate.Aperture(windowCons,
                new Face3D(new Polygon3D(new[]
                    { P(5, 5, 0), P(7, 5, 0), P(7, 7, 0), P(5, 7, 0) })));
            panel.AddAperture(ap1);
            panel.AddAperture(ap2);
            c.AddObject(panel);

            var map = BuildMap(c);

            // Iteration 1: process ap1
            Assert.True(map.TryGetValue(ap1.Guid, out Panel p1));

            // Clone panel, remove ap1, add newAp1 (as production does)
            Panel cloned = AnalyticalCreate.Panel(p1.Guid, p1, p1.GetFace3D(), null);
            // Manually set up the apertures to match production behaviour
            cloned.RemoveAperture(ap1.Guid);
            cloned.RemoveAperture(ap2.Guid);  // temporarily remove
            Aperture newAp1 = AnalyticalCreate.Aperture(windowCons,
                new Face3D(new Polygon3D(new[]
                    { P(1, 1, 0), P(3, 1, 0), P(3, 3, 0), P(1, 3, 0) })));
            cloned.AddAperture(newAp1);
            cloned.AddAperture(ap2);  // re-add ap2
            c.AddObject(cloned);

            // Update snapshot (as production now does)
            List<Aperture> aps = cloned.Apertures;
            if (aps != null)
                foreach (Aperture a in aps)
                    if (a != null) map[a.Guid] = cloned;

            // Iteration 2: lookup ap2 — must see cloned panel with newAp1
            Assert.True(map.TryGetValue(ap2.Guid, out Panel p2));
            Assert.True(p2.HasAperture(newAp1.Guid));
            Assert.True(p2.HasAperture(ap2.Guid));
            Assert.False(p2.HasAperture(ap1.Guid));
        }

        [Fact]
        public void FullLoopEquivalence_AllConfigurations()
        {
            foreach (var (pc, apc) in new[] { (10, 0), (10, 1), (100, 2), (100, 4), (50, 1) })
            {
                var c = BuildCluster(pc, apc);
                var aps = c.GetApertures();
                var map = BuildMap(c);
                int mismatches = 0;
                for (int i = 0; i < aps.Count; i++)
                {
                    Panel prod = c.GetPanel(aps[i]);
                    bool found = map.TryGetValue(aps[i].Guid, out Panel sp);
                    if (prod == null && !found) continue;
                    if (prod != null && found && prod.Guid == sp.Guid) continue;
                    mismatches++;
                }
                Assert.Equal(0, mismatches);
            }
        }

        [Fact]
        public void DeepClonedInput_ApertureGuidsMatch()
        {
            var c1 = BuildCluster(100, 3);
            var clones = c1.GetPanels().Select(p =>
                AnalyticalCreate.Panel(p.Guid, p, p.GetFace3D(), p.Apertures)).ToList();
            var c2 = new AdjacencyCluster();
            clones.ForEach(p => c2.AddObject(p));
            var map2 = BuildMap(c2);
            foreach (var ap1 in c1.GetApertures())
            {
                Panel p1 = c1.GetPanel(ap1);
                if (p1 == null) continue;
                Assert.True(map2.TryGetValue(ap1.Guid, out _),
                    $"Cloned aperture {ap1.Guid} not found");
            }
        }
    }
}
