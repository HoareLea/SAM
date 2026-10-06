// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// How the transfer air flow on one internal route was arrived at.
    /// <para>
    /// Approved Document F does not specify a unique litres per second value for every internal door.
    /// Where the dwelling's internal topology admits more than one airflow path, the split between them
    /// is an engineering decision, and this records which of those situations produced the number.
    /// </para>
    /// </summary>
    public enum PartFTransferRouteStatus
    {
        /// <summary>Nothing was computed for this route.</summary>
        [Description("Not Assessed")] NotAssessed,

        /// <summary>
        /// The dwelling's transfer graph is a tree, so conservation of air flow fixes the flow on this
        /// route exactly. No engineering choice was involved.
        /// </summary>
        [Description("Uniquely Determined")] UniquelyDetermined,

        /// <summary>
        /// More than one valid airflow path exists, so the documented deterministic allocation strategy
        /// was applied. The total is correct; the split between parallel paths is a design decision that
        /// the engineer may override.
        /// </summary>
        [Description("Calculated Using Allocation Strategy")] AllocationStrategy,

        /// <summary>The engineer supplied the transfer flow rate explicitly.</summary>
        [Description("User Overridden")] UserOverridden,

        /// <summary>
        /// Parallel transfer openings share one route and the split between them is not determined by
        /// the topology.
        /// </summary>
        [Description("Ambiguous")] Ambiguous,

        /// <summary>
        /// No transfer flow could be established, e.g. the space is disconnected from every extract
        /// location within its dwelling.
        /// </summary>
        [Description("Not Calculable")] NotCalculable,
    }
}
