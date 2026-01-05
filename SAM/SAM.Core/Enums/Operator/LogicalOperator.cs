// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [Description("Logical Operator")]
    public enum LogicalOperator
    {
        [Operator("&&"), Description("And")] And,
        [Operator("||"), Description("Or")] Or,
        [Operator("!"), Description("Not")] Not,
    }
}
