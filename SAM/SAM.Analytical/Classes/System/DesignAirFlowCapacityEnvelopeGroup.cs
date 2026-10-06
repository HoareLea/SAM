// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// One <b>serving equipment group</b>'s share of a selected-equipment capacity envelope: the air
    /// handling unit whose already-selected product is the ceiling, every ventilation system that unit
    /// supplies, how far the design those systems already carry could be grown proportionally before that
    /// ceiling bound, and which side of the unit bound it.
    ///
    /// <para><b>Why the group is the unit and not the dwelling</b></para>
    /// <para>
    /// A capacity ceiling belongs to a product, and a product is selected on an air handling unit -
    /// which <see cref="Query.AirHandlingUnitDesignDuty"/> already judges on the sum over <i>every</i>
    /// system it supplies. Two flats sharing one unit therefore share one ceiling and one scale factor:
    /// growing them independently would each spend headroom the other was also counting on, and the
    /// combined design would sit past a rating neither group thought it had reached. So the envelope is
    /// worked out per unit, and the Approved Document O one-unit-per-dwelling case is simply the shape this
    /// reduces to - never an assumption it depends on.
    /// </para>
    ///
    /// <para><b>The scale multiplies the design airflows themselves, absolutely</b></para>
    /// <para>
    /// <see cref="Scale"/> multiplies <i>every</i> space and direction the unit serves by one factor, so the
    /// design that comes out is the last valid design's own shape at a larger size - a flat at 40 supply /
    /// 22 + 18 extract on a 150/150 unit becomes 150 supply / 82.5 + 67.5 extract, and <c>22/18</c> is still
    /// <c>82.5/67.5</c>. It is <b>not</b> a scaling of the optimisation's deliberate increments, which spends
    /// the remaining headroom only on the rooms the optimiser happened to be pushing and would give
    /// 150 supply / 22 + 128 extract instead - coherent arithmetic, and a design nobody would build. See
    /// <see cref="Modify.EvaluateDesignAirFlowCapacityEnvelope"/> for why the diagnostic changed.
    /// </para>
    ///
    /// <para><b>Diagnostic, and only that</b></para>
    /// <para>
    /// Nothing here is an accepted design. The product on <see cref="AirHandlingUnit"/> is read and never
    /// written, no Approved Document F requirement moves, and no operating or runtime airflow is touched -
    /// see <see cref="DesignAirFlowCapacityEnvelope"/>.
    /// </para>
    /// </summary>
    public class DesignAirFlowCapacityEnvelopeGroup
    {
        internal DesignAirFlowCapacityEnvelopeGroup(AirHandlingUnit airHandlingUnit)
        {
            AirHandlingUnit = airHandlingUnit;
        }

        /// <summary>
        /// The air handling unit whose <b>already-selected</b> product is this group's ceiling. Null only
        /// where the group exists to record that no unit resolved at all - see
        /// <see cref="DesignAirFlowCapacityEnvelopeOutcome.CapacityUnresolved"/>.
        /// </summary>
        public AirHandlingUnit AirHandlingUnit { get; }

        /// <summary>The unit's name, so a report reads without resolving the object back through the model.</summary>
        public string Name => AirHandlingUnit?.Name;

        /// <summary>
        /// <b>Every</b> ventilation system this unit supplies - not only the ones a target named - in the
        /// order the envelope settled on, by system guid, so the report does not depend on the order the
        /// targets arrived in.
        /// <para>
        /// All of them, because the rating is compared against the unit's whole duty: a system left out of
        /// the growth would keep its old figure while the rest grew, and the answer would sit short of the
        /// ceiling while claiming to be on it.
        /// </para>
        /// </summary>
        public List<VentilationSystem> VentilationSystems { get; } = [];

        /// <summary>
        /// The product the unit is <b>currently</b> selected as. <b>Never changed by an envelope</b>: the
        /// whole question an envelope answers is what <i>this</i> product could deliver.
        /// </summary>
        public VentilationUnitReference VentilationUnitReference => AirHandlingUnit?.SelectedVentilationUnitReference();

        /// <summary>
        /// What that product can move, read from the catalogue the envelope was offered. Null where the
        /// capacity is not known - which is an unresolved ceiling, never an unlimited one.
        /// </summary>
        public VentilationUnitCapacityDescriptor VentilationUnitCapacityDescriptor { get; internal set; }

        /// <summary>The unit's design supply duty [l/s] before the envelope - the last ordinary accepted design's.</summary>
        public double SupplyDuty_Before_Lps { get; internal set; } = double.NaN;

        /// <summary>The unit's design extract duty [l/s] before the envelope.</summary>
        public double ExtractDuty_Before_Lps { get; internal set; } = double.NaN;

        /// <summary>The unit's design supply duty [l/s] the envelope produces. NaN where none was produced.</summary>
        public double SupplyDuty_After_Lps { get; internal set; } = double.NaN;

        /// <summary>The unit's design extract duty [l/s] the envelope produces.</summary>
        public double ExtractDuty_After_Lps { get; internal set; } = double.NaN;

        /// <summary>
        /// The factor <b>every</b> design airflow the unit serves was multiplied by - the one coherent
        /// growth of the whole design vector.
        /// <para>
        /// Always <b>greater than 1</b> on a scaled group: 1 is the last valid design restated, and a design
        /// already sitting on - or past - its rating is reported as
        /// <see cref="Enums.DesignAirFlowCapacityEnvelopeOutcome.NoHeadroom"/> rather than grown by a factor
        /// below 1, because an envelope never designs a dwelling downwards in the name of a diagnostic.
        /// </para>
        /// <para>NaN where no envelope was produced for this group.</para>
        /// </summary>
        public double Scale { get; internal set; } = double.NaN;

        /// <summary>
        /// The largest factor the selected product's capacity permits - <c>min(MaximumSupply / DesignSupply,
        /// MaximumExtract / DesignExtract)</c> - before the round itself was asked to confirm it.
        /// <see cref="Scale"/> equals it on the ordinary path, and is smaller where the deterministic solve
        /// had to retreat from it for a reason that is not capacity, which is then on <see cref="Notes"/>.
        /// </summary>
        public double Scale_Capacity { get; internal set; } = double.NaN;

        /// <summary>
        /// What the selected product has left on the supply side at the envelope design - its maximum less
        /// the unit's design duty. At a scaled envelope this is 0 on the binding side, within tolerance.
        /// <para><b>Reported, never spent.</b></para>
        /// </summary>
        public double SupplyHeadroom_Lps { get; internal set; } = double.NaN;

        /// <summary>The same on the extract side. See <see cref="SupplyHeadroom_Lps"/>.</summary>
        public double ExtractHeadroom_Lps { get; internal set; } = double.NaN;

        /// <summary>
        /// Which side of the selected product's rating the growth ran into, or
        /// <see cref="FlowClassification.Undefined"/> where the rating is <b>not</b> what stopped it.
        /// <para>
        /// <b>The tighter RATIO, not the tighter headroom</b> - a proportional growth meets the ratio, so
        /// the ratio is what is named. On the balanced designs an ordinary round admits the two agree, both
        /// sides carrying the same duty; see <see cref="Modify.EvaluateDesignAirFlowCapacityEnvelope"/> for
        /// why the ratio is nevertheless what is computed.
        /// </para>
        /// <para>
        /// Undefined on a <see cref="DesignAirFlowCapacityEnvelopeOutcome.Scaled"/> group is a real and
        /// meaningful state, not an omission: it says the growth was limited by something other than
        /// capacity, and <see cref="Scale_Capacity"/> standing above <see cref="Scale"/> is the evidence.
        /// A proportional growth of a compliant balanced design raises every room and moves both sides
        /// together, so in practice nothing else binds - but the round remains the authority and is asked
        /// rather than assumed, and this is what it says when the answer is no. Naming a side
        /// there would tell an engineer to buy a bigger unit that would not help, which is the opposite of
        /// what this diagnostic is for.
        /// </para>
        /// </summary>
        public FlowClassification BindingFlowClassification { get; internal set; } = FlowClassification.Undefined;

        /// <summary>What this group's envelope came to, or why none was calculated for it.</summary>
        public DesignAirFlowCapacityEnvelopeOutcome Outcome { get; internal set; } = DesignAirFlowCapacityEnvelopeOutcome.Undefined;

        /// <summary>
        /// Why, in one sentence - stated for <b>every</b> outcome including the successful one, because the
        /// value an envelope has to an engineer is the sentence and not the number.
        /// </summary>
        public string Reason { get; internal set; }

        /// <summary>How the scale was arrived at, and anything the solve had to retreat from.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>Whether this group produced a scaled envelope design.</summary>
        public bool IsScaled => Outcome == DesignAirFlowCapacityEnvelopeOutcome.Scaled;

        public override string ToString()
        {
            return string.Format(
                "{0}: x{1:0.####} -> {2:0.###}/{3:0.###} l/s ({4})",
                Name ?? "-",
                Scale,
                SupplyDuty_After_Lps,
                ExtractDuty_After_Lps,
                Core.Query.Description(Outcome));
        }
    }
}
