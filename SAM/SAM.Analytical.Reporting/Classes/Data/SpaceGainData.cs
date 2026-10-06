// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// One internal gain (lighting, equipment sensible or equipment latent).
    /// </summary>
    public sealed class SpaceGainData
    {
        /// <summary>
        /// Specific gain [W/m²] as authored on the internal condition.
        /// </summary>
        public ReportValue<Quantity> GainPerArea { get; init; }

        /// <summary>
        /// Total gain [W] for the space, derived from the internal condition and the space area / occupancy.
        /// </summary>
        public ReportValue<Quantity> Gain { get; init; }

        /// <summary>
        /// Design illuminance [lx]. Not applicable to equipment gains.
        /// </summary>
        public ReportValue<Quantity> Illuminance { get; init; }

        public ReportValue<string> Profile { get; init; }
    }
}
