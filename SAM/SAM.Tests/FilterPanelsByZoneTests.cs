// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    public class FilterPanelsByZoneTests
    {
        private static Construction CreateTestConstruction(string name)
        {
            return new Construction(Guid.NewGuid(), name);
        }

        private static Face3D CreateTestFace3D(double x, double y, double width = 10, double height = 3)
        {
            Point3D p1 = new Point3D(x, y, 0);
            Point3D p2 = new Point3D(x + width, y, 0);
            Point3D p3 = new Point3D(x + width, y, height);
            Point3D p4 = new Point3D(x, y, height);
            return new Face3D(new Polygon3D(new Point3D[] { p1, p2, p3, p4 }));
        }

        private static Panel CreateTestPanel(Construction construction, PanelType panelType, double x, double y)
        {
            return AnalyticalCreate.Panel(construction, panelType, CreateTestFace3D(x, y));
        }

        private static Zone CreateTestZone(string name, string zoneCategory)
        {
            Zone zone = new Zone(name);
            zone.SetValue(ZoneParameter.ZoneCategory, zoneCategory);
            return zone;
        }

        private static Space CreateTestSpace(string name)
        {
            return new Space(name);
        }

        private static void Relate(AdjacencyCluster adjacencyCluster, Zone zone, params Space[] spaces)
        {
            adjacencyCluster.AddObject(zone);
            foreach (Space space in spaces)
            {
                adjacencyCluster.AddObject(space);
                adjacencyCluster.AddRelation(zone, space);
            }
        }

        private static void Relate(AdjacencyCluster adjacencyCluster, Panel panel, params Space[] spaces)
        {
            adjacencyCluster.AddObject(panel);
            foreach (Space space in spaces)
            {
                adjacencyCluster.AddObject(space);
                adjacencyCluster.AddRelation(space, panel);
            }
        }

        [Fact]
        public void FilterPanelsByZone_OneZoneTwoSpaces_PartitionIsWithinZone()
        {
            Construction construction = CreateTestConstruction("Internal Partition");

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat01 = CreateTestZone("Flat 01", "Flat");
            Space bedroom = CreateTestSpace("Bedroom");
            Space livingRoom = CreateTestSpace("Living Room");
            Relate(adjacencyCluster, flat01, bedroom, livingRoom);

            Panel partition = CreateTestPanel(construction, PanelType.WallInternal, 0, 0);
            Relate(adjacencyCluster, partition, bedroom, livingRoom);

            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(new[] { flat01 }, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.Empty(externalPanels);
            Assert.Single(internalPanelsWithinZone);
            Assert.Equal(partition.Guid, internalPanelsWithinZone[0].Guid);
            Assert.Empty(internalPanelsToSpacesOutsideZone);
        }

        [Fact]
        public void FilterPanelsByZone_TwoFlatsSameCategory_PartyWallIsOutsideZone()
        {
            Construction construction = CreateTestConstruction("Party Wall");

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat01 = CreateTestZone("Flat 01", "Flat");
            Zone flat02 = CreateTestZone("Flat 02", "Flat");
            Space space01 = CreateTestSpace("Flat 01 Space");
            Space space02 = CreateTestSpace("Flat 02 Space");
            Relate(adjacencyCluster, flat01, space01);
            Relate(adjacencyCluster, flat02, space02);

            Panel partyWall = CreateTestPanel(construction, PanelType.WallInternal, 0, 0);
            Relate(adjacencyCluster, partyWall, space01, space02);

            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(new[] { flat01, flat02 }, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.Empty(externalPanels);
            Assert.Empty(internalPanelsWithinZone);
            Assert.Single(internalPanelsToSpacesOutsideZone);
            Assert.Equal(partyWall.Guid, internalPanelsToSpacesOutsideZone[0].Guid);
        }

        [Fact]
        public void FilterPanelsByZone_FlatAdjacentToCorridor_PanelIsOutsideZone()
        {
            Construction construction = CreateTestConstruction("Corridor Wall");

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat = CreateTestZone("Flat 01", "Flat");
            Zone corridor = CreateTestZone("Corridor", "Corridor");
            Space flatSpace = CreateTestSpace("Flat Space");
            Space corridorSpace = CreateTestSpace("Corridor Space");
            Relate(adjacencyCluster, flat, flatSpace);
            Relate(adjacencyCluster, corridor, corridorSpace);

            Panel panel = CreateTestPanel(construction, PanelType.WallInternal, 0, 0);
            Relate(adjacencyCluster, panel, flatSpace, corridorSpace);

            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(new[] { flat }, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.Empty(externalPanels);
            Assert.Empty(internalPanelsWithinZone);
            Assert.Single(internalPanelsToSpacesOutsideZone);
            Assert.Equal(panel.Guid, internalPanelsToSpacesOutsideZone[0].Guid);
        }

        [Fact]
        public void FilterPanelsByZone_ExternalFlatPanel_IsExternalOnly()
        {
            Construction construction = CreateTestConstruction("External Wall");

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat = CreateTestZone("Flat 01", "Flat");
            Space space = CreateTestSpace("Flat Space");
            Relate(adjacencyCluster, flat, space);

            Panel externalWall = CreateTestPanel(construction, PanelType.WallExternal, 0, 0);
            Relate(adjacencyCluster, externalWall, space);

            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(new[] { flat }, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.Single(externalPanels);
            Assert.Equal(externalWall.Guid, externalPanels[0].Guid);
            Assert.Empty(internalPanelsWithinZone);
            Assert.Empty(internalPanelsToSpacesOutsideZone);
        }

        [Fact]
        public void FilterPanelsByZone_MultipleFlats_PartitionsWithinZoneNoDuplicates()
        {
            Construction construction = CreateTestConstruction("Internal Partition");

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat01 = CreateTestZone("Flat 01", "Flat");
            Zone flat02 = CreateTestZone("Flat 02", "Flat");

            Space flat01SpaceA = CreateTestSpace("Flat 01 Bedroom");
            Space flat01SpaceB = CreateTestSpace("Flat 01 Living Room");
            Space flat02SpaceA = CreateTestSpace("Flat 02 Bedroom");
            Space flat02SpaceB = CreateTestSpace("Flat 02 Living Room");

            Relate(adjacencyCluster, flat01, flat01SpaceA, flat01SpaceB);
            Relate(adjacencyCluster, flat02, flat02SpaceA, flat02SpaceB);

            Panel partition01 = CreateTestPanel(construction, PanelType.WallInternal, 0, 0);
            Relate(adjacencyCluster, partition01, flat01SpaceA, flat01SpaceB);

            Panel partition02 = CreateTestPanel(construction, PanelType.WallInternal, 20, 0);
            Relate(adjacencyCluster, partition02, flat02SpaceA, flat02SpaceB);

            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(new[] { flat01, flat02 }, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.Empty(externalPanels);
            Assert.Equal(2, internalPanelsWithinZone.Count);
            Assert.Contains(internalPanelsWithinZone, x => x.Guid == partition01.Guid);
            Assert.Contains(internalPanelsWithinZone, x => x.Guid == partition02.Guid);
            Assert.Equal(2, internalPanelsWithinZone.Select(x => x.Guid).Distinct().Count());
            Assert.Empty(internalPanelsToSpacesOutsideZone);
        }

        [Fact]
        public void FilterPanelsByZone_UnzonedAdjacentSpace_PanelIsOutsideZone()
        {
            Construction construction = CreateTestConstruction("Internal Wall");

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat = CreateTestZone("Flat 01", "Flat");
            Space flatSpace = CreateTestSpace("Flat Space");
            Space unzonedSpace = CreateTestSpace("Unzoned Space");
            Relate(adjacencyCluster, flat, flatSpace);
            adjacencyCluster.AddObject(unzonedSpace);

            Panel panel = CreateTestPanel(construction, PanelType.WallInternal, 0, 0);
            Relate(adjacencyCluster, panel, flatSpace, unzonedSpace);

            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(new[] { flat }, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.Empty(externalPanels);
            Assert.Empty(internalPanelsWithinZone);
            Assert.Single(internalPanelsToSpacesOutsideZone);
            Assert.Equal(panel.Guid, internalPanelsToSpacesOutsideZone[0].Guid);
        }

        [Fact]
        public void FilterPanelsByZone_DuplicateDiscoveryFromBothSides_PanelReturnedOnce()
        {
            Construction construction = CreateTestConstruction("Corridor Wall");

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat01 = CreateTestZone("Flat 01", "Flat");
            Zone flat02 = CreateTestZone("Flat 02", "Flat");
            Space space01 = CreateTestSpace("Flat 01 Space");
            Space space02 = CreateTestSpace("Flat 02 Space");
            Relate(adjacencyCluster, flat01, space01);
            Relate(adjacencyCluster, flat02, space02);

            Panel partyWall = CreateTestPanel(construction, PanelType.WallInternal, 0, 0);
            Relate(adjacencyCluster, partyWall, space01, space02);

            // both Zones are matched, so the Panel is discoverable from either side
            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(new[] { flat01, flat02 }, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.Single(internalPanelsToSpacesOutsideZone);
        }

        [Fact]
        public void FilterPanelsByZone_NoMatchingZones_ReturnsEmptyLists()
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(new List<Zone>(), out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.NotNull(externalPanels);
            Assert.Empty(externalPanels);
            Assert.NotNull(internalPanelsWithinZone);
            Assert.Empty(internalPanelsWithinZone);
            Assert.NotNull(internalPanelsToSpacesOutsideZone);
            Assert.Empty(internalPanelsToSpacesOutsideZone);
        }

        [Fact]
        public void FilterPanelsByZone_NullAdjacencyClusterOrZones_ReturnsEmptyLists()
        {
            List<Panel> externalPanels = Analytical.Query.FilterPanelsByZone(null, null, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.NotNull(externalPanels);
            Assert.Empty(externalPanels);
            Assert.NotNull(internalPanelsWithinZone);
            Assert.Empty(internalPanelsWithinZone);
            Assert.NotNull(internalPanelsToSpacesOutsideZone);
            Assert.Empty(internalPanelsToSpacesOutsideZone);
        }

        [Fact]
        public void ZonesByCategoryName_CaseInsensitiveAndDeduplicated_ReturnsDistinctZones()
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat01 = CreateTestZone("Flat 01", "Flat");
            Zone flat02 = CreateTestZone("Flat 02", "Flat");
            Zone corridor = CreateTestZone("Corridor", "Corridor");
            adjacencyCluster.AddObject(flat01);
            adjacencyCluster.AddObject(flat02);
            adjacencyCluster.AddObject(corridor);

            List<Zone> zones = adjacencyCluster.ZonesByCategoryName(new[] { " flat ", "FLAT", "flat" });

            Assert.Equal(2, zones.Count);
            Assert.Contains(zones, x => x.Guid == flat01.Guid);
            Assert.Contains(zones, x => x.Guid == flat02.Guid);
        }

        [Fact]
        public void ZonesByCategoryName_BlankAndNullNames_ReturnsEmptyList()
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(CreateTestZone("Flat 01", "Flat"));

            List<Zone> zones = adjacencyCluster.ZonesByCategoryName(new[] { null, "   ", "" });

            Assert.NotNull(zones);
            Assert.Empty(zones);
        }

        [Fact]
        public void FilterPanelsByZone_OneAdjacentSpace_IsExternalOnly()
        {
            Construction construction = CreateTestConstruction("Internal Wall");

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat = CreateTestZone("Flat 01", "Flat");
            Space space = CreateTestSpace("Flat Space");
            Relate(adjacencyCluster, flat, space);

            // PanelType is "internal" by name, but the Panel is only related to a single Space - genuinely one-sided.
            Panel panel = CreateTestPanel(construction, PanelType.WallInternal, 0, 0);
            Relate(adjacencyCluster, panel, space);

            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(new[] { flat }, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.DoesNotContain(internalPanelsWithinZone, x => x.Guid == panel.Guid);
            Assert.DoesNotContain(internalPanelsToSpacesOutsideZone, x => x.Guid == panel.Guid);
            Assert.Contains(externalPanels, x => x.Guid == panel.Guid);
        }

        [Fact]
        public void ZonesByCategoryName_StoredCategoryHasWhitespace_TrimmedBeforeComparison()
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat = CreateTestZone("Flat 01", " Flat ");
            adjacencyCluster.AddObject(flat);

            List<Zone> zones = adjacencyCluster.ZonesByCategoryName(new[] { "Flat" });

            Assert.Single(zones);
            Assert.Equal(flat.Guid, zones[0].Guid);
        }

        [Fact]
        public void FilterPanelsByZone_MultipleCategoryNames_FlatCorridorPanelIsOutsideZoneOnce()
        {
            Construction construction = CreateTestConstruction("Corridor Wall");

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            Zone flat = CreateTestZone("Flat 01", "Flat");
            Zone corridor = CreateTestZone("Corridor", "Corridor");
            Space flatSpace = CreateTestSpace("Flat Space");
            Space corridorSpace = CreateTestSpace("Corridor Space");
            Relate(adjacencyCluster, flat, flatSpace);
            Relate(adjacencyCluster, corridor, corridorSpace);

            Panel panel = CreateTestPanel(construction, PanelType.WallInternal, 0, 0);
            Relate(adjacencyCluster, panel, flatSpace, corridorSpace);

            List<Zone> zones = adjacencyCluster.ZonesByCategoryName(new[] { "Flat", "Corridor" });
            Assert.Equal(2, zones.Count);

            List<Panel> externalPanels = adjacencyCluster.FilterPanelsByZone(zones, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone);

            Assert.Empty(externalPanels);
            Assert.DoesNotContain(internalPanelsWithinZone, x => x.Guid == panel.Guid);
            Assert.Single(internalPanelsToSpacesOutsideZone);
            Assert.Equal(panel.Guid, internalPanelsToSpacesOutsideZone[0].Guid);
        }
    }
}
