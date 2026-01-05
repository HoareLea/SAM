// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [Description("Increment And Decrement Operator")]
    public enum IncrementAndDecrementOperator
    {
        [Operator("++"), Description("Increment")] Increment,
        [Operator("--"), Description("Decrement")] Decrement,
    }
}
