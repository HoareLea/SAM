// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// <b>One</b> proposed design airflow optimisation round, across every dwelling it touches, evaluated to
    /// its full engineering consequence - and the model it produces, handed over only where all of it is
    /// valid.
    ///
    /// <para><b>What a round is, and why it is not a sequence of single changes</b></para>
    /// <para>
    /// A TM59 assessment fails three rooms of one flat. Raising them one at a time -
    /// <c>A +5, rebalance, B +5, rebalance, C +5, rebalance</c> - gives an answer that depends on the order
    /// the rooms happened to come out of the results, because each rebalance moves the rooms the next one
    /// is then allocated over. Worse, under the cooking-priority extract strategy a rebalance can land on
    /// the very kitchen the next step is about to target, so the engineer's figure for that room is
    /// overwritten by a consequence of another room's change and then targeted from the wrong starting
    /// point. Neither is engineering anybody could defend afterwards.
    /// </para>
    /// <para>
    /// A round takes <b>all</b> the deliberate targets at once, computes every deliberate delta first,
    /// derives the combined balancing consequence <b>once per dwelling</b>, and allocates it over the rooms
    /// nobody targeted. The answer is a function of the <i>set</i> of targets and nothing else - see
    /// <see cref="Modify.EvaluateTargetedDesignAirFlows"/>.
    /// </para>
    ///
    /// <para><b>All or nothing, and the caller's model is never touched</b></para>
    /// <para>
    /// The whole round is worked out on a copy. <see cref="AdjacencyCluster"/> is null unless every dwelling
    /// in it is valid, so there is no partial round to adopt by mistake and no half-optimised design left
    /// behind by a proposal nobody accepted. Taking <see cref="AdjacencyCluster"/> <b>is</b> the commit -
    /// the same rule <see cref="DwellingDesignAirFlowCandidate"/> and <see cref="PartOIterationPreparation"/>
    /// already follow, and for the same reason: a candidate held and committed later would carry balancing
    /// derived from a dwelling that no longer exists.
    /// </para>
    ///
    /// <para><b>Never a clamp</b></para>
    /// <para>
    /// A round either designs every target at exactly the figure it was asked for, or it refuses. It never
    /// settles for part of a step. <see cref="Modify.ResolveTargetedDesignAirFlow"/> deliberately does clamp,
    /// and that is right for an engineer asking "how much can I have?" - but an automatic optimiser running
    /// fixed steps must not quietly adopt three-fifths of one and call the round done, because the design
    /// it then simulates is not the design its policy says it is testing.
    /// </para>
    ///
    /// <para><b>Equipment is a constraint, never a variable</b></para>
    /// <para>
    /// Each dwelling's combined duty is checked against the unit <b>already selected</b> for it. A round
    /// that outgrows one is refused, with the duty, the rating and the remaining headroom on the record -
    /// it is never answered by selecting a bigger product. The check is against the recalculated
    /// system/unit duty, never against any single room.
    /// </para>
    /// </summary>
    public class DesignAirFlowRoundCandidate
    {
        /// <summary>
        /// The model this round produces. <b>Null unless <see cref="IsAccepted"/></b>, so an invalid or
        /// partial round cannot be adopted. The caller's own cluster is never reached, accepted or not.
        /// </summary>
        public AdjacencyCluster AdjacencyCluster { get; internal set; }

        /// <summary>
        /// What the round did to each dwelling it touched, one entry per serving ventilation system, ordered
        /// by that system's guid - so two runs over the same targets produce the same report in the same
        /// order whatever order the targets arrived in.
        /// </summary>
        public List<DwellingDesignAirFlowRound> DwellingRounds { get; } = [];

        /// <summary>
        /// Targets the round could not take, each with the reason - a room with no Approved Document O
        /// design terminal on that side, or one whose terminals cannot be attributed to a single dwelling.
        /// <para>
        /// <b>Dropped, not refused.</b> These do not make the round invalid; they are the explicit
        /// "not automatically optimisable" answer. A round in which <i>every</i> target lands here is
        /// refused, because there is then nothing to evaluate.
        /// </para>
        /// </summary>
        public List<DesignAirFlowTargetRefusal> TargetRefusals { get; } = [];

        /// <summary>Every deliberate adjustment the round made, across all dwellings.</summary>
        public List<DesignAirFlowAdjustment> TargetedAdjustments
        {
            get
            {
                List<DesignAirFlowAdjustment> result = [];

                foreach (DwellingDesignAirFlowRound dwellingDesignAirFlowRound in DwellingRounds)
                {
                    result.AddRange(dwellingDesignAirFlowRound.TargetedAdjustments);
                }

                return result;
            }
        }

        /// <summary>
        /// Every balancing adjustment the round derived, across all dwellings. <b>Not optimisation
        /// targets</b>, and reported separately for exactly that reason.
        /// </summary>
        public List<DesignAirFlowAdjustment> DerivedAdjustments
        {
            get
            {
                List<DesignAirFlowAdjustment> result = [];

                foreach (DwellingDesignAirFlowRound dwellingDesignAirFlowRound in DwellingRounds)
                {
                    result.AddRange(dwellingDesignAirFlowRound.DerivedAdjustments);
                }

                return result;
            }
        }

        /// <summary>
        /// The dwellings whose <b>already selected</b> ventilation unit is what refused the round - the
        /// distinction an automatic optimiser needs, because this is the "stop at capacity" case and not a
        /// design that failed to hold together. Empty on an accepted round.
        /// </summary>
        public List<DwellingDesignAirFlowRound> VentilationUnitRefusals
        {
            get
            {
                return DwellingRounds.FindAll(x => x.IsVentilationUnitRefusal);
            }
        }

        /// <summary>What the round changed, on what basis, and what each dwelling's unit made of it.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>Advisories that do not refuse the round.</summary>
        public List<string> Warnings { get; } = [];

        /// <summary>
        /// Why the round is not adoptable, one sentence each. Carries the round's own refusals and every
        /// dwelling's.
        /// </summary>
        public List<string> Refusals { get; } = [];

        /// <summary>
        /// Whether this round is a valid design that may be adopted - and therefore whether
        /// <see cref="AdjacencyCluster"/> is there to adopt.
        /// </summary>
        public bool IsAccepted
        {
            get
            {
                return Refusals.Count == 0 && AdjacencyCluster is not null && DwellingRounds.Count != 0 && DwellingRounds.TrueForAll(x => x.IsAccepted);
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} dwelling(s), {1} targeted, {2} derived, {3} not optimisable ({4})",
                DwellingRounds.Count,
                TargetedAdjustments.Count,
                DerivedAdjustments.Count,
                TargetRefusals.Count,
                IsAccepted ? "accepted" : "refused");
        }
    }
}
