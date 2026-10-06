// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// What a document describes.
    /// </summary>
    public enum DocumentScope
    {
        [Description("Space")] Space,
        [Description("Building")] Building,
        [Description("System")] System,
    }
}
