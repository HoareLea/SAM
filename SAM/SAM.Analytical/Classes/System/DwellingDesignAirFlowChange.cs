// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// What one design transaction did to a dwelling: the room it was aimed at, the rooms that moved as a
    /// consequence, and the duties the dwelling now has.
    /// <para>
    /// <b>All or nothing.</b> Where <see cref="Refusals"/> is non-empty nothing at all was written -
    /// <see cref="Successful"/> is false, both adjustment lists are empty, and the model is exactly as it
    /// was. A half-applied change is a dwelling whose supply and extract disagree, and that is the state
    /// the whole operation exists to make impossible.
    /// </para>
    /// <para>
    /// <b>Targeted and derived are kept apart on purpose.</b>
    /// <see cref="TargetedAdjustment"/> is the engineering decision; <see cref="DerivedAdjustments"/> are
    /// what the balanced network then required. Reporting them together would suggest the wet room was
    /// chosen for optimisation, which it was not.
    /// </para>
    /// </summary>
    public class DwellingDesignAirFlowChange
    {
        /// <summary>
        /// The room the change was aimed at, or null on a refusal. Exactly one - a transaction targets one
        /// room in one direction, and a caller wanting two makes two transactions.
        /// </summary>
        public DesignAirFlowAdjustment TargetedAdjustment { get; internal set; }

        /// <summary>
        /// Every room that moved as a consequence of keeping the dwelling balanced. Empty where the
        /// targeted change needed none - which happens when it moved nothing, and never otherwise on a
        /// balanced system.
        /// </summary>
        public List<DesignAirFlowAdjustment> DerivedAdjustments { get; } = [];

        /// <summary>The dwelling's design supply duty [l/s] after the transaction. <see cref="double.NaN"/> on a refusal.</summary>
        public double SupplyDuty_Lps { get; internal set; } = double.NaN;

        /// <summary>The dwelling's design extract duty [l/s] after the transaction. <see cref="double.NaN"/> on a refusal.</summary>
        public double ExtractDuty_Lps { get; internal set; } = double.NaN;

        /// <summary>
        /// The ventilation system the transaction was scoped to, or null on a refusal. One dwelling's
        /// system - nothing outside it was read and nothing outside it was written.
        /// </summary>
        public VentilationSystem VentilationSystem { get; internal set; }

        /// <summary>What was changed, what it was derived from, and by which allocation strategy.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>Advisories that do not make the transaction unsuccessful.</summary>
        public List<string> Warnings { get; } = [];

        /// <summary>Why nothing was written, one sentence each. Empty on success.</summary>
        public List<string> Refusals { get; } = [];

        /// <summary>Was the change applied?</summary>
        public bool Successful
        {
            get
            {
                return Refusals.Count == 0 && TargetedAdjustment is not null;
            }
        }

        /// <summary>
        /// What happened to the serving air handling unit's selected product, where a catalogue was
        /// offered to check it against. <see cref="VentilationUnitSelectionOutcome.NotApplicable"/> where
        /// none was, or where no air handling unit could be resolved for this system.
        /// <para>
        /// <b>Never part of <see cref="Successful"/>.</b> A design airflow change and an equipment
        /// adequacy check are separate questions - see <see cref="Modify.ApplyTargetedDesignAirFlow"/>'s
        /// own class documentation - so a <see cref="VentilationUnitSelectionOutcome.Refused"/> equipment
        /// outcome sits beside a <c>Successful</c> airflow change, not instead of it.
        /// </para>
        /// </summary>
        public VentilationUnitSelectionOutcome VentilationUnitSelectionOutcome { get; internal set; } = VentilationUnitSelectionOutcome.NotApplicable;

        /// <summary>The air handling unit this transaction's system supplies from, or null where none resolved.</summary>
        public AirHandlingUnit AirHandlingUnit { get; internal set; }

        /// <summary>
        /// The product identity <see cref="AirHandlingUnit"/> is now selected as, after this call -
        /// unchanged on <see cref="VentilationUnitSelectionOutcome.Kept"/> or
        /// <see cref="VentilationUnitSelectionOutcome.Refused"/>, the newly chosen product on
        /// <see cref="VentilationUnitSelectionOutcome.Reselected"/>, and null where nothing has ever been
        /// selected or no unit resolved.
        /// <para>
        /// Read off <see cref="AirHandlingUnit"/> on demand rather than stored separately, so the two can
        /// never disagree about what is actually selected.
        /// </para>
        /// </summary>
        public VentilationUnitReference VentilationUnitReference
        {
            get
            {
                return AirHandlingUnit?.SelectedVentilationUnitReference();
            }
        }

        /// <summary>
        /// Why equipment could not be validated as sufficient, where
        /// <see cref="VentilationUnitSelectionOutcome"/> is <see cref="VentilationUnitSelectionOutcome.Refused"/>.
        /// Null otherwise - a kept or reselected unit needs no explanation beyond the notes already given.
        /// </summary>
        public string VentilationUnitSelectionReason { get; internal set; }

        /// <summary>
        /// Every adjustment the transaction made, targeted first. A convenience for a report; the
        /// distinction between the two is not lost, because each adjustment carries
        /// <see cref="DesignAirFlowAdjustment.IsDerived"/>.
        /// </summary>
        public List<DesignAirFlowAdjustment> Adjustments
        {
            get
            {
                List<DesignAirFlowAdjustment> result = [];

                if (TargetedAdjustment is not null)
                {
                    result.Add(TargetedAdjustment);
                }

                result.AddRange(DerivedAdjustments);

                return result;
            }
        }
    }
}
