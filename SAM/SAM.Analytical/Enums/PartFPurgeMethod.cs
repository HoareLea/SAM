// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// How purge ventilation is provided to a habitable room, per Approved Document F, Volume 1:
    /// Dwellings (2021 edition, England) paragraph 1.28 (page 11).
    /// </summary>
    public enum PartFPurgeMethod
    {
        /// <summary>No purge provision is represented for this room.</summary>
        [Description("Not Represented")] NotRepresented,

        /// <summary>
        /// Openings, i.e. windows or external doors (paragraph 1.28a). The minimum opening areas of
        /// Table 1.4 (page 11) then apply.
        /// </summary>
        [Description("Openings")] Openings,

        /// <summary>A mechanical extract ventilation system (paragraph 1.28b).</summary>
        [Description("Mechanical Extract")] MechanicalExtract,
    }
}
