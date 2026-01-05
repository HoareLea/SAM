// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Enumerator defining the way in which two numbers are compared.")]
    public enum NumberComparisonType
    {
        [Description("Numer Equals")] Equals,
        [Description("Number Not Equals")] NotEquals,
        [Description("Number Greater Than")] Greater,
        [Description("Number Less Than")] Less,
        [Description("Number Less Than Or Equals")] LessOrEquals,
        [Description("Number Greater Than Or Equals")] GreaterOrEquals
    }
}
