// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Occupancy assumptions of the space's internal condition.
    /// </summary>
    public sealed class SpaceOccupancyData
    {
        /// <summary>
        /// Number of people [person].
        /// </summary>
        public ReportValue<Quantity> People { get; init; }

        public ReportValue<Quantity> AreaPerPerson { get; init; }

        public ReportValue<Quantity> SensibleGainPerPerson { get; init; }

        public ReportValue<Quantity> LatentGainPerPerson { get; init; }

        public ReportValue<Quantity> SensibleGain { get; init; }

        public ReportValue<Quantity> LatentGain { get; init; }

        public ReportValue<string> Profile { get; init; }

        /// <summary>
        /// Hours per year in which the occupancy profile is non-zero [h].
        /// </summary>
        public ReportValue<Quantity> OccupiedHoursPerYear { get; init; }
    }
}
