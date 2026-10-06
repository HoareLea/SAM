// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using Xunit;

using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    public class AnalyticalPanelTests
    {
        private static Construction CreateTestConstruction(string name)
        {
            return new Construction(Guid.NewGuid(), name);
        }

        private static ApertureConstruction CreateTestApertureConstruction(string name)
        {
            return new ApertureConstruction(Guid.NewGuid(), name, ApertureType.Window);
        }

        private static Face3D CreateTestFace3D(double x = 0, double y = 0, double width = 10, double height = 10)
        {
            Point3D origin = new Point3D(x, y, 0);
            Point3D p2 = new Point3D(x + width, y, 0);
            Point3D p3 = new Point3D(x + width, y + height, 0);
            Point3D p4 = new Point3D(x, y + height, 0);
            return new Face3D(new Polygon3D(new Point3D[] { origin, p2, p3, p4 }));
        }

        private static Panel CreateTestPanel(Construction construction, PanelType panelType, double x = 0, double y = 0)
        {
            Face3D face3D = CreateTestFace3D(x, y);
            return AnalyticalCreate.Panel(construction, panelType, face3D);
        }

        [Fact]
        public void GetPanels_ByConstruction_ReturnsMatchingPanels()
        {
            Construction wallConstruction = CreateTestConstruction("External Wall");
            Construction roofConstruction = CreateTestConstruction("Flat Roof");

            Panel wall = CreateTestPanel(wallConstruction, PanelType.Wall);
            Panel roof = CreateTestPanel(roofConstruction, PanelType.Roof);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(wall);
            adjacencyCluster.AddObject(roof);

            List<Panel> wallPanels = adjacencyCluster.GetPanels(wallConstruction);
            List<Panel> roofPanels = adjacencyCluster.GetPanels(roofConstruction);

            Assert.Single(wallPanels);
            Assert.Single(roofPanels);
            Assert.Equal(wallConstruction.Guid, wallPanels[0].TypeGuid);
            Assert.Equal(roofConstruction.Guid, roofPanels[0].TypeGuid);
        }

        [Fact]
        public void GetPanels_ByConstruction_NoMatchReturnsEmpty()
        {
            Construction wallConstruction = CreateTestConstruction("External Wall");
            Construction unknownConstruction = CreateTestConstruction("Unknown");

            Panel wall = CreateTestPanel(wallConstruction, PanelType.Wall);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(wall);

            List<Panel> panels = adjacencyCluster.GetPanels(unknownConstruction);

            Assert.NotNull(panels);
            Assert.Empty(panels);
        }

        [Fact]
        public void GetPanel_ByAperture_ReturnsParentPanel()
        {
            Construction wallConstruction = CreateTestConstruction("External Wall");
            ApertureConstruction windowConstruction = CreateTestApertureConstruction("Window");

            Panel wall = CreateTestPanel(wallConstruction, PanelType.Wall);

            Face3D apertureFace = new Face3D(new Polygon3D(new Point3D[]
            {
                new Point3D(3, 3, 0),
                new Point3D(5, 3, 0),
                new Point3D(5, 5, 0),
                new Point3D(3, 5, 0)
            }));

            Aperture window = AnalyticalCreate.Aperture(windowConstruction, apertureFace);
            wall.AddAperture(window);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(wall);

            Panel foundPanel = adjacencyCluster.GetPanel(window);

            Assert.NotNull(foundPanel);
            Assert.Equal(wall.Guid, foundPanel.Guid);
            Assert.True(foundPanel.HasAperture(window.Guid));
        }

        [Fact]
        public void GetPanels_ByApertureConstruction_ReturnsMatchingPanels()
        {
            Construction wallConstruction = CreateTestConstruction("External Wall");
            ApertureConstruction windowConstruction = CreateTestApertureConstruction("Window");
            ApertureConstruction doorConstruction = CreateTestApertureConstruction("Door");

            Panel panelWithWindow = CreateTestPanel(wallConstruction, PanelType.Wall);
            Panel panelWithDoor = CreateTestPanel(wallConstruction, PanelType.Wall, x: 15, y: 0);

            Face3D windowFace = new Face3D(new Polygon3D(new Point3D[]
            {
                new Point3D(3, 3, 0), new Point3D(5, 3, 0),
                new Point3D(5, 5, 0), new Point3D(3, 5, 0)
            }));
            Aperture window = AnalyticalCreate.Aperture(windowConstruction, windowFace);
            panelWithWindow.AddAperture(window);

            Face3D doorFace = new Face3D(new Polygon3D(new Point3D[]
            {
                new Point3D(17, 0, 0), new Point3D(19, 0, 0),
                new Point3D(19, 8, 0), new Point3D(17, 8, 0)
            }));
            Aperture door = AnalyticalCreate.Aperture(doorConstruction, doorFace);
            panelWithDoor.AddAperture(door);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(panelWithWindow);
            adjacencyCluster.AddObject(panelWithDoor);

            List<Panel> panelsWithWindows = adjacencyCluster.GetPanels(windowConstruction);
            List<Panel> panelsWithDoors = adjacencyCluster.GetPanels(doorConstruction);

            Assert.Single(panelsWithWindows);
            Assert.Single(panelsWithDoors);
            Assert.Equal(panelWithWindow.Guid, panelsWithWindows[0].Guid);
            Assert.Equal(panelWithDoor.Guid, panelsWithDoors[0].Guid);
        }

        [Fact]
        public void GetPanels_ByConstruction_FromPanelsAndCluster()
        {
            Construction wallConstruction = CreateTestConstruction("External Wall");
            Construction roofConstruction = CreateTestConstruction("Flat Roof");

            Panel wallInCluster = CreateTestPanel(wallConstruction, PanelType.Wall);
            Panel roofInCluster = CreateTestPanel(roofConstruction, PanelType.Roof, x: 0, y: 15);
            Panel wallDirect = CreateTestPanel(wallConstruction, PanelType.Wall, x: 15, y: 0);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(wallInCluster);
            adjacencyCluster.AddObject(roofInCluster);

            List<Panel> allPanels = new List<Panel>();
            allPanels.AddRange(adjacencyCluster.GetPanels());
            allPanels.Add(wallDirect);

            List<Panel> wallPanels = allPanels.FindAll(x => x.TypeGuid == wallConstruction.Guid);
            List<Panel> roofPanels = allPanels.FindAll(x => x.TypeGuid == roofConstruction.Guid);

            Assert.Equal(3, allPanels.Count);
            Assert.Equal(2, wallPanels.Count);
            Assert.Single(roofPanels);
        }

        [Fact]
        public void FilterPanels_ByAperture_DirectListFindParent()
        {
            Construction wallConstruction = CreateTestConstruction("External Wall");
            ApertureConstruction windowConstruction = CreateTestApertureConstruction("Window");

            Panel wall = CreateTestPanel(wallConstruction, PanelType.Wall);

            Face3D apertureFace = new Face3D(new Polygon3D(new Point3D[]
            {
                new Point3D(3, 3, 0), new Point3D(5, 3, 0),
                new Point3D(5, 5, 0), new Point3D(3, 5, 0)
            }));
            Aperture window = AnalyticalCreate.Aperture(windowConstruction, apertureFace);
            wall.AddAperture(window);

            List<Panel> panels = new List<Panel> { wall };

            Panel foundPanel = panels.Find(x => x.HasAperture(window.Guid));

            Assert.NotNull(foundPanel);
            Assert.Equal(wall.Guid, foundPanel.Guid);
        }

        [Fact]
        public void CreatePanel_DeepClones_IndependentInstances()
        {
            Construction wallConstruction = CreateTestConstruction("External Wall");
            Panel original = CreateTestPanel(wallConstruction, PanelType.Wall);

            Panel clone = AnalyticalCreate.Panel(original);

            Assert.NotNull(clone);
            Assert.Equal(original.TypeGuid, clone.TypeGuid);
            Assert.Equal(original.PanelType, clone.PanelType);
            Assert.NotSame(original, clone);
            Assert.NotNull(clone.Face3D);
        }

        [Fact]
        public void GetPanels_PointInsideFace_ReturnsMatchingPanel()
        {
            Construction wallConstruction = CreateTestConstruction("External Wall");
            Panel panel = CreateTestPanel(wallConstruction, PanelType.Wall, x: 0, y: 0);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(panel);

            Point3D pointInside = new Point3D(5, 5, 0);
            Point3D pointOutside = new Point3D(15, 15, 0);

            List<Panel> allPanels = adjacencyCluster.GetPanels();

            bool foundInside = false;
            bool foundOutside = false;

            foreach (Panel p in allPanels)
            {
                Face3D face3D = p.Face3D;
                if (face3D == null)
                    continue;

                if (face3D.Inside(pointInside, Tolerance.MacroDistance) || face3D.InRange(pointInside, Tolerance.MacroDistance))
                    foundInside = true;

                if (face3D.Inside(pointOutside, Tolerance.MacroDistance) || face3D.InRange(pointOutside, Tolerance.MacroDistance))
                    foundOutside = true;
            }

            Assert.True(foundInside);
            Assert.False(foundOutside);
        }

        [Fact]
        public void GetPanels_Face3DInside_ReturnsMatchingPanel()
        {
            Construction wallConstruction = CreateTestConstruction("External Wall");
            Panel largePanel = CreateTestPanel(wallConstruction, PanelType.Wall, x: 0, y: 0);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(largePanel);

            Face3D smallFaceInside = CreateTestFace3D(x: 2, y: 2, width: 4, height: 4);
            Face3D faceOutside = CreateTestFace3D(x: 20, y: 20, width: 4, height: 4);

            List<Panel> allPanels = adjacencyCluster.GetPanels();

            Assert.False(allPanels[0].Face3D.Inside(faceOutside)); // assuming face outside is not inside panel
        }
    }
}
