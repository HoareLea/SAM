// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Evaluates <b>one</b> Approved Document O design airflow optimisation round - every deliberate
        /// target it was given, across every dwelling those targets fall in, as a single transaction -
        /// without touching the model it was handed, and hands back the resulting model only where all of
        /// it is valid.
        ///
        /// <para><b>Why a round is an operation and not a loop over <see cref="ApplyTargetedDesignAirFlow"/></b></para>
        /// <para>
        /// One TM59 run fails a kitchen and an ensuite in the same flat. Applying them one at a time -
        /// <c>kitchen +5, rebalance, ensuite +5, rebalance</c> - is order dependent in two separate ways,
        /// and both of them are real:
        /// </para>
        /// <list type="number">
        /// <item>The first rebalance moves the rooms the second allocation is then computed over, so the
        /// shares the second change is split into depend on which room went first. The dwelling ends
        /// balanced either way, but a different design comes out of each ordering, and the ordering is
        /// whatever order the assessment results happened to come back in.</item>
        /// <item>Worse, under
        /// <see cref="PartFExtractAllocationStrategy.MinimumFirstCookingPriority"/> the balancing extract
        /// goes to the local kitchen extract. Target a bedroom's supply first and the derived extract lands
        /// on the kitchen - the very room the next deliberate target is about to set. The engineer's figure
        /// for the kitchen is then applied on top of a consequence of somebody else's room, or overwrites
        /// it, depending on the order. Neither is what anybody asked for.</item>
        /// </list>
        /// <para>
        /// So the round computes <b>all</b> the deliberate deltas first, derives the combined balancing
        /// consequence <b>once per dwelling</b>, and allocates it over the rooms nobody targeted. The result
        /// is a function of the <i>set</i> of targets: feeding the same targets in any order produces the
        /// same design, the same adjustments and the same report, in the same order.
        /// </para>
        ///
        /// <para><b>How the combined balancing consequence is decided</b></para>
        /// <para>
        /// Let <c>cS</c> and <c>cE</c> be the total deliberate change on each side of one dwelling. A
        /// balanced dwelling has to move both sides by the same amount, so the round picks a single
        /// dwelling movement <c>m</c> and derives the shortfall on each side from it:
        /// </para>
        /// <code>
        /// only supply targeted    m = cS      derived extract = m - cE   (= cS, the single-target rule)
        /// only extract targeted   m = cE      derived supply  = m - cS   (= cE)
        /// both sides targeted     m = max(cS, cE)
        /// </code>
        /// <para>
        /// <b><c>max</c>, so no deliberate figure is ever undone.</b> Where an engineer has stated both
        /// sides and the two disagree, the round moves the dwelling to whichever they asked for more of and
        /// makes up the difference on the other side, out of rooms nobody targeted. Choosing the smaller
        /// would mean writing a room back down below the figure that was deliberately requested for it,
        /// which is not a balancing consequence - it is overruling the request. It also means that in the
        /// both-sides case every derived change is an increase, so no Approved Document F floor can be
        /// approached by it at all.
        /// </para>
        /// <para>
        /// At most one side ever carries a derived change: the side that drove <c>m</c> is already there.
        /// The common Iteration 2B case of a supply target and an extract target of the same step in one
        /// flat gives <c>cS == cE</c>, so <c>m</c> is that step and there is <b>no</b> derived change at
        /// all - the two deliberate decisions balance each other directly, and the report says so rather
        /// than inventing a consequence.
        /// </para>
        ///
        /// <para><b>A target is never allocated a derived change</b></para>
        /// <para>
        /// The balancing consequence is shared only over rooms this round did <i>not</i> target on that
        /// side. That is what makes the deliberate figures survive: whatever the allocation strategy would
        /// prefer, it cannot reach a room somebody chose. The exclusion is per room <b>and direction</b> - a
        /// room targeted on supply can still absorb derived extract, because its extract was not the thing
        /// anybody decided.
        /// </para>
        ///
        /// <para><b>The engineering is borrowed, not reimplemented</b></para>
        /// <para>
        /// Every rule here is applied by calling the code that already owns it, and this operation is a
        /// sibling of <see cref="ApplyTargetedDesignAirFlow"/> in the same class for exactly that reason:
        /// <c>VentilationSystem</c> resolves the dwelling through the room's terminals, <c>TerminalsOfSystem</c>
        /// gates attribution, <see cref="Query.ReconcileVentilationSystemDesignDuty"/> states what compliant
        /// means, <c>Allocate</c> shares the balancing consequence, <c>IsRedistributable</c> and
        /// <see cref="SetSpaceDesignFlowRate"/> write it, and <c>IsWithinSelectedVentilationUnit</c> gives
        /// the capacity verdict. Not one Approved Document F, balancing or capacity equation is restated
        /// here, so a round cannot drift from what a single manual change does.
        /// </para>
        ///
        /// <para><b>All or nothing, and never a clamp</b></para>
        /// <para>
        /// Each target is designed at exactly the figure asked for, or the round refuses. It never settles
        /// for part of a step - <see cref="ResolveTargetedDesignAirFlow"/> deliberately does clamp, and that
        /// is right for an engineer asking how much they can have, but an automatic optimiser running fixed
        /// steps must not adopt three fifths of one and simulate it as though it were the step. The whole
        /// round is worked out on <c>new AdjacencyCluster(adjacencyCluster)</c>, and on any refusal the
        /// caller's model is exactly as it was and
        /// <see cref="DesignAirFlowRoundCandidate.AdjacencyCluster"/> is null.
        /// </para>
        ///
        /// <para><b>Equipment is the constraint, never a variable</b></para>
        /// <para>
        /// Each dwelling's <b>combined</b> recalculated duty is checked against the unit already selected
        /// for it - the system's duty, never any single room's airflow. A round that outgrows one is
        /// refused, with the duty, the rating and the remaining headroom on the record, and nothing is
        /// reselected. Selecting a product is <see cref="SelectVentilationUnit"/>, called deliberately, on
        /// its own.
        /// </para>
        ///
        /// <para><b>No dwelling-to-unit ownership is assumed</b></para>
        /// <para>
        /// Targets are grouped by the ventilation system their own terminals resolve to, and the capacity
        /// check runs through <see cref="Query.AirHandlingUnitDesignDuty"/>, which already sums every system
        /// a unit supplies. One unit serving several systems therefore has its whole duty checked, which is
        /// what a normal MEP arrangement needs and what the Part O one-unit-per-dwelling case happens to
        /// reduce to.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model to evaluate against. <b>Never modified.</b></param>
        /// <param name="designAirFlowTargets">
        /// Every deliberate target of this round. Order is irrelevant. A target naming a room with no design
        /// terminal on that side, or one whose terminals cannot be attributed to a single dwelling, is
        /// reported on <see cref="DesignAirFlowRoundCandidate.TargetRefusals"/> and dropped rather than
        /// refusing the round - see <see cref="DesignAirFlowTargetRefusal"/>. Two targets naming the same
        /// room and direction refuse the round, because which one was meant is not knowable.
        /// </param>
        /// <param name="partFExtractAllocationStrategy">How a derived extract change is shared out - the
        /// same strategy the Approved Document F calculation names, passed to the same allocator.</param>
        /// <param name="tolerance_Lps">Flow rate comparison tolerance [l/s].</param>
        /// <param name="ventilationUnitCapacityDescriptors">
        /// The products each dwelling's selected unit's capacity is read from. Null makes equipment no
        /// constraint on the round at all - the same backward-compatible meaning it has everywhere else in
        /// this area.
        /// </param>
        /// <returns>
        /// What was targeted, what was derived, the duties and equipment verdict per dwelling, what could
        /// not be targeted and why - and, only where every dwelling is valid, the model to adopt. Never null.
        /// </returns>
        public static DesignAirFlowRoundCandidate EvaluateTargetedDesignAirFlows(this AdjacencyCluster adjacencyCluster, IEnumerable<DesignAirFlowTarget> designAirFlowTargets, PartFExtractAllocationStrategy partFExtractAllocationStrategy = PartFExtractAllocationStrategy.MinimumFirstCookingPriority, double tolerance_Lps = 0.001, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null)
        {
            DesignAirFlowRoundCandidate result = new();

            if (adjacencyCluster is null)
            {
                result.Refusals.Add("No model was supplied, so no design airflow optimisation round could be evaluated.");

                return result;
            }

            //FIRST, because every Approved Document F, balance and capacity comparison below is made
            //against it. See Query.IsValidFlowRateTolerance.
            if (!Query.IsValidFlowRateTolerance(tolerance_Lps))
            {
                result.Refusals.Add(Query.FlowRateToleranceRefusal(tolerance_Lps));

                return result;
            }

            List<DesignAirFlowTarget> designAirFlowTargets_Temp = [];
            foreach (DesignAirFlowTarget designAirFlowTarget in designAirFlowTargets ?? [])
            {
                if (designAirFlowTarget is not null)
                {
                    designAirFlowTargets_Temp.Add(designAirFlowTarget);
                }
            }

            if (designAirFlowTargets_Temp.Count == 0)
            {
                result.Refusals.Add("No design airflow target was supplied, so there is no optimisation round to evaluate. A round is defined by the rooms it deliberately targets.");

                return result;
            }

            //THE boundary. Everything below happens on this copy and nowhere the caller can see. Shallow
            //for the same reason EvaluateTargetedDesignAirFlow's is: every write on this path is a
            //same-guid REPLACEMENT through SetSpaceDesignFlowRate rather than an in-place mutation, and the
            //one call in this area that does mutate an object in place - SelectVentilationUnit - is never
            //reached, precisely because a round never reselects.
            AdjacencyCluster adjacencyCluster_Candidate = new(adjacencyCluster);

            //Resolved through each target's OWN terminals, so nothing here assumes which dwelling a room
            //belongs to or which unit serves it.
            Dictionary<Guid, VentilationSystem> dictionary_VentilationSystem = [];
            Dictionary<Guid, List<DesignAirFlowTarget>> dictionary_Target = [];
            HashSet<string> keys_Target = [];

            //Collected rather than returned on, so EVERY target is still examined - see the loop below.
            //Malformed requests and duplicates both land here; both refuse the round.
            HashSet<string> keys_Duplicate = [];
            List<string> refusals_Round = [];

            //ONE snapshot for the whole resolution pass, and only for it. Every target used to search the
            //candidate's whole space list for its own room, which made resolving a round quadratic in the
            //model - a five thousand space project with a target per room spent longer finding the rooms
            //than evaluating them.
            //
            //Safe here and NOWHERE below: this loop writes nothing. EvaluateDwellingRound replaces spaces on
            //the candidate as it goes, so a snapshot taken here would be stale for the second dwelling
            //onwards - which is why the Approved Document F reads inside it are deliberately left resolving
            //against the cluster on every call.
            PartFIndex partFIndex_Resolution = new(adjacencyCluster_Candidate);

            foreach (DesignAirFlowTarget designAirFlowTarget in designAirFlowTargets_Temp)
            {
                if (!Resolve(adjacencyCluster_Candidate, partFIndex_Resolution, designAirFlowTarget, out Space space_Target, out VentilationSystem ventilationSystem, out string refusal_Target, out bool malformed))
                {
                    if (malformed)
                    {
                        //NOT a dropped target. A room with no lever to move is an engineering fact about
                        //the building and the round goes on without it; a request that is not a design
                        //airflow at all - no room, no direction, not a number - is a caller that has asked
                        //for something incoherent, and quietly doing the rest of what it asked for would
                        //simulate a design missing one of its deliberate targets while reporting success.
                        refusals_Round.Add(refusal_Target);

                        continue;
                    }

                    result.TargetRefusals.Add(new DesignAirFlowTargetRefusal(designAirFlowTarget, refusal_Target));

                    continue;
                }

                //One room and one direction can be asked for once. Two figures for the same terminal set is
                //not an ordering problem this operation can settle deterministically - it is a caller that
                //has not decided, and guessing between them would be the exact silent behaviour a round
                //exists to remove.
                string key = string.Format("{0}|{1}", space_Target.Guid, designAirFlowTarget.FlowClassification);

                if (!keys_Target.Add(key))
                {
                    //Recorded and the loop CONTINUES, rather than returning here. Returning made even the
                    //refused round's report depend on the order its targets arrived in - a target that
                    //happened to sit after the duplicate was never examined, so its reason went unreported
                    //and the sort below was skipped altogether. The round is refused either way; what it
                    //says about why has to be the same whichever way round the same set was handed over.
                    //
                    //Once per room and direction, however many times it was repeated: a caller that stated
                    //the same terminal set three times has made one mistake, not two.
                    if (keys_Duplicate.Add(key))
                    {
                        refusals_Round.Add(string.Format(
                            "Space '{0}' was given more than one deliberate {1} design airflow in the same optimisation round, so which one was meant is not knowable. State one target per room and direction. Nothing was changed.",
                            space_Target.Name,
                            Core.Query.Description(designAirFlowTarget.FlowClassification)));
                    }

                    continue;
                }

                dictionary_VentilationSystem[ventilationSystem.Guid] = ventilationSystem;

                if (!dictionary_Target.TryGetValue(ventilationSystem.Guid, out List<DesignAirFlowTarget> designAirFlowTargets_System))
                {
                    designAirFlowTargets_System = [];
                    dictionary_Target[ventilationSystem.Guid] = designAirFlowTargets_System;
                }

                designAirFlowTargets_System.Add(designAirFlowTarget);
            }

            //Sorted on the SAME key the taken targets are, so the report a round produces is a function of
            //the SET of targets in every part - the refusals included. They are appended in the caller's
            //enumeration order above because that is the order the reasons are discovered in; leaving them
            //that way would mean the same failing set read differently depending on how it happened to be
            //enumerated, which is exactly what this operation promises does not happen.
            result.TargetRefusals.Sort(CompareTargetRefusals);

            if (refusals_Round.Count != 0)
            {
                //Ordinally, for the same reason the target refusals are sorted: the same set of incoherent
                //requests must read the same whichever of them the caller listed first.
                refusals_Round.Sort(StringComparer.Ordinal);

                result.Refusals.AddRange(refusals_Round);

                return result;
            }

            if (dictionary_Target.Count == 0)
            {
                result.Refusals.Add(string.Format(
                    "None of the {0} target(s) this optimisation round was given can be taken, so there is no round to evaluate. Each one is reported with its own reason. Nothing was changed.",
                    designAirFlowTargets_Temp.Count));

                return result;
            }

            //Sorted, so the report - and the floating point summation inside every allocation below - does
            //not depend on the order the targets arrived in.
            List<Guid> guids_VentilationSystem = [.. dictionary_VentilationSystem.Keys];
            guids_VentilationSystem.Sort();

            foreach (Guid guid_VentilationSystem in guids_VentilationSystem)
            {
                List<DesignAirFlowTarget> designAirFlowTargets_System = dictionary_Target[guid_VentilationSystem];

                designAirFlowTargets_System.Sort(CompareTargets);

                DwellingDesignAirFlowRound dwellingDesignAirFlowRound = EvaluateDwellingRound(
                    adjacencyCluster,
                    adjacencyCluster_Candidate,
                    dictionary_VentilationSystem[guid_VentilationSystem],
                    designAirFlowTargets_System,
                    partFExtractAllocationStrategy,
                    tolerance_Lps,
                    ventilationUnitCapacityDescriptors);

                result.DwellingRounds.Add(dwellingDesignAirFlowRound);

                result.Warnings.AddRange(dwellingDesignAirFlowRound.Warnings);
                result.Refusals.AddRange(dwellingDesignAirFlowRound.Refusals);
            }

            if (result.Refusals.Count != 0)
            {
                //REFUSED, and no model handed back - not even for the dwellings that were fine. A round is
                //one transaction: adopting the valid half would leave the caller holding a design its own
                //policy never approved, and a subsequent round would then be computed from it.
                return result;
            }

            //ONLY NOW, with every dwelling of the round written. A unit's duty is the sum over every system
            //it supplies, so asking mid-round would judge it against a state that never existed - see
            //ValidateVentilationUnits.
            ValidateVentilationUnits(adjacencyCluster_Candidate, result, ventilationUnitCapacityDescriptors, tolerance_Lps);

            if (result.Refusals.Count != 0)
            {
                return result;
            }

            foreach (DwellingDesignAirFlowRound dwellingDesignAirFlowRound in result.DwellingRounds)
            {
                //Copied only NOW. A refused round's dwelling notes are written in the present tense - "the
                //system now designs ... l/s" - which is true of a round about to be adopted and false of
                //one that was rejected. They stay on the dwelling for anyone who wants them; they just do
                //not get to speak for the round as though it had happened.
                result.Notes.AddRange(dwellingDesignAirFlowRound.Notes);
            }

            result.AdjacencyCluster = adjacencyCluster_Candidate;

            return result;
        }

        /// <summary>
        /// One dwelling's whole share of the round: preconditions, every deliberate delta, the single
        /// derived balancing consequence, the writes, and the selected unit's verdict on the result.
        /// </summary>
        /// <param name="adjacencyCluster">The caller's model, read for the "before" duties only - reporting
        /// what adopting this would change means quoting the duty it would change FROM, and a "before"
        /// taken off the candidate would only ever agree with itself.</param>
        /// <param name="adjacencyCluster_Candidate">The copy everything is written on.</param>
        private static DwellingDesignAirFlowRound EvaluateDwellingRound(
            AdjacencyCluster adjacencyCluster,
            AdjacencyCluster adjacencyCluster_Candidate,
            VentilationSystem ventilationSystem,
            List<DesignAirFlowTarget> designAirFlowTargets,
            PartFExtractAllocationStrategy partFExtractAllocationStrategy,
            double tolerance_Lps,
            IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors)
        {
            DwellingDesignAirFlowRound result = new()
            {
                VentilationSystem = ventilationSystem,
            };

            VentilationSystem ventilationSystem_Before = adjacencyCluster.GetObject<VentilationSystem>(ventilationSystem.Guid);
            if (ventilationSystem_Before is not null)
            {
                adjacencyCluster.VentilationSystemDesignDuty(ventilationSystem_Before, out double supplyDuty_Before_Lps, out double extractDuty_Before_Lps);

                result.SupplyDuty_Before_Lps = supplyDuty_Before_Lps;
                result.ExtractDuty_Before_Lps = extractDuty_Before_Lps;
            }

            // ---- Preconditions, exactly the two ApplyTargetedDesignAirFlow states ------------------------

            //A balanced dwelling. A round adds deliberate changes and the matching derived change, which
            //preserves whatever residual was already there - it cannot repair one.
            adjacencyCluster_Candidate.VentilationSystemDesignDuty(ventilationSystem, out double supplyDuty_Lps, out double extractDuty_Lps);

            if (System.Math.Abs(supplyDuty_Lps - extractDuty_Lps) > tolerance_Lps)
            {
                result.Refusals.Add(string.Format(
                    "Ventilation system '{0}' already designs {1:0.###} l/s of supply against {2:0.###} l/s of extract, so the dwelling is not a valid balanced design to optimise. A round moves both sides together and cannot close a residual that was there first. Nothing was changed.",
                    ventilationSystem.FullName,
                    supplyDuty_Lps,
                    extractDuty_Lps));

                return result;
            }

            //And a COMPLIANT one - balance is a property of the totals, the Approved Document F floor is a
            //property of each room, and one is not evidence of the other. Asked through the single
            //definition of compliant so this cannot drift from what the preparation refuses to simulate.
            //An already-invalid dwelling is refused, never repaired: quietly fixing a room nobody targeted
            //would be an unrequested design decision.
            adjacencyCluster_Candidate.ReconcileVentilationSystemDesignDuty(ventilationSystem, out _, out _, out List<string> refusals_Compliance, tolerance_Lps);

            if (refusals_Compliance.Count != 0)
            {
                result.Refusals.Add(string.Format(
                    "Ventilation system '{0}' is not a valid design to optimise, because it does not currently meet Approved Document F: {1} Nothing was changed - an existing shortfall is not repaired as a side effect of an optimisation round.",
                    ventilationSystem.FullName,
                    string.Join(" ", refusals_Compliance)));

                return result;
            }

            // ---- Every room of the dwelling, on both sides, attributed before any of it is trusted -------

            //Both directions on every room, because a round's targets and its derived consequence can each
            //land on either side. The gate is the one ApplyTargetedDesignAirFlow uses and refuses for the
            //same reason: a room whose terminals are shared with another system cannot be read honestly or
            //written safely, and re-planning around it would balance the dwelling using a subset of its
            //rooms without saying so.
            List<Space> spaces_Dwelling = [.. adjacencyCluster_Candidate.GetRelatedObjects<Space>(ventilationSystem) ?? []];

            spaces_Dwelling.RemoveAll(x => x is null);
            spaces_Dwelling.Sort((x, y) => x.Guid.CompareTo(y.Guid));

            Dictionary<string, double> dictionary_Duty = [];
            Dictionary<string, double> dictionary_Requirement = [];

            List<Space> spaces_Supply = [];
            List<Space> spaces_Extract = [];

            foreach (Space space_Dwelling in spaces_Dwelling)
            {
                foreach (FlowClassification flowClassification in new[] { FlowClassification.Supply, FlowClassification.Extract })
                {
                    if (!TerminalsOfSystem(adjacencyCluster_Candidate, space_Dwelling, flowClassification, ventilationSystem, out List<VentilationTerminal> ventilationTerminals, out string refusal_Attribution))
                    {
                        result.Refusals.Add(refusal_Attribution);

                        return result;
                    }

                    if (ventilationTerminals.Count == 0)
                    {
                        continue;
                    }

                    (flowClassification == FlowClassification.Supply ? spaces_Supply : spaces_Extract).Add(space_Dwelling);

                    dictionary_Duty[Key(space_Dwelling, flowClassification)] = ventilationTerminals.VentilationTerminalDesignDuty_Lps(flowClassification) ?? 0;
                    dictionary_Requirement[Key(space_Dwelling, flowClassification)] = adjacencyCluster_Candidate.PartFRequiredFlowRate_Lps(space_Dwelling, flowClassification) ?? 0;
                }
            }

            // ---- Every deliberate delta, computed before anything is written -----------------------------

            Dictionary<string, double> dictionary_Planned = [];

            double change_Supply_Lps = 0;
            double change_Extract_Lps = 0;

            bool targeted_Supply = false;
            bool targeted_Extract = false;

            foreach (DesignAirFlowTarget designAirFlowTarget in designAirFlowTargets)
            {
                Space space_Target = spaces_Dwelling.Find(x => x.Guid == designAirFlowTarget.SpaceGuid);
                string key = Key(space_Target, designAirFlowTarget.FlowClassification);

                if (space_Target is null || !dictionary_Duty.TryGetValue(key, out double before_Lps))
                {
                    //Unreachable: the target resolved its system through these very terminals. Said loudly
                    //rather than assumed, because reaching it would mean the resolution and the dwelling
                    //enumeration disagree about which rooms belong to this system.
                    result.Refusals.Add(string.Format(
                        "Space '{0}' resolved to ventilation system '{1}' but is not among the rooms that system relates to, which should not be possible. Nothing was changed.",
                        designAirFlowTarget.SpaceName ?? "?",
                        ventilationSystem.FullName));

                    return result;
                }

                double designFlowRate_Lps = designAirFlowTarget.DesignFlowRate_Lps;
                double requirement_Lps = dictionary_Requirement[key];

                //The regulatory floor, read and never written. A target may be raised as far as anyone
                //likes and lowered only as far as Approved Document F allows.
                if (designFlowRate_Lps + tolerance_Lps < requirement_Lps)
                {
                    result.Refusals.Add(string.Format(
                        "Space '{0}': a design {1} airflow of {2:0.###} l/s is below the {3:0.###} l/s Approved Document F requires of that room. Design airflow is chosen above the regulatory minimum, never below it, so nothing was changed.",
                        space_Target.Name,
                        Core.Query.Description(designAirFlowTarget.FlowClassification),
                        designFlowRate_Lps,
                        requirement_Lps));

                    return result;
                }

                if (designFlowRate_Lps < requirement_Lps)
                {
                    //Within tolerance below the floor: snapped to the floor exactly, BEFORE the delta is
                    //taken, so the whole round is planned from the value that will actually be written.
                    //Tolerance decides whether two airflows are the same number; it never permits a design
                    //airflow to be recorded below the regulatory minimum.
                    result.Notes.Add(string.Format(
                        "Space '{0}': the requested design {1} airflow of {2:0.######} l/s is below the {3:0.###} l/s Approved Document F requires of that room by less than the {4:0.###} l/s tolerance, so it was raised to exactly that requirement before the round was planned.",
                        space_Target.Name,
                        Core.Query.Description(designAirFlowTarget.FlowClassification),
                        designFlowRate_Lps,
                        requirement_Lps,
                        tolerance_Lps));

                    designFlowRate_Lps = requirement_Lps;
                }

                dictionary_Planned[key] = designFlowRate_Lps;

                if (designAirFlowTarget.FlowClassification == FlowClassification.Supply)
                {
                    change_Supply_Lps += designFlowRate_Lps - before_Lps;
                    targeted_Supply = true;
                }
                else
                {
                    change_Extract_Lps += designFlowRate_Lps - before_Lps;
                    targeted_Extract = true;
                }

                result.TargetedAdjustments.Add(new DesignAirFlowAdjustment(
                    space_Target.Guid,
                    space_Target.Name,
                    designAirFlowTarget.FlowClassification,
                    before_Lps,
                    designFlowRate_Lps,
                    requirement_Lps,
                    false));
            }

            // ---- ONE combined balancing consequence, derived from those deltas together ------------------

            //See the class documentation for why max() is the right choice where both sides were targeted:
            //it is the only one that never writes a deliberately requested room back down.
            double movement_Lps = targeted_Supply && targeted_Extract
                ? System.Math.Max(change_Supply_Lps, change_Extract_Lps)
                : targeted_Supply ? change_Supply_Lps : change_Extract_Lps;

            foreach (FlowClassification flowClassification in new[] { FlowClassification.Supply, FlowClassification.Extract })
            {
                double change_Derived_Lps = movement_Lps - (flowClassification == FlowClassification.Supply ? change_Supply_Lps : change_Extract_Lps);

                if (System.Math.Abs(change_Derived_Lps) <= tolerance_Lps)
                {
                    continue;
                }

                //The rooms nobody targeted on this side, and no others. Excluding the targets is what makes
                //a deliberate figure survive a round: the allocation strategy cannot reach a room somebody
                //chose, whatever it would otherwise prefer.
                List<Space> spaces_Derived = [];
                Dictionary<Guid, double> dictionary_Duty_Derived = [];
                Dictionary<Guid, double> dictionary_Requirement_Derived = [];

                foreach (Space space_Side in flowClassification == FlowClassification.Supply ? spaces_Supply : spaces_Extract)
                {
                    string key = Key(space_Side, flowClassification);

                    if (dictionary_Planned.ContainsKey(key))
                    {
                        continue;
                    }

                    spaces_Derived.Add(space_Side);
                    dictionary_Duty_Derived[space_Side.Guid] = dictionary_Duty[key];
                    dictionary_Requirement_Derived[space_Side.Guid] = dictionary_Requirement[key];
                }

                if (spaces_Derived.Count == 0)
                {
                    result.Refusals.Add(string.Format(
                        "The round's deliberate targets in ventilation system '{0}' need {1:0.###} l/s of {2} to balance, and every room on that side of the dwelling is itself a deliberate target - so there is nowhere for the balancing consequence to go that would not overwrite a figure somebody chose. Target fewer rooms on that side, or state the balancing rooms deliberately. Nothing was changed.",
                        ventilationSystem.FullName,
                        System.Math.Abs(change_Derived_Lps),
                        Core.Query.Description(flowClassification)));

                    return result;
                }

                //The SAME allocator a single manual change uses, with the same strategy - so a round's
                //balancing and a manual edit's balancing cannot disagree.
                if (!Allocate(spaces_Derived, dictionary_Duty_Derived, dictionary_Requirement_Derived, change_Derived_Lps, flowClassification, partFExtractAllocationStrategy, adjacencyCluster_Candidate, tolerance_Lps, out Dictionary<Guid, double> dictionary_Planned_Derived, out string note_Allocation, out string refusal_Allocation))
                {
                    result.Refusals.Add(refusal_Allocation);

                    return result;
                }

                result.Notes.Add(note_Allocation);

                foreach (Space space_Derived in spaces_Derived)
                {
                    if (!dictionary_Planned_Derived.TryGetValue(space_Derived.Guid, out double planned_Lps))
                    {
                        continue;
                    }

                    string key = Key(space_Derived, flowClassification);

                    //Skipped only where the room genuinely does not move - never merely because its share
                    //is smaller than the tolerance. A change well above tolerance can divide into shares
                    //that are each below it, and skipping them all would write the targets, balance
                    //nothing, and leave exactly the partial round this operation promises never to produce.
                    if (planned_Lps == dictionary_Duty[key])
                    {
                        continue;
                    }

                    dictionary_Planned[key] = planned_Lps;

                    result.DerivedAdjustments.Add(new DesignAirFlowAdjustment(
                        space_Derived.Guid,
                        space_Derived.Name,
                        flowClassification,
                        dictionary_Duty[key],
                        planned_Lps,
                        dictionary_Requirement[key],
                        true));
                }
            }

            // ---- Last precondition, over every room the round will write ---------------------------------

            //A room total is shared out in proportion to what its terminals already carry, so a terminal
            //carrying something that is not a quantity of air makes that impossible - and the room TOTAL
            //does not reveal it, because the duty sum skips a NaN. Asked of every room here, before the
            //first write, so the all-or-nothing promise cannot be broken one room in.
            foreach (DesignAirFlowAdjustment designAirFlowAdjustment in result.Adjustments)
            {
                Space space_Written = spaces_Dwelling.Find(x => x.Guid == designAirFlowAdjustment.SpaceGuid);

                if (!IsRedistributable(Query.VentilationTerminals(adjacencyCluster_Candidate.VentilationTerminals(space_Written), designAirFlowAdjustment.FlowClassification), space_Written, designAirFlowAdjustment.FlowClassification, out string refusal_Redistributable))
                {
                    result.Refusals.Add(refusal_Redistributable);

                    return result;
                }
            }

            // ---- Write. Every floor is already checked, so nothing below can refuse ----------------------

            List<string> refusals = [];

            foreach (DesignAirFlowAdjustment designAirFlowAdjustment in result.Adjustments)
            {
                Space space_Written = spaces_Dwelling.Find(x => x.Guid == designAirFlowAdjustment.SpaceGuid);

                if (adjacencyCluster_Candidate.SetSpaceDesignFlowRate(space_Written, designAirFlowAdjustment.FlowClassification, designAirFlowAdjustment.After_Lps, out List<string> notes_Written, out List<string> refusals_Written, tolerance_Lps) is null)
                {
                    refusals.AddRange(refusals_Written);

                    continue;
                }

                result.Notes.AddRange(notes_Written);
            }

            if (refusals.Count != 0)
            {
                //Unreachable by design: the plan above checks exactly the conditions SetSpaceDesignFlowRate
                //checks. Reported rather than rolled back, because the candidate copy is discarded anyway -
                //the caller's model never saw any of it - and a loud, specific message is what would get
                //the drift fixed.
                result.Refusals.Add(string.Format(
                    "The optimisation round for ventilation system '{0}' was validated and then partly refused while being written to the candidate, which should not be possible: {1} Nothing was changed - the candidate is discarded.",
                    ventilationSystem.FullName,
                    string.Join(" ", refusals)));

                return result;
            }

            adjacencyCluster_Candidate.VentilationSystemDesignDuty(ventilationSystem, out double supplyDuty_After_Lps, out double extractDuty_After_Lps);

            result.SupplyDuty_After_Lps = supplyDuty_After_Lps;
            result.ExtractDuty_After_Lps = extractDuty_After_Lps;

            if (System.Math.Abs(supplyDuty_After_Lps - extractDuty_After_Lps) > tolerance_Lps)
            {
                //A REFUSAL, never a warning, and unreachable by design: the dwelling was balanced before
                //anything was written and the round moves both sides to the same movement. Reaching it
                //means the allocation and the duty derivation have drifted apart, which is worth saying.
                result.Refusals.Add(string.Format(
                    "Ventilation system '{0}' designs {1:0.###} l/s of supply against {2:0.###} l/s of extract after a round that was balanced when it was planned, which should not be possible. Nothing was changed - the candidate is discarded.",
                    ventilationSystem.FullName,
                    supplyDuty_After_Lps,
                    extractDuty_After_Lps));

                return result;
            }

            result.Notes.Add(string.Format(
                "Ventilation system '{0}' now designs {1:0.###} l/s of supply and {2:0.###} l/s of extract, from {3} deliberate target(s) and {4} derived balancing change(s) computed once from them together. Every Approved Document F requirement is unchanged, and the transfer network and the unit's design duty are recalculated by re-preparing the iteration.",
                ventilationSystem.FullName,
                supplyDuty_After_Lps,
                extractDuty_After_Lps,
                result.TargetedAdjustments.Count,
                result.DerivedAdjustments.Count));

            //Resolved on the CANDIDATE, because the duty it will be judged against is the candidate's. The
            //judging itself happens in a SECOND PASS, once every dwelling of the round has been written -
            //see ValidateVentilationUnits.
            result.AirHandlingUnit = Query.AirHandlingUnit(adjacencyCluster_Candidate, ventilationSystem);

            return result;
        }

        /// <summary>
        /// Checks every air handling unit the round touched against the unit <b>already selected</b> for
        /// it - <b>after</b> every dwelling of the round has been written, and <b>once per unit</b>.
        ///
        /// <para><b>Why this cannot happen inside the per-dwelling loop</b></para>
        /// <para>
        /// <see cref="Query.AirHandlingUnitDesignDuty"/> sums every system a unit supplies, which is what
        /// makes it correct for a unit serving more than one. Asked while the round is half written, it
        /// therefore reads a duty that is partly the round's and partly the design the round is replacing -
        /// a state that never existed and never will. Two systems on one unit, one rising 10 l/s and the
        /// other falling 10 l/s, would have the first checked at a duty 10 l/s above where the round
        /// actually leaves it: a unit sitting on its rating refused for a round that fits it exactly. In
        /// the cases that were accepted anyway, the earlier systems still reported headroom measured
        /// against that intermediate state - a wrong number in the audit trail of a run whose whole purpose
        /// is to be auditable.
        /// </para>
        /// <para>
        /// The Approved Document O workflow gives each dwelling its own unit, so there the two orders
        /// agree - but the general MEP arrangement of one unit serving several systems is precisely what
        /// this operation promises to remain correct for, and a promise kept only by the shape of one
        /// workflow is not kept.
        /// </para>
        ///
        /// <para><b>One verdict per unit, shared by every dwelling on it</b></para>
        /// <para>
        /// Dwellings that share a unit share its constraint, so they share its answer: the same descriptor,
        /// outcome, headroom and - where it refuses - the same refusal on each of them. That is what lets a
        /// caller retrying a round without the dwellings that hit capacity remove all of them together,
        /// rather than meeting the same unit again on the next attempt.
        /// </para>
        /// <para>
        /// Units are visited in name order, so what a round reports does not depend on the order its
        /// targets arrived in.
        /// </para>
        /// </summary>
        private static void ValidateVentilationUnits(AdjacencyCluster adjacencyCluster, DesignAirFlowRoundCandidate designAirFlowRoundCandidate, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, double tolerance_Lps)
        {
            //Grouped by the unit INSTANCE, so two systems on one unit are one check and one answer.
            Dictionary<Guid, List<DwellingDesignAirFlowRound>> dictionary = [];
            Dictionary<Guid, AirHandlingUnit> dictionary_AirHandlingUnit = [];

            foreach (DwellingDesignAirFlowRound dwellingDesignAirFlowRound in designAirFlowRoundCandidate.DwellingRounds)
            {
                AirHandlingUnit airHandlingUnit = dwellingDesignAirFlowRound.AirHandlingUnit;
                if (airHandlingUnit is null)
                {
                    //No unit resolved for this dwelling, so equipment is not a constraint on it. The
                    //outcome stays NotApplicable, exactly as it does where no catalogue was offered.
                    continue;
                }

                dictionary_AirHandlingUnit[airHandlingUnit.Guid] = airHandlingUnit;

                if (!dictionary.TryGetValue(airHandlingUnit.Guid, out List<DwellingDesignAirFlowRound> dwellingDesignAirFlowRounds))
                {
                    dwellingDesignAirFlowRounds = [];
                    dictionary[airHandlingUnit.Guid] = dwellingDesignAirFlowRounds;
                }

                dwellingDesignAirFlowRounds.Add(dwellingDesignAirFlowRound);
            }

            List<Guid> guids = [.. dictionary.Keys];

            guids.Sort((x, y) =>
            {
                int result = string.CompareOrdinal(dictionary_AirHandlingUnit[x].Name, dictionary_AirHandlingUnit[y].Name);

                return result != 0 ? result : x.CompareTo(y);
            });

            foreach (Guid guid in guids)
            {
                AirHandlingUnit airHandlingUnit = dictionary_AirHandlingUnit[guid];

                bool sufficient = IsWithinSelectedVentilationUnit(
                    adjacencyCluster,
                    airHandlingUnit,
                    ventilationUnitCapacityDescriptors,
                    tolerance_Lps,
                    out VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor,
                    out VentilationUnitSelectionOutcome ventilationUnitSelectionOutcome,
                    out string reason,
                    out double supplyHeadroom_Lps,
                    out double extractHeadroom_Lps,
                    out string note);

                //The UNIT's own duty, which for a unit serving several systems is not any one dwelling's -
                //and is the quantity the rating was actually compared against, so it is the one reported.
                if (!adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps))
                {
                    supplyDuty_Lps = double.NaN;
                    extractDuty_Lps = double.NaN;
                }

                string refusal = sufficient ? null : string.Format(
                    "The optimisation round was calculated and then rejected, because the ventilation unit selected for air handling unit '{0}' cannot carry it: {1} Nothing was changed - the model is exactly as it was. The round would have left that unit moving {2:0.###} l/s of supply and {3:0.###} l/s of extract against a rating of {4:0.###}/{5:0.###} l/s, leaving {6:0.###}/{7:0.###} l/s of headroom. The selected product is the constraint this round explores within; a larger one is chosen deliberately, on its own.",
                    airHandlingUnit.Name,
                    reason,
                    supplyDuty_Lps,
                    extractDuty_Lps,
                    ventilationUnitCapacityDescriptor?.MaximumSupplyFlowRate_Lps ?? double.NaN,
                    ventilationUnitCapacityDescriptor?.MaximumExtractFlowRate_Lps ?? double.NaN,
                    supplyHeadroom_Lps,
                    extractHeadroom_Lps);

                foreach (DwellingDesignAirFlowRound dwellingDesignAirFlowRound in dictionary[guid])
                {
                    dwellingDesignAirFlowRound.VentilationUnitCapacityDescriptor = ventilationUnitCapacityDescriptor;
                    dwellingDesignAirFlowRound.VentilationUnitSelectionOutcome = ventilationUnitSelectionOutcome;
                    dwellingDesignAirFlowRound.VentilationUnitSelectionReason = reason;
                    dwellingDesignAirFlowRound.SupplyHeadroom_Lps = supplyHeadroom_Lps;
                    dwellingDesignAirFlowRound.ExtractHeadroom_Lps = extractHeadroom_Lps;

                    if (sufficient)
                    {
                        if (note is not null)
                        {
                            dwellingDesignAirFlowRound.Notes.Add(note);
                        }

                        continue;
                    }

                    //Every dwelling on this unit carries the refusal, because every one of them is subject
                    //to it - and a caller dropping the dwellings that hit capacity then drops all of them.
                    dwellingDesignAirFlowRound.Refusals.Add(refusal);

                    designAirFlowRoundCandidate.Refusals.Add(refusal);
                }
            }
        }

        /// <summary>
        /// The room and the dwelling one target names, or the reason it cannot be taken - and, crucially,
        /// <b>which kind of reason it is</b>.
        ///
        /// <para><b>Two different things are being told apart here</b></para>
        /// <list type="bullet">
        /// <item><b>No lever</b> (<paramref name="malformed"/> false). The request is coherent and the
        /// building simply cannot answer it: the room has no design terminal on that side, or its terminals
        /// belong to no ventilation system or to more than one. That is an engineering fact, the target is
        /// dropped with its reason, and the round goes on without it - because a room this optimisation
        /// cannot move must not stop every other failing room in the building. Creating a terminal is never
        /// an answer: it would size a duty the Approved Document F assessment never asked for.</item>
        /// <item><b>Malformed</b> (<paramref name="malformed"/> true). The request is not a design airflow
        /// at all: no room, a direction that is neither supply nor extract, a rate that is not a finite
        /// non-negative number, or a room that is not in this model. Dropping one of these and applying the
        /// rest would execute part of a transaction the caller asked for as a whole - and, for an automatic
        /// optimiser, would simulate a design quietly missing one of its deliberate targets while reporting
        /// the round as a success. These refuse the round.</item>
        /// </list>
        /// </summary>
        /// <param name="malformed">Whether the request itself was incoherent, rather than the building
        /// being unable to answer a coherent one.</param>
        private static bool Resolve(AdjacencyCluster adjacencyCluster, PartFIndex partFIndex, DesignAirFlowTarget designAirFlowTarget, out Space space, out VentilationSystem ventilationSystem, out string refusal, out bool malformed)
        {
            space = null;
            ventilationSystem = null;
            refusal = null;
            malformed = true;

            if (designAirFlowTarget.Space is null)
            {
                refusal = "An optimisation round was given a target naming no space at all, so there is nothing to target. Nothing was changed.";

                return false;
            }

            if (designAirFlowTarget.FlowClassification != FlowClassification.Supply && designAirFlowTarget.FlowClassification != FlowClassification.Extract)
            {
                refusal = string.Format("Space '{0}': a design airflow has to be supply or extract, and '{1}' is neither. Nothing was changed.", designAirFlowTarget.SpaceName ?? "?", Core.Query.Description(designAirFlowTarget.FlowClassification));

                return false;
            }

            if (double.IsNaN(designAirFlowTarget.DesignFlowRate_Lps) || double.IsInfinity(designAirFlowTarget.DesignFlowRate_Lps) || designAirFlowTarget.DesignFlowRate_Lps < 0)
            {
                refusal = string.Format("Space '{0}': {1} l/s is not a design airflow - it has to be a finite, non-negative number of litres per second. Nothing was changed.", designAirFlowTarget.SpaceName ?? "?", designAirFlowTarget.DesignFlowRate_Lps);

                return false;
            }

            //Taken from the cluster rather than trusted as handed in: a caller may be holding a space from
            //before the Approved Document F rates were applied, and that one carries a different parameter
            //set. Resolved through the round's own snapshot of that same cluster, which answers exactly what
            //searching its space list answers and does not rebuild the list per target.
            space = partFIndex.Space(designAirFlowTarget.SpaceGuid);
            if (space is null)
            {
                refusal = string.Format("Space '{0}' is not in the model being optimised, so a round cannot target it. Nothing was changed.", designAirFlowTarget.SpaceName ?? "?");

                return false;
            }

            //From here the request is coherent: anything that refuses below is the building unable to
            //answer it, which drops the target rather than the round.
            malformed = false;

            //The SAME resolution a single manual change makes, through the room's own terminals rather than
            //through any zone or unit ownership - so an air handling unit serving more than one system, or
            //a model still carrying the systems it was authored with, resolves the same way for both.
            DwellingDesignAirFlowChange dwellingDesignAirFlowChange = new();

            ventilationSystem = VentilationSystem(adjacencyCluster, space, designAirFlowTarget.FlowClassification, dwellingDesignAirFlowChange);
            if (ventilationSystem is null)
            {
                refusal = string.Join(" ", dwellingDesignAirFlowChange.Refusals);

                return false;
            }

            if (!TerminalsOfSystem(adjacencyCluster, space, designAirFlowTarget.FlowClassification, ventilationSystem, out List<VentilationTerminal> _, out string refusal_Attribution))
            {
                ventilationSystem = null;
                refusal = refusal_Attribution;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Orders two targets of one dwelling by room guid then direction - the whole of what makes a round
        /// independent of the order its targets arrived in, since every value below is then summed and
        /// reported in this order rather than in the caller's.
        /// </summary>
        private static int CompareTargets(DesignAirFlowTarget x, DesignAirFlowTarget y)
        {
            int result = x.SpaceGuid.CompareTo(y.SpaceGuid);

            return result != 0 ? result : ((int)x.FlowClassification).CompareTo((int)y.FlowClassification);
        }

        /// <summary>
        /// Orders two dropped targets by the same key their taken counterparts are ordered on, so a round
        /// that could take none of what it was given still reports the same thing whichever way round it
        /// was handed them.
        /// <para>
        /// The reason breaks a tie, because a target naming no room at all carries
        /// <see cref="System.Guid.Empty"/> and several of those would otherwise be interchangeable to the
        /// sort while saying different things to a reader.
        /// </para>
        /// <para>
        /// The requested rate breaks the last tie. Two dropped targets can share a room, a direction and a
        /// reason - one bathroom asked for two different supply figures, with no supply terminal to move
        /// for either - and the report prints the rate, so a key without it leaves that pair to the mercy
        /// of the sort and the same set can still read two ways.
        /// </para>
        /// </summary>
        private static int CompareTargetRefusals(DesignAirFlowTargetRefusal x, DesignAirFlowTargetRefusal y)
        {
            int result = CompareTargets(x.DesignAirFlowTarget, y.DesignAirFlowTarget);

            if (result != 0)
            {
                return result;
            }

            result = string.CompareOrdinal(x.Reason, y.Reason);

            return result != 0 ? result : x.DesignAirFlowTarget.DesignFlowRate_Lps.CompareTo(y.DesignAirFlowTarget.DesignFlowRate_Lps);
        }

        /// <summary>One room and one direction, which is the grain everything in a round is keyed at.</summary>
        private static string Key(Space space, FlowClassification flowClassification)
        {
            return space is null ? null : string.Format("{0}|{1}", space.Guid, flowClassification);
        }
    }
}
