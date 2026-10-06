// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Horizontal alignment of a table column.
    /// </summary>
    public enum ColumnAlignment
    {
        [Description("Left")] Left,
        [Description("Right")] Right,
        [Description("Center")] Center,
    }
}
