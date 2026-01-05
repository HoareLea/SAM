// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Delimited File Type")]
    public enum DelimitedFileType
    {
        [Description("Undefined")] Undefined,
        [Description("Csv")] Csv,
        [Description("Tab Delimited")] TabDelimited
    }
}
