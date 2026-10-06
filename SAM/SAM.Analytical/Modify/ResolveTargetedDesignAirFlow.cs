// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// The absolute ceiling on <see cref="ResolveTargetedDesignAirFlow"/>'s bisections - a termination
        /// backstop, never the thing that decides the answer.
        /// <para>
        /// Halving any finite bracket this many times drives it below what a <see cref="double"/> can
        /// represent at either end, so the loop's own equality guard is always reached first. It exists so
        /// a tolerance of <b>zero</b>, which <see cref="Query.IsValidFlowRateTolerance"/> deliberately
        /// accepts as "compare exactly", asks for a bracket that cannot close rather than a search that
        /// cannot end.
        /// </para>
        /// </summary>
        private const int maximumDesignAirFlowHalvings = 1100;

        /// <summary>
        /// How many halvings a bracket of <paramref name="width_Lps"/> needs to close to
        /// <paramref name="tolerance_Lps"/> - <c>log2(width / tolerance)</c>, which is what the search's
        /// loop actually terminates on.
        ///
        /// <para><b>Why this is derived rather than a constant</b></para>
        /// <para>
        /// A fixed budget silently breaks the guarantee this search advertises. Sixty halvings close a
        /// 100 l/s bracket to a thousandth with forty to spare, and close a 1e18 l/s bracket to about
        /// 0.87 l/s - so a caller asking for an implausibly large airflow got an answer that was accepted,
        /// reported as "the closest design airflow the unit will carry", and 0.6 l/s short of the truth,
        /// with nothing in the result to say so. An engineer reading it would size equipment they did not
        /// need. The budget therefore follows the bracket the caller actually created.
        /// </para>
        /// <para>
        /// A zero tolerance asks for machine precision, which no finite count expresses, so it takes the
        /// ceiling and is ended by the loop's equality guard instead. The ceiling is also the answer for
        /// any bracket so wide that <c>log2</c> exceeds it - a request that far from the room's existing
        /// design is not an airflow anybody means, and the guard then ends the search at machine precision
        /// rather than short of it.
        /// </para>
        /// </summary>
        private static int DesignAirFlowHalvings(double width_Lps, double tolerance_Lps)
        {
            if (tolerance_Lps <= 0)
            {
                return maximumDesignAirFlowHalvings;
            }

            double halvings = System.Math.Ceiling(System.Math.Log(width_Lps / tolerance_Lps, 2));

            if (double.IsNaN(halvings) || halvings < 1)
            {
                //The bracket is already at or inside the tolerance. The loop's own condition will not run
                //a single iteration, and this simply agrees with it.
                return 1;
            }

            //One spare halving, so a bracket whose log2 lands a hair under an integer is never left one
            //halving short of the tolerance by the rounding alone.
            return halvings + 1 < maximumDesignAirFlowHalvings ? (int)halvings + 1 : maximumDesignAirFlowHalvings;
        }

        /// <summary>
        /// Designs a room as close to a requested airflow as the dwelling and its <b>already selected</b>
        /// ventilation unit will actually carry, and hands back the model that produces.
        ///
        /// <para><b>What this adds to <see cref="EvaluateTargetedDesignAirFlow"/>, and why</b></para>
        /// <para>
        /// A single candidate answers one question: is <i>this exact</i> airflow feasible? An engineer
        /// asking for a bedroom at 40 l/s against an MVHR-25 gets "no", and the thing they actually wanted
        /// to know - how much they <i>can</i> have without buying a bigger unit - is left for them to find
        /// by hand, one candidate at a time. This is that search, done once, here, so that every caller
        /// does not write it again and write it differently.
        /// </para>
        /// <code>
        /// requested 40.0  ->  achieved 36.4   IsRequestSatisfied false, LimitingReason names the unit
        /// requested 22.2  ->  achieved 22.2   IsRequestSatisfied true
        /// </code>
        ///
        /// <para><b>A clamp, and nothing more</b></para>
        /// <para>
        /// The request bounds the answer on one side and the room's existing design airflow bounds it on
        /// the other, so the search only ever moves the room <i>towards</i> what was asked for and never
        /// past it. That is what keeps this an optimisation of the engineer's intent rather than an
        /// optimiser with an objective of its own:
        /// </para>
        /// <list type="bullet">
        /// <item>Headroom the request did not ask for is never spent. A 50 l/s unit serving a request for
        /// 30 l/s answers 30, not 50 - the rule
        /// <see cref="DwellingDesignAirFlowCandidate.SupplyHeadroom_Lps"/> already states.</item>
        /// <item>A request the room cannot move towards is answered with the design as it stands
        /// (<see cref="DwellingDesignAirFlowResolution.IsChanged"/> false), never by moving it the other
        /// way. A reduction that cannot be balanced does not become an increase.</item>
        /// <item>The selected product is never changed. It is the constraint being resolved <i>within</i>;
        /// growing it is <see cref="SelectVentilationUnit"/>, called deliberately, on its own. See
        /// <see cref="DwellingDesignAirFlowCandidate.VentilationUnitSelectionOutcome"/>.</item>
        /// <item>No Approved Document F requirement, transfer path, air handling unit or runtime airflow
        /// is written. Only design airflow moves, and only in the rooms a candidate would move.</item>
        /// </list>
        ///
        /// <para><b>The engineering is borrowed, not reimplemented - including the bounds</b></para>
        /// <para>
        /// Every value the search tries is a full <see cref="EvaluateTargetedDesignAirFlow"/>, so every
        /// answer it gives is a candidate that genuinely passed the Approved Document F floors, the
        /// opposite-side allocation, the balance check and the selected unit's rating. The bounds are never
        /// computed here.
        /// </para>
        /// <para>
        /// They could have been. The binding limits are arithmetic - the target room's own Part F floor,
        /// the design headroom the opposite side holds above its floors, the selected unit's rating less
        /// the current duty - and closing the form would answer in one evaluation instead of about
        /// seventeen. It is deliberately not done: a closed form is a <i>second</i> statement of the
        /// engineering, sitting beside <see cref="ApplyTargetedDesignAirFlow"/> and free to disagree with
        /// it the moment either changes. Bisecting the real evaluation cannot disagree with it, because it
        /// <i>is</i> it, and the cost is a handful of candidate evaluations on one dwelling.
        /// </para>
        ///
        /// <para><b>Why bisection is valid here, and what protects it where it is not</b></para>
        /// <para>
        /// Feasibility is monotone along the bracket, in every constraint the evaluation applies:
        /// </para>
        /// <list type="bullet">
        /// <item>The <b>target room's Part F floor</b> is a floor - feasible above it, refused below.</item>
        /// <item>The <b>selected unit's rating</b> bounds the dwelling's duty, and the duty moves one for
        /// one with the targeted room because the targeted and derived changes move both sides by the same
        /// amount - so it is an upper bound and nothing else.</item>
        /// <item>The <b>balancing side</b> refuses a reduction only where the change exceeds the design
        /// headroom those rooms hold above their own floors, which is a fixed quantity read off the
        /// caller's unchanged model - so, again, one bound. An increase is never refused for want of
        /// headroom, because an increase does not consume any.</item>
        /// <item>Everything else the evaluation refuses on - an unbalanced or non-compliant dwelling, a
        /// room whose terminals belong to another system, a terminal carrying something that is not a
        /// quantity of air - does not depend on the value being tried at all, so it refuses the whole
        /// bracket. The anchor evaluation below catches exactly that case and refuses, rather than
        /// bisecting a bracket with no feasible point in it.</item>
        /// </list>
        /// <para>
        /// <b>And where that reasoning is ever wrong, the failure is conservative by construction.</b> The
        /// search never returns a value it has not evaluated and seen accepted, so a non-monotone
        /// constraint can only make it stop short of the true limit - reporting less design airflow than
        /// was available. It cannot make it return a design that does not hold together, which is the
        /// property that matters.
        /// </para>
        ///
        /// <para><b>An answer that falls short is an answer, not a refusal</b></para>
        /// <para>
        /// Where the request is infeasible but something between it and the room's current design is not,
        /// the result is <b>accepted</b>, carries that model, and says
        /// <see cref="DwellingDesignAirFlowResolution.IsRequestSatisfied"/> false with
        /// <see cref="DwellingDesignAirFlowResolution.LimitingReason"/> naming what stopped it. Refusing
        /// outright would make this method exactly <see cref="EvaluateTargetedDesignAirFlow"/> with extra
        /// evaluations, and the clamped value is the entire reason to ask. It is still never mistaken for
        /// the request, and it is still only adopted by taking
        /// <see cref="DwellingDesignAirFlowResolution.AdjacencyCluster"/> deliberately.
        /// </para>
        /// <para>
        /// Where <b>nothing</b> is feasible - the dwelling is not a valid design to change, or the request
        /// lies on the far side of a floor the room already sits on - the result is refused and carries no
        /// model, exactly as a candidate does.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model to resolve against. Never modified.</param>
        /// <param name="space">The one room the request targets.</param>
        /// <param name="flowClassification">Which side of that room moves - supply or extract.</param>
        /// <param name="designFlowRate_Lps">The design airflow being asked for [l/s]. A ceiling on an
        /// increase and a floor on a reduction; never exceeded in either direction.</param>
        /// <param name="partFExtractAllocationStrategy">How the balancing consequence is shared out -
        /// passed straight through to each candidate.</param>
        /// <param name="tolerance_Lps">Flow rate comparison tolerance [l/s], and the margin the answer is
        /// resolved to.</param>
        /// <param name="ventilationUnitCapacityDescriptors">The products the selected unit's capacity is
        /// read from. Null makes equipment no constraint on the search at all, the same
        /// backward-compatible meaning it has for a candidate and for a manual edit.</param>
        /// <returns>
        /// What was asked for, what was achieved, whether those are the same, what limited it, and - where
        /// anything was feasible - the model to adopt. Never null.
        /// </returns>
        public static DwellingDesignAirFlowResolution ResolveTargetedDesignAirFlow(this AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification, double designFlowRate_Lps, PartFExtractAllocationStrategy partFExtractAllocationStrategy = PartFExtractAllocationStrategy.MinimumFirstCookingPriority, double tolerance_Lps = 0.001, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null)
        {
            DwellingDesignAirFlowResolution result = new()
            {
                Requested_Lps = designFlowRate_Lps,
                Tolerance_Lps = tolerance_Lps,
            };

            if (adjacencyCluster is null || space is null)
            {
                result.Refusals.Add("No model or no space was supplied, so no design airflow could be resolved.");

                return result;
            }

            //BEFORE anything else, because the bracket below is closed against it as well as every
            //Approved Document F, balance and capacity comparison beneath it. See
            //Query.IsValidFlowRateTolerance - a tolerance that cannot be compared against would leave the
            //search deciding feasibility on comparisons that quietly always pass or always fail.
            if (!Query.IsValidFlowRateTolerance(tolerance_Lps))
            {
                result.Refusals.Add(Query.FlowRateToleranceRefusal(tolerance_Lps));

                return result;
            }

            //A bracket needs a real number at its far end. ApplyTargetedDesignAirFlow refuses these too,
            //but it is the SEARCH that cannot proceed on them, so it says so itself rather than bisecting
            //towards a value that is not a quantity of air.
            if (double.IsNaN(designFlowRate_Lps) || double.IsInfinity(designFlowRate_Lps) || designFlowRate_Lps < 0)
            {
                result.Refusals.Add(string.Format(
                    "Space '{0}': {1} l/s is not a design airflow to resolve towards - it has to be a finite, non-negative number of litres per second. Nothing was changed.",
                    space.Name,
                    designFlowRate_Lps));

                return result;
            }

            // ---- The request itself, first - and, in the ordinary case, last ---------------------------

            DwellingDesignAirFlowCandidate candidate_Requested = adjacencyCluster.EvaluateTargetedDesignAirFlow(space, flowClassification, designFlowRate_Lps, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors);

            result.Evaluations = 1;

            if (candidate_Requested.IsAccepted)
            {
                //Feasible exactly as asked. No search, no bracket, and the answer is the candidate an
                //engineer would have got by evaluating it themselves.
                Settle(result, candidate_Requested, null, 0);

                return result;
            }

            // ---- The anchor: the design exactly as it stands --------------------------------------------

            //Read off the CALLER's model, and the near end of the bracket. Evaluating it does two things at
            //once: it establishes a feasible point for the bisection to keep, and - because a change of
            //zero exercises every precondition while allocating nothing - it separates a request that is
            //merely too ambitious from a dwelling that was never a valid design to change.
            double? designFlowRate_Anchor_Lps = CurrentDesignFlowRate_Lps(adjacencyCluster, space, flowClassification);

            if (!designFlowRate_Anchor_Lps.HasValue || System.Math.Abs(designFlowRate_Lps - designFlowRate_Anchor_Lps.Value) <= tolerance_Lps)
            {
                //Either the room has no design terminal of this direction to read - in which case the
                //candidate's own refusal already says exactly that, far better than anything here could -
                //or the request IS the room's current design and there is no bracket to search. Either
                //way the request's refusal stands as the answer.
                Refuse(result, candidate_Requested, candidate_Requested.Refusals);

                return result;
            }

            DwellingDesignAirFlowCandidate candidate_Anchor = adjacencyCluster.EvaluateTargetedDesignAirFlow(space, flowClassification, designFlowRate_Anchor_Lps.Value, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors);

            result.Evaluations++;

            if (!candidate_Anchor.IsAccepted)
            {
                //The dwelling cannot carry its OWN design, so no value between it and the request can be
                //feasible either and there is nothing to bisect. Refused with the anchor's reasons, which
                //are the ones that actually need fixing - the request's refusal would only describe a
                //symptom of them.
                Refuse(result, candidate_Anchor, candidate_Anchor.Refusals);

                result.Refusals.Add(string.Format(
                    "Space '{0}' could not be resolved towards {1:0.###} l/s of {2}, because the dwelling's design as it already stands was rejected on the same terms - so no airflow between the two could be valid either. Nothing was changed.",
                    space.Name,
                    designFlowRate_Lps,
                    Core.Query.Description(flowClassification)));

                return result;
            }

            // ---- Bisect between a feasible anchor and an infeasible request -----------------------------

            //Direction-agnostic on purpose: an increase brackets [anchor, request] and a reduction brackets
            //[request, anchor], and neither the invariant nor the arithmetic below cares which. The
            //feasible bound only ever moves towards the request, so the answer is always the closest
            //feasible value to it that the search actually evaluated and saw accepted.
            DwellingDesignAirFlowCandidate candidate_Feasible = candidate_Anchor;

            double bound_Feasible_Lps = designFlowRate_Anchor_Lps.Value;
            double bound_Infeasible_Lps = designFlowRate_Lps;

            //The tightest infeasible candidate's reason - so the answer says what stopped it just past
            //where it stopped, rather than what stopped it at the original request.
            string reason_Limiting = string.Join(" ", candidate_Requested.Refusals);

            //Derived from the bracket this caller actually created, so the loop always terminates on the
            //tolerance rather than on a budget that ran out first. See DesignAirFlowHalvings.
            int halvings = DesignAirFlowHalvings(System.Math.Abs(bound_Infeasible_Lps - bound_Feasible_Lps), tolerance_Lps);

            for (int i = 0; i < halvings && System.Math.Abs(bound_Infeasible_Lps - bound_Feasible_Lps) > tolerance_Lps; i++)
            {
                //Written as an offset from the feasible bound rather than as a half-sum, so it cannot
                //overflow and cannot land outside the bracket on the way to it.
                double designFlowRate_Mid_Lps = bound_Feasible_Lps + ((bound_Infeasible_Lps - bound_Feasible_Lps) / 2);

                if (designFlowRate_Mid_Lps == bound_Feasible_Lps || designFlowRate_Mid_Lps == bound_Infeasible_Lps)
                {
                    //The bracket is now narrower than a double can halve. Only reachable on a tolerance of
                    //zero, which asks for an exact answer this arithmetic has already given as much of as
                    //it has.
                    break;
                }

                DwellingDesignAirFlowCandidate candidate = adjacencyCluster.EvaluateTargetedDesignAirFlow(space, flowClassification, designFlowRate_Mid_Lps, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors);

                result.Evaluations++;

                if (candidate.IsAccepted)
                {
                    candidate_Feasible = candidate;
                    bound_Feasible_Lps = designFlowRate_Mid_Lps;

                    continue;
                }

                bound_Infeasible_Lps = designFlowRate_Mid_Lps;
                reason_Limiting = string.Join(" ", candidate.Refusals);
            }

            Settle(result, candidate_Feasible, reason_Limiting, System.Math.Abs(bound_Infeasible_Lps - bound_Feasible_Lps));

            return result;
        }

        /// <summary>
        /// Takes the candidate the search settled on as the answer, carrying its notes, its warnings and -
        /// where it fell short of the request - what stopped it.
        /// </summary>
        private static void Settle(DwellingDesignAirFlowResolution result, DwellingDesignAirFlowCandidate candidate, string reason_Limiting, double residual_Lps)
        {
            result.Candidate = candidate;

            result.Notes.AddRange(candidate.Notes);
            result.Warnings.AddRange(candidate.Warnings);

            if (result.IsRequestSatisfied)
            {
                return;
            }

            result.LimitingReason = reason_Limiting;

            //Said plainly and up front, because the one thing a caller must never do with this result is
            //read Achieved_Lps as though it were what they asked for.
            result.Notes.Insert(0, string.Format(
                "Space '{0}' was requested at {1:0.###} l/s of {2} and resolved to {3:0.###} l/s - the closest design airflow to that request the dwelling and its selected ventilation unit will carry, found in {4} evaluation(s) and narrowed to within {5:0.######} l/s of the last airflow that was refused. {6}",
                result.TargetedAdjustment?.SpaceName ?? "-",
                result.Requested_Lps,
                Core.Query.Description(result.TargetedAdjustment?.FlowClassification ?? FlowClassification.Undefined),
                result.Achieved_Lps,
                result.Evaluations,
                residual_Lps,
                result.IsChanged
                    ? "Nothing was reselected and no headroom beyond the request was taken up."
                    : "That is the room's existing design airflow, so adopting this changes nothing."));
        }

        /// <summary>
        /// Answers with a candidate that was refused, so a caller can still read what the search reasoned
        /// about even though there is no model to adopt.
        /// </summary>
        private static void Refuse(DwellingDesignAirFlowResolution result, DwellingDesignAirFlowCandidate candidate, List<string> refusals)
        {
            result.Candidate = candidate;

            result.Warnings.AddRange(candidate.Warnings);
            result.Refusals.AddRange(refusals);

            result.LimitingReason = string.Join(" ", refusals);
        }

        /// <summary>
        /// The design airflow one room currently carries on one side, summed across its terminals exactly
        /// as <see cref="ApplyTargetedDesignAirFlow"/> sums it - the near end of the search bracket.
        /// <para>
        /// The room is taken from the cluster rather than trusted as handed in, for the same reason the
        /// transaction takes it from there: a caller may be holding a space from before the Approved
        /// Document F rates were applied. Null where there is no such room, or no terminal of that
        /// direction to read - conditions a candidate refuses far more usefully than this could.
        /// </para>
        /// </summary>
        private static double? CurrentDesignFlowRate_Lps(AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification)
        {
            Space space_Target = (adjacencyCluster.GetSpaces() ?? []).Find(x => x is not null && x.Guid == space.Guid);
            if (space_Target is null)
            {
                return null;
            }

            List<VentilationTerminal> ventilationTerminals = Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space_Target), flowClassification) ?? [];
            if (ventilationTerminals.Count == 0)
            {
                return null;
            }

            return ventilationTerminals.VentilationTerminalDesignDuty_Lps(flowClassification) ?? 0;
        }
    }
}
