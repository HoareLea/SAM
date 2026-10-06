// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Ventilation design air flows [m³/s] and the assigned ventilation system.
    /// </summary>
    public sealed class SpaceVentilationData
    {
        public ReportValue<Quantity> SupplyAirFlow { get; init; }

        public ReportValue<Quantity> ExtractAirFlow { get; init; }

        public ReportValue<Quantity> OutsideAirFlow { get; init; }

        /// <summary>
        /// Supply air changes per hour, derived from the supply air flow and the space volume.
        /// </summary>
        public ReportValue<Quantity> SupplyAirChangeRate { get; init; }

        public ReportValue<string> SystemType { get; init; }

        public ReportValue<string> SupplyUnit { get; init; }

        public ReportValue<string> ExtractUnit { get; init; }
    }
}
