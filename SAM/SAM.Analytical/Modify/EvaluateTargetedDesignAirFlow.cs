// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Works out what a targeted design airflow change would do - <b>all</b> of it, including whether
        /// the selected ventilation unit could carry the result - <b>without touching the model it was
        /// given</b>, and hands back the changed model only where every consequence is valid.
        ///
        /// <para><b>Why this exists beside <see cref="ApplyTargetedDesignAirFlow"/></b></para>
        /// <para>
        /// <see cref="ApplyTargetedDesignAirFlow"/> is a MANUAL engineering edit and its transactional
        /// semantics are deliberately correct for one: the airflow change commits, and an equipment refusal
        /// afterwards is reported beside it rather than rolled back into it. A person chose that airflow
        /// and can decide what to do about the unit. <b>Those semantics are unchanged, and nothing here
        /// changes them.</b>
        /// </para>
        /// <para>
        /// An optimisation cannot use them. It proposes changes it has not decided on, so it must never
        /// mutate the real design, discover afterwards that the selected equipment cannot carry it, and
        /// leave that design behind. This is the candidate/preflight half of that: propose, derive every
        /// balancing consequence, recalculate the duties, check every Approved Document F floor, check the
        /// selected unit - and on any failure, leave the caller's model completely untouched.
        /// </para>
        ///
        /// <para><b>The engineering is borrowed, not reimplemented</b></para>
        /// <para>
        /// Every rule this applies is applied by calling the code that already owns it, on the candidate
        /// copy: <see cref="ApplyTargetedDesignAirFlow"/> for the Part F floors, the opposite-side
        /// allocation, the balance preconditions and the duty recalculation, and
        /// <see cref="Query.IsVentilationUnitSufficient"/> for the equipment check. Nothing about Part F,
        /// balancing or the manufacturer catalogue is duplicated here, so this cannot drift from what a
        /// manual edit does.
        /// </para>
        ///
        /// <para><b>A candidate never reselects the unit</b></para>
        /// <para>
        /// The catalogue is used to READ the selected product's capacity and for nothing else - which is
        /// why <see cref="ApplyTargetedDesignAirFlow"/> is called <i>without</i> it below, and the check is
        /// made separately afterwards. A manual edit may escalate an outgrown unit to the next capable
        /// product; an optimiser must not, because the selected unit is the constraint it is exploring
        /// within, not a variable it gets to move. A candidate whose duty outgrows the selected product is
        /// REFUSED, even where the catalogue holds a bigger one.
        /// </para>
        ///
        /// <para><b>The copy, and why a shallow one is the right one</b></para>
        /// <para>
        /// <c>new AdjacencyCluster(adjacencyCluster)</c> - the same copy
        /// <see cref="AnalyticalModel.AdjacencyCluster"/> hands out, and the same one the Grasshopper seam
        /// already runs a manual edit on. Its object and relation dictionaries are rebuilt, so anything
        /// added to it is added to it alone. That is sufficient here because every write on the evaluated
        /// path is a same-guid REPLACEMENT rather than an in-place mutation:
        /// <see cref="SetSpaceDesignFlowRate"/> builds a new <see cref="VentilationTerminal"/> carrying the
        /// existing guid and adds it. The one place in this area that does mutate an object in place -
        /// <see cref="SelectVentilationUnit"/> writing the product reference onto the air handling unit -
        /// is never reached, precisely because a candidate never reselects.
        /// </para>
        /// <para>
        /// A deep clone was considered and is worse, not safer: <c>Core.Query.Clone</c> falls back to a
        /// parameterless constructor for any type without a copy constructor, which would put a
        /// DIFFERENTLY-guided object into the clone beside the original it failed to replace. The isolation
        /// this needs is pinned by test instead - the caller's whole cluster is compared before and after,
        /// on the accepted path and the refused one.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model to evaluate against. Never modified.</param>
        /// <param name="space">The one room the candidate targets.</param>
        /// <param name="flowClassification">Which side of that room the candidate moves - supply or extract.</param>
        /// <param name="designFlowRate_Lps">The design airflow the candidate proposes for it [l/s].</param>
        /// <param name="partFExtractAllocationStrategy">How the balancing consequence is shared out - passed
        /// straight through to <see cref="ApplyTargetedDesignAirFlow"/>.</param>
        /// <param name="tolerance_Lps">Flow rate comparison tolerance [l/s].</param>
        /// <param name="ventilationUnitCapacityDescriptors">
        /// The products the selected unit's capacity can be read from. Null makes equipment no constraint
        /// on the candidate at all - the same backward-compatible meaning it has for a manual edit.
        /// </param>
        /// <returns>
        /// The proposal, its derived consequences, the duties before and after, the selected unit's verdict
        /// and remaining headroom - and, only where all of it is valid, the model to adopt. Never null.
        /// </returns>
        public static DwellingDesignAirFlowCandidate EvaluateTargetedDesignAirFlow(this AdjacencyCluster adjacencyCluster, Space space, FlowClassification flowClassification, double designFlowRate_Lps, PartFExtractAllocationStrategy partFExtractAllocationStrategy = PartFExtractAllocationStrategy.MinimumFirstCookingPriority, double tolerance_Lps = 0.001, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors = null)
        {
            DwellingDesignAirFlowCandidate result = new();

            if (adjacencyCluster is null)
            {
                result.Refusals.Add("No model was supplied, so no design airflow candidate could be evaluated.");

                return result;
            }

            //THE boundary. Everything below happens here and nowhere the caller can see.
            AdjacencyCluster adjacencyCluster_Candidate = new(adjacencyCluster);

            //Deliberately WITHOUT the catalogue - see the class documentation. This is the whole airflow
            //transaction: Part F floors, opposite-side allocation, balance preconditions, duty recalculation.
            DwellingDesignAirFlowChange change = adjacencyCluster_Candidate.ApplyTargetedDesignAirFlow(space, flowClassification, designFlowRate_Lps, partFExtractAllocationStrategy, tolerance_Lps);

            result.Change = change;
            result.Warnings.AddRange(change.Warnings);

            //The dwelling's duty BEFORE, read off the caller's own model rather than recomputed on the
            //candidate: the point of reporting it is to say what adopting this would change, and a "before"
            //taken from the copy would only ever agree with itself.
            VentilationSystem ventilationSystem = change.VentilationSystem is null ? null : adjacencyCluster.GetObject<VentilationSystem>(change.VentilationSystem.Guid);
            if (ventilationSystem is not null)
            {
                adjacencyCluster.VentilationSystemDesignDuty(ventilationSystem, out double supplyDuty_Before_Lps, out double extractDuty_Before_Lps);

                result.SupplyDuty_Before_Lps = supplyDuty_Before_Lps;
                result.ExtractDuty_Before_Lps = extractDuty_Before_Lps;
            }

            if (!change.Successful)
            {
                //The airflow change itself was impossible. Its refusals are the candidate's refusals, and
                //there is nothing to check any equipment against.
                result.Refusals.AddRange(change.Refusals);

                return result;
            }

            //Resolved on the CANDIDATE, because the duty it will be judged against is the candidate's.
            AirHandlingUnit airHandlingUnit = Query.AirHandlingUnit(adjacencyCluster_Candidate, change.VentilationSystem);

            result.AirHandlingUnit = airHandlingUnit;

            if (!IsWithinSelectedVentilationUnit(adjacencyCluster_Candidate, airHandlingUnit, ventilationUnitCapacityDescriptors, result, tolerance_Lps))
            {
                //REFUSED, and no model handed back. This is the whole difference from the manual seam: the
                //caller's design is exactly as it was, and there is no half-applied candidate to find later.
                return result;
            }

            //Copied only NOW, and deliberately not on any path above. The transaction's notes are written
            //in the present tense - "the system now designs ... l/s" - which is true of a candidate that is
            //about to be adopted and false of one that was rejected. A rejected candidate's detail is still
            //there on Change.Notes for anyone who wants it; it just does not get to speak as though it had
            //happened.
            result.Notes.InsertRange(0, change.Notes);

            result.AdjacencyCluster = adjacencyCluster_Candidate;

            return result;
        }

        /// <summary>
        /// Whether the <b>currently selected</b> ventilation unit can carry the candidate's design duty,
        /// recording the verdict and the remaining headroom on <paramref name="result"/>.
        /// <para>
        /// Composed from <see cref="Query.IsVentilationUnitSufficient"/> and
        /// <see cref="Query.SelectedVentilationUnitCapacityDescriptor"/> rather than reimplemented, so the
        /// answer a candidate gets and the answer a manual edit gets are the same answer - including the
        /// conservative cases those already settle: an unknown capacity is a refusal rather than a pass,
        /// and a unit with no duty to speak of is not adequate by default.
        /// </para>
        /// <para>
        /// <b>Not applicable is not a refusal.</b> No catalogue offered, no unit resolving, or nothing ever
        /// selected all mean equipment is simply not a constraint on this candidate - the same meaning they
        /// have for <see cref="ApplyTargetedDesignAirFlow"/>, so a caller who never selected a product is
        /// not suddenly unable to explore a design.
        /// </para>
        /// </summary>
        private static bool IsWithinSelectedVentilationUnit(AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, DwellingDesignAirFlowCandidate result, double tolerance_Lps)
        {
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

            result.VentilationUnitCapacityDescriptor = ventilationUnitCapacityDescriptor;
            result.VentilationUnitSelectionOutcome = ventilationUnitSelectionOutcome;
            result.VentilationUnitSelectionReason = reason;

            if (!double.IsNaN(supplyHeadroom_Lps))
            {
                result.SupplyHeadroom_Lps = supplyHeadroom_Lps;
                result.ExtractHeadroom_Lps = extractHeadroom_Lps;
            }

            if (sufficient)
            {
                if (note is not null)
                {
                    result.Notes.Add(note);
                }

                return true;
            }

            //Added to Refusals, which is exactly where the manual seam deliberately does NOT put it. A
            //manual edit has already committed by this point and an equipment gap is reported beside it; a
            //candidate has committed nothing, so the gap rejects the whole proposal.
            result.Refusals.Add(string.Format(
                "The candidate design was calculated and then rejected, because the selected ventilation unit cannot carry it: {0} Nothing was changed - the model is exactly as it was. The candidate would have designed {1:0.###} l/s of supply and {2:0.###} l/s of extract. Reduce the targeted airflow, or select a larger product deliberately before proposing it again - a design change is not the place to buy equipment.",
                reason,
                result.SupplyDuty_After_Lps,
                result.ExtractDuty_After_Lps));

            return false;
        }

        /// <summary>
        /// The capacity verdict itself, with nowhere to write it - so a single candidate and a whole
        /// optimisation round can ask the same question and cannot get different answers to it.
        /// <para>
        /// Composed from <see cref="Query.IsVentilationUnitSufficient"/> and
        /// <see cref="Query.SelectedVentilationUnitCapacityDescriptor"/> rather than reimplemented, which is
        /// what keeps the conservative cases those already settle: an unknown capacity is a refusal rather
        /// than a pass, and a unit with no duty to speak of is not adequate by default.
        /// </para>
        /// <para>
        /// <b>Not applicable is not a refusal.</b> No catalogue, no unit, or nothing ever selected all mean
        /// equipment is simply not a constraint here, and the answer is true with the outcome left at
        /// <see cref="VentilationUnitSelectionOutcome.NotApplicable"/>.
        /// </para>
        /// <para>
        /// <b>Nothing is ever reselected.</b> The catalogue is read for the SELECTED product's rating and
        /// for nothing else.
        /// </para>
        /// </summary>
        /// <param name="supplyHeadroom_Lps">The rating less the duty, recorded whatever the verdict so a
        /// refusal can say how far past the unit it went. Negative where it is past. NaN where the capacity
        /// is not known.</param>
        /// <param name="note">The sentence to record on a kept unit, or null where equipment was not a
        /// constraint. Never written on a refusal - the caller words that itself, in its own terms.</param>
        private static bool IsWithinSelectedVentilationUnit(
            AdjacencyCluster adjacencyCluster,
            AirHandlingUnit airHandlingUnit,
            IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors,
            double tolerance_Lps,
            out VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor,
            out VentilationUnitSelectionOutcome ventilationUnitSelectionOutcome,
            out string reason,
            out double supplyHeadroom_Lps,
            out double extractHeadroom_Lps,
            out string note)
        {
            ventilationUnitCapacityDescriptor = null;
            ventilationUnitSelectionOutcome = VentilationUnitSelectionOutcome.NotApplicable;
            reason = null;
            supplyHeadroom_Lps = double.NaN;
            extractHeadroom_Lps = double.NaN;
            note = null;

            if (ventilationUnitCapacityDescriptors is null || airHandlingUnit is null || airHandlingUnit.SelectedVentilationUnitReference() is null)
            {
                return true;
            }

            ventilationUnitCapacityDescriptor = airHandlingUnit.SelectedVentilationUnitCapacityDescriptor(ventilationUnitCapacityDescriptors);

            if (ventilationUnitCapacityDescriptor is not null && adjacencyCluster.AirHandlingUnitDesignDuty(airHandlingUnit, out double supplyDuty_Lps, out double extractDuty_Lps))
            {
                supplyHeadroom_Lps = ventilationUnitCapacityDescriptor.MaximumSupplyFlowRate_Lps - supplyDuty_Lps;
                extractHeadroom_Lps = ventilationUnitCapacityDescriptor.MaximumExtractFlowRate_Lps - extractDuty_Lps;
            }

            if (!adjacencyCluster.IsVentilationUnitSufficient(airHandlingUnit, ventilationUnitCapacityDescriptors, out reason, tolerance_Lps))
            {
                ventilationUnitSelectionOutcome = VentilationUnitSelectionOutcome.Refused;

                return false;
            }

            ventilationUnitSelectionOutcome = VentilationUnitSelectionOutcome.Kept;

            note = string.Format(
                "Air handling unit '{0}' keeps its selected product '{1}': the candidate's design duty is within its rating, leaving {2:0.###} l/s supply and {3:0.###} l/s extract of headroom - which is reported and deliberately not spent.",
                airHandlingUnit.Name,
                airHandlingUnit.SelectedVentilationUnitReference(),
                supplyHeadroom_Lps,
                extractHeadroom_Lps);

            return true;
        }
    }
}
