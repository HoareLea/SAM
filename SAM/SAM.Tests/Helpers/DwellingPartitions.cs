// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests.Helpers
{
    /// <summary>
    /// Adds the internal partitions that make a fixture's rooms adjacent to one another.
    /// <para>
    /// A dwelling whose rooms share no separating element has no internal airflow network, so the air a
    /// balanced heat recovery design supplies into a bedroom has nowhere to go and the air it extracts from
    /// a bathroom has nowhere to come from. TAS refuses to simulate such a building, and the Part O Base
    /// MVHR preparation refuses to produce it - so a fixture that exercises the preparation has to be a
    /// dwelling rather than a bag of loose rooms.
    /// </para>
    /// <para>
    /// <see cref="PartFModel"/> does the same thing while building its own spaces; this is for fixtures
    /// that already have theirs.
    /// </para>
    /// </summary>
    public static class DwellingPartitions
    {
        /// <summary>
        /// Connects every one of <paramref name="names"/> to <paramref name="name_Hub"/> through its own
        /// internal partition - a flat whose rooms all open off one space.
        /// <para>
        /// A star has no loop in it, so conservation of air flow fixes every transfer flow on its own and
        /// nothing about the routing is an allocation the test would have to know about.
        /// </para>
        /// </summary>
        public static void Star(AdjacencyCluster adjacencyCluster, string name_Hub, params string[] names)
        {
            double x = 0;

            foreach (string name in names ?? [])
            {
                Partition(adjacencyCluster, name_Hub, name, x);

                x += 10;
            }
        }

        /// <summary>
        /// Makes two named spaces adjacent through an internal partition. Does nothing where either name
        /// does not resolve, so a fixture cannot be made to depend on a room it has not built.
        /// </summary>
        public static void Partition(AdjacencyCluster adjacencyCluster, string name_1, string name_2, double x)
        {
            List<Space> spaces = adjacencyCluster?.GetSpaces();
            if (spaces is null)
            {
                return;
            }

            Space space_1 = spaces.Find(y => y.Name == name_1);
            Space space_2 = spaces.Find(y => y.Name == name_2);

            if (space_1 is null || space_2 is null)
            {
                return;
            }

            Panel panel = AnalyticalCreate.Panel(new Construction(Guid.NewGuid(), "Internal Partition"), PanelType.WallInternal, Wall(x));

            adjacencyCluster.AddObject(panel);
            adjacencyCluster.AddRelation(space_1, panel);
            adjacencyCluster.AddRelation(space_2, panel);
        }

        private static Face3D Wall(double x)
        {
            return new Face3D(new Polygon3D(
            [
                new Point3D(x, 0, 0),
                new Point3D(x + 4, 0, 0),
                new Point3D(x + 4, 0, 3),
                new Point3D(x, 0, 3),
            ]));
        }
    }
}
