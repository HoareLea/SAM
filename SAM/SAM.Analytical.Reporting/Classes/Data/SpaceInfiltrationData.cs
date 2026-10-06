// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Infiltration assumptions.
    /// </summary>
    public sealed class SpaceInfiltrationData
    {
        public ReportValue<Quantity> AirChangeRate { get; init; }

        /// <summary>
        /// Infiltration air flow [m³/s] = volume × air change rate.
        /// </summary>
        public ReportValue<Quantity> AirFlow { get; init; }

        public ReportValue<string> Profile { get; init; }
    }
}
