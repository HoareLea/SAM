// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// What happened to an air handling unit's selected ventilation unit product after a design airflow
    /// change recalculated the dwelling's duty.
    /// <para>
    /// A targeted change never rolls back because equipment could not be validated: the design airflow
    /// this describes is a settled fact - see <see cref="Modify.ApplyTargetedDesignAirFlow"/> - and this
    /// enum reports what the <i>separate</i> equipment question resolved to, alongside it, never in place
    /// of it.
    /// </para>
    /// </summary>
    public enum VentilationUnitSelectionOutcome
    {
        /// <summary>
        /// No catalogue was offered, no air handling unit could be resolved for the system, or nothing has
        /// ever been selected on it. Equipment was not evaluated - not the same as being found sufficient.
        /// </summary>
        [Description("Not Applicable")] NotApplicable,

        /// <summary>The selected product still moves the recalculated design duty. Nothing was reselected.</summary>
        [Description("Kept")] Kept,

        /// <summary>
        /// The selected product no longer moves the recalculated design duty, and the next smallest
        /// capable product from the catalogue offered was selected in its place.
        /// </summary>
        [Description("Reselected")] Reselected,

        /// <summary>
        /// The selected product no longer moves the recalculated design duty, and no product in the
        /// catalogue offered can. The design airflow change already committed; the unit keeps whatever
        /// selection it had before this call.
        /// </summary>
        [Description("Refused")] Refused,
    }
}
