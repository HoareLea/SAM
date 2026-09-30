// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// What makes authored ventilation plant duty-bearing - the structured half of a
    /// <c>PartOMechanicalDutyEvidence</c>; its message is the other half. See <c>Query.PartOAuthoredPlantDuty</c>.
    /// <para>Members are appended, never reordered.</para>
    /// </summary>
    [Description("Part O Mechanical Duty Evidence Kind.")]
    public enum PartOMechanicalDutyEvidenceKind
    {
        [Description("Undefined")] Undefined,

        /// <summary>A design terminal states a finite, non-zero design airflow.</summary>
        [Description("Terminal Design Airflow")] TerminalDesignAirFlow,

        /// <summary>A <see cref="SpaceAirMovement"/> of the system or unit moves a finite, non-zero airflow.</summary>
        [Description("Space Air Movement")] SpaceAirMovement,

        /// <summary>The unit has a ventilation unit product selected (<see cref="AirHandlingUnitParameter.VentilationUnitReference"/>).</summary>
        [Description("Selected Product")] SelectedProduct,
    }
}
