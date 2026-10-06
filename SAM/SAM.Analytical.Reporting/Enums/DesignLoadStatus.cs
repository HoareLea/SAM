// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// What is known about the persisted Tas design loads of a space.
    /// <para>
    /// Phase 1 can only say whether loads are present. No record ties <c>SpaceParameter.DesignHeatingLoad</c> /
    /// <c>DesignCoolingLoad</c> to a TBD sizing run, and TSD result provenance must never be used to imply their
    /// currency, so present loads are always <see cref="Unknown"/> (Rev 3 R3.1). Current / out-of-date states wait
    /// for a design-load provenance record.
    /// </para>
    /// </summary>
    public enum DesignLoadStatus
    {
        [Description("None")] None,
        [Description("Unknown")] Unknown,
    }
}
