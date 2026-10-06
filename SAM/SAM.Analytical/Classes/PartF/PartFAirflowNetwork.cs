// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// The internal airflow network of one dwelling: which of its spaces are connected to which, how much
    /// air has to move between them, and through what.
    /// <para>
    /// Approved Document F, Volume 1: Dwellings (2021 edition, for use in England) paragraph 1.25
    /// (page 10) requires internal doors to allow air to flow through the dwelling. In a balanced
    /// mechanical ventilation with heat recovery design, supply enters the habitable rooms
    /// (paragraph 1.67) and extract leaves from the wet rooms (paragraph 1.70), so every litre supplied
    /// has to cross the dwelling to reach an extract terminal. This class works out where.
    /// </para>
    ///
    /// <para><b>What forms an edge.</b> Two spaces are connected where an internal separating element
    /// puts them next to each other. The connection, not the door, is the backbone: an analytical model
    /// frequently carries the partition between a studio and its bathroom without carrying a door
    /// aperture in it, and treating a missing aperture as a missing adjacency would silently report a
    /// dwelling as disconnected when it is only under-modelled. Door apertures attach to the connection
    /// where they exist, and their absence is reported as an unrepresented opening rather than as an
    /// absent route.</para>
    ///
    /// <para><b>What is excluded.</b> An element with only one adjacent space is external. A connection to
    /// a space outside this dwelling - a communal corridor, a neighbouring flat, an explicitly excluded
    /// zone - is never an edge, so no dwelling's transfer air can cross into another's. That exclusion is
    /// what stops a block of flats being solved as one large dwelling.</para>
    ///
    /// <para><b>How the flows are worked out.</b> Every space carries a net airflow: supply less extract.
    /// Air is routed from each net-supply space to each net-extract space in proportion to how much each
    /// has to give and take, along the shortest connected path between them, and the contributions are
    /// summed on each connection.</para>
    ///
    /// <para>
    /// Where the dwelling's connections form a tree this reproduces the one answer conservation of air
    /// flow allows, exactly. For a connection that splits the dwelling into sides A and B, conservation
    /// requires the flow across it to be the total net flow on side A. The proportional allocation gives
    /// (S(A)xD(B) - S(B)xD(A)) / T, and with S(A)+S(B) = D(A)+D(B) = T that is S(A) - D(A), which is the
    /// same number. So on a tree the result is not a strategy at all, and it is reported as
    /// <see cref="PartFTransferRouteStatus.UniquelyDetermined"/>.
    /// </para>
    /// <para>
    /// Where the connections contain a loop there is genuinely more than one valid answer, because
    /// Approved Document F does not say how air divides between parallel paths. The same allocation is
    /// applied, the total is still correct, and every route is reported as
    /// <see cref="PartFTransferRouteStatus.AllocationStrategy"/> so the engineer knows the split is a
    /// design decision they may override rather than a regulatory value.
    /// </para>
    /// </summary>
    public class PartFAirflowNetwork
    {
        /// <summary>
        /// Flow rates below this [l/s] are treated as zero when deciding whether a space is a source or a
        /// sink, so that rounding in the terminal allocation cannot invent a route carrying a millionth
        /// of a litre per second.
        /// </summary>
        public const double Tolerance_Lps = 1e-9;

        private readonly Dictionary<Guid, Space> dictionary_Space = [];

        /// <summary>Neighbours of each space, in deterministic order.</summary>
        private readonly Dictionary<Guid, List<Guid>> dictionary_Neighbour = [];

        /// <summary>The door apertures found on each connection, keyed by the ordered space pair.</summary>
        private readonly Dictionary<(Guid, Guid), List<Aperture>> dictionary_Aperture = [];

        /// <summary>Which connected component each space belongs to.</summary>
        private readonly Dictionary<Guid, int> dictionary_Component = [];

        /// <summary>True where the component is a tree, so its flows are uniquely determined.</summary>
        private readonly Dictionary<int, bool> dictionary_IsTree = [];

        /// <summary>
        /// Builds the network of one dwelling.
        /// </summary>
        /// <param name="adjacencyCluster">The model the adjacencies are read from.</param>
        /// <param name="spaces">The spaces of this dwelling, and only of this dwelling.</param>
        public PartFAirflowNetwork(AdjacencyCluster adjacencyCluster, IEnumerable<Space> spaces)
        {
            //Ordered by name and then by guid so that every list this class produces - neighbours,
            //shortest paths, route schedules - is the same on every run and on every machine. An
            //allocation that depended on dictionary ordering would give a different door flow each time
            //the model was opened.
            List<Space> spaces_Ordered = [.. (spaces ?? []).Where(x => x is not null).OrderBy(x => x.Name, StringComparer.Ordinal).ThenBy(x => x.Guid)];

            foreach (Space space in spaces_Ordered)
            {
                dictionary_Space[space.Guid] = space;
                dictionary_Neighbour[space.Guid] = [];
            }

            Build(adjacencyCluster);
            FindComponents();
        }

        /// <summary>The spaces of this dwelling, in deterministic order.</summary>
        public List<Space> Spaces
        {
            get { return [.. dictionary_Space.Values.OrderBy(x => x.Name, StringComparer.Ordinal).ThenBy(x => x.Guid)]; }
        }

        /// <summary>
        /// Every connection in the dwelling as an ordered space pair, in deterministic order. The pair is
        /// stored with the lexicographically smaller guid first; direction is a property of the flow, not
        /// of the connection.
        /// </summary>
        public List<(Guid, Guid)> Connections
        {
            get { return [.. dictionary_Aperture.Keys.OrderBy(x => Name(x.Item1), StringComparer.Ordinal).ThenBy(x => Name(x.Item2), StringComparer.Ordinal)]; }
        }

        /// <summary>The door apertures modelled on one connection. Empty where none is modelled.</summary>
        public List<Aperture> Apertures((Guid, Guid) connection)
        {
            return dictionary_Aperture.TryGetValue(connection, out List<Aperture> result) ? result : [];
        }

        /// <summary>The space behind a guid, or null where it is not part of this dwelling.</summary>
        public Space Space(Guid guid)
        {
            return dictionary_Space.TryGetValue(guid, out Space result) ? result : null;
        }

        /// <summary>The name of a space, or its guid where the space carries none.</summary>
        public string Name(Guid guid)
        {
            Space space = Space(guid);

            return string.IsNullOrWhiteSpace(space?.Name) ? guid.ToString() : space.Name;
        }

        /// <summary>
        /// True where the connections containing this space form a tree, so conservation of air flow fixes
        /// every flow in it without any engineering choice.
        /// </summary>
        public bool IsUniquelyDetermined(Guid guid)
        {
            return dictionary_Component.TryGetValue(guid, out int component) && dictionary_IsTree.TryGetValue(component, out bool result) && result;
        }

        /// <summary>
        /// Solves the network for one set of space net airflows and returns the flow on each connection.
        /// <para>
        /// The result is keyed by the ordered connection pair. A positive value flows from
        /// <c>Item1</c> to <c>Item2</c>; a negative value flows the other way.
        /// </para>
        /// </summary>
        /// <param name="netFlow_Lps">
        /// Net airflow [l/s] of each space: supply less extract. Positive where the space has air to pass
        /// on, negative where it has to draw air in.
        /// </param>
        /// <param name="guids_Unreachable">
        /// Spaces with a net airflow that could not be routed anywhere, because nothing of the opposite
        /// sign is reachable from them.
        /// </param>
        public Dictionary<(Guid, Guid), double> Solve(Func<Guid, double> netFlow_Lps, out List<Guid> guids_Unreachable)
        {
            Dictionary<(Guid, Guid), double> result = [];
            guids_Unreachable = [];

            foreach ((Guid, Guid) connection in dictionary_Aperture.Keys)
            {
                result[connection] = 0;
            }

            if (netFlow_Lps is null)
            {
                return result;
            }

            List<Guid> guids = [.. Spaces.ConvertAll(x => x.Guid)];

            List<Guid> guids_Source = [.. guids.Where(x => netFlow_Lps(x) > Tolerance_Lps)];
            List<Guid> guids_Sink = [.. guids.Where(x => netFlow_Lps(x) < -Tolerance_Lps)];

            if (guids_Source.Count == 0 || guids_Sink.Count == 0)
            {
                //Nothing to route. A dwelling with supply but no extract, or the reverse, is a modelling
                //problem the calculator reports on its own; it is not this class's to diagnose.
                guids_Unreachable.AddRange(guids_Source);
                guids_Unreachable.AddRange(guids_Sink);
                return result;
            }

            foreach (Guid guid_Source in guids_Source)
            {
                //Only sinks in the same component can take air from this source. Total is recomputed per
                //source rather than once, so a dwelling that is accidentally modelled as two disconnected
                //halves still balances within each half instead of silently losing air between them.
                List<Guid> guids_Sink_Reachable = [.. guids_Sink.Where(x => SameComponent(guid_Source, x))];

                double demand_Total = guids_Sink_Reachable.Sum(x => -netFlow_Lps(x));
                if (guids_Sink_Reachable.Count == 0 || demand_Total <= Tolerance_Lps)
                {
                    guids_Unreachable.Add(guid_Source);
                    continue;
                }

                double supply = netFlow_Lps(guid_Source);

                foreach (Guid guid_Sink in guids_Sink_Reachable)
                {
                    double share = supply * (-netFlow_Lps(guid_Sink) / demand_Total);
                    if (System.Math.Abs(share) <= Tolerance_Lps)
                    {
                        continue;
                    }

                    List<Guid> path = ShortestPath(guid_Source, guid_Sink);
                    if (path is null)
                    {
                        continue;
                    }

                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        (Guid, Guid) connection = Connection(path[i], path[i + 1]);

                        //Stored against the canonical pair, so a contribution travelling the other way
                        //subtracts rather than being lost.
                        result[connection] += connection.Item1 == path[i] ? share : -share;
                    }
                }
            }

            foreach (Guid guid_Sink in guids_Sink)
            {
                if (!guids_Source.Any(x => SameComponent(x, guid_Sink)))
                {
                    guids_Unreachable.Add(guid_Sink);
                }
            }

            return result;
        }

        /// <summary>
        /// Clear width [mm] of a door aperture, taken as the horizontal extent of its bounding box.
        /// <para>
        /// This is the width of the modelled opening. It is an upper bound on the clear width of the door
        /// leaf, because a frame reduces it and an analytical aperture does not model one, so it is used
        /// only to convert a provided undercut into a free area and never on its own as evidence of
        /// compliance.
        /// </para>
        /// </summary>
        public static double? ClearDoorWidth_mm(Aperture aperture)
        {
            BoundingBox3D boundingBox3D = aperture?.GetFace3D()?.GetBoundingBox();
            if (boundingBox3D is null)
            {
                return null;
            }

            double dX = boundingBox3D.Max.X - boundingBox3D.Min.X;
            double dY = boundingBox3D.Max.Y - boundingBox3D.Min.Y;

            double width_M = System.Math.Sqrt((dX * dX) + (dY * dY));
            if (double.IsNaN(width_M) || double.IsInfinity(width_M) || width_M <= 0)
            {
                return null;
            }

            return width_M * 1000;
        }

        private bool SameComponent(Guid guid_1, Guid guid_2)
        {
            return dictionary_Component.TryGetValue(guid_1, out int component_1)
                && dictionary_Component.TryGetValue(guid_2, out int component_2)
                && component_1 == component_2;
        }

        /// <summary>The canonical ordered pair for a connection between two spaces.</summary>
        private static (Guid, Guid) Connection(Guid guid_1, Guid guid_2)
        {
            return guid_1.CompareTo(guid_2) <= 0 ? (guid_1, guid_2) : (guid_2, guid_1);
        }

        /// <summary>
        /// Breadth-first shortest path, with neighbours visited in the deterministic order established in
        /// the constructor so that two equally short paths always resolve the same way.
        /// </summary>
        private List<Guid> ShortestPath(Guid guid_From, Guid guid_To)
        {
            if (guid_From == guid_To)
            {
                return [guid_From];
            }

            Dictionary<Guid, Guid> dictionary_Previous = [];
            HashSet<Guid> guids_Visited = [guid_From];
            Queue<Guid> queue = new();
            queue.Enqueue(guid_From);

            while (queue.Count != 0)
            {
                Guid guid = queue.Dequeue();

                foreach (Guid guid_Neighbour in dictionary_Neighbour[guid])
                {
                    if (!guids_Visited.Add(guid_Neighbour))
                    {
                        continue;
                    }

                    dictionary_Previous[guid_Neighbour] = guid;

                    if (guid_Neighbour == guid_To)
                    {
                        List<Guid> result = [guid_To];
                        Guid guid_Current = guid_To;
                        while (dictionary_Previous.TryGetValue(guid_Current, out Guid guid_Previous))
                        {
                            result.Insert(0, guid_Previous);
                            guid_Current = guid_Previous;
                        }

                        return result;
                    }

                    queue.Enqueue(guid_Neighbour);
                }
            }

            return null;
        }

        private void Build(AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster is null)
            {
                return;
            }

            //Panels are visited in a fixed order so the aperture list on each connection is stable.
            List<Panel> panels = [.. (adjacencyCluster.GetPanels() ?? []).Where(x => x is not null).OrderBy(x => x.Guid)];

            foreach (Panel panel in panels)
            {
                List<Space> spaces_Panel = adjacencyCluster.GetSpaces(panel);
                if (spaces_Panel is null || spaces_Panel.Count != 2)
                {
                    //One adjacent space means the element is external; none means it is orphaned. Neither
                    //moves air between two rooms of this dwelling.
                    continue;
                }

                Guid guid_1 = spaces_Panel[0].Guid;
                Guid guid_2 = spaces_Panel[1].Guid;

                if (guid_1 == guid_2)
                {
                    continue;
                }

                //Both ends must be inside this dwelling. A partition onto a communal corridor, a
                //neighbouring flat or an excluded zone is deliberately not an edge, so transfer air can
                //never cross a dwelling boundary.
                if (!dictionary_Space.ContainsKey(guid_1) || !dictionary_Space.ContainsKey(guid_2))
                {
                    continue;
                }

                (Guid, Guid) connection = Connection(guid_1, guid_2);

                if (!dictionary_Aperture.TryGetValue(connection, out List<Aperture> apertures))
                {
                    apertures = [];
                    dictionary_Aperture[connection] = apertures;

                    dictionary_Neighbour[guid_1].Add(guid_2);
                    dictionary_Neighbour[guid_2].Add(guid_1);
                }

                //Two rooms separated by several partitions are still one adjacency, so the doors in all of
                //them belong to the same connection rather than creating parallel edges.
                foreach (Aperture aperture in panel.Apertures ?? [])
                {
                    if (aperture is not null && aperture.ApertureType == ApertureType.Door)
                    {
                        apertures.Add(aperture);
                    }
                }
            }

            foreach (Guid guid in dictionary_Neighbour.Keys.ToList())
            {
                dictionary_Neighbour[guid] = [.. dictionary_Neighbour[guid].OrderBy(x => Name(x), StringComparer.Ordinal).ThenBy(x => x)];
            }
        }

        /// <summary>
        /// Labels the connected components and records which of them are trees. A component with exactly
        /// one fewer connection than it has spaces is a tree, and every flow inside it is fixed by
        /// conservation of air flow alone.
        /// </summary>
        private void FindComponents()
        {
            int component = 0;

            foreach (Space space in Spaces)
            {
                if (dictionary_Component.ContainsKey(space.Guid))
                {
                    continue;
                }

                List<Guid> guids_Component = [];

                Queue<Guid> queue = new();
                queue.Enqueue(space.Guid);
                dictionary_Component[space.Guid] = component;

                while (queue.Count != 0)
                {
                    Guid guid = queue.Dequeue();
                    guids_Component.Add(guid);

                    foreach (Guid guid_Neighbour in dictionary_Neighbour[guid])
                    {
                        if (dictionary_Component.ContainsKey(guid_Neighbour))
                        {
                            continue;
                        }

                        dictionary_Component[guid_Neighbour] = component;
                        queue.Enqueue(guid_Neighbour);
                    }
                }

                HashSet<Guid> guids_Set = [.. guids_Component];
                int connectionCount = dictionary_Aperture.Keys.Count(x => guids_Set.Contains(x.Item1));

                dictionary_IsTree[component] = connectionCount == guids_Component.Count - 1;

                component++;
            }
        }
    }
}
