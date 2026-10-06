// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// What a space's stored results say about one load type (heating or cooling), read only from the typed
    /// peaks (<c>SpaceSimulationResultParameter.DesignDayPeak</c> / <c>AnnualPeak</c>).
    /// </summary>
    public enum LoadResultStatus
    {
        /// <summary>No simulation result of this load type is related to the space.</summary>
        [Description("Not Simulated")] NotSimulated,

        /// <summary>
        /// A result of this load type exists but carries neither typed peak: it predates them, or its engine does not
        /// record them yet. Its legacy values are not used. Re-run the simulation.
        /// </summary>
        [Description("Peaks Not Recorded")] PeaksNotRecorded,

        /// <summary>Exactly one result of this load type carries typed peaks, and it is the one reported.</summary>
        [Description("Available")] Available,

        /// <summary>
        /// More than one result of this load type carries typed peaks (e.g. from different sources). None is chosen;
        /// pass a result source to the collector to pick one.
        /// </summary>
        [Description("Ambiguous")] Ambiguous,
    }
}
