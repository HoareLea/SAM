// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// How current an available value is relative to the model. Only meaningful when the value is available.
    /// </summary>
    public enum Freshness
    {
        [Description("Current")] Current,
        [Description("Out Of Date")] OutOfDate,
        [Description("Unknown")] Unknown,
    }
}
