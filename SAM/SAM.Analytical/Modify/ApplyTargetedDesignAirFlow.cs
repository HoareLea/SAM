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
        /// Sets <b>one</b> room's design airflow and rebalances the dwelling around it, as a single
        /// transaction.
        /// <para>
        /// <b>This is the architectural operation Approved Document O optimisation is built on, and the
        /// separation it draws is the point of it.</b> A failing bedroom is raised from 20 to 24 l/s. That
        /// is the <i>targeted</i> change, and it is the only room anybody chose: bedroom 2 and the living
        /// room pass, are not targets, and do not move. The dwelling's supply then exceeds its extract by
        /// 4 l/s, and a mechanical ventilation with heat recovery unit moves the air it takes in, so
        /// 4 l/s of extract has to appear somewhere. Where it appears is a <i>derived</i> consequence of
        /// the balanced network, decided by the allocation strategy the Part F calculation already names -
        /// never a second optimisation target.
        /// </para>
        /// <code>
        /// targeted:   Bedroom 1 supply 20 -> 24
        /// derived:    extract +4, allocated across the dwelling's extract terminals
        ///             transfer paths recalculated on the next preparation
        ///             air handling unit design duty follows
        /// unchanged:  every Approved Document F requirement, and every room nobody targeted
        /// </code>
        ///
        /// <para><b>The allocation rule is borrowed, not invented</b></para>
        /// <para>
        /// Extract above what Approved Document F requires is shared by
        /// <see cref="PartFExtractAllocationStrategy.MinimumFirstCookingPriority"/> - the surplus goes to
        /// the local kitchen extract, because the cooking function is the dwelling's largest single source
        /// of moisture and pollutants and removing them closest to source is the stated aim of requirement
        /// F1(1)(a). Where the dwelling has no local kitchen extract, and on the supply side where the
        /// Approved Document names no equivalent priority, the change is shared in proportion to what the
        /// rooms already carry, which preserves whatever balance a designer had already struck.
        /// <b><c>PartFCalculator.AllocateContinuousExtract</c> makes exactly this choice for exactly this
        /// reason</b>; this is the design-side application of it, and it is applied to the <i>change</i>
        /// rather than recomputed from scratch so that a deliberate imbalance a designer authored survives.
        /// </para>
        ///
        /// <para><b>All or nothing</b></para>
        /// <para>
        /// The whole plan - the targeted room and every derived one - is computed and checked against
        /// every Approved Document F floor <b>before a single terminal is written</b>. A dwelling that
        /// cannot be balanced is refused with nothing changed, rather than left with a raised bedroom and
        /// no matching extract. That state is exactly what
        /// <c>Modify.PreparePartOIteration</c>'s conservation check refuses to simulate, and producing it
        /// and then reporting it would be a worse answer than never producing it.
        /// </para>
        ///
        /// <para><b>What this does NOT do</b></para>
        /// <para>
        /// It writes design airflows and nothing else. The transfer paths, the inter-zone air movements
        /// and the air handling unit's derived duty are rebuilt by re-preparing the iteration, which is
        /// idempotent and already refuses where the dwelling's adjacencies cannot route the result - see
        /// <c>Modify.AddPartFTransferAirMovements</c>. No Approved Document F requirement is touched, and
        /// no runtime or profile airflow is written.
        /// </para>
        /// <para>
        /// <b>Equipment is validated, never selected, unless <paramref name="ventilationUnitCapacityDescriptors"/>
        /// is supplied</b> - and even then, ONLY after the airflow change above has already committed.
        /// <c>Query.IsVentilationUnitSufficient</c> checks the serving air handling unit's current
        /// selection against the recalculated duty; if it still suffices nothing is reselected
        /// (<see cref="Enums.VentilationUnitSelectionOutcome.Kept"/>); if not, <c>Modify.SelectVentilationUnit</c>
        /// escalates to the next compliant product from the catalogue offered
        /// (<see cref="Enums.VentilationUnitSelectionOutcome.Reselected"/>), or refuses if none is capable
        /// (<see cref="Enums.VentilationUnitSelectionOutcome.Refused"/>). <b>The airflow change is never
        /// rolled back because equipment could not be validated</b> - the two are separate questions, and
        /// an equipment gap is reported beside a successful design change, not instead of it. Leave the
        /// catalogue unconnected/null to keep exactly this method's behaviour from before equipment
        /// validation existed - see <see cref="DwellingDesignAirFlowChange.VentilationUnitSelectionOutcome"/>.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">
        /// The model. <b>Modified in place on success, and untouched on a refusal</b> - and <b>this cluster
        /// is as far as either half of the change reaches</b>. Both the airflow and any reselection are
        /// written as replacement objects, so a cluster taken from <c>AnalyticalModel.AdjacencyCluster</c>
        /// can be edited here and then kept or thrown away without the model behind it moving.
        /// </param>
        /// <param name="space">The one room being targeted.</param>
        /// <param name="flowClassification">Which side of that room is being set.</param>
        /// <param name="designFlowRate_Lps">The room's new design airflow [l/s], as a total across its terminals.</param>
        /// <param name="partFExtractAllocationStrategy">
        /// How a derived extract change is shared out. Defaults to the same strategy
        /// <c>PartFCalculator</c> defaults to, so a design rebalance and the sizing that preceded it agree
        /// about where surplus extract belongs.
        /// </param>
        /// <param name="ventilationUnitCapacityDescriptors">
        /// The products available to validate the serving air handling unit's selection against, once the
        /// airflow change has committed. Null keeps this method's behaviour exactly as it was before
        /// equipment validation existed - no unit is resolved, checked or reselected.
        /// </param>
        /// <returns>
        /// What was targeted, what was derived, the duties that resulted, and - where a catalogue was
        /// offered - what happened to the serving unit's selection. A refusal writes nothing. Never null.
        /// </returns>
        public static DwellingDesignAirFlowChange ApplyTargetedDesignAirFlow(this AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification, double designFlowRate_Lps, PartFExtractAllocationStrategy partFExtractAllocationStrategy = PartFExtractAllocationStrategy.MinimumFirstCookingPriority, double tolerance_Lps = 0.001, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null)
        {
            DwellingDesignAirFlowChange result = new();

            // ---- Resolve and validate, writing nothing --------------------------------------------------

            if (adjacencyCluster is null || space is null)
            {
                result.Refusals.Add("No space was supplied, so no design airflow could be applied.");

                return result;
            }

            if (flowClassification != FlowClassification.Supply && flowClassification != FlowClassification.Extract)
            {
                result.Refusals.Add(string.Format("A design airflow has to be supply or extract, and '{0}' is neither. Nothing was changed.", Core.Query.Description(flowClassification)));

                return result;
            }

            //FIRST, because every check below is a comparison against it. See Query.IsValidFlowRateTolerance.
            if (!Query.IsValidFlowRateTolerance(tolerance_Lps))
            {
                result.Refusals.Add(Query.FlowRateToleranceRefusal(tolerance_Lps));

                return result;
            }

            if (double.IsNaN(designFlowRate_Lps) || double.IsInfinity(designFlowRate_Lps) || designFlowRate_Lps < 0)
            {
                result.Refusals.Add(string.Format("Space '{0}': {1} l/s is not a design airflow. Nothing was changed.", space.Name, designFlowRate_Lps));

                return result;
            }

            //ONE snapshot for this transaction's reads. Everything between here and the writes below only
            //reads the model: the targeted room is resolved through it, and so is the Approved Document F
            //floor of every room on the other side of the dwelling. Searching the whole space list for each
            //of those made a transaction on a dwelling cost O(model) per room of it.
            PartFIndex partFIndex = new(adjacencyCluster);

            //Taken from the cluster rather than trusted as handed in: a caller may be holding a space from
            //before the Part F rates were applied, and that one carries a different parameter set.
            Space space_Target = partFIndex.Space(space.Guid);
            if (space_Target is null)
            {
                result.Refusals.Add(string.Format("Space '{0}' is not in the model, so its design airflow could not be applied.", space.Name));

                return result;
            }

            //The dwelling is the ventilation system the targeted room's terminals belong to. Resolved
            //through the terminals rather than through the space, because a model routinely still carries
            //the system assignment it was authored with and that assignment may describe a different design
            //stage - Modify.AddPartOBaseMVHRSystem keys reuse on the design terminals for the same reason.
            VentilationSystem ventilationSystem = VentilationSystem(adjacencyCluster, space_Target, flowClassification, result);
            if (ventilationSystem is null)
            {
                return result;
            }

            result.VentilationSystem = ventilationSystem;

            //A balanced dwelling is the PRECONDITION, checked before anything is written.
            //
            //This transaction adds a targeted change and the matching derived change, which preserves
            //whatever residual the dwelling already had - it cannot repair one. Reporting success while
            //leaving supply and extract disagreeing would hand back a model the preparation is going to
            //refuse anyway, with a transaction that claims it produced a valid design. An earlier revision
            //only warned here; that was wrong, and the contract is now: validate feasibility, then write.
            adjacencyCluster.VentilationSystemDesignDuty(ventilationSystem, out double supplyDuty_Before_Lps, out double extractDuty_Before_Lps);

            if (System.Math.Abs(supplyDuty_Before_Lps - extractDuty_Before_Lps) > tolerance_Lps)
            {
                result.Refusals.Add(string.Format(
                    "Ventilation system '{0}' already designs {1:0.###} l/s of supply against {2:0.###} l/s of extract, so the dwelling is not a valid balanced design to change. A targeted change and its balancing consequence move both sides together and cannot close a residual that was there first. Return the dwelling to a balanced design - or re-run the Approved Document F calculation and the iteration preparation - before targeting a room. Nothing was changed.",
                    ventilationSystem.FullName,
                    supplyDuty_Before_Lps,
                    extractDuty_Before_Lps));

                return result;
            }

            //A COMPLIANT dwelling is the other half of the precondition, and balance alone does not give it.
            //
            //A dwelling can balance globally while a room sits below its own Approved Document F floor -
            //a bathroom designed at 5 l/s against a 10 l/s requirement, offset by a kitchen at 15 against
            //10, totals 20 either way. Raising a bedroom by 1 l/s then derives 1 l/s of kitchen extract
            //under cooking priority, leaves the bathroom at 5, and reports success: a transaction claiming
            //a valid design for a dwelling that was never compliant. Balance is a property of the totals;
            //the Approved Document F floor is a property of each room, and one is not evidence of the other.
            //
            //Checked through Query.ReconcileVentilationSystemDesignDuty rather than re-derived here, so
            //there is exactly ONE definition of what compliant means and this can never drift from what
            //Modify.PreparePartOIteration refuses to simulate. Only its refusals are read: its notes and
            //warnings are about design headroom, which is legal and is not this method's business to report.
            //
            //An already-invalid dwelling is REFUSED, never repaired. Quietly fixing a room nobody targeted
            //would be an unrequested design decision, and it would hide the defect from the engineer who
            //has to answer for it.
            adjacencyCluster.ReconcileVentilationSystemDesignDuty(ventilationSystem, out _, out _, out List<string> refusals_Compliance, tolerance_Lps);

            if (refusals_Compliance.Count != 0)
            {
                result.Refusals.Add(string.Format(
                    "Ventilation system '{0}' is not a valid design to change, because it does not currently meet Approved Document F: {1} Nothing was changed - an existing shortfall is not repaired as a side effect of targeting a different room.",
                    ventilationSystem.FullName,
                    string.Join(" ", refusals_Compliance)));

                return result;
            }

            //Every terminal this transaction would read or write has to be unambiguously part of THIS
            //system before any of it is trusted - see TerminalsOfSystem.
            if (!TerminalsOfSystem(adjacencyCluster, space_Target, flowClassification, ventilationSystem, out List<VentilationTerminal> ventilationTerminals_Target, out string refusal_Target))
            {
                result.Refusals.Add(refusal_Target);

                return result;
            }

            double duty_Target_Before_Lps = ventilationTerminals_Target.VentilationTerminalDesignDuty_Lps(flowClassification) ?? 0;

            //The regulatory floor, read and never written. The targeted room may be raised as far as anyone
            //likes and lowered only as far as Approved Document F allows.
            double? requirement_Target_Lps = partFIndex.PartFRequiredFlowRate_Lps(space_Target, flowClassification);

            if (requirement_Target_Lps.HasValue && designFlowRate_Lps < requirement_Target_Lps.Value)
            {
                if (designFlowRate_Lps + tolerance_Lps < requirement_Target_Lps.Value)
                {
                    result.Refusals.Add(string.Format(
                        "Space '{0}': a design {1} airflow of {2:0.###} l/s is below the {3:0.###} l/s Approved Document F requires of that room. Design airflow is chosen above the regulatory minimum, never below it, so nothing was changed.",
                        space_Target.Name,
                        Core.Query.Description(flowClassification),
                        designFlowRate_Lps,
                        requirement_Target_Lps.Value));

                    return result;
                }

                //WITHIN tolerance below the floor - raised to the floor rather than written as asked.
                //
                //Tolerance decides whether two airflows are the SAME NUMBER. It is not a licence to record
                //a design airflow below the regulatory minimum, and the two readings only look alike until
                //something drives deliberately at the edge: a bounded search asking for the lowest design
                //this room will take answers 27.999 against a 28 l/s requirement, every comparison here
                //passes it, and the model then PERSISTS a room designed below what Approved Document F
                //requires of it. The Iteration 2 invariant is PartFRequired <= Design, and a value the
                //comparison cannot tell apart from the floor is set to the floor exactly.
                //
                //Snapped HERE, before change_Lps, so the whole transaction is planned from the value that
                //will actually be written: the opposite-side allocation balances against it, the duties
                //report it, and TargetedAdjustment states it. Planning from 27.999 and writing 28 would
                //leave the dwelling out of balance by exactly the amount that was corrected.
                result.Notes.Add(string.Format(
                    "Space '{0}': the requested design {1} airflow of {2:0.######} l/s is below the {3:0.###} l/s Approved Document F requires of that room by less than the {4:0.###} l/s tolerance, so it was raised to exactly that requirement before the change was planned. Tolerance decides whether two airflows are the same number; it never permits a design airflow to be recorded below the regulatory floor.",
                    space_Target.Name,
                    Core.Query.Description(flowClassification),
                    designFlowRate_Lps,
                    requirement_Target_Lps.Value,
                    tolerance_Lps));

                designFlowRate_Lps = requirement_Target_Lps.Value;
            }

            double change_Lps = designFlowRate_Lps - duty_Target_Before_Lps;

            // ---- Plan the derived consequence, still writing nothing ------------------------------------

            FlowClassification flowClassification_Opposite = flowClassification == FlowClassification.Supply ? FlowClassification.Extract : FlowClassification.Supply;

            //Every room of the dwelling that carries a terminal on the other side, with what it carries now
            //and what Approved Document F requires of it. These are the rooms the balancing consequence can
            //be spread over, and no others - a room in another dwelling is never one of them.
            List<Space> spaces_Opposite = [];
            Dictionary<Guid, double> dictionary_Duty = [];
            Dictionary<Guid, double> dictionary_Requirement = [];

            foreach (Space space_Related in adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [])
            {
                //Attribution is checked for EVERY candidate room before any of them is written, not only
                //for the rooms that end up moving. A room this transaction would have to leave alone
                //because its terminals are shared with another system is a room whose duty cannot be read
                //honestly either, so the whole transaction is refused rather than quietly re-planned around
                //it - re-planning would balance the dwelling using a subset of its rooms without saying so.
                if (!TerminalsOfSystem(adjacencyCluster, space_Related, flowClassification_Opposite, ventilationSystem, out List<VentilationTerminal> ventilationTerminals, out string refusal_Opposite))
                {
                    result.Refusals.Add(refusal_Opposite);

                    return result;
                }

                if (ventilationTerminals.Count == 0)
                {
                    continue;
                }

                spaces_Opposite.Add(space_Related);
                dictionary_Duty[space_Related.Guid] = ventilationTerminals.VentilationTerminalDesignDuty_Lps(flowClassification_Opposite) ?? 0;
                dictionary_Requirement[space_Related.Guid] = partFIndex.PartFRequiredFlowRate_Lps(space_Related, flowClassification_Opposite) ?? 0;
            }

            Dictionary<Guid, double> dictionary_Planned = [];

            if (System.Math.Abs(change_Lps) > tolerance_Lps)
            {
                if (spaces_Opposite.Count == 0)
                {
                    result.Refusals.Add(string.Format(
                        "Space '{0}' can be designed at {1:0.###} l/s of {2}, but ventilation system '{3}' has no {4} terminal for the resulting {5:0.###} l/s to be balanced by, so the dwelling would gain air it never loses and TAS would refuse to simulate it. Nothing was changed.",
                        space_Target.Name,
                        designFlowRate_Lps,
                        Core.Query.Description(flowClassification),
                        ventilationSystem.FullName,
                        Core.Query.Description(flowClassification_Opposite),
                        System.Math.Abs(change_Lps)));

                    return result;
                }

                if (!Allocate(spaces_Opposite, dictionary_Duty, dictionary_Requirement, change_Lps, flowClassification_Opposite, partFExtractAllocationStrategy, adjacencyCluster, tolerance_Lps, out dictionary_Planned, out string note_Allocation, out string refusal_Allocation))
                {
                    result.Refusals.Add(refusal_Allocation);

                    return result;
                }

                result.Notes.Add(note_Allocation);
            }

            //LAST precondition, and it covers every room this transaction will write - the target and each
            //planned derived room.
            //
            //A room total is shared out in proportion to what its terminals already carry, so a terminal
            //carrying something that is not a quantity of air makes that impossible. The room TOTAL does
            //not reveal it: Query.VentilationTerminalDesignDuty_Lps skips a NaN, so a room holding one NaN
            //terminal beside healthy ones sums to a total that meets its requirement and passes every plan
            //check above. Only the setter notices, one room in - by which time the target has been written
            //and the all-or-nothing promise is already broken. So it is asked here, of every room, before
            //the first write.
            List<Space> spaces_Written = [space_Target];
            foreach (Space space_Opposite in spaces_Opposite)
            {
                if (dictionary_Planned.ContainsKey(space_Opposite.Guid))
                {
                    spaces_Written.Add(space_Opposite);
                }
            }

            foreach (Space space_Written in spaces_Written)
            {
                FlowClassification flowClassification_Written = space_Written.Guid == space_Target.Guid ? flowClassification : flowClassification_Opposite;

                if (!IsRedistributable(Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space_Written), flowClassification_Written), space_Written, flowClassification_Written, out string refusal_Redistributable))
                {
                    result.Refusals.Add(refusal_Redistributable);

                    return result;
                }
            }

            // ---- Apply. Every floor is already checked, so nothing below can refuse ---------------------

            List<string> refusals = [];

            List<VentilationTerminal> ventilationTerminals_Written = adjacencyCluster.SetSpaceDesignFlowRate(space_Target, flowClassification, designFlowRate_Lps, out List<string> notes_Target, out List<string> refusals_Target, tolerance_Lps);

            refusals.AddRange(refusals_Target);

            if (ventilationTerminals_Written is not null)
            {
                result.Notes.AddRange(notes_Target);

                result.TargetedAdjustment = new DesignAirFlowAdjustment(
                    space_Target.Guid,
                    space_Target.Name,
                    flowClassification,
                    duty_Target_Before_Lps,
                    designFlowRate_Lps,
                    requirement_Target_Lps ?? double.NaN,
                    false);
            }

            foreach (Space space_Opposite in spaces_Opposite)
            {
                if (!dictionary_Planned.TryGetValue(space_Opposite.Guid, out double planned_Lps))
                {
                    continue;
                }

                double before_Lps = dictionary_Duty[space_Opposite.Guid];

                //Skipped only where the room genuinely does not move. NOT where its share is merely
                //smaller than the tolerance: a change well above tolerance can divide into shares that are
                //each below it - 1.5 l/s across two rooms against a 1 l/s tolerance gives two 0.75 l/s
                //shares - and skipping them all would write the target, balance nothing, and leave exactly
                //the partial change this transaction promises never to produce. The tolerance decides
                //whether a CHANGE is worth making, which was settled above; it does not get to veto the
                //pieces that change is made of.
                if (planned_Lps == before_Lps)
                {
                    continue;
                }

                if (adjacencyCluster.SetSpaceDesignFlowRate(space_Opposite, flowClassification_Opposite, planned_Lps, out List<string> notes_Opposite, out List<string> refusals_Opposite, tolerance_Lps) is null)
                {
                    refusals.AddRange(refusals_Opposite);

                    continue;
                }

                result.Notes.AddRange(notes_Opposite);

                result.DerivedAdjustments.Add(new DesignAirFlowAdjustment(
                    space_Opposite.Guid,
                    space_Opposite.Name,
                    flowClassification_Opposite,
                    before_Lps,
                    planned_Lps,
                    dictionary_Requirement[space_Opposite.Guid],
                    true));
            }

            if (refusals.Count != 0)
            {
                //Unreachable by design: the plan above checks exactly the conditions SetSpaceDesignFlowRate
                //checks, so a write that refuses here means the two have drifted apart. Reported rather than
                //rolled back, because a rollback nobody can trigger is untested code, and a loud, specific
                //message is what would actually get the drift fixed.
                result.Refusals.Add(string.Format(
                    "The design change to space '{0}' was validated and then partly refused while being written, which should not be possible: {1} The model may now hold a partly applied change - re-run the Part F calculation and the iteration preparation before trusting it.",
                    space_Target.Name,
                    string.Join(" ", refusals)));

                return result;
            }

            adjacencyCluster.VentilationSystemDesignDuty(ventilationSystem, out double supplyDuty_Lps, out double extractDuty_Lps);

            result.SupplyDuty_Lps = supplyDuty_Lps;
            result.ExtractDuty_Lps = extractDuty_Lps;

            if (System.Math.Abs(supplyDuty_Lps - extractDuty_Lps) > tolerance_Lps)
            {
                //A REFUSAL, never a warning. A successful transaction means a valid balanced design, and a
                //result that says "successful" beside a dwelling gaining air it never loses is the exact
                //claim this operation exists to make impossible.
                //
                //Unreachable by design: the dwelling was checked balanced before anything was written, and
                //the targeted and derived changes move both sides by the same amount. Reaching it means the
                //allocation and the duty derivation have drifted apart, which is worth saying loudly.
                result.Refusals.Add(string.Format(
                    "Ventilation system '{0}' designs {1:0.###} l/s of supply against {2:0.###} l/s of extract after a change that was balanced when it was planned, which should not be possible. The model may now hold a partly applied change - re-run the Approved Document F calculation and the iteration preparation before trusting it.",
                    ventilationSystem.FullName,
                    supplyDuty_Lps,
                    extractDuty_Lps));

                return result;
            }

            result.Notes.Add(string.Format(
                "Ventilation system '{0}' now designs {1:0.###} l/s of supply and {2:0.###} l/s of extract, from one targeted change to space '{3}' and {4} derived balancing change(s). Every Approved Document F requirement is unchanged, and the transfer network and the unit's design duty are recalculated by re-preparing the iteration.",
                ventilationSystem.FullName,
                supplyDuty_Lps,
                extractDuty_Lps,
                space_Target.Name,
                result.DerivedAdjustments.Count));

            //Equipment is validated AFTER the airflow change above has already committed, and never rolls
            //it back - see the class documentation. Skipped entirely (outcome stays NotApplicable) unless
            //a catalogue was actually offered, which keeps every existing caller's behaviour unchanged.
            if (ventilationUnitCapacityDescriptors is not null)
            {
                ValidateVentilationUnit(adjacencyCluster, ventilationSystem, ventilationUnitCapacityDescriptors, result, tolerance_Lps);
            }

            return result;
        }

        /// <summary>
        /// Checks the serving air handling unit's CURRENT selection against the duty
        /// <see cref="ApplyTargetedDesignAirFlow"/> just recalculated, and re-selects only where it no
        /// longer suffices - see <see cref="Query.IsVentilationUnitSufficient"/> and
        /// <see cref="SelectVentilationUnit"/>, which this composes rather than reimplements.
        /// <para>
        /// <b>Never called just because a smaller capable product exists.</b> A currently sufficient unit
        /// is left exactly as selected - re-running the smallest-capable search on every change would let
        /// an unrelated catalogue edit swap out equipment nobody asked to change.
        /// </para>
        /// <para>
        /// <b>Validates an existing selection; never makes a first one.</b> A unit nothing has ever been
        /// selected for is not "exhausted" and reselecting for it is not what this call was asked to do -
        /// see <see cref="VentilationUnitSelectionOutcome.NotApplicable"/>. Making a first selection is
        /// still exactly <see cref="SelectVentilationUnit"/>, called deliberately, on its own.
        /// </para>
        /// <para>
        /// <b>An unknown capacity is refused, never treated as exhausted.</b> Where the CURRENTLY selected
        /// product is not among the descriptors this call was given -
        /// <see cref="Query.SelectedVentilationUnitCapacityDescriptor"/> finds nothing - its adequacy is
        /// unknown, not insufficient. Falling through to reselection here would let a filtered or
        /// otherwise incomplete catalogue silently downgrade a unit that is still entirely adequate, just
        /// not describable from what this call was handed.
        /// </para>
        /// </summary>
        private static void ValidateVentilationUnit(AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, DwellingDesignAirFlowChange result, double tolerance_Lps)
        {
            AirHandlingUnit airHandlingUnit = Query.AirHandlingUnit(adjacencyCluster, ventilationSystem);
            if (airHandlingUnit is null)
            {
                //Nothing to validate - a system with no resolvable unit (the natural ventilation route, or
                //one not yet bound to any). Outcome stays NotApplicable.
                return;
            }

            //Resolved either way, so a caller can always find the unit this call reasoned about - even
            //where nothing further below runs.
            result.AirHandlingUnit = airHandlingUnit;

            VentilationUnitReference ventilationUnitReference_Selected = airHandlingUnit.SelectedVentilationUnitReference();
            if (ventilationUnitReference_Selected is null)
            {
                //A unit nothing has ever been selected for is not an equipment GAP this call was asked to
                //close. Outcome stays NotApplicable.
                return;
            }

            if (airHandlingUnit.SelectedVentilationUnitCapacityDescriptor(ventilationUnitCapacityDescriptors) is null)
            {
                //Unknown, not insufficient - the catalogue this call was given does not describe the
                //product actually selected, so nothing here can say whether it still suffices. Refused,
                //exactly as an exhausted unit is: nothing is written, the existing selection stands.
                result.VentilationUnitSelectionOutcome = VentilationUnitSelectionOutcome.Refused;

                result.VentilationUnitSelectionReason = string.Format(
                    "Air handling unit '{0}' is selected as '{1}', which is not among the ventilation unit products offered to this call, so its adequacy against the recalculated design duty is unknown rather than met. Nothing was reselected.",
                    airHandlingUnit.Name,
                    ventilationUnitReference_Selected);

                return;
            }

            if (adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, ventilationUnitCapacityDescriptors, out _, tolerance_Lps))
            {
                result.VentilationUnitSelectionOutcome = VentilationUnitSelectionOutcome.Kept;

                result.Notes.Add(string.Format(
                    "Air handling unit '{0}' keeps its selected product '{1}': the recalculated design duty is still within its rating.",
                    airHandlingUnit.Name,
                    result.VentilationUnitReference));

                return;
            }

            VentilationUnitSelection ventilationUnitSelection = adjacencyCluster.SelectVentilationUnit(airHandlingUnit, ventilationUnitCapacityDescriptors, out List<string> notes_Selection, out List<string> refusals_Selection, tolerance_Lps);

            //The cluster's own instance, re-resolved by guid after SelectVentilationUnit's own write - so
            //this reports what the model now holds rather than the (pre-write) instance asked about.
            result.AirHandlingUnit = adjacencyCluster.GetObject<AirHandlingUnit>(airHandlingUnit.Guid) ?? airHandlingUnit;

            if (ventilationUnitSelection.IsSelected)
            {
                result.VentilationUnitSelectionOutcome = VentilationUnitSelectionOutcome.Reselected;
                result.Notes.AddRange(notes_Selection);

                return;
            }

            //Deliberately NOT added to result.Refusals: the airflow change already committed, by the
            //contract ApplyTargetedDesignAirFlow's own tests already pin. An equipment gap is reported
            //here, beside a successful design change, never rolled back into it.
            result.VentilationUnitSelectionOutcome = VentilationUnitSelectionOutcome.Refused;
            result.VentilationUnitSelectionReason = string.Join(" ", refusals_Selection);
        }

        /// <summary>
        /// The one ventilation system the targeted room's terminals of this direction belong to.
        /// <para>
        /// Zero refuses - there is nothing to set and nothing to balance against. More than one refuses as
        /// ambiguous, because which dwelling is being rebalanced would be a guess, and
        /// <c>Modify.AddPartOBaseMVHRSystem</c> refuses on exactly the same condition.
        /// </para>
        /// </summary>
        private static VentilationSystem VentilationSystem(AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification, DwellingDesignAirFlowChange result)
        {
            List<VentilationTerminal> ventilationTerminals = Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space), flowClassification) ?? [];

            if (ventilationTerminals.Count == 0)
            {
                result.Refusals.Add(string.Format(
                    "Space '{0}' has no design {1} terminal, so there is no design airflow to set. Realize the Approved Document F requirements first, or add a terminal deliberately - creating one here would invent a duty the assessment did not size.",
                    space.Name,
                    Core.Query.Description(flowClassification)));

                return null;
            }

            List<VentilationSystem> ventilationSystems = [];

            foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
            {
                foreach (VentilationSystem ventilationSystem in adjacencyCluster.GetRelatedObjects<VentilationSystem>(ventilationTerminal) ?? [])
                {
                    if (ventilationSystem is not null && ventilationSystems.Find(x => x.Guid == ventilationSystem.Guid) is null)
                    {
                        ventilationSystems.Add(ventilationSystem);
                    }
                }
            }

            if (ventilationSystems.Count == 0)
            {
                result.Refusals.Add(string.Format(
                    "The design {0} terminals of space '{1}' belong to no ventilation system, so there is no dwelling to rebalance the change over. Prepare the iteration first. Nothing was changed.",
                    Core.Query.Description(flowClassification),
                    space.Name));

                return null;
            }

            if (ventilationSystems.Count > 1)
            {
                result.Refusals.Add(string.Format(
                    "The design {0} terminals of space '{1}' belong to {2} ventilation systems ({3}), so which dwelling the change would be rebalanced over is ambiguous. Nothing was changed.",
                    Core.Query.Description(flowClassification),
                    space.Name,
                    ventilationSystems.Count,
                    string.Join(", ", ventilationSystems.ConvertAll(x => string.Format("'{0}'", x.FullName)))));

                return null;
            }

            return ventilationSystems[0];
        }

        /// <summary>
        /// The design terminals of one room and one direction, <b>only</b> where every one of them is
        /// unambiguously part of <paramref name="ventilationSystem"/>.
        /// <para>
        /// <b>Why this gate exists.</b> A duty is summed per room and per direction, and
        /// <see cref="SetSpaceDesignFlowRate"/> writes every terminal of that room and direction. Where a
        /// room holds terminals belonging to this Part O system <i>and</i> to another ventilation system -
        /// a dwelling served by a second unit, a model still carrying the systems it was authored with -
        /// both the sum and the write would silently reach across the boundary, and a targeted change to
        /// one dwelling would move another system's design duty while reporting itself as belonging to
        /// this one.
        /// </para>
        /// <para>
        /// <b>Refused rather than filtered, deliberately.</b> Writing only the subset that belongs here
        /// needs a system-scoped setter that does not exist, and inventing one would be a multi-system
        /// allocation architecture Iteration 2 has no business introducing. Refusing is the smallest safe
        /// answer: it changes nothing, and it names the condition an engineer has to resolve.
        /// </para>
        /// <para>
        /// A terminal related to <b>no</b> system at all is caught by the same rule and for the same
        /// reason - nothing says it belongs to this dwelling, so writing it would be a guess.
        /// </para>
        /// </summary>
        /// <returns>False where the room cannot be attributed, with <paramref name="refusal"/> set.</returns>
        private static bool TerminalsOfSystem(AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification, VentilationSystem ventilationSystem, out List<VentilationTerminal> ventilationTerminals, out string refusal)
        {
            ventilationTerminals = Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space), flowClassification) ?? [];
            refusal = null;

            List<string> names_Foreign = [];
            bool unattributed = false;

            foreach (VentilationTerminal ventilationTerminal in ventilationTerminals)
            {
                List<VentilationSystem> ventilationSystems = adjacencyCluster.GetRelatedObjects<VentilationSystem>(ventilationTerminal) ?? [];

                if (ventilationSystems.Count == 0)
                {
                    unattributed = true;

                    continue;
                }

                foreach (VentilationSystem ventilationSystem_Related in ventilationSystems)
                {
                    if (ventilationSystem_Related is not null && ventilationSystem_Related.Guid != ventilationSystem.Guid && !names_Foreign.Contains(ventilationSystem_Related.FullName))
                    {
                        names_Foreign.Add(ventilationSystem_Related.FullName);
                    }
                }
            }

            if (names_Foreign.Count == 0 && !unattributed)
            {
                return true;
            }

            names_Foreign.Sort(StringComparer.Ordinal);

            string reason = names_Foreign.Count == 0
                ? "at least one of them belongs to no ventilation system at all"
                : string.Format("some of them belong to {0}", string.Join(", ", names_Foreign.ConvertAll(x => string.Format("'{0}'", x))));

            refusal = string.Format(
                "Space '{0}' holds design {1} terminals that are not all part of ventilation system '{2}' - {3}. A room's design airflow is set across every terminal of that direction, so changing this room would move a duty that does not belong to this dwelling. Nothing was changed. Separate the terminals onto the systems that own them, or target a room whose terminals are unambiguous.",
                space.Name,
                Core.Query.Description(flowClassification),
                ventilationSystem.FullName,
                reason);

            ventilationTerminals = [];

            return false;
        }

        /// <summary>
        /// Shares <paramref name="change_Lps"/> across the rooms on the other side of the system, and says
        /// whether it can be done without taking any of them below what Approved Document F requires.
        /// <para>
        /// <b>Applied to the change, never recomputed from scratch.</b> Re-deriving every room's share from
        /// its requirement would silently undo any imbalance a designer had deliberately authored between
        /// them, which is a design decision this has no business overwriting.
        /// </para>
        /// <para>
        /// <b>An increase and a reduction are not the same problem, and are not shared the same way.</b>
        /// An increase can go anywhere, so it follows the allocation strategy. A reduction can only come
        /// out of design airflow that is actually <i>there to remove</i> - the headroom a room has above
        /// its own Approved Document F requirement, <c>max(0, duty - requirement)</c> - and a room sitting
        /// exactly on its floor has none. Sharing a reduction in proportion to total duty, as an earlier
        /// revision did, hands a share to a room that cannot give it up and then refuses the whole change
        /// as impossible while another room was holding all the headroom needed. That made reversing a
        /// previous targeted change - the most ordinary thing an optimisation does - fail.
        /// </para>
        /// </summary>
        private static bool Allocate(
            List<Space> spaces,
            Dictionary<Guid, double> dictionary_Duty,
            Dictionary<Guid, double> dictionary_Requirement,
            double change_Lps,
            FlowClassification flowClassification,
            PartFExtractAllocationStrategy partFExtractAllocationStrategy,
            AdjacencyCluster adjacencyCluster,
            double tolerance_Lps,
            out Dictionary<Guid, double> dictionary_Planned,
            out string note,
            out string refusal)
        {
            dictionary_Planned = [];
            note = null;
            refusal = null;

            //The rooms the cooking-priority strategy prefers, where it applies at all. Exactly as
            //PartFCalculator.AllocateContinuousExtract uses it for the surplus above the Table 1.2
            //minimums: extract beyond what the Approved Document requires belongs at the cooking function,
            //the dwelling's largest single source of moisture and cooking pollutants. It applies only to
            //extract, and only where the dwelling actually has a local kitchen extract terminal.
            List<Space> spaces_Preferred = [];

            if (flowClassification == FlowClassification.Extract && partFExtractAllocationStrategy == PartFExtractAllocationStrategy.MinimumFirstCookingPriority)
            {
                foreach (Space space in spaces)
                {
                    if (IsLocalKitchenExtract(adjacencyCluster, space))
                    {
                        spaces_Preferred.Add(space);
                    }
                }
            }

            string basis;

            if (change_Lps >= 0)
            {
                // ---- An increase can go anywhere, so the strategy decides where -------------------------

                List<Space> spaces_Target;

                if (spaces_Preferred.Count != 0)
                {
                    spaces_Target = spaces_Preferred;
                    basis = "the minimum-first, cooking-priority strategy, which puts extract above the Approved Document F requirement at the cooking function";
                }
                else
                {
                    //Either the strategy is volume weighted, or the side being balanced is supply, or the
                    //dwelling has no local kitchen extract. Sharing in proportion to what the rooms already
                    //carry is the neutral rule: it is independent of the order the rooms are listed in, and
                    //it preserves whatever proportion the design already had between them.
                    spaces_Target = spaces;
                    basis = "sharing in proportion to the design airflow those rooms already carry";
                }

                double weight_Total = 0;
                foreach (Space space in spaces_Target)
                {
                    weight_Total += dictionary_Duty[space.Guid];
                }

                foreach (Space space in spaces_Target)
                {
                    double share_Lps = weight_Total > tolerance_Lps
                        ? change_Lps * (dictionary_Duty[space.Guid] / weight_Total)
                        : change_Lps / spaces_Target.Count;

                    dictionary_Planned[space.Guid] = dictionary_Duty[space.Guid] + share_Lps;
                }

                Note(spaces_Target, change_Lps, flowClassification, basis, out note);

                return true;
            }

            // ---- A reduction can only come out of headroom that is there to remove ----------------------

            //What each room can actually give up: its design airflow above its own Approved Document F
            //requirement, and never a litre more. A room sitting exactly on its floor offers nothing and is
            //simply not asked - which is the whole fix. Handing it a proportional share and then refusing
            //the change as impossible was a false refusal, and it made reversing a previous targeted change
            //fail even though the dwelling could plainly absorb it.
            Dictionary<Guid, double> dictionary_Removable = [];

            foreach (Space space in spaces)
            {
                dictionary_Removable[space.Guid] = System.Math.Max(0, dictionary_Duty[space.Guid] - dictionary_Requirement[space.Guid]);
                dictionary_Planned[space.Guid] = dictionary_Duty[space.Guid];
            }

            //Preferred rooms first, then the rest. On the cooking-priority strategy that removes the
            //surplus from where the strategy put it, so a reduction retraces the increase that created it
            //and a reversal lands exactly back where it started. Rooms with no headroom drop out of both
            //tiers on their own, because their removable is zero.
            List<List<Space>> tiers = [];

            if (spaces_Preferred.Count != 0)
            {
                tiers.Add(spaces_Preferred);

                List<Space> spaces_Rest = [];
                foreach (Space space in spaces)
                {
                    if (spaces_Preferred.Find(x => x.Guid == space.Guid) is null)
                    {
                        spaces_Rest.Add(space);
                    }
                }

                tiers.Add(spaces_Rest);

                basis = "the minimum-first, cooking-priority strategy, which takes extract back from the cooking function first and only then from the other rooms with design headroom";
            }
            else
            {
                tiers.Add(spaces);
                basis = "sharing in proportion to the design headroom those rooms hold above their Approved Document F requirement";
            }

            double remaining_Lps = -change_Lps;

            List<Space> spaces_Reduced = [];

            foreach (List<Space> tier in tiers)
            {
                if (remaining_Lps <= tolerance_Lps)
                {
                    break;
                }

                double removable_Tier_Lps = 0;
                foreach (Space space in tier)
                {
                    removable_Tier_Lps += dictionary_Removable[space.Guid];
                }

                if (removable_Tier_Lps <= tolerance_Lps)
                {
                    continue;
                }

                //Never more than the tier holds, so a share can never exceed a room's own headroom and no
                //floor can be breached by construction.
                double take_Lps = System.Math.Min(remaining_Lps, removable_Tier_Lps);

                foreach (Space space in tier)
                {
                    double removable_Lps = dictionary_Removable[space.Guid];
                    if (removable_Lps <= 0)
                    {
                        continue;
                    }

                    double share_Lps = take_Lps * (removable_Lps / removable_Tier_Lps);

                    dictionary_Planned[space.Guid] = dictionary_Duty[space.Guid] - share_Lps;

                    spaces_Reduced.Add(space);
                }

                remaining_Lps -= take_Lps;
            }

            if (remaining_Lps > tolerance_Lps)
            {
                //Genuinely impossible: the rooms on this side are holding less design headroom between them
                //than the change needs to give up, so balancing it would have to reduce at least one of
                //them below what the Approved Document requires. Requirements are never lowered to make a
                //design balance.
                double removable_Total_Lps = 0;
                foreach (Space space in spaces)
                {
                    removable_Total_Lps += dictionary_Removable[space.Guid];
                }

                refusal = string.Format(
                    "Balancing the change needs {0:0.###} l/s less {1}, and the rooms on that side of ventilation system's design hold only {2:0.###} l/s of headroom above what Approved Document F requires of them. Reducing them further would take a room below its regulatory minimum, which is never done to make a design balance, so nothing was changed.",
                    -change_Lps,
                    Core.Query.Description(flowClassification),
                    removable_Total_Lps);

                return false;
            }

            //Belt and braces: the tier arithmetic cannot breach a floor, and this says so out loud rather
            //than trusting it silently. A breach here would mean the removable figures and the planned
            //figures have drifted apart.
            foreach (Space space in spaces)
            {
                if (dictionary_Planned[space.Guid] + tolerance_Lps < dictionary_Requirement[space.Guid])
                {
                    refusal = string.Format(
                        "Balancing the change would take space '{0}' to {1:0.###} l/s against the {2:0.###} l/s Approved Document F requires of it. Nothing was changed.",
                        space.Name,
                        dictionary_Planned[space.Guid],
                        dictionary_Requirement[space.Guid]);

                    return false;
                }
            }

            Note(spaces_Reduced, change_Lps, flowClassification, basis, out note);

            return true;
        }

        /// <summary>
        /// Says which rooms absorbed the balancing consequence and on what basis - and says, every time,
        /// that they were not chosen for optimisation.
        /// </summary>
        private static void Note(List<Space> spaces, double change_Lps, FlowClassification flowClassification, string basis, out string note)
        {
            List<string> names = spaces.ConvertAll(x => x.Name);
            names.Sort(StringComparer.Ordinal);

            note = string.Format(
                "The {0:0.###} l/s of {1} {2} to keep the dwelling balanced was allocated across {3} by {4}. This is a derived consequence of the targeted change, not a room selected for optimisation.",
                System.Math.Abs(change_Lps),
                Core.Query.Description(flowClassification),
                change_Lps >= 0 ? "needed" : "given up",
                names.Count == 0 ? "no room" : string.Join(", ", names.ConvertAll(x => string.Format("'{0}'", x))),
                basis);
        }

        /// <summary>
        /// Whether a room's extract is local to the cooking function, read off the Approved Document F
        /// lineage its design terminals carry rather than guessed from the room's name or use.
        /// </summary>
        private static bool IsLocalKitchenExtract(AdjacencyCluster adjacencyCluster, Space space)
        {
            foreach (VentilationTerminal ventilationTerminal in Query.VentilationTerminals(adjacencyCluster.VentilationTerminals(space), FlowClassification.Extract) ?? [])
            {
                if (ventilationTerminal?.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference)?.TerminalRole == PartFTerminalRole.LocalKitchenExtract)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
