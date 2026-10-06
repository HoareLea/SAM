// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Heating and cooling system references and risers.
    /// </summary>
    public sealed class SpaceSystemsData
    {
        public ReportValue<string> HeatingSystem { get; init; }

        public ReportValue<string> CoolingSystem { get; init; }

        public ReportValue<string> VentilationRiser { get; init; }

        public ReportValue<string> HeatingRiser { get; init; }

        public ReportValue<string> CoolingRiser { get; init; }
    }
}
