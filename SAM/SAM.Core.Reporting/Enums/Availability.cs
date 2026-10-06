// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Whether a report value exists. Derived from how a ReportValue was constructed, never set directly.
    /// </summary>
    public enum Availability
    {
        [Description("Available")] Available,
        [Description("Not Available")] NotAvailable,
        [Description("Not Applicable")] NotApplicable,
    }
}
