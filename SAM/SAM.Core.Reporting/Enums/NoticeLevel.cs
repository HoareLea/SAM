// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Severity of a notice block.
    /// </summary>
    public enum NoticeLevel
    {
        [Description("Information")] Information,
        [Description("Warning")] Warning,
        /// <summary>
        /// A supporting remark under a block (for example the fabric "Not present" list), printed small rather than
        /// as a callout.
        /// </summary>
        [Description("Note")] Note,
    }
}
