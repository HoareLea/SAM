// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Where a report value came from.
    /// </summary>
    public enum ReportValueSource
    {
        [Description("SAM")] SAM,
        [Description("TBD")] TBD,
        [Description("TSD")] TSD,
        [Description("Derived")] Derived,
        [Description("User")] User,

        /// <summary>
        /// A simulation result stored on the model, whichever engine produced it. Unlike <see cref="TSD"/> it
        /// names no engine: the result itself records that (Result.Source).
        /// </summary>
        [Description("Simulation Result")] SimulationResult,
    }
}
