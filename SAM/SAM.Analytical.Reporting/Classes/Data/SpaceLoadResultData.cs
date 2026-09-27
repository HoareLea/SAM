// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using System;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// A space's stored results for one load type: its design-day peak and its full-year peak, side by side and
    /// independent. Read only from the typed peaks; the legacy result values (Load, LoadIndex, SizingMethod) are
    /// never consulted.
    /// </summary>
    public sealed class SpaceLoadResultData
    {
        public LoadType LoadType { get; init; }

        public LoadResultStatus Status { get; init; }

        /// <summary>
        /// The source label the reported result carries (e.g. the engine's name). Available only when
        /// <see cref="Status"/> is <see cref="LoadResultStatus.Available"/>.
        /// </summary>
        public ReportValue<string> ResultSource { get; init; }

        /// <summary>
        /// When the results were read into the model (Result.DateTime). Not the simulation time, and not proof that
        /// the results match the current design.
        /// </summary>
        public ReportValue<DateTime> ConvertedAt { get; init; }

        public SpaceLoadPeakData DesignDay { get; init; }

        public SpaceLoadPeakData Annual { get; init; }
    }
}
