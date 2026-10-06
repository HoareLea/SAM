// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Regression tests for <see cref="Modify.RemoveInvalidAirPanels(AdjacencyCluster)"/>.
    /// </summary>
    /// <remarks>
    /// The method used to treat a missing construction as air-like, which made any construction-less panel a
    /// removal candidate in its own right. A panel's construction comes from the default construction library,
    /// and that library is read from the installed SAM resources directory rather than from anything the build
    /// produces, so on a machine without that installation every panel is created without one. The old rule
    /// therefore deleted every external boundary in the model - each bounds a single space - and the shell
    /// creation path returned an empty cluster. These tests pin the boundary between the two behaviours.
    /// </remarks>
    public class RemoveInvalidAirPanelsTests
    {
        [Fact]
        public void RemoveInvalidAirPanels_NonAirPanelWithoutConstruction_IsKept()
        {
            AdjacencyCluster adjacencyCluster = Cluster(out Space space);
            Panel panel = AddPanel(adjacencyCluster, space, PanelType.WallExternal, null);

            List<System.Guid> guids = adjacencyCluster.RemoveInvalidAirPanels();

            Assert.Empty(guids);
            Assert.NotNull(adjacencyCluster.GetObject<Panel>(panel.Guid));
        }

        [Fact]
        public void RemoveInvalidAirPanels_AirPanelBoundingOneSpace_IsRemoved()
        {
            AdjacencyCluster adjacencyCluster = Cluster(out Space space);
            Panel panel = AddPanel(adjacencyCluster, space, PanelType.Air, new Construction("Test Construction"));

            List<System.Guid> guids = adjacencyCluster.RemoveInvalidAirPanels();

            Assert.Equal(panel.Guid, Assert.Single(guids));
            Assert.Null(adjacencyCluster.GetObject<Panel>(panel.Guid));
        }

        /// <summary>An air panel between two spaces is a real virtual boundary and must survive.</summary>
        [Fact]
        public void RemoveInvalidAirPanels_AirPanelBoundingTwoSpaces_IsKept()
        {
            AdjacencyCluster adjacencyCluster = Cluster(out Space space);

            Space space_Second = new Space("Second Space", new Point3D(1, 1, 3));
            adjacencyCluster.AddObject(space_Second);

            Panel panel = AddPanel(adjacencyCluster, space, PanelType.Air, null);
            adjacencyCluster.AddRelation(space_Second, panel);

            List<System.Guid> guids = adjacencyCluster.RemoveInvalidAirPanels();

            Assert.Empty(guids);
            Assert.NotNull(adjacencyCluster.GetObject<Panel>(panel.Guid));
        }

        /// <summary>
        /// The whole point of the change: a model whose panels all lack a construction, as happens wherever
        /// the default construction library is unavailable, must not be emptied.
        /// </summary>
        [Fact]
        public void RemoveInvalidAirPanels_ModelWithoutConstructions_IsNotEmptied()
        {
            AdjacencyCluster adjacencyCluster = Cluster(out Space space);

            AddPanel(adjacencyCluster, space, PanelType.SlabOnGrade, null);
            AddPanel(adjacencyCluster, space, PanelType.Roof, null);
            AddPanel(adjacencyCluster, space, PanelType.WallExternal, null);

            List<System.Guid> guids = adjacencyCluster.RemoveInvalidAirPanels();

            Assert.Empty(guids);
            Assert.Equal(3, adjacencyCluster.GetPanels().Count);
            Assert.Single(adjacencyCluster.GetSpaces());
        }

        private static AdjacencyCluster Cluster(out Space space)
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            space = new Space("Test Space", new Point3D(1, 1, 1));
            adjacencyCluster.AddObject(space);
            return adjacencyCluster;
        }

        private static Panel AddPanel(AdjacencyCluster adjacencyCluster, Space space, PanelType panelType, Construction construction)
        {
            Panel panel = Analytical.Create.Panel(construction, panelType, Quad());
            Assert.NotNull(panel);
            adjacencyCluster.AddObject(panel);
            adjacencyCluster.AddRelation(space, panel);
            return panel;
        }

        private static Face3D Quad()
        {
            Polygon3D polygon3D = Geometry.Spatial.Create.Polygon3D(
                new[]
                {
                    new Point3D(0, 0, 0),
                    new Point3D(2, 0, 0),
                    new Point3D(2, 2, 0),
                    new Point3D(0, 2, 0)
                },
                Core.Tolerance.Distance);

            return new Face3D(polygon3D);
        }
    }
}
