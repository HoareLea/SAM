// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// One heat-balance term at a peak, exactly as stored: W, signed as the engine signs it (+ gain to room air,
    /// − loss). The sign is never normalised for presentation.
    /// </summary>
    public sealed class SpaceLoadPeakComponentData
    {
        public LoadPeakComponent Component { get; init; }

        public ReportValue<Quantity> Value { get; init; }
    }
}
