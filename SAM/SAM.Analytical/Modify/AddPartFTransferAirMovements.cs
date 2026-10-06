// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Realizes the dwelling's internal transfer air as runtime air movements, so that what the design
        /// says leaves a supplied room actually goes somewhere and what an extracted room gives up actually
        /// comes from somewhere.
        ///
        /// <para><b>Why this exists, and what refuses without it</b></para>
        /// <para>
        /// TAS will not simulate a building in which the inter-zone air movements of any one zone do not
        /// balance - a zone that gains air it never loses is refused with <c>Simulation Failed</c>, and the
        /// EDSL documentation states the rule as "any air flow imbalance will be reported as a Max Pressure
        /// Exceeded error". A balanced heat recovery dwelling balances at the SYSTEM, so a bedroom is
        /// supplied and never extracted and a bathroom is extracted and never supplied, and every one of
        /// those rooms is individually out of balance. The air that closes each of them is transfer air.
        /// </para>
        ///
        /// <para><b>Where the routes come from</b></para>
        /// <para>
        /// From <see cref="PartFAirflowNetwork"/>, which is the same network Approved Document F paragraph
        /// 1.25 is assessed over and which the Part F calculation already solves for the door schedule. No
        /// route is invented here: the connections are the model's own internal adjacencies, and the flows
        /// are the ones the Part F solver puts through them for these net airflows. Where the network
        /// cannot route a space's net - a room with no internal connection to anything of the opposite
        /// sign - this refuses rather than making a route up.
        /// </para>
        ///
        /// <para><b>Which spaces the network is solved over</b></para>
        /// <para>
        /// The <b>dwelling</b>, not the spaces the ventilation system serves. Approved Document F sizes no
        /// terminal for circulation, so an internal hall is not a served space - and it is the room the
        /// transfer air crosses and divides at, so a network without it has no route through the flat at
        /// all. It is equally not every space in the model: a communal corridor belongs to no dwelling and
        /// must never carry a flat's air or join two flats together.
        /// <see cref="Query.PartFTransferAirSpaces"/> settles the boundary from the Part F calculation's own
        /// dwelling rule.
        /// </para>
        ///
        /// <para><b>A network, not a set of journeys</b></para>
        /// <para>
        /// One movement per connection, carrying that connection's net flow in the direction it actually
        /// flows. A space may have several movements in and several out, a flow may split across connections
        /// and recombine, and no movement has a matching partner - a bedroom of the acceptance dwelling divides
        /// its supply between three rooms, and one of its ensuites draws from two. Balance is therefore
        /// verified by summing every movement at each node, never by pairing routes off against each other.
        /// </para>
        ///
        /// <para><b>The terminal duties are untouched.</b></para>
        /// <para>
        /// This decides where the air goes, never how much of it there is. No supply or extract duty is
        /// adjusted to make a room balance; the transfer flows are what conservation leaves once the design
        /// duties are taken as given.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model. <b>Modified in place.</b></param>
        /// <param name="profileLibrary">
        /// The library a space's ventilation profile name is resolved through. A transfer movement runs on
        /// the profile of the space it arrives in, which is the profile of the extract it is feeding.
        /// </param>
        /// <param name="ventilationSystem">
        /// The system whose <b>served</b> spaces seed the dwelling. <b>Null means every space in the
        /// model.</b> The network is solved over the dwelling those spaces sit in, not over the served
        /// spaces alone - see <see cref="Query.PartFTransferAirSpaces"/>.
        /// </param>
        /// <param name="notes">What was routed, and where the routing was a design choice rather than forced.</param>
        /// <param name="refusals">
        /// Non-empty where the transfer air could not be established. A caller that continues past a
        /// refusal exports a model TAS will not simulate.
        /// </param>
        public static List<SpaceAirMovement> AddPartFTransferAirMovements(this AdjacencyCluster adjacencyCluster, ProfileLibrary profileLibrary, VentilationSystem ventilationSystem, out List<string> notes, out List<string> refusals)
        {
            notes = [];
            refusals = [];

            if (adjacencyCluster is null)
            {
                refusals.Add("No model was supplied, so no transfer air could be established.");

                return null;
            }

            List<Space> spaces;

            if (ventilationSystem is null)
            {
                spaces = adjacencyCluster.GetSpaces();
            }
            else
            {
                //The system's relation names the SERVED spaces - the ones carrying a design terminal - and
                //that is the right thing for it to name. The dwelling those rooms sit in is wider: an
                //internal hall has no terminal and is not served, and it is exactly the space the dwelling's
                //transfer air passes through. The scope is therefore asked for rather than taken from the
                //relation.
                spaces = adjacencyCluster.PartFTransferAirSpaces(adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem), out List<string> notes_Scope);

                notes.AddRange(notes_Scope ?? []);
            }

            return AddPartFTransferAirMovements(adjacencyCluster, profileLibrary, spaces, notes, refusals);
        }

        /// <summary>
        /// Realizes the internal transfer air of one <b>explicitly scoped</b> set of spaces.
        /// <para>
        /// This is the primary form, and the scope is an input rather than something derived here: the
        /// spaces a ventilation system serves and the spaces of the dwelling it serves them in are two
        /// different sets. A zero-terminal internal hall is in the second and not the first, and it is the
        /// space the transfer air passes through and divides at.
        /// <see cref="Query.PartFTransferAirSpaces"/> is what turns one set into the other; a caller that
        /// already knows the dwelling states it here instead.
        /// </para>
        /// <para>See the <see cref="VentilationSystem"/> overload for what this does and why.</para>
        /// </summary>
        /// <param name="adjacencyCluster">The model. <b>Modified in place.</b></param>
        /// <param name="profileLibrary">The library a space's ventilation profile name is resolved through.</param>
        /// <param name="spaces">
        /// Every space of the dwelling the transfer air may route through - the ones with a design terminal
        /// and the ones without.
        /// </param>
        /// <param name="notes">What was routed, and where the routing was a design choice rather than forced.</param>
        /// <param name="refusals">Non-empty where the transfer air could not be established.</param>
        public static List<SpaceAirMovement> AddPartFTransferAirMovements(this AdjacencyCluster adjacencyCluster, ProfileLibrary profileLibrary, IEnumerable<Space> spaces, out List<string> notes, out List<string> refusals)
        {
            notes = [];
            refusals = [];

            return AddPartFTransferAirMovements(adjacencyCluster, profileLibrary, spaces, notes, refusals);
        }

        private static List<SpaceAirMovement> AddPartFTransferAirMovements(AdjacencyCluster adjacencyCluster, ProfileLibrary profileLibrary, IEnumerable<Space> spaces_Scope, List<string> notes, List<string> refusals)
        {
            if (adjacencyCluster is null)
            {
                refusals.Add("No model was supplied, so no transfer air could be established.");

                return null;
            }

            List<Space> spaces = spaces_Scope is null ? null : [.. spaces_Scope];

            if (spaces is null || spaces.Count == 0)
            {
                refusals.Add("No space was resolved for the dwelling, so there is nothing to route transfer air through.");

                return null;
            }

            //Net [l/s] of every space: what its design terminals supply, less what they extract. A space
            //with no terminal is not excluded - a corridor with neither carries the dwelling's transfer air
            //between the rooms that do, and dropping it would disconnect the network.
            Dictionary<Guid, double> dictionary_Net_Lps = [];
            Dictionary<Guid, Space> dictionary_Space = [];

            foreach (Space space in spaces)
            {
                if (space is null)
                {
                    continue;
                }

                dictionary_Space[space.Guid] = space;

                List<VentilationTerminal> ventilationTerminals = adjacencyCluster.VentilationTerminals(space);

                double supply_Lps = Query.VentilationTerminalDesignDuty_Lps(ventilationTerminals, FlowClassification.Supply) ?? 0;
                double extract_Lps = Query.VentilationTerminalDesignDuty_Lps(ventilationTerminals, FlowClassification.Extract) ?? 0;

                dictionary_Net_Lps[space.Guid] = supply_Lps - extract_Lps;
            }

            bool unbalanced = false;
            foreach (KeyValuePair<Guid, double> keyValuePair in dictionary_Net_Lps)
            {
                if (System.Math.Abs(keyValuePair.Value) > PartFAirflowNetwork.Tolerance_Lps)
                {
                    unbalanced = true;
                    break;
                }
            }

            if (!unbalanced)
            {
                //Every room already supplies exactly what it extracts, so there is nothing to transfer and
                //nothing to refuse. Not an error: a fully balanced room-by-room design is a valid one.
                notes.Add("Every space supplies exactly what it extracts, so the dwelling needs no internal transfer air.");

                return [];
            }

            PartFAirflowNetwork partFAirflowNetwork = new(adjacencyCluster, [.. dictionary_Space.Values]);

            Dictionary<(Guid, Guid), double> dictionary_Flow_Lps = partFAirflowNetwork.Solve(
                x => dictionary_Net_Lps.TryGetValue(x, out double value) ? value : 0,
                out List<Guid> guids_Unreachable);

            if (guids_Unreachable is not null && guids_Unreachable.Count != 0)
            {
                List<string> names = [];
                foreach (Guid guid in guids_Unreachable)
                {
                    string name = partFAirflowNetwork.Name(guid);
                    if (!names.Contains(name))
                    {
                        names.Add(name);
                    }
                }

                names.Sort(StringComparer.Ordinal);

                refusals.Add(string.Format("The design airflow of {0} cannot reach anywhere it could come from or go to: nothing of the opposite sign is connected to it through the dwelling's internal adjacencies. TAS refuses a zone whose air movements do not balance, so no route is invented here - model the internal separating elements that connect {1}, or correct the design terminals.", string.Join(", ", names), names.Count == 1 ? "it" : "them"));

                return null;
            }

            List<SpaceAirMovement> result = [];
            int count_AllocationStrategy = 0;

            foreach ((Guid, Guid) connection in partFAirflowNetwork.Connections)
            {
                if (!dictionary_Flow_Lps.TryGetValue(connection, out double flow_Lps) || System.Math.Abs(flow_Lps) <= PartFAirflowNetwork.Tolerance_Lps)
                {
                    //A connection the solution puts nothing through is a door, not an air movement.
                    continue;
                }

                //The movement is written in the direction the air actually travels, so nothing downstream
                //has to interpret a negative flow.
                Guid guid_Upstream = flow_Lps > 0 ? connection.Item1 : connection.Item2;
                Guid guid_Downstream = flow_Lps > 0 ? connection.Item2 : connection.Item1;

                if (!dictionary_Space.TryGetValue(guid_Upstream, out Space space_Upstream) || !dictionary_Space.TryGetValue(guid_Downstream, out Space space_Downstream))
                {
                    continue;
                }

                if (!partFAirflowNetwork.IsUniquelyDetermined(guid_Upstream))
                {
                    count_AllocationStrategy++;
                }

                double airFlow = System.Math.Abs(flow_Lps) / 1000.0;

                //The profile of the space the air ARRIVES in, which is the profile its extract runs on. A
                //transfer that ran on a different profile from the movements it balances would balance the
                //design condition and unbalance every other hour.
                Profile profile = space_Downstream.InternalCondition?.GetProfile(ProfileType.Ventilation, profileLibrary);

                string name = string.Format("{0} to {1} transfer", space_Upstream.Name, space_Downstream.Name);

                ObjectReference objectReference_Upstream = new(space_Upstream);
                ObjectReference objectReference_Downstream = new(space_Downstream);

                SpaceAirMovement spaceAirMovement = profile is null
                    ? new SpaceAirMovement(name, airFlow, objectReference_Upstream.ToString(), objectReference_Downstream.ToString())
                    : new SpaceAirMovement(name, airFlow, profile, objectReference_Upstream.ToString(), objectReference_Downstream.ToString());

                adjacencyCluster.AddObject(spaceAirMovement);

                //Related to ONE space, the one it arrives in. Relating it to both would have the TBD writer
                //walk it twice and write the dwelling two identical inter-zone air movements.
                adjacencyCluster.AddRelation(spaceAirMovement, space_Downstream);

                result.Add(spaceAirMovement);
            }

            notes.Add(string.Format("Realized {0} internal transfer air movement(s) over {1} internal connection(s), routed by the Approved Document F airflow network from the spaces with air to pass on to the spaces that have to draw it in.", result.Count, partFAirflowNetwork.Connections.Count));

            if (count_AllocationStrategy != 0)
            {
                notes.Add(string.Format("{0} of those cross connections that form a loop, where Approved Document F does not say how the air divides between parallel paths. The totals are fixed by conservation; the split between those routes is an allocation, not a regulatory value.", count_AllocationStrategy));
            }

            return result;
        }
    }
}
