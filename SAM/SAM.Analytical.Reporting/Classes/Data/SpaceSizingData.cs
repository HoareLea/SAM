// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Persisted Tas design loads and sizing factors.
    /// <para>
    /// Design loads come from <c>SpaceParameter.DesignHeatingLoad</c> / <c>DesignCoolingLoad</c> (written from the
    /// TBD by <c>UpdateDesignLoads</c>). Nothing records which sizing run produced them, so when present they are
    /// Available, Source = TBD, Freshness = Unknown (Rev 3 R3.1). TSD provenance is never used for them.
    /// </para>
    /// </summary>
    public sealed class SpaceSizingData
    {
        public DesignLoadStatus DesignLoadStatus { get; init; }

        public ReportValue<Quantity> DesignHeatingLoad { get; init; }

        public ReportValue<Quantity> DesignCoolingLoad { get; init; }

        public ReportValue<Quantity> DesignHeatingLoadPerArea { get; init; }

        public ReportValue<Quantity> DesignCoolingLoadPerArea { get; init; }

        /// <summary>
        /// Heating sizing factor: a multiplier on the Tas design load (SAM_Tas sets maxHeatingLoad × factor), so 1.2
        /// means ×1.20. The space value wins; 0 means not set, in which case the model value applies, as in the Tas
        /// export. Whether the persisted design load already includes it is not recorded. Set on neither, it is
        /// NotApplicable: SAM_Tas then applies no multiplier.
        /// </summary>
        public ReportValue<Quantity> HeatingSizingFactor { get; init; }

        public ReportValue<Quantity> CoolingSizingFactor { get; init; }
    }
}
