// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// One dwelling's share of a single design airflow optimisation round: every room the round deliberately
    /// targeted there, the one balancing consequence those targets derived between them, the duties before
    /// and after, and what the dwelling's <b>already selected</b> ventilation unit made of the result.
    /// <para>
    /// <b>Targeted and derived are kept apart, and here it matters more than anywhere.</b> A round raises
    /// several rooms at once, so a report that merged the two would be claiming that every room which moved
    /// was chosen for optimisation. <see cref="TargetedAdjustments"/> is the engineering decision;
    /// <see cref="DerivedAdjustments"/> is what a balanced network then required, worked out
    /// <b>once</b> from the combined deliberate change rather than once per target - see
    /// <see cref="Modify.EvaluateTargetedDesignAirFlows"/>.
    /// </para>
    /// <para>
    /// <b>The four quantities stay apart.</b>
    /// <c>PartFRequiredAirFlow != DesignAirFlow != SelectedEquipmentCapacity != OperatingAirFlow</c>.
    /// <see cref="DesignAirFlowAdjustment.Requirement_Lps"/> is read as a floor and never written;
    /// <see cref="VentilationUnitCapacityDescriptor"/> is read as a constraint and never becomes a design
    /// airflow or a thing to grow into; no runtime airflow is touched at all.
    /// </para>
    /// </summary>
    public class DwellingDesignAirFlowRound
    {
        /// <summary>The dwelling this part of the round was scoped to - resolved through the targeted rooms' terminals.</summary>
        public VentilationSystem VentilationSystem { get; internal set; }

        /// <summary>The air handling unit that system supplies from, where one resolves.</summary>
        public AirHandlingUnit AirHandlingUnit { get; internal set; }

        /// <summary>
        /// Every room the round deliberately targeted in this dwelling, in the order the round settled on -
        /// sorted by room guid and direction, so it does not depend on the order the caller supplied them.
        /// </summary>
        public List<DesignAirFlowAdjustment> TargetedAdjustments { get; } = [];

        /// <summary>
        /// Every room that moved to keep this dwelling balanced. <b>Never a target</b>: a room the round
        /// deliberately targeted is excluded from the balancing allocation entirely, so a derived
        /// consequence can never overwrite an engineering decision.
        /// </summary>
        public List<DesignAirFlowAdjustment> DerivedAdjustments { get; } = [];

        /// <summary>Every adjustment this dwelling saw, targeted first. Each one still carries <see cref="DesignAirFlowAdjustment.IsDerived"/>.</summary>
        public List<DesignAirFlowAdjustment> Adjustments
        {
            get
            {
                List<DesignAirFlowAdjustment> result = [.. TargetedAdjustments];

                result.AddRange(DerivedAdjustments);

                return result;
            }
        }

        /// <summary>The dwelling's design supply duty [l/s] before the round, read off the caller's own model.</summary>
        public double SupplyDuty_Before_Lps { get; internal set; } = double.NaN;

        /// <summary>The dwelling's design extract duty [l/s] before the round.</summary>
        public double ExtractDuty_Before_Lps { get; internal set; } = double.NaN;

        /// <summary>The design supply duty [l/s] the round produces for this dwelling.</summary>
        public double SupplyDuty_After_Lps { get; internal set; } = double.NaN;

        /// <summary>The design extract duty [l/s] the round produces for this dwelling.</summary>
        public double ExtractDuty_After_Lps { get; internal set; } = double.NaN;

        /// <summary>
        /// The product the serving unit is <b>currently</b> selected as. Never changed by a round - see
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
        /// What that product can move, where the catalogue offered to the round describes it. Null where no
        /// catalogue was offered, nothing is selected, or the selection is not among the products offered -
        /// which is an unknown capacity, never an unlimited one.
        /// </summary>
        public VentilationUnitCapacityDescriptor VentilationUnitCapacityDescriptor { get; internal set; }

        /// <summary>
        /// What the round found about the selected unit.
        /// <list type="bullet">
        /// <item><see cref="Enums.VentilationUnitSelectionOutcome.NotApplicable"/> - equipment is not a
        /// constraint on this round: no catalogue, no unit, or nothing ever selected.</item>
        /// <item><see cref="Enums.VentilationUnitSelectionOutcome.Kept"/> - the selected product carries the
        /// round's combined duty, and nothing was written to find that out.</item>
        /// <item><see cref="Enums.VentilationUnitSelectionOutcome.Refused"/> - it cannot, or its capacity is
        /// unknown. The round is refused and no model is handed back.</item>
        /// </list>
        /// <para>
        /// <b><see cref="Enums.VentilationUnitSelectionOutcome.Reselected"/> is never produced.</b> The
        /// selected unit is the constraint an optimisation round explores within, not a variable it may
        /// move. Buying a bigger product is <see cref="Modify.SelectVentilationUnit"/>, called deliberately,
        /// on its own.
        /// </para>
        /// </summary>
        public VentilationUnitSelectionOutcome VentilationUnitSelectionOutcome { get; internal set; } = VentilationUnitSelectionOutcome.NotApplicable;

        /// <summary>Why the selected unit refused this dwelling's share of the round, where it did.</summary>
        public string VentilationUnitSelectionReason { get; internal set; }

        /// <summary>
        /// What the selected product would have left on the supply side had the round been adopted - its
        /// maximum less this dwelling's design duty. NaN where the capacity is not known.
        /// <para><b>Reported, never spent.</b> Headroom is not a target.</para>
        /// </summary>
        public double SupplyHeadroom_Lps { get; internal set; } = double.NaN;

        /// <summary>The same on the extract side. See <see cref="SupplyHeadroom_Lps"/>.</summary>
        public double ExtractHeadroom_Lps { get; internal set; } = double.NaN;

        /// <summary>What was changed here, on what basis, and what the unit made of it.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>Advisories that do not refuse the round.</summary>
        public List<string> Warnings { get; } = [];

        /// <summary>
        /// Why this dwelling's share of the round is not valid. <b>Non-empty refuses the whole round</b> -
        /// see <see cref="DesignAirFlowRoundCandidate"/>, which never hands back a model where any dwelling
        /// refused.
        /// </summary>
        public List<string> Refusals { get; } = [];

        /// <summary>
        /// Whether the selected ventilation unit is what refused this dwelling - the one refusal an
        /// automatic optimiser answers by stopping at capacity rather than by treating the design as broken.
        /// </summary>
        public bool IsVentilationUnitRefusal
        {
            get
            {
                return VentilationUnitSelectionOutcome == VentilationUnitSelectionOutcome.Refused;
            }
        }

        /// <summary>Whether this dwelling's share of the round holds together.</summary>
        public bool IsAccepted
        {
            get
            {
                return Refusals.Count == 0 && TargetedAdjustments.Count != 0;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0}: {1} targeted, {2} derived, {3:0.###}/{4:0.###} -> {5:0.###}/{6:0.###} l/s ({7})",
                VentilationSystem?.FullName ?? "-",
                TargetedAdjustments.Count,
                DerivedAdjustments.Count,
                SupplyDuty_Before_Lps,
                ExtractDuty_Before_Lps,
                SupplyDuty_After_Lps,
                ExtractDuty_After_Lps,
                IsAccepted ? "accepted" : "refused");
        }
    }
}
