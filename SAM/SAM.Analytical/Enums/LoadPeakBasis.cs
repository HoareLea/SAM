// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Which simulation a <see cref="SpaceLoadPeak"/> was taken from. A design-day peak and an annual peak are
    /// different results and are never merged into one value.
    /// </summary>
    [Description("Load Peak Basis.")]
    public enum LoadPeakBasis
    {
        [Description("Undefined")] Undefined,

        /// <summary>
        /// The peak of a heating or cooling design-day run. It has an hour of the design day but no calendar
        /// date: where the engine places the design day in its hourly series is an internal slot, not a date.
        /// </summary>
        [Description("Design Day")] DesignDay,

        /// <summary>The peak of the full-year simulation. It has an hour of the year and so a calendar date.</summary>
        [Description("Annual Simulation")] AnnualSimulation,
    }
}
