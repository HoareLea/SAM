// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// One <b>proposed</b> targeted design airflow change, evaluated to its full engineering consequence
    /// against the selected ventilation unit - and the model that change produces, handed over
    /// <b>only</b> where every one of those consequences is valid.
    /// <para>
    /// <b>The difference from <see cref="DwellingDesignAirFlowChange"/> is the transaction boundary, not
    /// the engineering.</b> <see cref="Modify.ApplyTargetedDesignAirFlow"/> is a MANUAL engineering edit:
    /// an engineer chose a room and a number, the change commits on the model they handed in, and an
    /// equipment refusal afterwards is reported beside it rather than rolled back into it. That is
    /// deliberately correct for a person making a decision, and it is unchanged.
    /// </para>
    /// <para>
    /// An <b>optimisation</b> cannot use those semantics. It proposes changes it has not decided on yet,
    /// and a proposal the selected unit cannot carry has to leave no trace at all - not a design the
    /// engineer never chose, sitting in the model beside a refusal nobody may read. So the evaluation
    /// happens on a copy, and this object carries the resulting
    /// <see cref="AdjacencyCluster"/> <b>only when the whole candidate is valid</b>.
    /// </para>
    ///
    /// <para><b>Commit is adoption, and there is deliberately no second call</b></para>
    /// <para>
    /// This follows <see cref="PartOIterationPreparation"/>, which is how SAM already states a transaction
    /// that must not touch what it was given: the operation returns the resulting model on its result, and
    /// leaves it null where it refused. Taking <see cref="AdjacencyCluster"/> IS the commit.
    /// </para>
    /// <para>
    /// A separate <c>Commit(candidate)</c> call was considered and rejected. It would open a window between
    /// evaluation and commit in which the real model can move - another edit, another candidate committed
    /// first - and the candidate's DERIVED adjustments were balanced against the duties as they were at
    /// evaluation. Committing it afterwards would write balancing changes derived from a dwelling that no
    /// longer exists, and the resulting design would be unbalanced by exactly what happened in between. A
    /// stale candidate is not representable here: the only thing that can be committed is the whole
    /// evaluated model, which is internally consistent by construction.
    /// </para>
    ///
    /// <para><b>The authority boundary this exists to protect</b></para>
    /// <code>
    /// PartFRequiredAirFlow  !=  DesignAirFlow  !=  SelectedEquipmentCapacity  !=  OperatingAirFlow
    /// </code>
    /// <para>
    /// The candidate moves <b>design airflow only</b>. The Approved Document F requirement is read as a
    /// floor and never written. The selected unit's capacity is read as a CONSTRAINT - it decides whether
    /// the candidate is accepted, and it never becomes a design airflow, a requirement, or a target to
    /// grow into. Runtime/operating airflow is not touched at all; it is recalculated by re-preparing the
    /// iteration, exactly as it is after a manual edit.
    /// </para>
    /// </summary>
    public class DwellingDesignAirFlowCandidate
    {
        /// <summary>
        /// The model this candidate produces - the caller's model with the targeted and derived design
        /// airflows applied.
        /// <para>
        /// <b>Null unless the candidate was accepted</b>, so an invalid candidate cannot be adopted by
        /// mistake. The same rule <see cref="PartOIterationPreparation.AnalyticalModel"/> follows.
        /// </para>
        /// <para>
        /// The caller's own cluster is never reached by the evaluation, accepted or not.
        /// </para>
        /// </summary>
        public AdjacencyCluster AdjacencyCluster { get; internal set; }

        /// <summary>
        /// The airflow transaction itself, exactly as <see cref="Modify.ApplyTargetedDesignAirFlow"/>
        /// produced it on the candidate copy - the same targeted and derived adjustments, the same Part F
        /// floors, the same allocation. Never null after an evaluation that got as far as attempting one.
        /// </summary>
        public DwellingDesignAirFlowChange Change { get; internal set; }

        /// <summary>The one room the candidate targets, and what it would move it from and to.</summary>
        public DesignAirFlowAdjustment TargetedAdjustment
        {
            get
            {
                return Change?.TargetedAdjustment;
            }
        }

        /// <summary>
        /// The balancing changes the targeted one derives on the opposite side. Never a second target -
        /// see <see cref="Modify.ApplyTargetedDesignAirFlow"/>.
        /// </summary>
        public List<DesignAirFlowAdjustment> DerivedAdjustments
        {
            get
            {
                return Change is null ? [] : Change.DerivedAdjustments;
            }
        }

        /// <summary>The dwelling's design supply duty before the candidate, on the caller's model.</summary>
        public double SupplyDuty_Before_Lps { get; internal set; } = double.NaN;

        /// <summary>The dwelling's design extract duty before the candidate, on the caller's model.</summary>
        public double ExtractDuty_Before_Lps { get; internal set; } = double.NaN;

        /// <summary>The dwelling's design supply duty the candidate would produce.</summary>
        public double SupplyDuty_After_Lps
        {
            get
            {
                return Change is null ? double.NaN : Change.SupplyDuty_Lps;
            }
        }

        /// <summary>The dwelling's design extract duty the candidate would produce.</summary>
        public double ExtractDuty_After_Lps
        {
            get
            {
                return Change is null ? double.NaN : Change.ExtractDuty_Lps;
            }
        }

        /// <summary>The dwelling the candidate rebalances - resolved through the targeted room's terminals.</summary>
        public VentilationSystem VentilationSystem
        {
            get
            {
                return Change?.VentilationSystem;
            }
        }

        /// <summary>The air handling unit serving that dwelling, where one resolves.</summary>
        public AirHandlingUnit AirHandlingUnit { get; internal set; }

        /// <summary>
        /// The product that unit is <b>currently</b> selected as. Never changed by an evaluation - see
        /// <see cref="VentilationUnitSelectionOutcome"/>.
        /// </summary>
        public VentilationUnitReference VentilationUnitReference
        {
            get
            {
                return AirHandlingUnit?.SelectedVentilationUnitReference();
            }
        }

        /// <summary>
        /// What the currently selected product can move, where the catalogue offered to the evaluation
        /// describes it. Null where no catalogue was offered, nothing is selected, or the selection is not
        /// among the products offered - which is an unknown capacity, not an unlimited one.
        /// </summary>
        public VentilationUnitCapacityDescriptor VentilationUnitCapacityDescriptor { get; internal set; }

        /// <summary>
        /// What the evaluation found about the selected unit.
        /// <list type="bullet">
        /// <item><see cref="Enums.VentilationUnitSelectionOutcome.NotApplicable"/> - no catalogue was
        /// offered, no unit resolves, or nothing has ever been selected for it. Equipment is then not a
        /// constraint on this candidate, exactly as it is not one for a manual edit without a catalogue.</item>
        /// <item><see cref="Enums.VentilationUnitSelectionOutcome.Kept"/> - the selected product can carry
        /// the candidate's duty. Nothing was written to reach that answer.</item>
        /// <item><see cref="Enums.VentilationUnitSelectionOutcome.Refused"/> - it cannot, or its capacity
        /// is unknown. The candidate is rejected and no model is handed back.</item>
        /// </list>
        /// <para>
        /// <b><see cref="Enums.VentilationUnitSelectionOutcome.Reselected"/> is never produced here, and
        /// that is the point.</b> A manual edit may escalate the unit to the next capable product, because
        /// a person asked for the airflow and can be told the equipment had to grow. An optimiser exploring
        /// design airflow must not buy a bigger unit as a side effect of a proposal - the selected unit is
        /// the CONSTRAINT it is optimising within. Choosing a product is still exactly
        /// <see cref="Modify.SelectVentilationUnit"/>, called deliberately, on its own.
        /// </para>
        /// </summary>
        public VentilationUnitSelectionOutcome VentilationUnitSelectionOutcome { get; internal set; } = VentilationUnitSelectionOutcome.NotApplicable;

        /// <summary>Why the selected unit refused the candidate, where it did. Null otherwise.</summary>
        public string VentilationUnitSelectionReason { get; internal set; }

        /// <summary>
        /// What the selected product would have left on the supply side had the candidate been committed -
        /// its maximum less the candidate's design duty. NaN where the capacity is not known.
        /// <para>
        /// <b>Reported, never spent.</b> Headroom is not a target: a 150 l/s unit serving a dwelling
        /// designed at 63 l/s has 87 l/s of headroom and a design duty of 63, and nothing here proposes
        /// changing that merely because the headroom exists.
        /// </para>
        /// </summary>
        public double SupplyHeadroom_Lps { get; internal set; } = double.NaN;

        /// <summary>The same on the extract side. See <see cref="SupplyHeadroom_Lps"/>.</summary>
        public double ExtractHeadroom_Lps { get; internal set; } = double.NaN;

        /// <summary>What the evaluation found worth saying about a candidate it did not refuse.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>Design headroom and similar - legal, and not a reason to reject anything.</summary>
        public List<string> Warnings { get; } = [];

        /// <summary>
        /// Why the candidate was rejected. Carries the airflow transaction's own refusals where the change
        /// itself was impossible, and the equipment refusal where the selected unit could not carry it.
        /// </summary>
        public List<string> Refusals { get; } = [];

        /// <summary>
        /// Whether this candidate is a valid design that may be adopted - and therefore whether
        /// <see cref="AdjacencyCluster"/> is there to adopt.
        /// <para>
        /// Both halves have to hold: the airflow transaction succeeded on its own terms, AND the selected
        /// unit did not refuse it.
        /// </para>
        /// </summary>
        public bool IsAccepted
        {
            get
            {
                return Refusals.Count == 0 && AdjacencyCluster is not null && TargetedAdjustment is not null;
            }
        }
    }
}
