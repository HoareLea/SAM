// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("CombineType Enum")]
    public enum CombineType
    {
        [Description("Undefined")] Undefined,
        [Description("Sum")] Sum,
        [Description("Average")] Average,
        [Description("Min")] Min,
        [Description("Max")] Max,
    }
}
