// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// The <b>selected-equipment capacity envelope</b> of a design: what the already-selected ventilation
    /// units could support if the <b>last valid design</b> were grown coherently to each unit's own
    /// design-capacity ceiling, <b>preserving the proportions between every terminal that unit serves</b>.
    ///
    /// <para><b>A diagnostic, and emphatically not another optimisation round</b></para>
    /// <para>
    /// An ordinary Approved Document O Iteration 2B round is all-or-nothing at a fixed step - see
    /// <see cref="Modify.EvaluateTargetedDesignAirFlows"/> - and that rule is not weakened here. An
    /// envelope is calculated <i>after</i> the ordinary optimisation has stopped, and it answers a
    /// different question: not "can this design take another whole step?" but "what is the best this
    /// dwelling and this already-bought unit could do?". Those are different questions and their answers
    /// are kept in different places. <see cref="AdjacencyCluster"/> is a model to <b>look at</b>; it is
    /// never the accepted design, never fed back into a later round, and a caller that adopts it as one has
    /// misread it. The clamp an ordinary round refuses is exactly what an envelope is <i>for</i>, which is
    /// why it is a separate operation with a separate result rather than a flag on the round.
    /// </para>
    ///
    /// <para><b>What "coherent" means, and why it is the whole design</b></para>
    /// <para>
    /// <b>One factor per equipment group, applied to every space and direction that unit serves.</b> A flat
    /// designed 40 supply / 22 + 18 extract on a 150/150 unit grows by <c>min(150/40, 150/40) = 3.75</c> to
    /// 150 supply / 82.5 + 67.5 extract - the same dwelling, larger, with <c>22/18</c> still equal to
    /// <c>82.5/67.5</c>. Because the design being grown is balanced at every system, both sides move by the
    /// same amount and the ordinary round derives no balancing consequence at all.
    /// </para>
    /// <para>
    /// It is <b>not</b> a scaling of the optimisation's deliberate increments. That reading answers "how far
    /// can the current targeted direction continue?" and spends the remaining headroom only on the rooms the
    /// optimiser happened to be pushing - the same flat comes out at 150 supply / 22 + 128 extract, the
    /// bathroom carrying the whole increase and the studio's extract untouched. Coherent arithmetic, and a
    /// design nobody would build. The useful diagnostic is "what design could the already-selected unit
    /// support?", and that is a proportional growth of the design that exists.
    /// </para>
    ///
    /// <para><b>The scale is not capped at one step, and never falls below 1</b></para>
    /// <para>
    /// The bound is selected-equipment feasibility. Where the ordinary optimisation stopped on its iteration
    /// guard rather than on capacity, several steps' worth of headroom may remain and the factor is well
    /// above 1. It is never <i>below</i> 1: a design already sitting on - or past - its rating is reported
    /// as <see cref="DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom"/>, because an envelope never designs a
    /// dwelling downwards in the name of a diagnostic.
    /// </para>
    ///
    /// <para><b>Every authority stays where it was</b></para>
    /// <para>
    /// <c>PartFRequiredAirFlow != DesignAirFlow != SelectedEquipmentCapacity != OperatingAirFlow.</c> The
    /// envelope moves design airflow and nothing else: the Approved Document F requirement is read as a
    /// floor by the round it delegates to and never written; the selected product is read as a ceiling and
    /// <b>never reselected</b>; no operating, profile or runtime airflow is touched, so nothing here is
    /// Iteration 3 behaviour. Capacity is judged by <see cref="Query.AirHandlingUnitDesignDuty"/>, which
    /// sums every system a unit supplies, so no dwelling-to-unit ownership is assumed.
    /// </para>
    ///
    /// <para><b>Nothing is searched by mutating the model</b></para>
    /// <para>
    /// The feasible factor is a division - the selected product's rating over the duty the design already
    /// carries, on whichever side gives the tighter ratio - then confirmed by evaluating the round once.
    /// Where that is refused for a reason which is not capacity, a bounded deterministic solve retreats
    /// within <c>[1, factor]</c>; where the round refuses the <i>source</i> design too, no factor repairs
    /// that and the refusal is reported as it stands. Every attempt is
    /// evaluated against the <i>caller's own</i> model, which <see cref="Modify.EvaluateTargetedDesignAirFlows"/>
    /// never modifies - so no search state accumulates anywhere and the same input always gives the same
    /// answer.
    /// </para>
    /// </summary>
    public class DesignAirFlowCapacityEnvelope
    {
        /// <summary>
        /// The envelope model - the design the selected units' ceilings permit.
        /// <b>Null unless <see cref="IsScaled"/></b>.
        /// <para>
        /// <b>Diagnostic. Never the accepted design.</b> An ordinary optimisation's last accepted model is a
        /// separate thing that this does not replace, and feeding this back into a later round would make
        /// the next round's baseline a design the optimiser's own policy refused.
        /// </para>
        /// </summary>
        public AdjacencyCluster AdjacencyCluster { get; internal set; }

        /// <summary>
        /// The ordinary round that produced <see cref="AdjacencyCluster"/>, evaluated over the <b>grown</b>
        /// design vector by <see cref="Modify.EvaluateTargetedDesignAirFlows"/> - so the targeted and
        /// derived adjustments, the duties and the per-dwelling equipment verdicts of the envelope are the
        /// same shape, from the same authority, as an ordinary round's. Null where no group scaled.
        /// <para>
        /// Its own <c>TargetRefusals</c> are empty and are <b>not</b> the ones a caller wants: the envelope's
        /// round is given the design that exists, which every room can carry by definition. The failing rooms
        /// the ordinary policy could not target are on <see cref="TargetRefusals"/> here.
        /// </para>
        /// </summary>
        public DesignAirFlowRoundCandidate RoundCandidate { get; internal set; }

        /// <summary>
        /// The deliberate targets the ordinary optimisation would next have asked for that the building has
        /// <b>no lever</b> for - a failing room with no design terminal on the side that failed, say - each
        /// with its own reason, ordered on the same key an ordinary round orders its own by.
        /// <para>
        /// A dropped target still <i>counts</i>: the envelope reads the target vector for scope, so a room
        /// dropped here is a room whose equipment was never brought into the diagnostic. Where every one of
        /// them is dropped the answer is <see cref="DesignAirFlowCapacityEnvelopeOutcome.NoTargets"/>, and
        /// an engineer does something quite different about that than about an exhausted unit.
        /// </para>
        /// </summary>
        public List<DesignAirFlowTargetRefusal> TargetRefusals { get; } = [];

        /// <summary>
        /// One entry per serving equipment group the envelope considered - <b>including</b> the groups it
        /// produced nothing for, each with the reason. Ordered by unit name then guid, so the report does
        /// not depend on the order the targets arrived in.
        /// </summary>
        public List<DesignAirFlowCapacityEnvelopeGroup> Groups { get; } = [];

        /// <summary>The groups that reached a scaled envelope design.</summary>
        public List<DesignAirFlowCapacityEnvelopeGroup> Groups_Scaled => Groups.FindAll(x => x.IsScaled);

        /// <summary>
        /// Every deliberate adjustment the envelope's grown round made - one per space and direction the
        /// scaled units serve, which is what makes the visible rows of a report reconcile to the units'
        /// supply and extract duties with no hidden terminal contribution.
        /// <para>
        /// <b>Not an accepted optimisation step, and not an optimisation's targets.</b> These rooms were
        /// chosen by <i>this operation</i>, to keep their share of the design vector - a report has to say
        /// so in its own word (<c>SCALED</c>) rather than borrow the one an ordinary round uses, which would
        /// claim the optimisation had asked for these figures.
        /// </para>
        /// </summary>
        public List<DesignAirFlowAdjustment> TargetedAdjustments => RoundCandidate?.TargetedAdjustments ?? [];

        /// <summary>
        /// Every balancing adjustment the scaled round derived. Kept apart from the targeted ones for the
        /// same reason an ordinary round keeps them apart: a report that merged the two would claim every
        /// room that moved was chosen.
        /// </summary>
        public List<DesignAirFlowAdjustment> DerivedAdjustments => RoundCandidate?.DerivedAdjustments ?? [];

        /// <summary>
        /// The overall outcome - the best any group reached, or the single reason none did. Never inferred
        /// from prose: a caller has to be able to tell "the units have nothing left" from "the envelope
        /// could not be worked out".
        /// </summary>
        public DesignAirFlowCapacityEnvelopeOutcome Outcome { get; internal set; } = DesignAirFlowCapacityEnvelopeOutcome.Undefined;

        /// <summary>Why the envelope came to what it did, in one sentence.</summary>
        public string Reason { get; internal set; }

        /// <summary>How each group's ceiling was reached, and what the scaled round then reported.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>Advisories that do not stop the envelope.</summary>
        public List<string> Warnings { get; } = [];

        /// <summary>
        /// What was refused, and why - the whole envelope where none could be calculated at all, or one
        /// equipment group whose design could not be grown even where another group's was.
        /// <para>
        /// Empty on a <see cref="DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom"/> or
        /// <see cref="DesignAirFlowCapacityEnvelopeOutcome.NoTargets"/> answer, because neither of those is a
        /// failure: they are the diagnostic reporting that the design is already at the limit of what this
        /// operation can say anything about. Non-empty is therefore <b>not</b> the same as "no model" - ask
        /// <see cref="IsScaled"/> for that.
        /// </para>
        /// </summary>
        public List<string> Refusals { get; } = [];

        /// <summary>
        /// Whether the envelope produced a design worth simulating - and therefore whether
        /// <see cref="AdjacencyCluster"/> is there to simulate.
        /// </summary>
        public bool IsScaled => Outcome == DesignAirFlowCapacityEnvelopeOutcome.Scaled && AdjacencyCluster is not null;

        public override string ToString()
        {
            return string.Format(
                "{0} group(s), {1} scaled, {2} targeted, {3} derived ({4})",
                Groups.Count,
                Groups_Scaled.Count,
                TargetedAdjustments.Count,
                DerivedAdjustments.Count,
                Core.Query.Description(Outcome));
        }
    }
}
