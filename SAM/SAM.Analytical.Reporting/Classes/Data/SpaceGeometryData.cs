// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Space geometry in SAM canonical units (m², m³, m).
    /// </summary>
    public sealed class SpaceGeometryData
    {
        public ReportValue<Quantity> Area { get; init; }

        public ReportValue<Quantity> Volume { get; init; }

        /// <summary>
        /// Volume divided by area: an average, not a measured height.
        /// </summary>
        public ReportValue<Quantity> AverageHeight { get; init; }
    }
}
