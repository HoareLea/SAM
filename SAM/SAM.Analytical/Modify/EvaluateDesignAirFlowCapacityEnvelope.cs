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
        /// Calculates the <b>selected-equipment capacity envelope</b> of a design: what the already-selected
        /// ventilation units could support if the <b>last valid design</b> were grown coherently to each
        /// unit's own capacity ceiling, <b>preserving the proportions between every terminal that unit
        /// serves</b> - without touching the model it was handed.
        ///
        /// <para><b>The question this answers, and the one it does not</b></para>
        /// <para>
        /// An ordinary Approved Document O Iteration 2B round is all-or-nothing at a fixed step: it designs
        /// every target at exactly the figure asked for or it refuses, and
        /// <see cref="EvaluateTargetedDesignAirFlows"/> is deliberately never allowed to settle for part of
        /// a step. <b>That rule is not weakened here and this operation is not a way round it.</b> An
        /// envelope is calculated only <i>after</i> the ordinary optimisation has stopped, and it answers a
        /// different question:
        /// </para>
        /// <para>
        /// <i>"If the last valid design were increased coherently while preserving its terminal airflow
        /// proportions, what design could the already-selected unit support at its capacity ceiling?"</i>
        /// </para>
        /// <para>
        /// It is emphatically <b>not</b> "how far can the current targeted optimisation direction continue?".
        /// That earlier reading scaled the ordinary <i>target vector</i>, which spends the remaining headroom
        /// only on the rooms the optimiser happened to be pushing - a flat designed 40 supply / 22 + 18
        /// extract on a 150/150 unit came out at 150 supply / 22 + 128 extract, with the bathroom carrying
        /// the whole increase and the studio's 22 l/s of extract left exactly where it was. Coherent
        /// arithmetic, but the wrong diagnostic: it describes a design nobody would build. The proportional
        /// reading gives 150 supply / 82.5 + 67.5 extract, which is the same dwelling, larger.
        /// </para>
        /// <para>
        /// The two answers are therefore kept in two places. <see cref="DesignAirFlowCapacityEnvelope.AdjacencyCluster"/>
        /// is a model to look at; it is never the accepted design and must never become the baseline of a
        /// later round, because a round computed from it would be computed from a design the optimiser's own
        /// policy refused.
        /// </para>
        ///
        /// <para><b>What the target vector is still for - scope, and nothing else</b></para>
        /// <para>
        /// The deliberate targets the ordinary policy would next have asked for no longer supply any
        /// <i>figure</i>. They say <b>which equipment the diagnostic is about</b>: the units serving the
        /// rooms that are actually failing. Each target is resolved through its own terminals to its system
        /// and from there to its air handling unit, exactly as an ordinary round resolves it - so a room the
        /// building has no lever for is still dropped with its reason, and a request that is not a design
        /// airflow at all still refuses the envelope. Nothing else about the vector is read, which is why an
        /// envelope no longer depends on that vector being a valid design at its own full step: whether the
        /// next ordinary round would have been accepted has no bearing on what the unit already bought could
        /// support.
        /// </para>
        ///
        /// <para><b>What is scaled - the whole unit's design vector, absolutely</b></para>
        /// <para>
        /// Every space and direction served by the unit is read off the last valid design and multiplied by
        /// <b>one factor</b>. The Flat 1 case:
        /// </para>
        /// <code>
        /// last valid   Studio   supply  40      selected  150/150 l/s
        ///              Studio   extract 22      scale     min(150/40, 150/40) = 3.75
        ///              Bathroom extract 18
        /// envelope     Studio   supply  150     unit 150/150, on the rating
        ///              Studio   extract 82.5    22/18 == 82.5/67.5 - the proportions survive
        ///              Bathroom extract 67.5
        /// </code>
        /// <para>
        /// Not every terminal independently at its maximum, not an equal split by terminal count, and not
        /// another instalment of the optimisation's target vector. One factor, applied to everything, so the
        /// design that comes out is the last valid design's own shape at a larger size - which is what makes
        /// it readable as a design rather than as an allocation artefact.
        /// </para>
        ///
        /// <para><b>Why one factor is coherent with the system-balance authority</b></para>
        /// <para>
        /// <see cref="EvaluateTargetedDesignAirFlows"/> refuses a dwelling that is not already balanced, so
        /// the design being grown has <c>supply == extract</c> at every system. Multiplying both sides by
        /// the same factor therefore moves them by the same amount, which is exactly what the round's
        /// balancing rule derives - so the scaled vector needs <b>no</b> derived consequence at all, and the
        /// balance holds by construction rather than by repair. Nothing about the existing balance authority
        /// is bypassed or duplicated: the scaled vector is handed to the ordinary round, which applies every
        /// Approved Document F floor, every attribution rule and every capacity verdict as usual.
        /// </para>
        ///
        /// <para><b>The limiting side, and why there is only one factor even when the ratings differ</b></para>
        /// <para>
        /// <c>scale = min(MaximumSupply / DesignSupply, MaximumExtract / DesignExtract)</c> over the sides
        /// that carry design air. The <b>first limiting ratio</b> binds both sides, because two different
        /// multipliers would change the relationship between supply and extract - which is the design vector
        /// itself, and the thing this operation exists to preserve. A side carrying no design air imposes no
        /// limit, since nothing multiplied by anything is still nothing.
        /// </para>
        ///
        /// <para><b>The group is the unit, and no dwelling-to-unit ownership is assumed</b></para>
        /// <para>
        /// A capacity ceiling belongs to a product, a product is selected on an air handling unit, and
        /// <see cref="Query.AirHandlingUnitDesignDuty"/> judges a unit on the sum over <b>every</b> system
        /// it supplies. So the factor is solved per unit over that whole duty, and applied to every system
        /// on it - including one no target named, because air the unit moves for that system is part of the
        /// duty the rating is compared against and leaving it out would put the answer off the ceiling. Two
        /// flats sharing one unit share one factor and grow together. The Approved Document O
        /// one-unit-per-dwelling arrangement is the shape this reduces to, never an assumption it rests on.
        /// </para>
        ///
        /// <para><b>How the factor is confirmed</b></para>
        /// <para>
        /// The factor is arithmetic, not a search: it is a division, and the scaled round is then evaluated
        /// once to confirm the resulting design is valid. Where that is refused for a reason belonging to
        /// the <i>source design</i> - an unbalanced dwelling, a room already below its Approved Document F
        /// floor - no scaling can help and the refusal is reported as it stands. Where the source design is
        /// fine and only the grown one is refused, a bounded, monotonic, deterministic bisection retreats
        /// within <c>[1, scale]</c> and the retreat is recorded.
        /// </para>
        /// <para>
        /// <b>No search state accumulates anywhere.</b> Every attempt is evaluated against the caller's own
        /// model, which <see cref="EvaluateTargetedDesignAirFlows"/> never modifies, so the source model is
        /// read many times and mutated never - and the same inputs always produce the same answer.
        /// </para>
        ///
        /// <para><b>Every authority stays where it was</b></para>
        /// <para>
        /// <c>PartFRequiredAirFlow != DesignAirFlow != SelectedEquipmentCapacity != OperatingAirFlow.</c>
        /// Design airflow moves and nothing else: the requirement is read as a floor by the round this
        /// delegates to and never written, the selected product is read as a ceiling and <b>never
        /// reselected</b>, and no operating, profile or runtime airflow is touched at all - so nothing here
        /// is the Iteration 3 behaviour of running a room at one airflow normally and another during hot
        /// hours. A proportional growth of a design that already meets Approved Document F can only raise
        /// every room, so no floor is even approached.
        /// </para>
        ///
        /// <para><b>Every "no" is stated</b></para>
        /// <para>
        /// No eligible target, no useful headroom, an unresolvable ceiling, a design vector that cannot be
        /// read safely - each is a different fact about the design and each is recorded as its own
        /// <see cref="DesignAirFlowCapacityEnvelopeOutcome"/> with its own sentence, per group and overall.
        /// A diagnostic that silently produces nothing is worse than no diagnostic.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">
        /// The <b>last accepted ordinary optimisation design</b> to envelope from - the design whose
        /// proportions are preserved. <b>Never modified.</b>
        /// </param>
        /// <param name="designAirFlowTargets">
        /// The deliberate target vector the normal policy would currently create. Read for <b>scope only</b>
        /// - which equipment the diagnostic is about - and never for its figures. Order is irrelevant. A
        /// target the building has no lever for is dropped with its reason exactly as an ordinary round
        /// drops it; a target that is not a design airflow at all refuses the envelope, for the same reason
        /// it refuses a round.
        /// </param>
        /// <param name="partFExtractAllocationStrategy">The strategy passed to the same round authority the
        /// ordinary optimisation uses. A proportional growth of a balanced design needs no balancing
        /// consequence, so in practice it has nothing to allocate - it is passed so an envelope cannot
        /// disagree with a round about how one would be shared out if it ever arose.</param>
        /// <param name="tolerance_Lps">Flow rate comparison tolerance [l/s].</param>
        /// <param name="ventilationUnitCapacityDescriptors">
        /// The products each unit's selected capacity is read from. <b>Required</b>: unlike an ordinary
        /// round, where a null catalogue means equipment is simply not a constraint, an envelope <i>is</i>
        /// the equipment ceiling, so without one there is nothing to calculate and the answer is
        /// <see cref="DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved"/>.
        /// </param>
        /// <returns>
        /// What each equipment group could reach and why, the grown round where one was reached, and the
        /// diagnostic model to simulate. Never null.
        /// </returns>
        public static DesignAirFlowCapacityEnvelope EvaluateDesignAirFlowCapacityEnvelope(this AdjacencyCluster adjacencyCluster, IEnumerable<DesignAirFlowTarget> designAirFlowTargets, PartFExtractAllocationStrategy partFExtractAllocationStrategy = PartFExtractAllocationStrategy.MinimumFirstCookingPriority, double tolerance_Lps = 0.001, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null)
        {
            DesignAirFlowCapacityEnvelope result = new();

            if (adjacencyCluster is null)
            {
                return Stop(result, DesignAirFlowCapacityEnvelopeOutcome.Refused, "No model was supplied, so no selected-equipment capacity envelope could be calculated.", true);
            }

            //FIRST, because every duty, ratio and scale comparison below is made against it.
            if (!Query.IsValidFlowRateTolerance(tolerance_Lps))
            {
                return Stop(result, DesignAirFlowCapacityEnvelopeOutcome.Refused, Query.FlowRateToleranceRefusal(tolerance_Lps), true);
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
                return Stop(result, DesignAirFlowCapacityEnvelopeOutcome.NoTargets, "No deliberate target was supplied, so there is no failing room to say which equipment this diagnostic is about. A capacity envelope grows the design served by the units the ordinary optimisation would next have pushed, and with nothing to push there is nothing to envelope.", false);
            }

            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_Temp = [];
            foreach (VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor in ventilationUnitCapacityDescriptors ?? [])
            {
                if (ventilationUnitCapacityDescriptor is not null)
                {
                    ventilationUnitCapacityDescriptors_Temp.Add(ventilationUnitCapacityDescriptor);
                }
            }

            if (ventilationUnitCapacityDescriptors_Temp.Count == 0)
            {
                //NOT the backward-compatible "equipment is no constraint" meaning a null catalogue has
                //everywhere else in this area. There, no catalogue means a design may be explored freely;
                //here the catalogue IS the thing being explored towards, and an unknown ceiling is never an
                //unlimited one - growing a design towards it would produce a design airflow with no
                //authority behind it at all.
                return Stop(result, DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved, "No ventilation unit products were offered, so no selected unit's capacity can be read and there is no ceiling for a capacity envelope to grow towards. An unknown capacity is not an unlimited one.", false);
            }

            // ---- Scope: the equipment the failing rooms sit on ------------------------------------------

            //The targets are read for their EQUIPMENT and not for their figures - see the summary. Resolved
            //through each room's OWN terminals by the same helper an ordinary round resolves with, so
            //nothing here assumes which dwelling a room belongs to or which unit serves it, and a dropped
            //target is dropped for exactly the reason a round would drop it.
            Dictionary<Guid, AirHandlingUnit> dictionary_AirHandlingUnit = [];
            Dictionary<Guid, VentilationSystem> dictionary_VentilationSystem_Unresolved = [];

            List<string> refusals_Scope = [];

            //ONE snapshot for this resolution pass. The model is only read here, and resolving each target's
            //room against the whole space list one target at a time made scoping the diagnostic quadratic in
            //the model.
            PartFIndex partFIndex_Resolution = new(adjacencyCluster);

            foreach (DesignAirFlowTarget designAirFlowTarget in designAirFlowTargets_Temp)
            {
                if (!Resolve(adjacencyCluster, partFIndex_Resolution, designAirFlowTarget, out Space _, out VentilationSystem ventilationSystem, out string refusal_Target, out bool malformed))
                {
                    if (malformed)
                    {
                        //NOT a dropped target. A room with no lever to move is an engineering fact about
                        //the building and the diagnostic goes on without it; a request that is not a design
                        //airflow at all - no room, no direction, not a number - is a caller that has asked
                        //for something incoherent, and quietly enveloping the equipment behind the rest
                        //would answer a question about a scope nobody stated.
                        refusals_Scope.Add(refusal_Target);

                        continue;
                    }

                    result.TargetRefusals.Add(new DesignAirFlowTargetRefusal(designAirFlowTarget, refusal_Target));

                    continue;
                }

                //A DUPLICATE target is harmless here and is deliberately not refused. An ordinary round
                //cannot tell which of two figures for one terminal set was meant; an envelope reads no
                //figure at all, so a room named twice names one unit twice and says the same thing.
                AirHandlingUnit airHandlingUnit = Query.AirHandlingUnit(adjacencyCluster, ventilationSystem);

                if (airHandlingUnit is null)
                {
                    dictionary_VentilationSystem_Unresolved[ventilationSystem.Guid] = ventilationSystem;

                    continue;
                }

                dictionary_AirHandlingUnit[airHandlingUnit.Guid] = airHandlingUnit;
            }

            //Sorted on the SAME key a round's are, so what an envelope reports is a function of the SET of
            //targets in every part - the dropped ones included - rather than of the order they arrived in.
            result.TargetRefusals.Sort(CompareTargetRefusals);

            foreach (DesignAirFlowTargetRefusal designAirFlowTargetRefusal in result.TargetRefusals)
            {
                //Carried whatever happens next: a room the building has no lever for is a fact about the
                //design and the diagnostic has to say so, exactly as an ordinary round does.
                result.Notes.Add(designAirFlowTargetRefusal.ToString());
            }

            if (refusals_Scope.Count != 0)
            {
                //Ordinally, for the same reason the dropped targets are sorted: the same set of incoherent
                //requests must read the same whichever of them the caller listed first.
                refusals_Scope.Sort(StringComparer.Ordinal);

                result.Refusals.AddRange(refusals_Scope);

                return Stop(result, DesignAirFlowCapacityEnvelopeOutcome.Refused, string.Format(
                    "The target vector this capacity envelope was given to scope itself by contains {0} request(s) that are not design airflows at all, so which equipment the diagnostic is about cannot be established: {1} Nothing was changed, and no envelope was calculated.",
                    refusals_Scope.Count,
                    string.Join(" ", refusals_Scope)), true);
            }

            if (dictionary_AirHandlingUnit.Count == 0 && dictionary_VentilationSystem_Unresolved.Count == 0)
            {
                //EVERY target was dropped, and a dropped target is the building being unable to answer a
                //coherent request rather than anything failing. So this is "nothing eligible left to
                //envelope" and not a refusal - the same distinction an ordinary round draws, and the one an
                //engineer acts differently on: a room with no design terminal on the side that failed needs
                //a design decision, not a bigger unit. Each dropped target's own reason is already on
                //Notes.
                return Stop(result, DesignAirFlowCapacityEnvelopeOutcome.NoTargets, string.Format(
                    "None of the {0} deliberate target(s) this capacity envelope was given can be taken - each one is reported with its own reason - so there is no failing room to say which equipment the diagnostic is about.",
                    designAirFlowTargets_Temp.Count), false);
            }

            // ---- One factor per unit, solved against that unit's WHOLE design vector --------------------

            //Units in NAME order, so what the envelope reports - and the order the grown targets are then
            //assembled and summed in - does not depend on the order the targets arrived in.
            List<Guid> guids = [.. dictionary_AirHandlingUnit.Keys];

            guids.Sort((x, y) =>
            {
                int comparison = string.CompareOrdinal(dictionary_AirHandlingUnit[x].Name, dictionary_AirHandlingUnit[y].Name);

                return comparison != 0 ? comparison : x.CompareTo(y);
            });

            List<DesignAirFlowTarget> designAirFlowTargets_Envelope = [];

            foreach (Guid guid in guids)
            {
                DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = Solve(
                    adjacencyCluster,
                    dictionary_AirHandlingUnit[guid],
                    partFExtractAllocationStrategy,
                    tolerance_Lps,
                    ventilationUnitCapacityDescriptors_Temp,
                    out List<DesignAirFlowTarget> designAirFlowTargets_Group);

                result.Groups.Add(designAirFlowCapacityEnvelopeGroup);

                if (designAirFlowCapacityEnvelopeGroup.IsScaled)
                {
                    designAirFlowTargets_Envelope.AddRange(designAirFlowTargets_Group);
                }
            }

            List<Guid> guids_Unresolved = [.. dictionary_VentilationSystem_Unresolved.Keys];
            guids_Unresolved.Sort();

            foreach (Guid guid in guids_Unresolved)
            {
                VentilationSystem ventilationSystem = dictionary_VentilationSystem_Unresolved[guid];

                //A group whose AirHandlingUnit is null is NOT a unit: it exists to record that a failing
                //dwelling resolves to no equipment at all, which is a diagnostic finding rather than
                //something to drop silently.
                DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup = new(null)
                {
                    Outcome = DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved,
                    Reason = string.Format(
                        "Ventilation system '{0}' resolves to no air handling unit, so there is no selected product whose capacity its design could be grown towards. No unit was invented and no ownership was assumed from any name.",
                        ventilationSystem.FullName),
                };

                designAirFlowCapacityEnvelopeGroup.VentilationSystems.Add(ventilationSystem);

                result.Groups.Add(designAirFlowCapacityEnvelopeGroup);
            }

            foreach (DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup in result.Groups)
            {
                result.Notes.Add(designAirFlowCapacityEnvelopeGroup.Reason);
                result.Notes.AddRange(designAirFlowCapacityEnvelopeGroup.Notes);

                //A REFUSED group is a refusal, and belongs where a caller looks for one rather than only in
                //the narrative - even where another group scaled and a model was produced. "This unit has
                //nothing left to give" is an answer and stays a note; "the design this unit serves could not
                //be grown at all" is not, and an engineer has to see it without reading every note.
                if (designAirFlowCapacityEnvelopeGroup.Outcome == DesignAirFlowCapacityEnvelopeOutcome.Refused && !string.IsNullOrWhiteSpace(designAirFlowCapacityEnvelopeGroup.Reason))
                {
                    result.Refusals.Add(designAirFlowCapacityEnvelopeGroup.Reason);
                }
            }

            if (designAirFlowTargets_Envelope.Count == 0)
            {
                //No group reached a ceiling worth simulating. Which of the reasons that is decides the
                //overall answer, and every one of them is already on a group.
                return Stop(result, Outcome(result), Reason(result), false);
            }

            // ---- The envelope design itself: ONE round over every grown group together --------------------

            //With the catalogue this time, so the model handed back has been through the same equipment
            //verdict an ordinary round applies. The groups are independent - each writes only its own
            //systems' rooms and is judged only on its own unit's duty - so a combined round over factors
            //that were each feasible alone is feasible; the refusal below is a loud statement of a drift
            //rather than an expected branch.
            DesignAirFlowRoundCandidate designAirFlowRoundCandidate = adjacencyCluster.EvaluateTargetedDesignAirFlows(designAirFlowTargets_Envelope, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors_Temp);

            if (!designAirFlowRoundCandidate.IsAccepted)
            {
                result.Refusals.AddRange(designAirFlowRoundCandidate.Refusals);

                return Stop(result, DesignAirFlowCapacityEnvelopeOutcome.Refused, string.Format(
                    "Every equipment group's capacity envelope was feasible on its own and the combined envelope was then refused, which should not be possible - the groups write different rooms and are judged on different units: {0} No envelope model was produced.",
                    string.Join(" ", designAirFlowRoundCandidate.Refusals)), true);
            }

            result.RoundCandidate = designAirFlowRoundCandidate;
            result.AdjacencyCluster = designAirFlowRoundCandidate.AdjacencyCluster;

            result.Warnings.AddRange(designAirFlowRoundCandidate.Warnings);

            //Read off the model the envelope actually produced, so the duties and headrooms a report shows
            //are the combined design's rather than the per-group solve's intermediate ones.
            foreach (DesignAirFlowCapacityEnvelopeGroup designAirFlowCapacityEnvelopeGroup in result.Groups_Scaled)
            {
                if (!designAirFlowRoundCandidate.AdjacencyCluster.AirHandlingUnitDesignDuty(designAirFlowCapacityEnvelopeGroup.AirHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps))
                {
                    continue;
                }

                designAirFlowCapacityEnvelopeGroup.SupplyDuty_After_Lps = supplyDuty_Lps;
                designAirFlowCapacityEnvelopeGroup.ExtractDuty_After_Lps = extractDuty_Lps;

                VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = designAirFlowCapacityEnvelopeGroup.VentilationUnitCapacityDescriptor;

                if (ventilationUnitCapacityDescriptor is not null)
                {
                    designAirFlowCapacityEnvelopeGroup.SupplyHeadroom_Lps = ventilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps - supplyDuty_Lps;
                    designAirFlowCapacityEnvelopeGroup.ExtractHeadroom_Lps = ventilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps - extractDuty_Lps;
                }
            }

            result.Notes.AddRange(designAirFlowRoundCandidate.Notes);

            return Stop(result, DesignAirFlowCapacityEnvelopeOutcome.Scaled, Reason(result), false);
        }

        /// <summary>
        /// One equipment group's factor: the last valid design vector of every system the unit supplies, the
        /// arithmetic ceiling that vector reaches, the round's confirmation of it, and - only where that was
        /// refused for a reason the source design does not already carry - a bounded deterministic retreat.
        ///
        /// <para><b>Why the arithmetic is a division and not a search</b></para>
        /// <para>
        /// The whole design vector is multiplied by one factor, so the unit's duty on each side is exactly
        /// <c>scale * duty</c> and the first binding constraint sits at exactly
        /// <c>min(MaximumSupply / DesignSupply, MaximumExtract / DesignExtract)</c>. Nothing is guessed at
        /// and nothing is iterated towards. A side carrying no design air imposes no limit, because nothing
        /// multiplied by anything is still nothing.
        /// </para>
        /// <para>
        /// <b>The tighter RATIO binds, not the tighter headroom</b> - which is what a proportional growth
        /// actually meets. On the balanced designs an ordinary round admits the two agree, because both
        /// sides carry the same duty and the smaller rating is therefore both the smaller headroom and the
        /// smaller ratio; a unit rated 150/120 on a design of 40/40 reaches its extract rating at x3 and has
        /// the tighter extract headroom too. They diverge only where the duties themselves differ, which
        /// <see cref="EvaluateTargetedDesignAirFlows"/> refuses outright - so the ratio is written and
        /// reported not because the headroom would currently give a different answer, but because it is the
        /// rule that stays correct if that precondition ever loosens, and because the ratio is the thing the
        /// growth is bounded by.
        /// </para>
        /// <para>
        /// <b>The headroom is not stretched by the tolerance.</b> Comparisons accept a duty a tolerance past
        /// the rating, and an envelope deliberately does not spend that: it exists to say what the product
        /// can carry, and a diagnostic that quietly borrowed a thousandth of a litre per second past the
        /// rating would be answering with a design the product is not rated for.
        /// </para>
        ///
        /// <para><b>Why there is a retreat at all</b></para>
        /// <para>
        /// Capacity is the only constraint the division accounts for. A proportional growth of a compliant
        /// balanced design raises every room and moves both sides together, so in practice nothing else
        /// binds - but the round remains the authority on that and is asked rather than assumed. Where it
        /// refuses the <i>source</i> design too, no factor repairs that and the refusal is reported as it
        /// stands; where only the grown design is refused, the retreat bisects <c>[1, scale]</c> - bounded
        /// by a fixed number of halvings, so it terminates and gives the same answer every time - and its
        /// use is recorded.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The caller's model. Read repeatedly, modified never.</param>
        /// <param name="designAirFlowTargets">The group's grown design vector, where one was reached.</param>
        private static DesignAirFlowCapacityEnvelopeGroup Solve(
            AdjacencyCluster adjacencyCluster,
            AirHandlingUnit airHandlingUnit,
            PartFExtractAllocationStrategy partFExtractAllocationStrategy,
            double tolerance_Lps,
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors,
            out List<DesignAirFlowTarget> designAirFlowTargets)
        {
            designAirFlowTargets = [];

            DesignAirFlowCapacityEnvelopeGroup result = new(airHandlingUnit);

            //EVERY system the unit supplies, not only the ones a target named - see the operation summary.
            //Sorted by guid, the same key the round orders a dwelling's targets on, so the vector below is
            //assembled and summed in an order that does not depend on the caller's.
            List<VentilationSystem> ventilationSystems = Query.VentilationSystems(adjacencyCluster, airHandlingUnit) ?? [];

            ventilationSystems.RemoveAll(x => x is null);
            ventilationSystems.Sort((x, y) => x.Guid.CompareTo(y.Guid));

            result.VentilationSystems.AddRange(ventilationSystems);

            // ---- What this group's selected product is rated at, and what it already carries -------------

            result.VentilationUnitCapacityDescriptor = airHandlingUnit.SelectedVentilationUnitCapacityDescriptor(ventilationUnitCapacityDescriptors);

            if (airHandlingUnit.SelectedVentilationUnitReference() is null)
            {
                result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved;
                result.Reason = string.Format(
                    "Air handling unit '{0}' has no ventilation unit product selected, so there is no capacity ceiling for a design to be grown towards. Nothing was selected to create one - buying equipment is a deliberate decision and never a consequence of a diagnostic.",
                    airHandlingUnit.Name);

                return result;
            }

            if (result.VentilationUnitCapacityDescriptor is null)
            {
                result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved;
                result.Reason = string.Format(
                    "Air handling unit '{0}' is selected as '{1}', which is not among the ventilation unit products offered, so its capacity is unknown and no ceiling can be grown towards. An unknown capacity is not an unlimited one.",
                    airHandlingUnit.Name,
                    airHandlingUnit.SelectedVentilationUnitReference());

                return result;
            }

            //A catalogue entry that EXISTS but does not state a usable capacity - a negative or non-finite
            //maximum, which VentilationUnitCapacityDescriptor.IsValid already rejects. Asked through that
            //property rather than restated, so what "a usable capacity" means cannot drift from what
            //selection means by it.
            //
            //Caught HERE, before the arithmetic, because the arithmetic would otherwise turn it into the
            //wrong answer rather than no answer: a NaN maximum gives a NaN ratio and a negative one gives a
            //ratio below 1, and both fall into the NoHeadroom branch below - reporting a malformed ceiling
            //as a perfectly good unit with nothing left to give. An unknown capacity is not an unlimited
            //one, and it is not an exhausted one either.
            if (!result.VentilationUnitCapacityDescriptor.IsValid)
            {
                result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved;
                result.Reason = string.Format(
                    "Air handling unit '{0}' is selected as '{1}', and the catalogue entry offered for it states {2:0.###}/{3:0.###} l/s, which is not a usable capacity - so its ceiling is unknown and no design can be grown towards it. An unknown capacity is neither an unlimited one nor an exhausted one.",
                    airHandlingUnit.Name,
                    result.VentilationUnitReference,
                    result.VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps,
                    result.VentilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps);

                return result;
            }

            //The duty of the LAST ACCEPTED ORDINARY DESIGN, read off the caller's own model through the same
            //authority that will judge the envelope - because that is the design being grown and the ratio
            //being solved is against what it already moves. Summed over every system the unit supplies.
            if (!adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Before_Lps, out double extractDuty_Before_Lps))
            {
                result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved;
                result.Reason = string.Format(
                    "Air handling unit '{0}' supplies no ventilation system carrying design terminals in the model being envelopped, so there is no design duty to grow.",
                    airHandlingUnit.Name);

                return result;
            }

            result.SupplyDuty_Before_Lps = supplyDuty_Before_Lps;
            result.ExtractDuty_Before_Lps = extractDuty_Before_Lps;

            // ---- The last valid design vector, room by room ----------------------------------------------

            if (!Vector(adjacencyCluster, ventilationSystems, out List<DesignAirFlowTarget> designAirFlowTargets_Design, out double supplyVector_Lps, out double extractVector_Lps, out string refusal_Vector))
            {
                result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.Refused;
                result.Reason = string.Format(
                    "The design airflow air handling unit '{0}' already moves could not be read room by room, so there is no design vector whose proportions a capacity envelope could preserve: {1} No envelope was calculated for this equipment, and nothing was repaired to make one possible.",
                    airHandlingUnit.Name,
                    refusal_Vector);

                return result;
            }

            //The vector is summed room by room through each system's own attributable terminals; the duty
            //above is summed over each system's terminals directly. They are two derivations of the same
            //quantity and a proportional envelope is only coherent while they agree - a terminal counted in
            //the duty but belonging to no room of the system would be air the growth cannot reach, and the
            //answer would sit short of the rating while claiming to be on it. Said loudly rather than
            //papered over.
            if (System.Math.Abs(supplyVector_Lps - supplyDuty_Before_Lps) > tolerance_Lps || System.Math.Abs(extractVector_Lps - extractDuty_Before_Lps) > tolerance_Lps)
            {
                result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.Refused;
                result.Reason = string.Format(
                    "Air handling unit '{0}' has a design duty of {1:0.###}/{2:0.###} l/s, and the design terminals of the rooms its systems serve total {3:0.###}/{4:0.###} l/s - so some of the air it moves belongs to no room of any system it supplies, and a proportional growth could not reach it. No envelope was calculated for this equipment; a design vector that cannot be read whole cannot have its proportions preserved.",
                    airHandlingUnit.Name,
                    supplyDuty_Before_Lps,
                    extractDuty_Before_Lps,
                    supplyVector_Lps,
                    extractVector_Lps);

                return result;
            }

            // ---- The coherent factor: the FIRST limiting ratio, applied to both sides ---------------------

            double maximum_Supply_Lps = result.VentilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps;
            double maximum_Extract_Lps = result.VentilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps;

            result.SupplyHeadroom_Lps = maximum_Supply_Lps - supplyDuty_Before_Lps;
            result.ExtractHeadroom_Lps = maximum_Extract_Lps - extractDuty_Before_Lps;

            //A side that carries no design air imposes NO limit: nothing multiplied by anything is still
            //nothing, and a rating it can never reach is not a constraint on the other side's growth.
            //Guarded on the tolerance rather than on zero so a residual thousandth cannot divide into a
            //ratio of thousands and pass itself off as the binding side.
            double ratio_Supply = supplyDuty_Before_Lps > tolerance_Lps ? maximum_Supply_Lps / supplyDuty_Before_Lps : double.PositiveInfinity;
            double ratio_Extract = extractDuty_Before_Lps > tolerance_Lps ? maximum_Extract_Lps / extractDuty_Before_Lps : double.PositiveInfinity;

            if (double.IsPositiveInfinity(ratio_Supply) && double.IsPositiveInfinity(ratio_Extract))
            {
                result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom;
                result.Reason = string.Format(
                    "Air handling unit '{0}' moves no design air at all - {1:0.###}/{2:0.###} l/s - so there is no design vector to grow and no proportions to preserve. Its selected product '{3}' could carry {4:0.###}/{5:0.###} l/s, and a capacity envelope grows an existing design rather than inventing one.",
                    airHandlingUnit.Name,
                    supplyDuty_Before_Lps,
                    extractDuty_Before_Lps,
                    result.VentilationUnitReference,
                    maximum_Supply_Lps,
                    maximum_Extract_Lps);

                return result;
            }

            //The TIGHTER RATIO binds - see the method summary for why that is not the tighter headroom.
            //Extract only where it is strictly tighter, so an exactly equal pair reports supply - arbitrary,
            //but fixed, and a diagnostic whose binding side depended on floating point noise would be
            //unreadable.
            double scale_Capacity = System.Math.Min(ratio_Supply, ratio_Extract);

            result.Scale_Capacity = scale_Capacity;
            result.BindingFlowClassification = ratio_Extract < ratio_Supply ? FlowClassification.Extract : FlowClassification.Supply;

            //How far the unit's duty would actually move at that factor. A factor of 1 or less - a design
            //sitting on, or somehow past, its rating - moves it nowhere or backwards, and an envelope never
            //designs a dwelling DOWNWARDS in the name of a diagnostic. Written as !(x > t) rather than
            //x <= t so a NaN falls into this branch instead of past it.
            double movement_Lps = System.Math.Max((scale_Capacity - 1) * supplyDuty_Before_Lps, (scale_Capacity - 1) * extractDuty_Before_Lps);

            if (!(movement_Lps > tolerance_Lps))
            {
                result.BindingFlowClassification = FlowClassification.Undefined;
                result.Scale_Capacity = double.NaN;

                result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom;
                result.Reason = string.Format(
                    "Air handling unit '{0}' is selected as '{1}', rated {2:0.###}/{3:0.###} l/s, and the last accepted design already has it moving {4:0.###}/{5:0.###} l/s - leaving {6:0.###}/{7:0.###} l/s. There is no useful headroom for that design to be grown into: this design IS what that selected product can support.",
                    airHandlingUnit.Name,
                    result.VentilationUnitReference,
                    maximum_Supply_Lps,
                    maximum_Extract_Lps,
                    supplyDuty_Before_Lps,
                    extractDuty_Before_Lps,
                    result.SupplyHeadroom_Lps,
                    result.ExtractHeadroom_Lps);

                return result;
            }

            // ---- Confirmed by the round, and retreated within only where the GROWTH is what refused ------

            double scale = scale_Capacity;

            DesignAirFlowRoundCandidate designAirFlowRoundCandidate = Round(adjacencyCluster, designAirFlowTargets_Design, scale, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors, out List<DesignAirFlowTarget> designAirFlowTargets_Scaled);

            if (!designAirFlowRoundCandidate.IsAccepted)
            {
                List<string> refusals = [.. designAirFlowRoundCandidate.Refusals];

                //Capacity is NOT what refused - the arithmetic point is feasible for the unit by
                //construction - so from here on nothing about this group's answer is bound by its rating,
                //and reporting a binding side would name a constraint that is not the one that stopped it.
                //Cleared before the retreat rather than after, so no path out of this block can carry the
                //stale value.
                result.BindingFlowClassification = FlowClassification.Undefined;

                //FIRST, the identity growth - the last valid design, restated as its own vector. It writes
                //every room back to the figure it already carries, so a round that refuses it is refusing
                //the SOURCE DESIGN rather than the growth: an unbalanced dwelling, a room already below its
                //Approved Document F floor, terminals that cannot be attributed. No factor repairs any of
                //those, so there is nothing to retreat to and the refusal is reported as it stands.
                //
                //Asked before the bisection rather than left to it, because halving an interval whose every
                //point is refused would copy the model thirty-two times to reach the same answer, and would
                //reach it with the wrong reason attached.
                if (!Round(adjacencyCluster, designAirFlowTargets_Design, 1, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors, out List<DesignAirFlowTarget> _).IsAccepted)
                {
                    result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.Refused;
                    result.Reason = string.Format(
                        "The last accepted design served by air handling unit '{0}' is not itself a valid design to grow, so no growth of it produces one either: {1} No envelope was calculated for this equipment, and nothing was repaired to make one possible.",
                        airHandlingUnit.Name,
                        string.Join(" ", refusals));

                    return result;
                }

                //A MONOTONIC deterministic retreat on [1, ceiling]. The upper end is known refused and the
                //lower end is the design we started from, known accepted, so the largest accepted point of
                //a fixed bisection is the answer - the same answer every time, without the source model
                //being written to once.
                scale = double.NaN;

                double scale_Low = 1;
                double scale_High = scale_Capacity;

                for (int i = 0; i < 32; i++)
                {
                    double scale_Middle = (scale_Low + scale_High) / 2;

                    //Below this the grown vector no longer raises the design by a quantity the tolerance can
                    //tell from nothing, so there is no envelope left to find on this interval.
                    if (!((scale_Middle - 1) * System.Math.Max(supplyDuty_Before_Lps, extractDuty_Before_Lps) > tolerance_Lps))
                    {
                        break;
                    }

                    DesignAirFlowRoundCandidate designAirFlowRoundCandidate_Middle = Round(adjacencyCluster, designAirFlowTargets_Design, scale_Middle, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors, out List<DesignAirFlowTarget> designAirFlowTargets_Middle);

                    if (designAirFlowRoundCandidate_Middle.IsAccepted)
                    {
                        scale = scale_Middle;
                        scale_Low = scale_Middle;

                        designAirFlowRoundCandidate = designAirFlowRoundCandidate_Middle;
                        designAirFlowTargets_Scaled = designAirFlowTargets_Middle;

                        continue;
                    }

                    scale_High = scale_Middle;
                }

                if (double.IsNaN(scale))
                {
                    result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.Refused;
                    result.Reason = string.Format(
                        "Air handling unit '{0}' could carry a x{1:0.####} growth of the design it already serves on its selected product '{2}' alone, and no growth of that design at all produces a valid one: {3} No envelope was calculated for this equipment, and nothing was repaired to make one possible.",
                        airHandlingUnit.Name,
                        scale_Capacity,
                        result.VentilationUnitReference,
                        string.Join(" ", refusals));

                    return result;
                }

                result.Notes.Add(string.Format(
                    "Air handling unit '{0}' could take a x{1:0.####} proportional growth of the design it serves on its selected product's capacity alone, and that design was refused for a reason which is not capacity, so the envelope retreated deterministically to x{2:0.####}. Its selected product is therefore NOT what limits this group - see the headroom left at that scale - and the constraint that does is: {3}",
                    airHandlingUnit.Name,
                    scale_Capacity,
                    scale,
                    string.Join(" ", refusals)));
            }

            result.Scale = scale;

            designAirFlowTargets = designAirFlowTargets_Scaled;

            result.Outcome = DesignAirFlowCapacityEnvelopeOutcome.Scaled;

            //Worded off what actually bound. A group that reached its rating says which side did; one that
            //retreated says the product is not the limit, because claiming a capacity ceiling it never
            //touched would send an engineer to buy a bigger unit that would not help.
            result.Reason = result.BindingFlowClassification == FlowClassification.Undefined
                ? string.Format(
                    "Air handling unit '{0}' keeps its selected product '{1}', rated {2:0.###}/{3:0.###} l/s, and the last accepted design it serves was grown proportionally by x{4:0.####} - which its rating did NOT limit: the product would have supported x{5:0.####}, and something else stopped the growth first (see the notes). Every terminal keeps its share of the design vector, so the dwelling that comes out is the one that went in, larger. This is a diagnostic capacity envelope and not an accepted optimisation round.",
                    airHandlingUnit.Name,
                    result.VentilationUnitReference,
                    maximum_Supply_Lps,
                    maximum_Extract_Lps,
                    result.Scale,
                    scale_Capacity)
                : string.Format(
                    "Air handling unit '{0}' keeps its selected product '{1}', rated {2:0.###}/{3:0.###} l/s, and the last accepted design it serves - {4:0.###}/{5:0.###} l/s - was grown proportionally by x{6:0.####}, the factor at which the {7} side of that rating binds first, from the {8:0.###}/{9:0.###} l/s of headroom that design left. Every terminal keeps its share of the design vector, so the dwelling that comes out is the one that went in, larger. This is a diagnostic capacity envelope and not an accepted optimisation round.",
                    airHandlingUnit.Name,
                    result.VentilationUnitReference,
                    maximum_Supply_Lps,
                    maximum_Extract_Lps,
                    supplyDuty_Before_Lps,
                    extractDuty_Before_Lps,
                    result.Scale,
                    Core.Query.Description(result.BindingFlowClassification),
                    maximum_Supply_Lps - supplyDuty_Before_Lps,
                    maximum_Extract_Lps - extractDuty_Before_Lps);

            return result;
        }

        /// <summary>
        /// The <b>last valid design vector</b> of every system one unit supplies, as one deliberate target
        /// per room and direction at the figure that room already carries - the thing a capacity envelope
        /// multiplies.
        ///
        /// <para><b>Why every room, and why absolutely</b></para>
        /// <para>
        /// The proportions being preserved are the proportions of the <i>whole</i> design, so every room on
        /// the unit is in the vector - including the ones no target named and the ones on the side nobody
        /// was pushing. A room left out would keep its old figure while the rest of the dwelling grew, which
        /// is exactly the distortion this reading of the envelope exists to remove.
        /// </para>
        ///
        /// <para><b>Read through the same attribution rule a write uses</b></para>
        /// <para>
        /// A room's design airflow is summed over <b>this system's</b> terminals in it, by the same helper
        /// <see cref="ApplyTargetedDesignAirFlow"/> reads and writes through. A room whose terminals of that
        /// direction are not all this system's cannot be read honestly or written safely, so it refuses the
        /// group rather than being filtered - the same answer, for the same reason, as an ordinary round.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model to read the design off. Never modified.</param>
        /// <param name="ventilationSystems">Every system the unit supplies, in guid order.</param>
        /// <param name="designAirFlowTargets">One target per room and direction, at the figure it already
        /// carries. Empty where the design could not be read.</param>
        /// <param name="supplyFlowRate_Lps">The vector's supply total [l/s], for reconciliation against the
        /// unit's own design duty.</param>
        /// <param name="extractFlowRate_Lps">The same on the extract side.</param>
        /// <param name="refusal">Why the design could not be read, where it could not.</param>
        /// <returns>False where the design cannot be read, with <paramref name="refusal"/> set.</returns>
        private static bool Vector(AdjacencyCluster adjacencyCluster, List<VentilationSystem> ventilationSystems, out List<DesignAirFlowTarget> designAirFlowTargets, out double supplyFlowRate_Lps, out double extractFlowRate_Lps, out string refusal)
        {
            designAirFlowTargets = [];
            supplyFlowRate_Lps = 0;
            extractFlowRate_Lps = 0;
            refusal = null;

            HashSet<string> keys = [];

            foreach (VentilationSystem ventilationSystem in ventilationSystems)
            {
                List<Space> spaces = [.. adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? []];

                spaces.RemoveAll(x => x is null);
                spaces.Sort((x, y) => x.Guid.CompareTo(y.Guid));

                foreach (Space space in spaces)
                {
                    foreach (FlowClassification flowClassification in new[] { FlowClassification.Supply, FlowClassification.Extract })
                    {
                        if (!TerminalsOfSystem(adjacencyCluster, space, flowClassification, ventilationSystem, out List<VentilationTerminal> ventilationTerminals, out string refusal_Attribution))
                        {
                            refusal = refusal_Attribution;

                            return Fail(out designAirFlowTargets, out supplyFlowRate_Lps, out extractFlowRate_Lps);
                        }

                        if (ventilationTerminals.Count == 0)
                        {
                            continue;
                        }

                        //One room and one direction can appear once. Two systems of the same unit both
                        //claiming a room would otherwise contribute it twice, and the round refuses a
                        //duplicated target - said here instead, where what is wrong can be named.
                        if (!keys.Add(Key(space, flowClassification)))
                        {
                            refusal = string.Format(
                                "Space '{0}' carries a design {1} airflow for more than one of the ventilation systems this air handling unit supplies, so how much of that room's air belongs to which system is not knowable and its share of the design vector cannot be stated once.",
                                space.Name,
                                Core.Query.Description(flowClassification));

                            return Fail(out designAirFlowTargets, out supplyFlowRate_Lps, out extractFlowRate_Lps);
                        }

                        double designFlowRate_Lps = ventilationTerminals.VentilationTerminalDesignDuty_Lps(flowClassification) ?? 0;

                        //A room total that is not a quantity of air cannot be multiplied. The round's own
                        //IsRedistributable catches the individual terminal behind it; this catches the
                        //total, which is what the factor would be applied to.
                        if (double.IsNaN(designFlowRate_Lps) || double.IsInfinity(designFlowRate_Lps) || designFlowRate_Lps < 0)
                        {
                            refusal = string.Format(
                                "Space '{0}' has a design {1} airflow of {2} l/s, which is not a quantity of air, so it has no share of the design vector to preserve.",
                                space.Name,
                                Core.Query.Description(flowClassification),
                                designFlowRate_Lps);

                            return Fail(out designAirFlowTargets, out supplyFlowRate_Lps, out extractFlowRate_Lps);
                        }

                        if (flowClassification == FlowClassification.Supply)
                        {
                            supplyFlowRate_Lps += designFlowRate_Lps;
                        }
                        else
                        {
                            extractFlowRate_Lps += designFlowRate_Lps;
                        }

                        designAirFlowTargets.Add(new DesignAirFlowTarget(space, flowClassification, designFlowRate_Lps));
                    }
                }
            }

            return true;
        }

        /// <summary>Clears a half-read design vector, so no caller can act on part of one.</summary>
        private static bool Fail(out List<DesignAirFlowTarget> designAirFlowTargets, out double supplyFlowRate_Lps, out double extractFlowRate_Lps)
        {
            designAirFlowTargets = [];
            supplyFlowRate_Lps = 0;
            extractFlowRate_Lps = 0;

            return false;
        }

        /// <summary>
        /// One group's design vector at a given factor, evaluated by the ordinary round authority against
        /// the caller's own model.
        ///
        /// <para><b>The airflows are scaled, absolutely, and every one of them is a target</b></para>
        /// <para>
        /// Each room's design airflow becomes <c>scale * before</c>. Every room on the unit is therefore a
        /// <i>deliberate</i> target of the diagnostic - which is the honest description of what happened to
        /// it: it was chosen, by this operation, to keep its share of the design vector. Since the design
        /// being grown is balanced at every system, both sides move by the same amount and the round derives
        /// <b>no</b> balancing consequence at all; where one ever arose it would be derived once, by the
        /// round, from the same allocator an ordinary round uses.
        /// </para>
        /// <para>
        /// <b>A capacity envelope's targets are not an optimisation's targets</b>, and a report must not
        /// print them as though they were - see the <c>SCALED</c> evidence type the presentation uses for
        /// them, which says the room moved because the whole design vector was grown rather than because the
        /// optimisation asked for that figure.
        /// </para>
        /// </summary>
        private static DesignAirFlowRoundCandidate Round(
            AdjacencyCluster adjacencyCluster,
            List<DesignAirFlowTarget> designAirFlowTargets_Design,
            double scale,
            PartFExtractAllocationStrategy partFExtractAllocationStrategy,
            double tolerance_Lps,
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors,
            out List<DesignAirFlowTarget> designAirFlowTargets)
        {
            designAirFlowTargets = designAirFlowTargets_Design.ConvertAll(x => new DesignAirFlowTarget(x.Space, x.FlowClassification, scale * x.DesignFlowRate_Lps));

            return adjacencyCluster.EvaluateTargetedDesignAirFlows(designAirFlowTargets, partFExtractAllocationStrategy, tolerance_Lps, ventilationUnitCapacityDescriptors);
        }

        /// <summary>
        /// The overall outcome, decided from the groups rather than restated - because every reason an
        /// envelope has is already a fact about one piece of equipment, and the run-level answer has to be
        /// the same one a reader would reach from the group rows.
        /// <para>
        /// A scaled group wins, because the envelope did produce a design worth simulating. Otherwise a
        /// refusal outranks the two "nothing to say" answers, which outrank an unresolved ceiling only in
        /// that a stated limit is more informative than an unknown one.
        /// </para>
        /// </summary>
        private static DesignAirFlowCapacityEnvelopeOutcome Outcome(DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope)
        {
            foreach (DesignAirFlowCapacityEnvelopeOutcome designAirFlowCapacityEnvelopeOutcome in new[]
            {
                DesignAirFlowCapacityEnvelopeOutcome.Scaled,
                DesignAirFlowCapacityEnvelopeOutcome.Refused,
                DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom,
                DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved,
            })
            {
                if (designAirFlowCapacityEnvelope.Groups.Exists(x => x.Outcome == designAirFlowCapacityEnvelopeOutcome))
                {
                    return designAirFlowCapacityEnvelopeOutcome;
                }
            }

            return DesignAirFlowCapacityEnvelopeOutcome.NoTargets;
        }

        /// <summary>The overall outcome in one sentence, counted off the groups.</summary>
        private static string Reason(DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope)
        {
            int count_Scaled = designAirFlowCapacityEnvelope.Groups_Scaled.Count;

            if (count_Scaled != 0)
            {
                return string.Format(
                    "{0} of {1} serving equipment group(s) had the design they already serve grown proportionally to the ceiling of the product selected for them, and the resulting design is a DIAGNOSTIC capacity envelope - what this dwelling and this equipment could support, never an accepted optimisation round and never the design a later round is computed from. Each group's own reason states what bound it.",
                    count_Scaled,
                    designAirFlowCapacityEnvelope.Groups.Count);
            }

            return string.Format(
                "No selected-equipment capacity envelope was calculated for any of the {0} serving equipment group(s) considered. Each one's own reason says why - which is the diagnostic.",
                designAirFlowCapacityEnvelope.Groups.Count);
        }

        /// <summary>
        /// Records the envelope's overall answer and hands it back - and, on anything but a scaled one,
        /// makes sure no model goes with it.
        /// </summary>
        /// <param name="refusal">Whether <paramref name="reason"/> is a refusal rather than a finding.
        /// "The selected unit has nothing left to give" is an answer; "the design vector could not be read"
        /// is not.</param>
        private static DesignAirFlowCapacityEnvelope Stop(DesignAirFlowCapacityEnvelope designAirFlowCapacityEnvelope, DesignAirFlowCapacityEnvelopeOutcome designAirFlowCapacityEnvelopeOutcome, string reason, bool refusal)
        {
            designAirFlowCapacityEnvelope.Outcome = designAirFlowCapacityEnvelopeOutcome;
            designAirFlowCapacityEnvelope.Reason = reason;

            if (refusal && !string.IsNullOrWhiteSpace(reason))
            {
                designAirFlowCapacityEnvelope.Refusals.Insert(0, reason);
            }

            if (designAirFlowCapacityEnvelopeOutcome != DesignAirFlowCapacityEnvelopeOutcome.Scaled)
            {
                designAirFlowCapacityEnvelope.AdjacencyCluster = null;
                designAirFlowCapacityEnvelope.RoundCandidate = null;
            }

            return designAirFlowCapacityEnvelope;
        }
    }
}
