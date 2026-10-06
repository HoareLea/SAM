// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// What one ventilation terminal does, independently of what its room is.
    /// <para>
    /// A room can hold more than one terminal. Approved Document F, Volume 1: Dwellings (2021 edition,
    /// England) paragraph 1.67 requires mechanical supply to every habitable room, while paragraph 1.17a
    /// requires extract ventilation from the room containing the cooking function. A studio and an open
    /// plan living kitchen are both habitable (Appendix A: not <i>solely</i> a kitchen) and both contain
    /// the cooking function, so both requirements apply to the same room and both terminals must exist.
    /// </para>
    /// </summary>
    public enum PartFTerminalRole
    {
        /// <summary>No role established.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Mechanical supply to a habitable room, per paragraph 1.67 (page 16).
        /// </summary>
        [Description("Supply")] Supply,

        /// <summary>
        /// General wet room extract, per paragraph 1.17 (page 8) and Table 1.2 (page 10): a utility
        /// room, bathroom or sanitary accommodation.
        /// </summary>
        [Description("General Extract")] GeneralExtract,

        /// <summary>
        /// Extract local to the cooking function, per paragraph 1.17a (page 8) and Table 1.2 (page 10).
        /// Held separately from <see cref="GeneralExtract"/> because extract from a bathroom or ensuite
        /// may balance the dwelling airflow but is not local kitchen extract.
        /// </summary>
        [Description("Local Kitchen Extract")] LocalKitchenExtract,
    }
}
