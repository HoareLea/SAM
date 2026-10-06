// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// The value-independent part of a <see cref="ReportValue{T}"/>: availability and provenance.
    /// </summary>
    public interface IReportValue
    {
        Availability Availability { get; }

        bool HasValue { get; }

        /// <summary>
        /// Where the value came from. Null unless the value is available.
        /// </summary>
        ReportValueSource? Source { get; }

        /// <summary>
        /// How current the value is. Null unless the value is available.
        /// </summary>
        Freshness? Freshness { get; }

        /// <summary>
        /// When the source was produced, if known. Null unless the value is available.
        /// </summary>
        DateTime? SourceTimestamp { get; }

        /// <summary>
        /// Optional remark on an available value, or the reason a value is not available / not applicable.
        /// </summary>
        string Note { get; }
    }
}
