// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Whether a dwelling's selected Approved Document O strategy includes active cooling - one property of a
    /// <c>PartODwellingStrategy</c>, orthogonal to its ventilation route.
    /// <para>
    /// <b>Recorded, and refused, in PR1.</b> Any active cooling today moves the whole mechanical building onto
    /// the TAS Systems (TPD) route and strips every IZAM and ticV transfer, so a cooled dwelling cannot share a
    /// simulation with its non-cooled neighbours until the licensed PR3 proof settles that authority.
    /// <c>Modify.MaterialisePartODwellingStrategies</c> refuses <see cref="SupplyAirCooling"/> by name; see
    /// <c>documentation/PartO-MixedDwellingStrategies-PR0.md</c> §D5.
    /// </para>
    /// </summary>
    [Description("Part O Active Cooling.")]
    public enum PartOActiveCooling
    {
        /// <summary>Not stated, or unreadable. Never read as "no cooling": a strategy carrying it is invalid.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>No active cooling. The dwelling is assessed at its base iteration.</summary>
        [Description("None")] None,

        /// <summary>Active cooling of the MVHR supply air. Gated - see the enum.</summary>
        [Description("Supply Air Cooling")] SupplyAirCooling,
    }
}
