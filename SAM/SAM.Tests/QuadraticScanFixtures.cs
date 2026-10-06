// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Tests
{
    /// <summary>
    /// Deterministic fixtures for the quadratic-scan audit. Everything here is laid out on a
    /// fixed lattice - no randomness - so a run at 2n is strictly a superset-shaped version of
    /// the run at n and the measured growth is attributable to collection size alone.
    /// </summary>
    internal static class QuadraticScanFixtures
    {
        /// <summary>
        /// n axis-aligned closed box shells spread over a square lattice with a gap between
        /// them, so no pair is ever in range of another. This isolates the cost of the
        /// broad-phase scan itself from the cost of the geometric work it guards.
        /// </summary>
        public static List<Shell> DisjointBoxShells(int count, double size = 4, double spacing = 10)
        {
            int columns = (int)System.Math.Ceiling(System.Math.Sqrt(count));

            List<Shell> result = new List<Shell>();
            for (int i = 0; i < count; i++)
            {
                double x = (i % columns) * spacing;
                double y = (i / columns) * spacing;
                result.Add(BoxShell(x, y, 0, size, size, 3));
            }

            return result;
        }

        /// <summary>
        /// n vertical wall panels, each on its own plane, spread over a lattice so that no two
        /// are coplanar-within-offset. Drives MergeOverlapPanels_Vertical.
        /// </summary>
        public static List<Panel> DisjointWallPanels(int count, double size = 3, double spacing = 10)
        {
            int columns = (int)System.Math.Ceiling(System.Math.Sqrt(count));

            List<Panel> result = new List<Panel>();
            for (int i = 0; i < count; i++)
            {
                double x = (i % columns) * spacing;
                double y = (i / columns) * spacing;

                Face3D face3D = Quad(
                    new Point3D(x, y, 0),
                    new Point3D(x + size, y, 0),
                    new Point3D(x + size, y, size),
                    new Point3D(x, y, size));

                result.Add(Analytical.Create.Panel(Analytical.Query.DefaultConstruction(PanelType.Wall), PanelType.Wall, face3D));
            }

            return result;
        }

        /// <summary>
        /// n horizontal floor panels laid out edge to edge on one elevation. They land in a
        /// single elevation group, so the whole group goes through the NTS polygon pass of
        /// MergeOverlapPanels_Horizontal together.
        /// </summary>
        public static List<Panel> CoplanarFloorPanels(int count, double size = 4, double spacing = 5)
        {
            int columns = (int)System.Math.Ceiling(System.Math.Sqrt(count));

            List<Panel> result = new List<Panel>();
            for (int i = 0; i < count; i++)
            {
                double x = (i % columns) * spacing;
                double y = (i / columns) * spacing;

                Face3D face3D = Quad(
                    new Point3D(x, y, 0),
                    new Point3D(x + size, y, 0),
                    new Point3D(x + size, y + size, 0),
                    new Point3D(x, y + size, 0));

                result.Add(Analytical.Create.Panel(Analytical.Query.DefaultConstruction(PanelType.Floor), PanelType.Floor, face3D));
            }

            return result;
        }

        public static Shell BoxShell(double x0, double y0, double z0, double width, double depth, double height)
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

            return new Shell(new List<Face3D>()
            {
                Quad(p000, p100, p110, p010),
                Quad(p001, p011, p111, p101),
                Quad(p000, p001, p101, p100),
                Quad(p010, p110, p111, p011),
                Quad(p000, p010, p011, p001),
                Quad(p100, p101, p111, p110),
            });
        }

        public static Face3D Quad(Point3D point3D_1, Point3D point3D_2, Point3D point3D_3, Point3D point3D_4)
        {
            return new Face3D(new Polygon3D(new Point3D[] { point3D_1, point3D_2, point3D_3, point3D_4 }));
        }
    }
}
