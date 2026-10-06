// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The spaces the dwelling's Approved Document F <b>transfer air</b> is routed over: the spaces a
        /// mechanical system actually serves, <b>plus every other space of the dwellings those spaces belong
        /// to</b> - the internal hall, the landing, the lobby that carries the air between them and that
        /// Approved Document F never sized a terminal for.
        ///
        /// <para><b>Why the served spaces alone are not the dwelling</b></para>
        /// <para>
        /// A <see cref="VentilationSystem"/> is related only to the spaces that carry a design terminal, and
        /// that relation is correct - a unit that moves no air into a hall does not serve it. But paragraph
        /// 1.25's transfer air crosses that hall: a bedroom supplied with 20 l/s passes it into the hall,
        /// and the hall divides it between the bathroom and the kitchen that are extracted. Solving the
        /// network over the served spaces alone deletes the middle of that route, and the bedroom and the
        /// wet rooms are then reported as having no internal connection at all.
        /// </para>
        ///
        /// <para><b>Why it is not simply every space in the model</b></para>
        /// <para>
        /// A communal corridor, a stair, a landlord area or a neighbouring flat is not part of this
        /// dwelling, and letting the solver route through one would carry a flat's supply air into the
        /// common parts - or, worse, use the corridor as a shortcut between two dwellings that share nothing
        /// but a wall. The boundary is asked for rather than guessed at.
        /// </para>
        ///
        /// <para><b>The authority</b></para>
        /// <para>
        /// <see cref="PartFDwellingZones(IEnumerable{Zone})"/>, which is the single source of the
        /// dwelling-selection policy and what <c>PartFCalculator</c> itself sizes with - so a space this
        /// calls part of a dwelling is exactly a space the Part F calculation sized as part of that
        /// dwelling, and the two cannot drift apart. Membership is the model's own <c>Zone</c> to
        /// <c>Space</c> relation. Nothing is inferred from geometry: two rooms being next to each other says
        /// nothing about whose home they are in.
        /// </para>
        /// <para>
        /// Where the model carries <b>no zones at all</b> there is no dwelling structure to read and the
        /// calculation's whole-model mode applies - <c>PartFCalculator.Calculate()</c> sizes every space "as
        /// a single dwelling" - so every space is in scope and a note says so. Where the model carries zones
        /// but a served space belongs to none of them, that space stays in scope on its own and nothing is
        /// expanded around it: it is served, so it must still balance, but the model has said nothing about
        /// which dwelling it is in.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model. <b>Not modified.</b></param>
        /// <param name="spaces_Served">
        /// The spaces the ventilation system serves - the ones carrying a design terminal.
        /// </param>
        /// <param name="notes">How the scope was decided, and what it added.</param>
        /// <returns>The scoped spaces in deterministic order, or null where no model was supplied.</returns>
        public static List<Space> PartFTransferAirSpaces(this AdjacencyCluster adjacencyCluster, IEnumerable<Space> spaces_Served, out List<string> notes)
        {
            notes = [];

            if (adjacencyCluster is null)
            {
                return null;
            }

            //One snapshot of the model's space identities, built from the single GetSpaces() this already
            //made. The served spaces are resolved through it rather than searched for one at a time: the
            //search is linear and the served set is the whole of a dwelling, so doing it per space made this
            //quadratic in the model.
            PartFIndex partFIndex = new(adjacencyCluster);

            List<Space> spaces_Cluster = partFIndex.Spaces;

            //Resolved against the cluster so that a caller holding a detached copy of a space still gets the
            //instance the relations are keyed on.
            Dictionary<Guid, Space> dictionary_Result = [];

            foreach (Space space in spaces_Served ?? [])
            {
                Space space_Cluster = space is null ? null : partFIndex.Space(space.Guid);
                if (space_Cluster is not null)
                {
                    dictionary_Result[space_Cluster.Guid] = space_Cluster;
                }
            }

            if (dictionary_Result.Count == 0)
            {
                return [];
            }

            List<Zone> zones = adjacencyCluster.GetZones() ?? [];

            if (zones.Count == 0)
            {
                //No dwelling structure at all. This is the Part F calculation's whole-model mode, in which
                //the model IS one dwelling, so every space of it is an internal space of that dwelling.
                foreach (Space space in spaces_Cluster)
                {
                    if (space is not null)
                    {
                        dictionary_Result[space.Guid] = space;
                    }
                }

                notes.Add(string.Format("The model carries no zones, so it was treated as a single dwelling - the same scope the Approved Document F calculation sizes a zone-less model at - and all {0} of its spaces are internal spaces the transfer air may route through.", dictionary_Result.Count));

                return OrderedTransferAirSpaces(dictionary_Result.Values);
            }

            //The one rule, asked rather than restated: exactly the zones PartFCalculator sizes as dwellings.
            //A zone explicitly marked Is Dwelling = No - a communal corridor, a stair, a landlord area - is
            //not among them, so nothing it contains can become a transfer route.
            List<Zone> zones_Dwelling = zones.PartFDwellingZones() ?? [];

            //Read once. GetRelatedObjects rebuilds its result on every call, and this is asked per zone and
            //then again per space below.
            Dictionary<Guid, List<Space>> dictionary_Zone = [];
            HashSet<Guid> guids_Dwelling = [];

            foreach (Zone zone in zones_Dwelling)
            {
                if (zone is null)
                {
                    continue;
                }

                List<Space> spaces_Zone = adjacencyCluster.GetRelatedObjects<Space>(zone) ?? [];

                dictionary_Zone[zone.Guid] = spaces_Zone;

                foreach (Space space in spaces_Zone)
                {
                    if (space is not null)
                    {
                        guids_Dwelling.Add(space.Guid);
                    }
                }
            }

            List<string> names_Added = [];
            List<string> names_Dwelling = [];
            int count_Served = dictionary_Result.Count;

            foreach (Zone zone in zones_Dwelling.Where(x => x is not null).OrderBy(x => x.Name, StringComparer.Ordinal).ThenBy(x => x.Guid))
            {
                List<Space> spaces_Zone = dictionary_Zone.TryGetValue(zone.Guid, out List<Space> spaces_Temp) ? spaces_Temp : [];

                //Only a dwelling one of these spaces is actually served from. A flat this system moves no
                //air into is somebody else's dwelling, and its rooms are not this network's to route
                //through.
                if (!spaces_Zone.Any(x => x is not null && dictionary_Result.ContainsKey(x.Guid)))
                {
                    continue;
                }

                names_Dwelling.Add(zone.Name);

                foreach (Space space in spaces_Zone.Where(x => x is not null).OrderBy(x => x.Name, StringComparer.Ordinal).ThenBy(x => x.Guid))
                {
                    if (dictionary_Result.ContainsKey(space.Guid))
                    {
                        continue;
                    }

                    dictionary_Result[space.Guid] = space;
                    names_Added.Add(space.Name);
                }
            }

            if (names_Added.Count == 0)
            {
                notes.Add(string.Format("The dwelling transfer air is routed over the {0} space(s) the system serves. No further internal space belongs to the dwelling(s) they are in, so nothing was added to the network.", count_Served));
            }
            else
            {
                notes.Add(string.Format("The dwelling transfer air is routed over the {0} space(s) the system serves and {1} further internal space(s) of the same dwelling(s) ({2}): {3}. These carry no design ventilation terminal and are NOT served by the system - they are the rooms the dwelling's transfer air passes through.", count_Served, names_Added.Count, string.Join(", ", names_Dwelling), string.Join(", ", names_Added)));
            }

            List<string> names_Unassigned = [];
            foreach (Space space in dictionary_Result.Values)
            {
                if (!guids_Dwelling.Contains(space.Guid))
                {
                    names_Unassigned.Add(space.Name);
                }
            }

            if (names_Unassigned.Count != 0)
            {
                names_Unassigned.Sort(StringComparer.Ordinal);

                notes.Add(string.Format("{0} served space(s) belong to no dwelling zone ({1}), so no dwelling could be expanded around them. They stay in the network because the system moves air into or out of them and they still have to balance; nothing was added beside them.", names_Unassigned.Count, string.Join(", ", names_Unassigned)));
            }

            return OrderedTransferAirSpaces(dictionary_Result.Values);
        }

        /// <summary>Deterministic order, so the same model gives the same routing on every machine.</summary>
        private static List<Space> OrderedTransferAirSpaces(IEnumerable<Space> spaces)
        {
            return [.. spaces.OrderBy(x => x.Name, StringComparer.Ordinal).ThenBy(x => x.Guid)];
        }
    }
}
