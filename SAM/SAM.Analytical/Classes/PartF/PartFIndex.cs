// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// One snapshot of the space identities an Approved Document F question is answered through, so a caller
    /// asking about <b>many</b> spaces resolves the model once instead of once per space.
    ///
    /// <para><b>The problem it exists for</b></para>
    /// <para>
    /// Every Part F query re-resolves the space it is handed against the cluster before reading it - "taken
    /// from the cluster rather than trusted as handed in", because the Part F application replaces spaces
    /// wholesale and a caller may be holding one from before that write. That resolution is
    /// <c>GetSpaces().Find(...)</c> on the space guid, and it is correct; it is also <c>O(spaces)</c>, and
    /// <c>AdjacencyCluster.GetSpaces()</c> rebuilds the whole list from the relation cluster on every call.
    /// Asked once, that is nothing. Asked inside a loop over the model's spaces - which is what a Part O
    /// inspection, an optimisation round and a transfer air scope all do - it is <c>O(spaces²)</c>, and on a
    /// five thousand space project it is the difference between a fraction of a second and several seconds.
    /// </para>
    ///
    /// <para><b>What it indexes, and what it deliberately does not</b></para>
    /// <para>
    /// <b>Identity only.</b> This holds which <see cref="Analytical.Space"/> instance the model currently
    /// carries for a guid, and nothing else. It stores no flow rate, no <see cref="PartFSpaceData"/>, no
    /// requirement and no derived engineering value of any kind - every rate is read live from the resolved
    /// space on every call, exactly as
    /// <see cref="Query.PartFRequiredFlowRate_Lps(AdjacencyCluster, Space, FlowClassification)"/> reads it.
    /// A cached rate would be a second answer to a regulatory question and the two would disagree the first
    /// time Part F was recalculated; that is the rule <c>Query.PartFRequiredFlowRate_Lps</c> is written
    /// around, and this does not weaken it.
    /// </para>
    /// <para>
    /// So this is <b>not a second engineering authority</b>. It accelerates the existing rules and restates
    /// none of them: <see cref="PartFRequiredFlowRate_Lps"/> below hands the resolved space to the very
    /// reader the one-space query hands it to.
    /// </para>
    ///
    /// <para><b>Request scoped. It is a snapshot and it does not know when it is stale</b></para>
    /// <para>
    /// Build one, use it for one traversal, drop it. It is not stored on the cluster, not held in a static,
    /// and nothing refreshes it: a space added to or replaced in the model after it was built is not in it.
    /// That is the same contract <see cref="PartFAirflowNetwork"/> has, and it is why the index is passed
    /// down a call rather than kept beside a window - a cache nobody can see going stale is worse than the
    /// quadratic loop it replaced. A caller that mutates the model mid-traversal builds a new one.
    /// </para>
    ///
    /// <para><b>Approved Document F has no space multiplier</b></para>
    /// <para>
    /// Said explicitly because a bulk aggregate is exactly where one would be expected: nothing on the Part F
    /// path scales a room's requirement by a count of identical rooms. A requirement belongs to one space,
    /// each space carries its own <see cref="PartFSpaceData"/>, and a dwelling total is the plain sum over
    /// its rooms. There is no multiplier to preserve, and this index introduces none.
    /// </para>
    /// </summary>
    public class PartFIndex
    {
        private readonly AdjacencyCluster adjacencyCluster;

        private readonly List<Space> spaces = [];

        private readonly Dictionary<Guid, Space> dictionary_Space = [];

        /// <summary>
        /// Indexes the spaces one model currently carries.
        /// </summary>
        /// <param name="adjacencyCluster">
        /// The model. <b>Not modified, and not copied.</b> A null model indexes nothing and answers exactly
        /// what the one-space queries answer for a null model, which is nothing - never a fallback onto
        /// whatever space the caller happened to be holding.
        /// </param>
        public PartFIndex(AdjacencyCluster adjacencyCluster)
        {
            this.adjacencyCluster = adjacencyCluster;

            foreach (Space space in adjacencyCluster?.GetSpaces() ?? [])
            {
                if (space is null)
                {
                    continue;
                }

                spaces.Add(space);

                //FIRST occurrence wins, because List.Find - which every one-space query resolves with -
                //returns the first match. A cluster cannot currently hold two spaces on one guid (objects
                //are stored per type name in a Dictionary<Guid, X>, and Space has no subtype anywhere in
                //SAM), so this is unreachable today. It is written this way regardless: the day a subtype
                //appears, the index has to keep answering what Find answers rather than quietly picking the
                //other one.
                if (!dictionary_Space.ContainsKey(space.Guid))
                {
                    dictionary_Space[space.Guid] = space;
                }
            }
        }

        /// <summary>The model this was built from. Null where it was built from none.</summary>
        public AdjacencyCluster AdjacencyCluster
        {
            get { return adjacencyCluster; }
        }

        /// <summary>How many distinct space identities the model carried when this was built.</summary>
        public int Count
        {
            get { return dictionary_Space.Count; }
        }

        /// <summary>
        /// Every space the model carried when this was built, in the cluster's own order - the same spaces,
        /// in the same order, <c>AdjacencyCluster.GetSpaces()</c> returns, with nulls dropped.
        /// </summary>
        public List<Space> Spaces
        {
            get { return [.. spaces]; }
        }

        /// <summary>
        /// The instance the model currently carries for a guid, or null where it carries none.
        /// <para>
        /// <b>Virtual so a test can count what a bulk caller actually asks of one index.</b> The claim this
        /// class makes is that the number of resolutions is linear in the spaces looked at, and a counting
        /// subclass is how that is asserted without a stopwatch. Nothing in production overrides it.
        /// </para>
        /// </summary>
        public virtual Space Space(Guid guid)
        {
            return dictionary_Space.TryGetValue(guid, out Space result) ? result : null;
        }

        /// <summary>
        /// The resolution the one-space Part F queries make: the model's own instance for this space where
        /// it has one, and <b>the space as handed in</b> where it does not.
        /// <para>
        /// The fallback is not a defect and is not corrected here. A caller may legitimately be asking about
        /// a space this model does not carry - a detached copy, a space belonging to a candidate cluster -
        /// and <c>Query.PartFRequiredFlowRate_Lps</c> answers that from the instance it was given. A caller
        /// that needs "is this space in the model at all" asks <see cref="Space(Guid)"/>, which says null.
        /// </para>
        /// </summary>
        public Space Space(Space space)
        {
            return space is null ? null : Space(space.Guid) ?? space;
        }

        /// <summary>
        /// The current instances of every space the given zones relate to, first occurrence winning, in zone
        /// order and then in relation order.
        /// <para>
        /// A dwelling scope, resolved once against this snapshot. A related space the model no longer
        /// carries is dropped rather than returned stale, and a space shared by two zones in the set appears
        /// once.
        /// </para>
        /// <para>
        /// <b>Which zones are dwellings is not decided here.</b> That is
        /// <see cref="Query.PartFDwellingZones(IEnumerable{Zone})"/>, the single source of the
        /// dwelling-selection policy, and a caller states the zones it has already selected with it.
        /// </para>
        /// </summary>
        public List<Space> Spaces_Zones(IEnumerable<Zone> zones)
        {
            List<Space> result = [];

            if (adjacencyCluster is null || zones is null)
            {
                return result;
            }

            HashSet<Guid> guids = [];

            foreach (Zone zone in zones)
            {
                if (zone is null)
                {
                    continue;
                }

                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(zone) ?? [])
                {
                    if (space is null || !guids.Add(space.Guid))
                    {
                        continue;
                    }

                    Space space_Current = Space(space.Guid);

                    if (space_Current is not null)
                    {
                        result.Add(space_Current);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// What Approved Document F requires of one space in one direction - <b>the same answer</b>
        /// <see cref="Query.PartFRequiredFlowRate_Lps(AdjacencyCluster, Space, FlowClassification)"/> gives,
        /// reached without rebuilding the model's space list.
        /// <para>
        /// The rule itself is not here. Once the space is resolved this hands it straight to the shared
        /// reader both forms use, so the two cannot come to disagree.
        /// </para>
        /// <para>
        /// Null where the space was never sized - which is not the same as a requirement of zero - and null
        /// for a direction that is neither supply nor extract.
        /// </para>
        /// </summary>
        public virtual double? PartFRequiredFlowRate_Lps(Space space, FlowClassification flowClassification)
        {
            //A null model answers nothing, exactly as the one-space query does - NOT the handed-in space's
            //own data. Without this, an index built from no model at all would start answering questions the
            //oracle refuses.
            if (adjacencyCluster is null || space is null)
            {
                return null;
            }

            return Query.PartFRequiredFlowRate_Lps(Space(space), flowClassification);
        }
    }
}
