// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [Description("Relational Operator")]
    public enum RelationalOperator
    {
        [Operator("=="), Description("Equal")] Equal,
        [Operator("!="), Description("Not Equal")] NotEqual,
        [Operator(">"), Description("Greater Than")] GreaterThan,
        [Operator("<"), Description("Less Than")] LessThan,
        [Operator("<="), Description("Less Than Or Equal")] LessThanOrEqual,
        [Operator(">="), Description("Greater Than Or Equal")] GreaterThanOrEqual,
    }
}
